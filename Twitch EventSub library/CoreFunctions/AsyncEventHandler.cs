namespace Twitch_EventSub_library.CoreFunctions
{
    public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs e);
}
