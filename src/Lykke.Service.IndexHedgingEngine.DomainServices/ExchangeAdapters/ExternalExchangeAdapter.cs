using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.ExchangeAdapter.Client;
using Lykke.Common.ExchangeAdapter.SpotController;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Infrastructure;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;
using TradeType = Lykke.Common.ExchangeAdapter.Contracts.TradeType;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.ExchangeAdapters
{
    public class ExternalExchangeAdapter : IExchangeAdapter
    {
        private readonly ExchangeAdapterClientFactory _exchangeAdapterClientFactory;
        private readonly IHedgeLimitOrderService _hedgeLimitOrderService;
        private readonly IInstrumentService _instrumentService;
        private readonly IExternalOrderRepository _externalOrderRepository;
        private readonly IExternalTradeService _externalTradeService;
        private readonly IPositionService _positionService;
        private readonly ILog _log;

        public ExternalExchangeAdapter(
            string exchangeName,
            ExchangeAdapterClientFactory exchangeAdapterClientFactory,
            IHedgeLimitOrderService hedgeLimitOrderService,
            IInstrumentService instrumentService,
            IExternalOrderRepository externalOrderRepository,
            IExternalTradeService externalTradeService,
            IPositionService positionService,
            ILogFactory logFactory)
        {
            Name = exchangeName;
            _exchangeAdapterClientFactory = exchangeAdapterClientFactory;
            _hedgeLimitOrderService = hedgeLimitOrderService;
            _instrumentService = instrumentService;
            _externalOrderRepository = externalOrderRepository;
            _externalTradeService = externalTradeService;
            _positionService = positionService;
            _log = logFactory.CreateLog(this);
        }

        public string Name { get; }

        public async Task CancelLimitOrderAsync(string assetId)
        {
            ExternalOrder externalOrder = await _externalOrderRepository.GetAsync(Name, assetId);

            if (externalOrder == null)
                return;

            ISpotController spotController = _exchangeAdapterClientFactory.GetSpotController(Name);

            try
            {
                await spotController.CancelLimitOrderAsync(new CancelLimitOrderRequest {OrderId = externalOrder.Id});

                HedgeLimitOrder hedgeLimitOrder =
                    await _hedgeLimitOrderService.GetByIdAsync(externalOrder.HedgeLimitOrderId);

                OrderModel order = await spotController.LimitOrderStatusAsync(externalOrder.Id);

                if (order == null)
                {
                    _log.WarningWithDetails("External order not found", externalOrder);
                    return;
                }

                if (order.ExecutionStatus == OrderStatus.Fill)
                {
                    await _positionService.UpdateAsync(hedgeLimitOrder.AssetId, hedgeLimitOrder.Exchange,
                        hedgeLimitOrder.Type == LimitOrderType.Sell
                            ? Domain.TradeType.Sell
                            : Domain.TradeType.Buy,
                        order.ExecutedVolume, order.ExecutedVolume * order.AvgExecutionPrice);

                    await _externalOrderRepository.DeleteAsync(externalOrder.Exchange, externalOrder.Asset);

                    await _externalTradeService.RegisterAsync(new ExternalTrade
                    {
                        Id = Guid.NewGuid().ToString("D"),
                        Exchange = hedgeLimitOrder.Exchange,
                        LimitOrderId = hedgeLimitOrder.Id,
                        ExchangeOrderId = externalOrder.Id,
                        AssetPairId = hedgeLimitOrder.AssetPairId,
                        Type = hedgeLimitOrder.Type == LimitOrderType.Sell
                            ? Domain.TradeType.Sell
                            : Domain.TradeType.Buy,
                        Timestamp = order.Timestamp,
                        Price = order.AvgExecutionPrice,
                        Volume = order.ExecutedVolume,
                        Status = order.RemainingAmount > 0
                            ? TradeStatus.PartialFill
                            : TradeStatus.Fill,
                        OriginalVolume = order.OriginalVolume,
                        RemainingVolume = order.RemainingAmount
                    });

                    _hedgeLimitOrderService.Close(hedgeLimitOrder);
                }
                else
                {
                    _log.WarningWithDetails("Can not cancel external order in progress", externalOrder);
                }
            }
            catch (Exception exception)
            {
                _log.WarningWithDetails("An error occurred while canceling limit order", exception,
                    externalOrder);
            }
        }

        public async Task ExecuteLimitOrderAsync(HedgeLimitOrder hedgeLimitOrder)
        {
            await _hedgeLimitOrderService.AddAsync(hedgeLimitOrder);

            ExternalOrder externalOrder =
                await _externalOrderRepository.GetAsync(hedgeLimitOrder.Exchange, hedgeLimitOrder.AssetId);

            if (externalOrder != null)
            {
                hedgeLimitOrder.Error = LimitOrderError.Unknown;
                hedgeLimitOrder.ErrorMessage = "Already exists";

                return;
            }

            AssetPairSettings assetPairSettings =
                await _instrumentService.GetAssetPairAsync(hedgeLimitOrder.AssetPairId, hedgeLimitOrder.Exchange);

            if (assetPairSettings == null)
            {
                hedgeLimitOrder.Error = LimitOrderError.Unknown;
                hedgeLimitOrder.ErrorMessage = "Instrument not configured";

                _log.WarningWithDetails("No settings for instrument", hedgeLimitOrder);

                return;
            }

            decimal price = hedgeLimitOrder.Price
                .TruncateDecimalPlaces(assetPairSettings.PriceAccuracy, hedgeLimitOrder.Type == LimitOrderType.Sell);

            decimal volume = Math.Round(hedgeLimitOrder.Volume, assetPairSettings.VolumeAccuracy);

            if (volume < assetPairSettings.MinVolume)
            {
                hedgeLimitOrder.Error = LimitOrderError.TooSmallVolume;
                return;
            }

            ISpotController spotController = _exchangeAdapterClientFactory.GetSpotController(Name);

            try
            {
                OrderIdResponse response = await spotController.CreateLimitOrderAsync(new LimitOrderRequest
                {
                    Instrument = assetPairSettings.AssetPairId,
                    TradeType = hedgeLimitOrder.Type == LimitOrderType.Sell ? TradeType.Sell : TradeType.Buy,
                    Price = price,
                    Volume = volume
                });

                externalOrder = new ExternalOrder(response.OrderId, hedgeLimitOrder.Id, hedgeLimitOrder.AssetId,
                    hedgeLimitOrder.Exchange);

                await _externalOrderRepository.InsertAsync(externalOrder);

                _log.InfoWithDetails("External order created", externalOrder);
            }
            catch (Exception exception)
            {
                hedgeLimitOrder.Error = LimitOrderError.Unknown;
                hedgeLimitOrder.ErrorMessage = exception.Message;

                _log.WarningWithDetails("An error occurred while creating external order", exception, hedgeLimitOrder);
            }
        }
    }
}
