using Twitch.EventSub.CoreFunctions;

namespace Twitch.EventSub.Library.CoreFunctions
{
    public static class EventInvocationExtensions
    {
        /// <summary>
        /// Invokes the event handler when it is not null. Returns a completed task otherwise.
        /// </summary>
        internal static Task TryInvoke<TEventArgs>(this AsyncEventHandler<TEventArgs> eventHandler, object sender, TEventArgs eventArgs)
        {
            return eventHandler?.Invoke(sender, eventArgs) ?? Task.CompletedTask;
        }
    }
}
