using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ChannelGuestStarSettingsUpdateEvent : WebSocketNotificationEvent
    {
        [JsonProperty("is_moderator_send_live_enabled")]
        public bool IsModeratorSendLiveEnabled { get; set; }

        [JsonProperty("slot_count")]
        public int SlotCount { get; set; }

        [JsonProperty("is_browser_source_audio_enabled")]
        public bool IsBrowserSourceAudioEnabled { get; set; }

        [JsonProperty("group_layout")]
        public string GroupLayout { get; set; }
    }
}