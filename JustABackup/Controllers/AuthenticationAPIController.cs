using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JustABackup.Database.Repositories;
using JustABackup.Models.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JustABackup.Controllers
{
    [Produces("application/json")]
    [Route("api/authentication")]
    public class AuthenticationAPIController : Controller
    {
        private IAuthenticatedSessionRepository authenticatedSessionRepo;

        public AuthenticationAPIController(IAuthenticatedSessionRepository authenticatedSessionRepo)
        {
            this.authenticatedSessionRepo = authenticatedSessionRepo;
        }

        public async Task<IActionResult> GetAll(string type)
        {
            try
            {
                var authenticatedSessions = await authenticatedSessionRepo.GetAll(type);

                IEnumerable<AuthenticatedSessionModel> models = authenticatedSessions.Select(a => new AuthenticatedSessionModel
                {
                    ID = a.ID,
                    Name = a.Name,
                    Provider = a.Provider.Provider.Name,
                    HasChangedModel = a.HasChangedModel
                });

                return Ok(models);
            }
            catch (Exception)
            {
                // TODO: log
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}