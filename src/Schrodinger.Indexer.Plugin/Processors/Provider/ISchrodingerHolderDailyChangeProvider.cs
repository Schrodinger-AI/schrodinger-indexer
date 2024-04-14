using AElfIndexer.Client.Handlers;

namespace Schrodinger.Indexer.Plugin.Processors.Provider;

public interface ISchrodingerHolderDailyChangeProvider
{
    public Task SaveSchrodingerHolderDailyChangeAsync(string symbol, string address, long amount, LogEventContext context);

}