﻿using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Twitch_EventSub_library.CoreFunctions
{
    public class GenericWebsocket
    {
        private ClientWebSocket? _clientWebSocket;
        private readonly Timer _sendTimer;
        private bool _sendIsProcessing;
        private bool _disconnectInProgress;
        private readonly TimeSpan _speedOfListening;
        private readonly ConcurrentQueue<string> _messagesToSend = new();
        private readonly ILogger<GenericWebsocket> _logger;
        private static CancellationTokenSource? _receiveCancelSource;
        private static CancellationTokenSource? _sendCancelSource;
        private ArraySegment<byte> _readBuffer;
        public Task? ReceiveTask { get; private set; }

        private MemoryStream? _readMemoryStream;

        private const int MaximumWaitTimeBeforeForcingDisconnection = 2000; // In ms

        public event AsyncEventHandler<string>? OnMessageReceived;
        public event AsyncEventHandler<string>? OnServerSideTerminationReasoning;

        public GenericWebsocket(
            ILogger<GenericWebsocket> logger,
            TimeSpan listenSpeed)
        {
            _sendIsProcessing = false;
            _logger = logger;
            _speedOfListening = listenSpeed;

            _sendTimer = new Timer(
                x => SendRoutineTick((CancellationToken)(x ?? throw new ArgumentNullException(nameof(x)))),
                _sendCancelSource?.Token,
                Timeout.InfiniteTimeSpan,
                TimeSpan.Zero);
        }

        private async Task InvokeMessageReceivedAsync(string e)
        {
            try
            {
                if (OnMessageReceived is not null)
                {
                    await OnMessageReceived(this, e);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Fail to invoke message received: {ex}", ex);
            }
        }

        /// <summary>
        ///     Connects to server
        /// </summary>
        /// <param name="serverAddress">Server address to connect to.</param>
        public async Task<bool> ConnectAsync(string serverAddress)
        {
            _receiveCancelSource = new CancellationTokenSource();
            _sendCancelSource = new CancellationTokenSource();
            _readBuffer = new byte[4096];
            _readMemoryStream = new();

            _clientWebSocket = new ClientWebSocket();

            if (serverAddress == null)
            {
                throw new ArgumentNullException(nameof(serverAddress));
            }

            try
            {
                await _clientWebSocket.ConnectAsync(new Uri(serverAddress), CancellationToken.None);
                ReceiveTask = ReceiveRoutineAsync(_receiveCancelSource.Token);
                _sendTimer.Change(TimeSpan.Zero, _speedOfListening);
                return true;
            }
            catch (WebSocketException ex)
            {
#pragma warning disable CA2254
                _logger.LogDebug("Case of incomplete disconnect - close sent / close received, proceed" + ex.Message);
#pragma warning restore CA2254
            }
            catch (OperationCanceledException ex)
            {
#pragma warning disable CA2254
                _logger.LogDebug("Process canceled, proceed" + ex.Message);
#pragma warning restore CA2254
            }

            return false;
        }

        /// <summary>
        ///     Receiving loop
        /// </summary>
        private async Task? ReceiveRoutineAsync(CancellationToken cancel)
        {
            while (!cancel.IsCancellationRequested)
            {
                try
                {
                    if (_clientWebSocket != null && _clientWebSocket.State != WebSocketState.Closed && !cancel.IsCancellationRequested)
                    {
                        await ReceiveDataInternalAsync(cancel);
                    }
                    else
                    {
                        _sendCancelSource?.Cancel();
                        _clientWebSocket?.Dispose();
                        _clientWebSocket = null;
                    }
                }
                catch (WebSocketException ex)
                {
#pragma warning disable CA2254
                    _logger.LogDebug("Case of running into cancel while receiving - close sent / close received" + ex.Message);
#pragma warning restore CA2254
                    await DisconnectAsync();
                }
                catch (OperationCanceledException)
                {
                    // normal upon task/token cancellation
                }
                catch (Exception ex)
                {
                    _logger.LogError("Web socket encountered unexpected error: {ex}", ex);
                }

                await Task.Delay(200, cancel);
            }
        }

        private async Task ReceiveDataInternalAsync(CancellationToken cancel)
        {
            var receiveResult = await _clientWebSocket?.ReceiveAsync(_readBuffer, cancel)!;

            // If the token is canceled while ReceiveAsync is blocking, the socket state changes to aborted and it can't be used
            if (cancel.IsCancellationRequested)
            {
                return;
            }

            // Termination received from server
            if (_clientWebSocket.State == WebSocketState.CloseReceived &&
                receiveResult.MessageType == WebSocketMessageType.Close)
            {

                if (receiveResult.CloseStatusDescription != null && OnServerSideTerminationReasoning != null)
                {
                    await OnServerSideTerminationReasoning.Invoke(this, receiveResult.CloseStatusDescription);
                }
                _sendCancelSource?.Cancel();
                await _clientWebSocket.CloseOutputAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Acknowledge Close frame",
                    CancellationToken.None);
            }

            // Received some data
            if (_clientWebSocket.State == WebSocketState.Open &&
                receiveResult.MessageType != WebSocketMessageType.Close)
            {
                if (_readMemoryStream != null)
                {
                    await _readMemoryStream.WriteAsync(_readBuffer.Array.AsMemory(0, receiveResult.Count), cancel);
                    if (_readBuffer.Array != null)
                        Array.Clear(_readBuffer.Array);
                    if (receiveResult.EndOfMessage)
                    {
                        _readMemoryStream.Seek(0, SeekOrigin.Begin);

                        using (StreamReader readStream = new(_readMemoryStream,
                                   Encoding.UTF8,
                                   true))
                        {
                            var message = await readStream.ReadToEndAsync();
                            await InvokeMessageReceivedAsync(message);

                            // Console.WriteLine(message);
                        }
                        _readMemoryStream = new MemoryStream();
                    }
                }
            }
        }

        /// <summary>
        ///     Sending loop
        /// </summary>
        private async void SendRoutineTick(CancellationToken cancel)
        {
            if (_sendIsProcessing)
            {
                // Sending is in progress, wait before it's finished
                return;
            }

            _sendIsProcessing = true;
            try
            {
                if (!cancel.IsCancellationRequested && _messagesToSend.TryDequeue(out var message))
                {
                    await _clientWebSocket?.SendAsync(
                        Encoding.UTF8.GetBytes(message),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None)!;
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during sending a message for Web Socket: {Ex}", ex);
            }
            finally
            {
                _sendIsProcessing = false;
            }
        }

        /// <summary>
        ///     Send message.
        /// </summary>
        /// <param name="message">Message to be send.</param>
        public void Send(string message)
        {
            _messagesToSend.Enqueue(message);
        }

        /// <summary>
        ///     Disconnects from the client.
        /// </summary>
        public async Task DisconnectAsync()
        {
            _sendTimer?.Change(Timeout.InfiniteTimeSpan, TimeSpan.Zero);
            _sendCancelSource?.Cancel();

            if (_clientWebSocket == null ||
                _clientWebSocket.State != WebSocketState.Open ||
                _disconnectInProgress)
            {
                return;
            }

            _disconnectInProgress = true;
            _logger.LogInformation("Disconnecting web socket...");
            var timeout = new CancellationTokenSource(MaximumWaitTimeBeforeForcingDisconnection);
            try
            {
                await _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token);

                while (_clientWebSocket.State != WebSocketState.Closed && !timeout.Token.IsCancellationRequested)
                {
                    // Awaiting server response, wait 200ms before checking again
                    await Task.Delay(200, CancellationToken.None);
                }
            }
            catch (OperationCanceledException)
            {
            }

            _receiveCancelSource?.Cancel();
            _disconnectInProgress = false;
            _readMemoryStream = null;
            if (_readBuffer.Array != null)
                Array.Clear(_readBuffer.Array);
        }
    }
}
