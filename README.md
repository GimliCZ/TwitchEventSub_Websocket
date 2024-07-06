# TwitchEventSub_Websocket
<p align="center">
  <img src="https://buildstats.info/nuget/Twitch.EventSub.Websocket" style="max-height: 300px;" alt="Platform: iOS">
  <img src="https://img.shields.io/badge/Platform-.NET%208-orange.svg"style="max-height: 300px;" alt="Platform: iOS">
</p>

## About
Handles comunication with twitch eventsub via websocket

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
await eventSubClient.Stop();
```
#### Authorization
* **EventSub does not provide refresh token capabilities. You have to provide your own.**
* To function properly, you are required to subscribe to EventSubClientOnRefreshToken event in order to refresh token
```csharp
eventSubClient.OnRefreshToken -= EventSubClientOnRefreshToken;
eventSubClient.OnRefreshToken += EventSubClientOnRefreshToken;
```
* Then run your token refreshing function
```csharp
await RefreshAccessTokenAsync();
await eventSubClient.UpdateOnFly(clientId, userId, accessToken, listOfSubs);
```
#### Reconnection
* You may also listen to Unexpected Connetion Termination
```csharp
eventSubClient.OnUnexpectedConnectionTermination -= EventSubClientOnUnexpectedConnectionTermination;
eventSubClient.OnUnexpectedConnectionTermination += EventSubClientOnUnexpectedConnectionTermination;
```
This can trigger your own procedure which could restart entire client or log what happened.

#### State Machine Structure
![alt text](https://github.com/GimliCZ/TwitchEventSub_Websocket/blob/Conduit-Websocket/graphviz.png)

## License
This project is available under the MIT license. See the LICENSE file for more info.
