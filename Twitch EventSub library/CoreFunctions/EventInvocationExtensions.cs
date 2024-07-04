using Twitch.EventSub.CoreFunctions;

namespace Twitch.EventSub.Library.CoreFunctions
{
    public static class EventInvocationExtensions
    {
        /// <summary>
        /// Invokes the event handler when it is not null. Returns a completed task otherwise.
        /// </summary>
        internal static Task TryInvoke<TEventArgs, T>(this AsyncEventHandler<TEventArgs, T> eventHandler, T sender, TEventArgs eventArgs)
        {
            return eventHandler?.Invoke(sender, eventArgs) ?? Task.CompletedTask;
        }
    }
}
