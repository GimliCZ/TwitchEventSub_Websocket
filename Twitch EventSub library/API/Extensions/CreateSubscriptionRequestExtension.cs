using Twitch.EventSub.API.Models;

namespace Twitch.EventSub.API.Extensions
{
    public static class CreateSubscriptionRequestExtension
    {
        private static readonly Dictionary<SubscriptionType,
            (string Type, string Version, List<ConditionType> Conditions)> TypeVersionConditionMap = new()
        {
            {
                SubscriptionType.ChannelUpdate,
                (
                    //The subscription type name
                    "channel.update",
                    //The subscription type version
                    "2",
                    //Subscription-specific parameters
                    CondList(ConditionType.BroadcasterUserId)
                    // Transport is defined as websocket

                )
            },

            { SubscriptionType.ChannelFollow, ("channel.follow", "2",
                CondList(ConditionType.BroadcasterUserId,ConditionType.ModeratorUserId)) },

            { SubscriptionType.ChannelSubscribe, ("channel.subscribe", "1",
                CondList(ConditionType.BroadcasterUserId))},

            {SubscriptionType.ChannelAdBreakBegin, ("channel.ad_break.begin","1",
                CondList(ConditionType.BroadcasterUserId))},

            { SubscriptionType.ChannelChatClear, ("channel.chat.clear", "1",
                    CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelChatClearUserMessages, ("channel.chat.clear_user_messages", "1",
                    CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelChatMessage, ("channel.chat.message", "1",
                    CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelChatMessageDelete, ("channel.chat.message_delete", "1",
                    CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelChatNotification, ("channel.chat.notification", "1",
                    CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.BetaChannelChatSettingsUpdate, ("channel.chat_settings.update", "beta",
                    CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelSubscriptionEnd, ("channel.subscription.end", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelSubscriptionGift, ("channel.subscription.gift", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelSubscriptionMessage, ("channel.subscription.message", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelCheer, ("channel.cheer", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelRaid, ("channel.raid", "1",
                CondList(ConditionType.ToBroadcasterUserId)) },

            { SubscriptionType.ChannelBan, ("channel.ban", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelUnban, ("channel.unban", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelModeratorAdd, ("channel.moderator.add", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelModeratorRemove, ("channel.moderator.remove", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.BetaChannelGuestStarSessionBegin, ("channel.guest_star_session.begin", "beta",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.BetaChannelGuestStarSessionEnd, ("channel.guest_star_session.end", "beta",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.BetaChannelGuestStarGuestUpdate, ("channel.guest_star_guest.update", "beta",
                CondList(ConditionType.BroadcasterUserId,ConditionType.ModeratorUserId)) },

            { SubscriptionType.BetaChannelGuestStarSlotUpdate, ("channel.guest_star_slot.update", "beta",
                CondList(ConditionType.BroadcasterUserId,ConditionType.ModeratorUserId)) },

            { SubscriptionType.BetaChannelGuestStarSettingsUpdate, ("channel.guest_star_settings.update", "beta",
                CondList(ConditionType.BroadcasterUserId,ConditionType.ModeratorUserId)) },

            { SubscriptionType.ChannelPointsCustomRewardAdd, ("channel.channel_points_custom_reward.add", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelPointsCustomRewardUpdate, ("channel.channel_points_custom_reward.update", "1",
                CondList(ConditionType.BroadcasterUserId,ConditionType.RewardId)) },

            { SubscriptionType.ChannelPointsCustomRewardRemove, ("channel.channel_points_custom_reward.remove", "1",
                CondList(ConditionType.BroadcasterUserId,ConditionType.RewardId)) },

            { SubscriptionType.ChannelPointsCustomRewardRedemptionAdd, ("channel.channel_points_custom_reward_redemption.add", "1",
                CondList(ConditionType.BroadcasterUserId, ConditionType.RewardId)) },

            { SubscriptionType.ChannelPointsCustomRewardRedemptionUpdate, ("channel.channel_points_custom_reward_redemption.update", "1",
                CondList(ConditionType.BroadcasterUserId, ConditionType.RewardId)) },

            { SubscriptionType.ChannelPollBegin, ("channel.poll.begin", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelPollProgress, ("channel.poll.progress", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelPollEnd, ("channel.poll.end", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelPredictionBegin, ("channel.prediction.begin", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelPredictionProgress, ("channel.prediction.progress", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelPredictionLock, ("channel.prediction.lock", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelPredictionEnd, ("channel.prediction.end", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelHypeTrainBegin, ("channel.hype_train.begin", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelHypeTrainProgress, ("channel.hype_train.progress", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelHypeTrainEnd, ("channel.hype_train.end", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.CharityDonation, ("channel.charity_campaign.donate", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.CharityCampaignStart, ("channel.charity_campaign.start", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.CharityCampaignProgress, ("channel.charity_campaign.progress", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.CharityCampaignStop, ("channel.charity_campaign.stop", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelShieldModeBegin, ("channel.shield_mode.begin", "1",
                CondList(ConditionType.BroadcasterUserId,ConditionType.ModeratorUserId)) },

            { SubscriptionType.ChannelShieldModeEnd, ("channel.shield_mode.end", "1",
                CondList(ConditionType.BroadcasterUserId,ConditionType.ModeratorUserId)) },

            { SubscriptionType.ChannelShoutoutCreate, ("channel.shoutout.create", "1",
                CondList(ConditionType.BroadcasterUserId,ConditionType.ModeratorUserId)) },

            { SubscriptionType.ChannelShoutoutReceived, ("channel.shoutout.receive", "1",
                CondList(ConditionType.BroadcasterUserId,ConditionType.ModeratorUserId)) },
            //this is webhook only feature
            /*{ SubscriptionType.DropEntitlementGrant, ("drop.entitlement.grant", "1",
                CondList(ConditionType.OrganizationId,ConditionType.CategoryId,
                    ConditionType.CampaignId)) },
           
            PS: Grant also misses some of the body types. 


            { SubscriptionType.ExtensionBitsTransactionCreate, ("extension.bits_transaction.create", "1",
                CondList(ConditionType.ExtensionClientId)) },*/
            { SubscriptionType.ChannelGoalBegin, ("channel.goal.begin", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelGoalProgress, ("channel.goal.progress", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.ChannelGoalEnd, ("channel.goal.end", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.StreamOnline, ("stream.online", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            { SubscriptionType.StreamOffline, ("stream.offline", "1",
                CondList(ConditionType.BroadcasterUserId)) },

            //this is webhook only feature
            /*{ SubscriptionType.UserAuthorizationGrant, ("user.authorization.grant", "1",
                CondList(ConditionType.ClientId)) },
            {SubscriptionType.UserAuthorizationRevoke, ("user.authorization.revoke", "1",
             CondList(ConditionType.ClientId)) },*/

            { SubscriptionType.UserUpdate, ("user.update", "1",
                CondList(ConditionType.UserId))}
        };

        private static List<ConditionType> CondList(params ConditionType[] types)
        {
            var list = new List<ConditionType>();
            list.AddRange(types);
            return list;
        }

        //Reward Id enables to sub to specific reward only. As null it subs all rewards
        public static CreateSubscriptionRequest SetSubscriptionType(this CreateSubscriptionRequest request,
         SubscriptionType subscriptionType, string userId, string? rewardId = null)
        {
            if (TypeVersionConditionMap.TryGetValue(subscriptionType, out var typeVersionCondition))
            {
                request.Type = typeVersionCondition.Type;
                request.Version = typeVersionCondition.Version;
                foreach (var conditionType in typeVersionCondition.Conditions)
                {
                    switch (conditionType)
                    {
                        case ConditionType.BroadcasterUserId:
                            request.Condition.BroadcasterUserId = userId;
                            break;
                        case ConditionType.ToBroadcasterUserId:
                            request.Condition.ToBroadcasterUserId = userId;
                            break;
                        case ConditionType.ModeratorUserId:
                            request.Condition.ModeratorUserId = userId;
                            break;
                        //webhook only
                        /*    case ConditionType.OrganizationId:
                                request.Condition.OrganizationId = organizationId;
                                break;
                            case ConditionType.CampaignId:
                                request.Condition.CampaignId = campaignId;
                                break;
                            case ConditionType.CategoryId:
                                request.Condition.CategoryId = categoryId;
                                break;
                            case ConditionType.ClientId:
                                request.Condition.ClientId = userId;
                                break;
                        case ConditionType.ExtensionClientId:
                            request.Condition.ExtensionClientId = userId;
                            break;*/
                        case ConditionType.UserId:
                            request.Condition.UserId = userId;
                            break;
                        case ConditionType.RewardId:
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
