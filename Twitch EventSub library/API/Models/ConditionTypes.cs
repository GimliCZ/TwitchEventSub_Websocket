namespace Twitch.EventSub.API.Models
{
    public enum ConditionType
    {
        UserId,
        BroadcasterUserId,
        ToBroadcasterUserId,
        RewardId,
        ModeratorUserId,
        //webhook only
        /* OrganizationId,
         CategoryId,
         CampaignId,
         ExtensionClientId,*/
        ClientId
    }
}