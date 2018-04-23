using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using JustABackup.Core.Services;
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
        private IProviderMappingService providerMappingService;

        public ProviderAPIController(IProviderRepository providerRepository, IProviderMappingService providerMappingService)
        {
            this.providerRepository = providerRepository;
            this.providerMappingService = providerMappingService;
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
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [Route("{providerID}/fields")]
        public async Task<IActionResult> GetFields(int providerID, int? instanceID)
        {
            try
            {
                Provider provider;
                Dictionary<string, string> propertyValues = new Dictionary<string, string>();
                if (instanceID.HasValue)
                {
                    ProviderInstance providerInstance = await providerRepository.GetInstance(instanceID.Value);
                    provider = providerInstance.Provider;

                    foreach(ProviderInstanceProperty property in providerInstance.Values)
                    {
                        try
                        {
                            object value = await providerMappingService.GetPresentationValue(property);
                            propertyValues.Add(property.Property.TypeName, value?.ToString());
                        }catch { }
                    }
                }
                else
                {
                    provider = await providerRepository.Get(providerID);
                }

                if (provider == null)
                    return NotFound();

                List<ProviderFieldModel> fields = new List<ProviderFieldModel>();
                foreach(ProviderProperty property in provider.Properties)
                {
                    ProviderFieldModel fieldModel = new ProviderFieldModel();
                    fieldModel.ID = property.TypeName;
                    fieldModel.Name = property.Name;
                    fieldModel.Type = property.Type.ToString().ToLowerInvariant();
                    fieldModel.Value = propertyValues.ContainsKey(property.TypeName) ? propertyValues[property.TypeName] : null;

                    if (property.Attributes.Any(a => a.Name == PropertyAttribute.Password))
                        fieldModel.Type = "password";

                    if (property.Type == PropertyType.Authentication && property.Attributes.Any(a => a.Name == PropertyAttribute.GenericParameter))
                    {
                        fieldModel.Type = "dropdown";

                        string genericType = HttpUtility.UrlEncode(property.Attributes.FirstOrDefault(a => a.Name == PropertyAttribute.GenericParameter).Value);
                        fieldModel.DataSource = $"/api/authentication?type={genericType}";
                    }

                    fields.Add(fieldModel);
                }

                return Ok(fields);
            }
            catch (Exception)
            {
                // TODO: log
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}