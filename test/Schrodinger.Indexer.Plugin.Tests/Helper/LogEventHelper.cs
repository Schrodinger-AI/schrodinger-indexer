using AElf.Types;
using AElfIndexer.Grains.State.Client;

namespace Schrodinger.Indexer.Plugin.Tests.Helper;

public static class LogEventHelper
{
    public static LogEventInfo ConvertAElfLogEventToLogEventInfo(LogEvent logEvent)
    {
        var logEventInfo = new LogEventInfo
        {
            ExtraProperties = new Dictionary<string, string>
            {
                { "Indexed", logEvent.Indexed.ToString() },
                { "NonIndexed", logEvent.NonIndexed.ToBase64() }
            }
        };
        return logEventInfo;
    }
}