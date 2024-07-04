namespace Twitch.EventSub.CoreFunctions
{
    public delegate Task AsyncEventHandler<in TEventArgs, T>(T sender, TEventArgs e);
}
