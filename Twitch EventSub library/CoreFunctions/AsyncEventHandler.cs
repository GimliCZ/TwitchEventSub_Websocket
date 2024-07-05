namespace Twitch.EventSub.CoreFunctions
{
    public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs e);
}
