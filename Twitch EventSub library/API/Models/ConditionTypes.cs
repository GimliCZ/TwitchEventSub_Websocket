namespace Twitch.EventSub.API.Models
{
    public enum ConditionTypes
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