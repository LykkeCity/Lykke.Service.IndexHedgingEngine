using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Obsolete]
    [Route("/api/[controller]")]
    public class AssetLinksController : Controller
    {
        /// <response code="200">A collection of asset links.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<AssetLinkModel>), (int) HttpStatusCode.OK)]
        public Task<IReadOnlyCollection<AssetLinkModel>> GetAllAsync()
        {
            return Task.FromResult<IReadOnlyCollection<AssetLinkModel>>(new AssetLinkModel[0]);
        }

        /// <response code="200">A collection of asset identifiers.</response>
        [HttpGet("missed")]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int) HttpStatusCode.OK)]
        public Task<IReadOnlyCollection<string>> GetMissedAsync()
        {
            return Task.FromResult<IReadOnlyCollection<string>>(new string[0]);
        }

        /// <response code="204">The asset link successfully added.</response>
        /// <response code="409">The asset link already exists.</response>
        [HttpPost]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Conflict)]
        public Task AddAsync([FromBody] AssetLinkModel model)
        {
            return Task.CompletedTask;
        }

        /// <response code="204">The asset link successfully updated.</response>
        /// <response code="404">The asset link does not exist.</response>
        [HttpPut]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public Task UpdateAsync([FromBody] AssetLinkModel model)
        {
            return Task.CompletedTask;
        }

        /// <response code="204">The asset link successfully deleted.</response>
        /// <response code="404">The asset link does not exist.</response>
        [HttpDelete("{assetId}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public Task DeleteAsync(string assetId)
        {
            return Task.CompletedTask;
        }
        
        public class AssetLinkModel
        {
            public string AssetId { get; set; }

            public string LykkeAssetId { get; set; }
        }
    }
}
