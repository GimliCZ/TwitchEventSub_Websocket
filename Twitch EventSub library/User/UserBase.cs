using Stateless;
using Twitch.EventSub.API.Models;
using Websocket.Client;

namespace Twitch.EventSub.User
{
    public abstract class UserBase
    {
        public const string DefaultWebSocketUrl = "wss://eventsub.wss.twitch.tv/ws";
        public Uri Url { get; set; }
        public UserState State { get; set; }
        public WebsocketClient Socket { get; set; }
        public string UserId { get; protected set; }
        public string SessionId { get; set; }
        public string Conduit { get; set; }
        public string AccessToken { get; protected set; }
        public Stateless.StateMachine<UserState, UserActions> StateMachine { get; set; }
        public CancellationTokenSource ManagerCancelationSource { get; set; }

        //TODO: Make global
        public string ClientId { get; set; }

        public List<CreateSubscriptionRequest> RequestedSubscriptions;
        public InvalidAccessTokenException? LastAccessViolationException { get; set; }
        internal event EventHandler<string?> OnDispose;

        public UserBase(string id, string access, List<CreateSubscriptionRequest> requestedSubscriptions)
        {
            State = UserState.Registred;
            Socket = new WebsocketClient(Url ?? new Uri(DefaultWebSocketUrl));
            Socket.IsReconnectionEnabled = false;
            UserId = id;
            AccessToken = access;
            StateMachine = new Stateless.StateMachine<UserState, UserActions>(() => State, s => State = s);
            StateMachineCofiguration(StateMachine);
            ManagerCancelationSource = new CancellationTokenSource();
            RequestedSubscriptions = requestedSubscriptions;
        }

        public bool IsDisposed()
        {
            return StateMachine.State == UserState.Disposed;
        }
        public async Task StartAsync()
        {
            await StateMachine.ActivateAsync();

            if (StateMachine.State == UserState.Registred)
            {
                await StateMachine.FireAsync(UserActions.AccessTesting);
            }
        }
        public bool Update(string access, List<CreateSubscriptionRequest> requestedSubscriptions)
        {
            if (State == UserState.Awaiting ||
                State == UserState.AwaitNewTokenAfterFailedTest ||
                State == UserState.AwaitNewTokenAfterFailedHandShake ||
                State == UserState.AwaitNewTokenAfterFailedRun)
            {
                AccessToken = access;
                RequestedSubscriptions = requestedSubscriptions;
                return true;
            }
            return false;
        }
        public async Task<bool> StopAsync()
        {
            if (StateMachine.State == UserState.Disposed)
            {
                return true;
            }
            if (StateMachine.CanFire(UserActions.Stop))
            {
                await StateMachine.FireAsync(UserActions.Stop);
                return true;
            }
            return false;
        }
        public enum UserState
        {
            //Inicial condition is registred, since we add to list 
            Registred,
            InicialAccessTest,
            Websocket,
            WellcomeMessage,
            HandShake,
            Running,
            Awaiting,
            Reconnecting,
            AwaitNewTokenAfterFailedTest,
            AwaitNewTokenAfterFailedHandShake,
            AwaitNewTokenAfterFailedRun,
            Stoping,
            Failing,
            Disposed
        }

        public enum UserActions
        {
            AccessTesting,
            AccessFailed,
            AccessSuccess,
            WebsocketFail,
            WebsocketSuccess,
            WelcomeMessageSuccess,
            WelcomeMessageFail,
            HandShakeFail,
            HandShakeAccessFail,
            HandShakeSuccess,
            RunningAwait,
            RunningProceed,
            RunningAccessFail,
            ReconnectRequested,
            NewTokenProvidedReturnToInicialTest,
            NewTokenProvidedReturnToHandShake,
            NewTokenProvidedReturnToRunning,
            AwaitNewTokenFailed,
            ReconnectSuccess,
            ReconnectFail,
            Stop,
            Fail,
            Dispose
        }

