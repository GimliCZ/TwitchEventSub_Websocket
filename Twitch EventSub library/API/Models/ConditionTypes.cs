using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twitch_EventSub_library.API.Models
{
    public class ConditionTypes
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
            ExtensionClientId,
            ClientId*/
        }
    }
}
