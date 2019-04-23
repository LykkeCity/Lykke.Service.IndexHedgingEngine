using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Api;

namespace Lykke.Service.IndexHedgingEngine.Client
{
    /// <summary>
    /// IndexHedgingEngine engine service client.
    /// </summary>
    [PublicAPI]
    public interface IIndexHedgingEngineClient
    {
        /// <summary>
        /// Asset hedge settings API.
        /// </summary>
        IAssetHedgeSettingsApi AssetHedgeSettings { get; }

        /// <summary>
        /// Asset pairs API.
        /// </summary>
        IAssetPairsApi AssetPairs { get; }

        /// <summary>
        /// Assets API.
        /// </summary>
        IAssetsApi Assets { get; }

        /// <summary>
        /// Audit API.
        /// </summary>
        IAuditApi Audit { get; }

        /// <summary>
        /// Balances API.
        /// </summary>
        IBalancesApi Balances { get; }

        /// <summary>
        /// Funding API.
        /// </summary>
        IFundingApi Funding { get; }

        /// <summary>
        /// Hedge limit orders API.
        /// </summary>
        IHedgeLimitOrdersApi HedgeLimitOrders { get; }

        /// <summary>
        /// Index settings API.
        /// </summary>
        IIndexSettingsApi IndexSettings { get; }

        /// <summary>
        /// Cross asset pair settings API.
        /// </summary>
        ICrossIndexApi CrossIndex { get; }

        /// <summary>
        /// Market maker API.
        /// </summary>
        IMarketMakerApi MarketMaker { get; }

        /// <summary>
        /// Index states API.
        /// </summary>
        IIndexPricesApi IndexPrices { get; }

        /// <summary>
        /// Order books API.
        /// </summary>
        IOrderBooksApi OrderBooks { get; }

        /// <summary>
        /// Positions API.
        /// </summary>
        IPositionsApi Positions { get; }

        /// <summary>
        /// Quotes API.
        /// </summary>
        IQuotesApi Quotes { get; set; }

        /// <summary>
        /// Reports API.
        /// </summary>
        IReportsApi Reports { get; set; }

        /// <summary>
        /// Settings API.
        /// </summary>
        ISettingsApi Settings { get; }

        /// <summary>
        /// Settlements API.
        /// </summary>
        ISettlementsApi Settlements { get; }

        /// <summary>
        /// Simulation API.
        /// </summary>
        ISimulationApi Simulation { get; }

        /// <summary>
        /// Tokens API.
        /// </summary>
        ITokensApi Tokens { get; }

        /// <summary>
        /// Trades API.
        /// </summary>
        ITradesApi Trades { get; }

        /// <summary>
        /// Primary Market API.
        /// </summary>
        IPrimaryMarketApi PrimaryMarket { get; }
    }
}
