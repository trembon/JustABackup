using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JustABackup.Database.Entities;
using JustABackup.Database.Enum;
using JustABackup.Database.Repositories;
using JustABackup.Models.Provider;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JustABackup.Controllers
{
    [Produces("application/json")]
    [Route("api/provider")]
    public class ProviderAPIController : Controller
    {
        private IProviderRepository providerRepository;

        public ProviderAPIController(IProviderRepository providerRepository)
        {
            this.providerRepository = providerRepository;
        }

        public async Task<IActionResult> GetAll(ProviderType? type = null)
        {
            if (!type.HasValue)
                return BadRequest("type is required");

            try
            {
                IEnumerable<Provider> providers = await providerRepository.Get(type.Value);
                List<ProviderModel> mappedProviders = providers.Select(p => new ProviderModel
                {
                    ID = p.ID,
                    Name = p.Name,
                    Type = p.Type.ToString()
                }).ToList();

                return Ok(mappedProviders);
            }
            catch (Exception)
            {
                // TODO: log
                return StatusCode(500);
            }
        }
    }
}