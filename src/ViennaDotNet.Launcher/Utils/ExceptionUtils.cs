namespace ViennaDotNet.Launcher.Utils;

internal static class ExceptionUtils
{
    public static string GetInnerMostMessage(this Exception ex)
        => ex is AggregateException { InnerException: { } } agg ? GetInnerMostMessage(agg.InnerException!) : ex.Message;
}
