﻿using Twitch.EventSub.API.Models;
using Websocket.Client;

namespace Twitch.EventSub.User
{
    public abstract class UserBase
    {
        public const string DefaultWebSocketUrl = "wss://eventsub.wss.twitch.tv/ws";
        public Uri Url { get; set; }
        public UserState State { get; set; }
        public WebsocketClient Socket { get; set; }
        public string UserIdInternal { get; set; }
        public string SessionId { get; set; }
        public string Conduit { get; set; }
        public string AccessToken { get; set; }
        public Stateless.StateMachine<UserState, UserActions> StateMachine { get; set; }
        public CancellationTokenSource ManagerCancelationSource { get; set; }

        //TODO: Make global
        public string ClientId { get; set; }

        public List<CreateSubscriptionRequest> RequestedSubscriptions;
        public InvalidAccessTokenException? LastAccessViolationException { get; set; }

        public UserBase(string id, string access, List<CreateSubscriptionRequest> requestedSubscriptions)
        {
            State = UserState.Registred;
            Socket = new WebsocketClient(Url ?? new Uri(DefaultWebSocketUrl));
            UserIdInternal = id;
            AccessToken = access;
            StateMachine = new Stateless.StateMachine<UserState, UserActions>(() => State, s => State = s);
            StateMachineCofiguration(StateMachine);
            ManagerCancelationSource = new CancellationTokenSource();
            RequestedSubscriptions = requestedSubscriptions;
        }
        public async Task StartAsync()
        {
            await StateMachine.ActivateAsync();
        }
        public bool Update(string id, string access, List<CreateSubscriptionRequest> requestedSubscriptions)
        {
            if (State == UserState.Awaiting)
            {
                UserIdInternal = id;
                AccessToken = access;
                RequestedSubscriptions = requestedSubscriptions;
                return true;
            }
            return false;
        }
        public async Task<bool> StopAsync()
        {
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
                .OnEntry(_ => StateMachine.Fire(UserActions.AccessTesting))
                .Permit(UserActions.AccessTesting, UserState.InicialAccessTest);
            //If result of testing is good, try to establish connection, else, request new token
            machine.Configure(UserState.InicialAccessTest)
                .OnEntryAsync(InicialAccessToken())
                .Permit(UserActions.AccessSuccess, UserState.Websocket)
                .Permit(UserActions.AccessFailed, UserState.AwaitNewTokenAfterFailedTest);
            //Try to establish connection, if succeeds, try to complete handshake, else, fail
            machine.Configure(UserState.Websocket)
                .OnEntryAsync(RunWebsocketAsync())
                .Permit(UserActions.WebsocketSuccess, UserState.WellcomeMessage)
                .Permit(UserActions.WebsocketFail, UserState.Failing);
            machine.Configure(UserState.WellcomeMessage).
                OnEntryAsync(AwaitWelcomeMessage())
                .Permit(UserActions.WelcomeMessageSuccess, UserState.HandShake)
                .Permit(UserActions.WelcomeMessageFail, UserState.Failing);
            //If handShake Succeeds, then run, else if we get notification, we lost access, try to reestablish connection
            //Else, fail over
            machine.Configure(UserState.HandShake)
                .OnEntryAsync(RunHandshakeAsync())
                .Permit(UserActions.HandShakeSuccess, UserState.Running)
                .Permit(UserActions.HandShakeAccessFail, UserState.AwaitNewTokenAfterFailedHandShake)
                .Permit(UserActions.HandShakeFail, UserState.Failing)
                .Permit(UserActions.WebsocketFail, UserState.Failing);
            //If running, enable token reestablishment, stop and failover
            machine.Configure(UserState.Running)
                .PermitReentry(UserActions.RunningProceed)
                .OnEntryAsync(RunManagerAsync())
                .Permit(UserActions.RunningAwait, UserState.Awaiting)
                .Permit(UserActions.RunningAccessFail, UserState.AwaitNewTokenAfterFailedRun)
                .Permit(UserActions.ReconnectRequested, UserState.Reconnecting)
                .Permit(UserActions.Stop, UserState.Stoping)
                .Permit(UserActions.Fail, UserState.Failing)
                .Permit(UserActions.HandShakeFail, UserState.Failing)
                .Permit(UserActions.WebsocketFail, UserState.Failing);
            machine.Configure(UserState.Awaiting)
                .OnEntryAsync(AwaitManagerAsync())
                .Permit(UserActions.RunningAccessFail, UserState.AwaitNewTokenAfterFailedRun)
                .Permit(UserActions.ReconnectRequested, UserState.Reconnecting)
                .Permit(UserActions.Stop, UserState.Stoping)
                .Permit(UserActions.Fail, UserState.Failing)
                .Permit(UserActions.HandShakeFail, UserState.Failing)
                .Permit(UserActions.WebsocketFail, UserState.Failing);
            //If NewAcessToken Requested, Start timer and 
            machine.Configure(UserState.AwaitNewTokenAfterFailedTest)
                .OnEntryAsync(NewAccessTokenRequest())
                .Permit(UserActions.NewTokenProvidedReturnToInicialTest, UserState.InicialAccessTest)
                .Permit(UserActions.AwaitNewTokenFailed, UserState.Stoping);
            machine.Configure(UserState.AwaitNewTokenAfterFailedHandShake)
                .OnEntryAsync(NewAccessTokenRequest())
                .Permit(UserActions.NewTokenProvidedReturnToHandShake, UserState.HandShake)
                .Permit(UserActions.AwaitNewTokenFailed, UserState.Stoping);
            machine.Configure(UserState.AwaitNewTokenAfterFailedRun)
                .OnEntryAsync(NewAccessTokenRequest())
                .Permit(UserActions.NewTokenProvidedReturnToRunning, UserState.Running)
                .Permit(UserActions.AwaitNewTokenFailed, UserState.Stoping);
            machine.Configure(UserState.Reconnecting)
                .Permit(UserActions.ReconnectSuccess, UserState.Running)
                .Permit(UserActions.ReconnectFail, UserState.Failing);
            machine.Configure(UserState.Stoping)
                .OnEntryAsync(StopProcedure())
                .Permit(UserActions.Dispose, UserState.Disposed);
            machine.Configure(UserState.Failing)
                .OnEntryAsync(FailProcedure())
                .Permit(UserActions.Dispose, UserState.Disposed);
            machine.Configure(UserState.Disposed)
                .OnEntryAsync(DisposeProcedure());
        }

        protected abstract Func<Task> FailProcedure();
        protected abstract Func<Task> StopProcedure();
        private Func<Task> DisposeProcedure() => async Task () =>
        {
            Socket.Dispose();
            ManagerCancelationSource.Cancel();
            await StateMachine.DeactivateAsync();
        };

        protected abstract Func<Task> AwaitManagerAsync();
        protected abstract Func<Task> StopManagerAsync();
        protected abstract Func<Task> AwaitWelcomeMessage();
        protected abstract Func<Task> InicialAccessToken();
        protected abstract Func<Task> NewAccessTokenRequest();
        protected abstract Func<Task> RunManagerAsync();
        protected abstract Func<Task> RunHandshakeAsync();
        protected abstract Func<Task> RunWebsocketAsync();

    }
}