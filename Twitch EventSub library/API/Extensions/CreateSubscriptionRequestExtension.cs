using Twitch_EventSub_library.API.Models;

namespace Twitch_EventSub_library.API.Extensions
{
    public static class CreateSubscriptionRequestExtension
    {
        private static readonly Dictionary<SubscriptionTypes.SubscriptionType,
            (string Type, string Version, List<ConditionTypes.ConditionType> Conditions)> TypeVersionConditionMap = new()
        {
            {
                SubscriptionTypes.SubscriptionType.ChannelUpdate,
                (
                    //The subscription type name
                    "channel.update",
                    //The subscription type version
                    "2",
                    //Subscription-specific parameters
                    CondList(ConditionTypes.ConditionType.BroadcasterUserId)
                    // Transport is defined as websocket

                )
            },

            { SubscriptionTypes.SubscriptionType.ChannelFollow, ("channel.follow", "2",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId,ConditionTypes.ConditionType.ModeratorUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelSubscribe, ("channel.subscribe", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId))},

            { SubscriptionTypes.SubscriptionType.ChannelSubscriptionEnd, ("channel.subscription.end", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelSubscriptionGift, ("channel.subscription.gift", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelSubscriptionMessage, ("channel.subscription.message", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelCheer, ("channel.cheer", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelRaid, ("channel.raid", "1",
                CondList(ConditionTypes.ConditionType.ToBroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelBan, ("channel.ban", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelUnban, ("channel.unban", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelModeratorAdd, ("channel.moderator.add", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelModeratorRemove, ("channel.moderator.remove", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.BetaChannelGuestStarSessionBegin, ("channel.guest_star_session.begin", "beta",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.BetaChannelGuestStarSessionEnd, ("channel.guest_star_session.end", "beta",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.BetaChannelGuestStarGuestUpdate, ("channel.guest_star_guest.update", "beta",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId,ConditionTypes.ConditionType.ModeratorUserId)) },

            { SubscriptionTypes.SubscriptionType.BetaChannelGuestStarSlotUpdate, ("channel.guest_star_slot.update", "beta",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId,ConditionTypes.ConditionType.ModeratorUserId)) },

            { SubscriptionTypes.SubscriptionType.BetaChannelGuestStarSettingsUpdate, ("channel.guest_star_settings.update", "beta",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId,ConditionTypes.ConditionType.ModeratorUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelPointsCustomRewardAdd, ("channel.channel_points_custom_reward.add", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelPointsCustomRewardUpdate, ("channel.channel_points_custom_reward.update", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId,ConditionTypes.ConditionType.RewardId)) },

            { SubscriptionTypes.SubscriptionType.ChannelPointsCustomRewardRemove, ("channel.channel_points_custom_reward.remove", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId,ConditionTypes.ConditionType.RewardId)) },

            { SubscriptionTypes.SubscriptionType.ChannelPointsCustomRewardRedemptionAdd, ("channel.channel_points_custom_reward_redemption.add", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId, ConditionTypes.ConditionType.RewardId)) },

            { SubscriptionTypes.SubscriptionType.ChannelPointsCustomRewardRedemptionUpdate, ("channel.channel_points_custom_reward_redemption.update", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId, ConditionTypes.ConditionType.RewardId)) },

            { SubscriptionTypes.SubscriptionType.ChannelPollBegin, ("channel.poll.begin", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelPollProgress, ("channel.poll.progress", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelPollEnd, ("channel.poll.end", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelPredictionBegin, ("channel.prediction.begin", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelPredictionProgress, ("channel.prediction.progress", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelPredictionLock, ("channel.prediction.lock", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelPredictionEnd, ("channel.prediction.end", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelHypeTrainBegin, ("channel.hype_train.begin", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelHypeTrainProgress, ("channel.hype_train.progress", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelHypeTrainEnd, ("channel.hype_train.end", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.CharityDonation, ("channel.charity_campaign.donate", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.CharityCampaignStart, ("channel.charity_campaign.start", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.CharityCampaignProgress, ("channel.charity_campaign.progress", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.CharityCampaignStop, ("channel.charity_campaign.stop", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelShieldModeBegin, ("channel.shield_mode.begin", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId,ConditionTypes.ConditionType.ModeratorUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelShieldModeEnd, ("channel.shield_mode.end", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId,ConditionTypes.ConditionType.ModeratorUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelShoutoutCreate, ("channel.shoutout.create", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId,ConditionTypes.ConditionType.ModeratorUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelShoutoutReceived, ("channel.shoutout.receive", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId,ConditionTypes.ConditionType.ModeratorUserId)) },

            //this is webhook only feature
            /*{ SubscriptionTypes.SubscriptionType.DropEntitlementGrant, ("drop.entitlement.grant", "1",
                CondList(ConditionTypes.ConditionType.OrganizationId,ConditionTypes.ConditionType.CategoryId,
                    ConditionTypes.ConditionType.CampaignId)) },
           
            PS: Grant also misses some of the body types. 


            { SubscriptionTypes.SubscriptionType.ExtensionBitsTransactionCreate, ("extension.bits_transaction.create", "1",
                CondList(ConditionTypes.ConditionType.ExtensionClientId)) },*/
            { SubscriptionTypes.SubscriptionType.ChannelGoalBegin, ("channel.goal.begin", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelGoalProgress, ("channel.goal.progress", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.ChannelGoalEnd, ("channel.goal.end", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.StreamOnline, ("stream.online", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            { SubscriptionTypes.SubscriptionType.StreamOffline, ("stream.offline", "1",
                CondList(ConditionTypes.ConditionType.BroadcasterUserId)) },

            //this is webhook only feature
            /*{ SubscriptionTypes.SubscriptionType.UserAuthorizationGrant, ("user.authorization.grant", "1",
                CondList(ConditionTypes.ConditionType.ClientId)) },
            {SubscriptionTypes.SubscriptionType.UserAuthorizationRevoke, ("user.authorization.revoke", "1",
             CondList(ConditionTypes.ConditionType.ClientId)) },*/

            { SubscriptionTypes.SubscriptionType.UserUpdate, ("user.update", "1",
                CondList(ConditionTypes.ConditionType.UserId))}
        };

        private static List<ConditionTypes.ConditionType> CondList(params ConditionTypes.ConditionType[] types)
        {
            var list = new List<ConditionTypes.ConditionType>();
            list.AddRange(types);
            return list;
        }

        //Reward Id enables to sub to specific reward only. As null it subs all rewards
        public static CreateSubscriptionRequest SetSubscriptionType(this CreateSubscriptionRequest request,
         SubscriptionTypes.SubscriptionType subscriptionType, string userId, string? rewardId = null)
        {
            if (TypeVersionConditionMap.TryGetValue(subscriptionType, out var typeVersionCondition))
            {
                request.Type = typeVersionCondition.Type;
                request.Version = typeVersionCondition.Version;
                foreach (var conditionType in typeVersionCondition.Conditions)
                {
                    switch (conditionType)
                    {
                        case ConditionTypes.ConditionType.BroadcasterUserId:
                            request.Condition.BroadcasterUserId = userId;
                            break;
                        case ConditionTypes.ConditionType.ToBroadcasterUserId:
                            request.Condition.ToBroadcasterUserId = userId;
                            break;
                        case ConditionTypes.ConditionType.ModeratorUserId:
                            request.Condition.ModeratorUserId = userId;
                            break;
                        //webhook only
                        /*    case ConditionTypes.ConditionType.OrganizationId:
                                request.Condition.OrganizationId = organizationId;
                                break;
                            case ConditionTypes.ConditionType.CampaignId:
                                request.Condition.CampaignId = campaignId;
                                break;
                            case ConditionTypes.ConditionType.CategoryId:
                                request.Condition.CategoryId = categoryId;
                                break;
                            case ConditionTypes.ConditionType.ClientId:
                                request.Condition.ClientId = userId;
                                break;
                        case ConditionTypes.ConditionType.ExtensionClientId:
                            request.Condition.ExtensionClientId = userId;
                            break;*/
                        case ConditionTypes.ConditionType.UserId:
                            request.Condition.UserId = userId;
                            break;
                        case ConditionTypes.ConditionType.RewardId:
                            request.Condition.RewardId = rewardId;
                            break;
                        default:
                            throw new ArgumentException("Invalid subscription type");
                    }
                }

                return request;
            }
            throw new ArgumentException("Invalid subscription");
        }
    }
}
