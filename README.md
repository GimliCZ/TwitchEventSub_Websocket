# TwitchEventSub_Websocket
<p align="center">
  <img src="https://img.shields.io/badge/Platform-.NET-lightgrey.svg" style="max-height: 300px;" alt="Platform: iOS">
  <a href="https://www.microsoft.com/net"><img src="https://img.shields.io/badge/.NET%20Framework-6.0.0-orange.svg" style="max-height: 300px;"></a>
</p>
## About
* Handles comunication with twitch eventsub via websocket

## Implementation

#### Start Function
```csharp
var listOfSubs = new List<SubscriptionType>
{
   SubscriptionType.ChannelFollow
};
eventSubClient.OnFollowEvent -= EventSubClientOnFollowEvent;
eventSubClient.OnFollowEvent += EventSubClientOnFollowEvent;
eventSubClient.Start(clientId,userId, accessToken, listOfSubs);
```

* Client Id is identifier of your aplication
* User Id is identifier of twitch user
* AccessToken is token requested via bearer token of user
* TwitchEventSub Library contains set of enums SubscriptionType which are setting contens of list of subscriptions
#### Stop Function
```csharp
await _eventSubClient.Stop();
```
#### Authorization
* To function properly, you are required to subscribe to EventSubClientOnRefreshToken event in order to refresh token
```csharp
eventSubClient.OnRefreshToken -= EventSubClientOnRefreshToken;
eventSubClient.OnRefreshToken += EventSubClientOnRefreshToken;
```
* Then run your token refreshing function
```csharp
await RefreshAccessTokenAsync();
await _eventSubClient.UpdateOnFly(clientId, userId, accessToken, listOfSubs);
```
#### Reconnection
* You may also listen to Unexpected Connetion Termination
```csharp
_eventSubClient.OnUnexpectedConnectionTermination -= EventSubClientOnUnexpectedConnectionTermination;
_eventSubClient.OnUnexpectedConnectionTermination += EventSubClientOnUnexpectedConnectionTermination;
```
This can trigger your own procedure which could restart entire client or log what happened.

## License
This project is available under the MIT license. See the LICENSE file for more info.
