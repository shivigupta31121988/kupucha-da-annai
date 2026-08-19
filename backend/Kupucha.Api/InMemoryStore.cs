using System.Collections.Concurrent;

namespace Kupucha.Api;

public static class InMemoryStore
{
    public static ConcurrentDictionary<string, Models.Stock> Stocks = new();
    public static ConcurrentDictionary<Guid, Models.Order> Orders = new();

    static InMemoryStore()
    {
        Stocks["KPU"] = new Models.Stock { Symbol = "KPU", Name = "Kupucha Ltd", MarketRate = 124.5m, Yesterday = 120.0m, Predicted = 126.3m };
        Stocks["ANN"] = new Models.Stock { Symbol = "ANN", Name = "Annai Holdings", MarketRate = 58.2m, Yesterday = 60.0m, Predicted = 59.1m };
    }
}
