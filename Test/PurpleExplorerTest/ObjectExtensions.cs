namespace PurpleExplorerTest;

internal static class ObjectExtensions
{
    internal static T With<T>(this T me, Action<T> action)
    {
        action(me);
        return me;
    } 
}