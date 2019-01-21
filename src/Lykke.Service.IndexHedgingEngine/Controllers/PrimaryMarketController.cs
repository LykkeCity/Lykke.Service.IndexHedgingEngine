using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.PrimaryMarket;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class PrimaryMarketController : Controller, IPrimaryMarketApi
    {
        private readonly IPrimaryMarketService _primaryMarketService;
        
        public PrimaryMarketController(
            IPrimaryMarketService primaryMarketService)
        {
            _primaryMarketService = primaryMarketService;
        }

        [HttpGet("info")]
        public async Task<PrimaryMarketInfoModel> GetInfoAsync()
        {
            return new PrimaryMarketInfoModel
            {
                WalletId = await _primaryMarketService.GetPrimaryMarketWalletIdAsync()
            };
        }

        [HttpGet("balances")]
        public async Task<IReadOnlyList<PrimaryMarketBalanceModel>> GetBalancesAsync()
        {
            return Mapper.Map<PrimaryMarketBalanceModel[]>(await _primaryMarketService.GetBalancesAsync());
        }

        [HttpPost("update")]
        public Task ChangeBalance(string assetId, decimal amount, string userId, string comment)
        {
            return _primaryMarketService.UpdateBalanceAsync(assetId, amount, userId, comment);
        }
        
        [HttpGet("history")]
        public async Task<IReadOnlyList<PrimaryMarketBalanceChangeModel>> GetBalanceChangeHistoryAsync()
        {
            return Mapper.Map<PrimaryMarketBalanceChangeModel[]>(await _primaryMarketService.GetHistoryAsync());
        }
    }
}