        private void StateMachineCofiguration(Stateless.StateMachine<UserState, UserActions> machine)
        {
            //If User is pressent within list, Try to test his access
            machine.Configure(UserState.Registred)
                .Permit(UserActions.AccessTesting, UserState.InicialAccessTest);
            //If result of testing is good, try to establish connection, else, request new token
            machine.Configure(UserState.InicialAccessTest)
                .OnEntryAsync(InicialAccessTokenAsync)
                .Permit(UserActions.AccessSuccess, UserState.Websocket)
                .Permit(UserActions.AccessFailed, UserState.AwaitNewTokenAfterFailedTest);
            //Try to establish connection, if succeeds, try to complete handshake, else, fail
            machine.Configure(UserState.Websocket)
                .OnEntryAsync(RunWebsocketAsync)
                .Permit(UserActions.WebsocketSuccess, UserState.WellcomeMessage)
                .Permit(UserActions.WebsocketFail, UserState.Failing);
            machine.Configure(UserState.WellcomeMessage).
                OnEntryAsync(AwaitWelcomeMessageAsync)
                .Permit(UserActions.WelcomeMessageSuccess, UserState.HandShake)
                .Permit(UserActions.WelcomeMessageFail, UserState.Failing);
            //If handShake Succeeds, then run, else if we get notification, we lost access, try to reestablish connection
            //Else, fail over
            machine.Configure(UserState.HandShake)
                .OnEntryAsync(RunHandshakeAsync)
                .Permit(UserActions.HandShakeSuccess, UserState.Running)
                .Permit(UserActions.HandShakeAccessFail, UserState.AwaitNewTokenAfterFailedHandShake)
                .Permit(UserActions.HandShakeFail, UserState.Failing)
                .Permit(UserActions.WebsocketFail, UserState.Failing);
            //If running, enable token reestablishment, stop and failover
            machine.Configure(UserState.Running)
                .PermitReentry(UserActions.RunningProceed)
                .OnEntryAsync(RunManagerAsync)
                .Permit(UserActions.RunningAwait, UserState.Awaiting)
                .Permit(UserActions.RunningAccessFail, UserState.AwaitNewTokenAfterFailedRun)
                .Permit(UserActions.ReconnectRequested, UserState.Reconnecting)
                .Permit(UserActions.Stop, UserState.Stoping)
                .Permit(UserActions.Fail, UserState.Failing)
                .Permit(UserActions.HandShakeFail, UserState.Failing)
                .Permit(UserActions.WebsocketFail, UserState.Failing);
            machine.Configure(UserState.Awaiting)
                .OnEntryAsync(AwaitManagerAsync)
                .Permit(UserActions.RunningProceed,UserState.Running)
                .Permit(UserActions.RunningAccessFail, UserState.AwaitNewTokenAfterFailedRun)
                .Permit(UserActions.ReconnectRequested, UserState.Reconnecting)
                .Permit(UserActions.Stop, UserState.Stoping)
                .Permit(UserActions.Fail, UserState.Failing)
                .Permit(UserActions.HandShakeFail, UserState.Failing)
                .Permit(UserActions.WebsocketFail, UserState.Failing);
            //If NewAcessToken Requested, Start timer and 
            machine.Configure(UserState.AwaitNewTokenAfterFailedTest)
                .OnEntryAsync(NewAccessTokenRequestAsync)
                .Permit(UserActions.NewTokenProvidedReturnToInicialTest, UserState.InicialAccessTest)
                .Permit(UserActions.AwaitNewTokenFailed, UserState.Stoping);
            machine.Configure(UserState.AwaitNewTokenAfterFailedHandShake)
                .OnEntryAsync(NewAccessTokenRequestAsync)
                .Permit(UserActions.NewTokenProvidedReturnToHandShake, UserState.HandShake)
                .Permit(UserActions.AwaitNewTokenFailed, UserState.Stoping);
            machine.Configure(UserState.AwaitNewTokenAfterFailedRun)
                .OnEntryAsync(NewAccessTokenRequestAsync)
                .Permit(UserActions.NewTokenProvidedReturnToRunning, UserState.Running)
                .Permit(UserActions.AwaitNewTokenFailed, UserState.Stoping);
            machine.Configure(UserState.Reconnecting)
                .Permit(UserActions.ReconnectSuccess, UserState.Running)
                .Permit(UserActions.ReconnectFail, UserState.Failing);
            machine.Configure(UserState.Stoping)
                .OnEntryAsync(StopProcedureAsync)
                .Permit(UserActions.Dispose, UserState.Disposed);
            machine.Configure(UserState.Failing)
                .OnEntryAsync(FailProcedureAsync)
                .Permit(UserActions.Dispose, UserState.Disposed);
            machine.Configure(UserState.Disposed)
                .OnEntryAsync(DisposeProcedureAsync);
        }

        protected abstract Task FailProcedureAsync();
        protected abstract Task StopProcedureAsync();
        private async Task DisposeProcedureAsync()
        {
            Socket.Dispose();
            await ManagerCancelationSource.CancelAsync();
            await StateMachine.DeactivateAsync();
            OnDispose?.Invoke(this,UserId);
        }

        protected abstract Task AwaitManagerAsync();
        
        protected abstract Task AwaitWelcomeMessageAsync();
        protected abstract Task InicialAccessTokenAsync();
        protected abstract Task NewAccessTokenRequestAsync();
        protected abstract Task RunManagerAsync();
        protected abstract Task RunHandshakeAsync();
        protected abstract Task RunWebsocketAsync();

    }
}
