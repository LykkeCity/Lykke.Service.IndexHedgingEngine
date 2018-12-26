using System.Collections.Generic;
using System.Linq;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Utils
{
    public class TraceWriter
    {
        private readonly ILog _log;

        public TraceWriter(ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
        }

        public void LimitOrders(string assetPair, IReadOnlyCollection<LimitOrder> limitOrders)
        {
            LimitOrder[] sellLimitOrders = limitOrders
                .Where(o => o.Error == LimitOrderError.None && o.Type == LimitOrderType.Sell)
                .ToArray();

            LimitOrder[] byuLimitOrders = limitOrders
                .Where(o => o.Error == LimitOrderError.None && o.Type == LimitOrderType.Buy)
                .ToArray();

            _log.Info("Limit orders", new
            {
                AssetPair = assetPair,
                SellPrice = sellLimitOrders.FirstOrDefault()?.Price ?? 0,
                SellVolume = sellLimitOrders.Sum(o => o.Volume),
                BuyPrice = byuLimitOrders.FirstOrDefault()?.Price ?? 0,
                BuyVolume = byuLimitOrders.Sum(o => o.Volume)
            });
        }

        public void Balances(IReadOnlyCollection<Balance> balances)
        {
            _log.Info("Balances", balances.ToDictionary(o => o.AssetId, o => new
            {
                o.Amount,
                o.Reserved
            }));
        }
    }
}
