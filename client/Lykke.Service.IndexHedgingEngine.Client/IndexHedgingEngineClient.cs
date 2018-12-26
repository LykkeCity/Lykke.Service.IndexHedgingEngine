using Lykke.HttpClientGenerator;
using Lykke.Service.IndexHedgingEngine.Client.Api;

namespace Lykke.Service.IndexHedgingEngine.Client
{
    /// <summary>
    /// Index hedging engine service client.
    /// </summary>
    public class IndexHedgingEngineClient : IIndexHedgingEngineClient
    {
        /// <summary>
        /// Initializes a new instance of <see cref="IndexHedgingEngineClient"/> with <param name="httpClientGenerator"></param>.
        /// </summary> 
        public IndexHedgingEngineClient(IHttpClientGenerator httpClientGenerator)
        {
            AssetHedgeSettings = httpClientGenerator.Generate<IAssetHedgeSettingsApi>();
            AssetPairs = httpClientGenerator.Generate<IAssetPairsApi>();
            Assets = httpClientGenerator.Generate<IAssetsApi>();
            Audit = httpClientGenerator.Generate<IAuditApi>();
            Balances = httpClientGenerator.Generate<IBalancesApi>();
            Funding = httpClientGenerator.Generate<IFundingApi>();
            HedgeLimitOrders = httpClientGenerator.Generate<IHedgeLimitOrdersApi>();
            IndexSettings = httpClientGenerator.Generate<IIndexSettingsApi>();
            IndexPrices = httpClientGenerator.Generate<IIndexPricesApi>();
            OrderBooks = httpClientGenerator.Generate<IOrderBooksApi>();
            Positions = httpClientGenerator.Generate<IPositionsApi>();
            Reports = httpClientGenerator.Generate<IReportsApi>();
            Settings = httpClientGenerator.Generate<ISettingsApi>();
            Tokens = httpClientGenerator.Generate<ITokensApi>();
            Trades = httpClientGenerator.Generate<ITradesApi>();
        }

        /// <inheritdoc/>
        public IAssetHedgeSettingsApi AssetHedgeSettings { get; }

        /// <inheritdoc/>
        public IAssetPairsApi AssetPairs { get; }

        /// <inheritdoc/>
        public IAssetsApi Assets { get; }

        /// <inheritdoc/>
        public IAuditApi Audit { get; }

        /// <inheritdoc/>
        public IBalancesApi Balances { get; }

        /// <inheritdoc/>
        public IFundingApi Funding { get; }

        /// <inheritdoc/>
        public IHedgeLimitOrdersApi HedgeLimitOrders { get; }

        /// <inheritdoc/>
        public IIndexSettingsApi IndexSettings { get; }

        /// <inheritdoc/>
        public IMarketMakerApi MarketMaker { get; }

        /// <inheritdoc/>
        public IIndexPricesApi IndexPrices { get; }

        /// <inheritdoc/>
        public IOrderBooksApi OrderBooks { get; }

        /// <inheritdoc/>
        public IPositionsApi Positions { get; }

        /// <inheritdoc/>
        public IReportsApi Reports { get; set; }

        /// <inheritdoc/>
        public ISettingsApi Settings { get; }

        /// <inheritdoc/>
        public ITokensApi Tokens { get; }

        /// <inheritdoc/>
        public ITradesApi Trades { get; }
    }
}
