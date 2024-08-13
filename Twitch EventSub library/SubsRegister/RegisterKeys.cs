using System.Linq.Expressions;
using System.Reflection;

namespace Twitch.EventSub.SubsRegister
{
    //TOTAL 66
    public static class RegisterKeys
    {
        public const string AutomodMessageHold = "automod.message.hold";
        public const string AutomodMessageUpdate = "automod.message.update";

        //public const string AutomodSettingsUpdate = "automod.settings.update"; // anomalous structure
        public const string AutomodTermsUpdate = "automod.terms.update";

        public const string ConduitShardDisabled = "conduit.shard.disabled";

        // public const string DropEntitlementGrant = "drop.entitlement.grant"; // webhook
        // public const string ExtensionBitsTransactionCreate = "extension.bits_transaction.create"; // webhook
        public const string ChannelAdBreakBegin = "channel.ad_break.begin";

        public const string ChannelBan = "channel.ban";
        public const string ChannelFollow = "channel.follow";
        public const string ChannelGoalBegin = "channel.goal.begin";
        public const string ChannelGoalEnd = "channel.goal.end";
        public const string ChannelGoalProgress = "channel.goal.progress";
        public const string ChannelGuestStarGuestUpdate = "channel.guest_star_guest.update";
        public const string ChannelGuestStarSessionBegin = "channel.guest_star_session.begin";
        public const string ChannelGuestStarSessionEnd = "channel.guest_star_session.end";
        public const string ChannelGuestStarSettingsUpdate = "channel.guest_star_settings.update";
        public const string ChannelHypeTrainBegin = "channel.hype_train.begin";
        public const string ChannelHypeTrainEnd = "channel.hype_train.end";
        public const string ChannelHypeTrainProgress = "channel.hype_train.progress";
        public const string ChannelCharityCampaignProgress = "channel.charity_campaign.progress";
        public const string ChannelCharityCampaignStart = "channel.charity_campaign.start";
        public const string ChannelCharityCampaignStop = "channel.charity_campaign.stop";
        public const string ChannelCharityDonation = "channel.charity_campaign.donate";
        public const string ChannelChatClear = "channel.chat.clear";
        public const string ChannelChatClearUserMessages = "channel.chat.clear_user_messages";
        public const string ChannelChatMessage = "channel.chat.message";
        public const string ChannelChatMessageDelete = "channel.chat.message_delete";
        public const string ChannelChatNotification = "channel.chat.notification";
        public const string ChannelChatSettingsUpdate = "channel.chat_settings.update";
        public const string ChannelChatUserMessageHold = "channel.chat.user_message_hold";
        public const string ChannelChatUserMessageUpdate = "channel.chat.user_message_update";
        public const string ChannelCheer = "channel.cheer";
        public const string ChannelModeratorAdd = "channel.moderator.add";
        public const string ChannelModeratorRemove = "channel.moderator.remove";
        public const string ChannelPointsAutomaticRewardRedemptionAdd = "channel.channel_points_automatic_reward_redemption.add";
        public const string ChannelPointsCustomRewardAdd = "channel.channel_points_custom_reward.add";
        public const string ChannelPointsCustomRewardRedemptionAdd = "channel.channel_points_custom_reward_redemption.add";
        public const string ChannelPointsCustomRewardRedemptionUpdate = "channel.channel_points_custom_reward_redemption.update";
        public const string ChannelPointsCustomRewardRemove = "channel.channel_points_custom_reward.remove";
        public const string ChannelPointsCustomRewardUpdate = "channel.channel_points_custom_reward.update";
        public const string ChannelPollBegin = "channel.poll.begin";
        public const string ChannelPollEnd = "channel.poll.end";
        public const string ChannelPollProgress = "channel.poll.progress";
        public const string ChannelPredictionBegin = "channel.prediction.begin";
        public const string ChannelPredictionEnd = "channel.prediction.end";
        public const string ChannelPredictionLock = "channel.prediction.lock";
        public const string ChannelPredictionProgress = "channel.prediction.progress";
        public const string ChannelRaid = "channel.raid";
        public const string ChannelShieldModeBegin = "channel.shield_mode.begin";
        public const string ChannelShieldModeEnd = "channel.shield_mode.end";
        public const string ChannelShoutoutCreate = "channel.shoutout.create";
        public const string ChannelShoutoutReceived = "channel.shoutout.receive";
        public const string ChannelSubscribe = "channel.subscribe";
        public const string ChannelSubscriptionEnd = "channel.subscription.end";
        public const string ChannelSubscriptionGift = "channel.subscription.gift";
        public const string ChannelSubscriptionMessage = "channel.subscription.message";

        // public const string UserAuthorizationGrant = "user.authorization.grant"; // webhook
        // public const string UserAuthorizationRevoke = "user.authorization.revoke"; // webhook
        // public const string UserUpdate = "user.update"; // anomalous structure
        // public const string WhisperReceived = "user.whisper.message"; // anomalous structure
        public const string ChannelSuspiciousUserMessage = "channel.suspicious_user.message";

        public const string ChannelSuspiciousUserUpdate = "channel.suspicious_user.update";
        public const string ChannelUnban = "channel.unban";
        public const string ChannelUnbanRequestCreate = "channel.unban_request.create";
        public const string ChannelUnbanRequestResolve = "channel.unban_request.resolve";
        public const string ChannelUpdate = "channel.update";
        public const string ChannelVIPAdd = "channel.vip.add";
        public const string ChannelVIPRemove = "channel.vip.remove";
        public const string ChannelWarningAcknowledge = "channel.warning.acknowledge";
        public const string ChannelWarningSend = "channel.warning.send";
        public const string StreamOffline = "stream.offline";
        public const string StreamOnline = "stream.online";

        public static readonly List<string> KeysList = GetRegistryKeysList();

        public static List<string> GetRegistryKeysList()
        {
            var type = typeof(RegisterKeys);

            // Get all public static fields
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name != nameof(KeysList));

            // Create a list to hold the values
            var registryKeysList = new List<string>();

            foreach (var field in fields)
            {
                // Create a delegate for each static field getter
                var getter = CreateStaticFieldGetter(type, field.Name);
                // Invoke the getter and add the value to the list
                registryKeysList.Add(getter());
            }

            return registryKeysList;
        }

        private static Func<string> CreateStaticFieldGetter(Type type, string fieldName)
        {
            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static)
                         ?? throw new ArgumentException($"Field '{fieldName}' not found.");

            var fieldExp = Expression.Field(null, field);
            var castExp = Expression.Convert(fieldExp, typeof(string)); // Convert to string
            var lambda = Expression.Lambda<Func<string>>(castExp);

            return lambda.Compile();
        }
    }
};