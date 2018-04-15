using JustABackup.Base;
using JustABackup.Core.Extensions;
using JustABackup.Core.Services;
using JustABackup.Database;
using JustABackup.Database.Entities;
using JustABackup.Database.Enum;
using JustABackup.Database.Repositories;
using JustABackup.Models;
using JustABackup.Models.Authentication;
using JustABackup.Models.Job;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace JustABackup.Controllers
{
    public class AuthenticationController : ControllerBase
    {
        private ILogger<AuthenticationController> logger;

        private IEncryptionService encryptionService;
        private IProviderRepository providerRepository;
        private IProviderMappingService providerMappingService;
        private IAuthenticatedSessionRepository authenticatedSessionRepository;

        public AuthenticationController(IAuthenticatedSessionRepository authenticatedSessionRepository, IProviderRepository providerRepository, IProviderMappingService providerMappingService, IEncryptionService encryptionService, ILogger<AuthenticationController> logger)
        {
            this.logger = logger;

            this.encryptionService = encryptionService;
            this.providerRepository = providerRepository;
            this.providerMappingService = providerMappingService;
            this.authenticatedSessionRepository = authenticatedSessionRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ListAuthenticatedSessionModel model = CreateModel<ListAuthenticatedSessionModel>();

            var sessions = await authenticatedSessionRepository.Get();

            model.Sessions = sessions
                .Select(api => new AuthenticatedSessionModel
                {
                    ID = api.ID,
                    Name = api.Name,
                    Provider = api.Provider.Provider.Name,
                    HasChangedModel = api.HasChangedModel
                })
                .ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            AuthenticatedSession authenticatedSession = await authenticatedSessionRepository.Get(id);
            if (authenticatedSession == null)
                return NotFound();

            AuthenticationSessionDetailModel model = CreateModel<AuthenticationSessionDetailModel>("Scheduled Backup");
            model.ID = id;
            model.Name = authenticatedSession.Name;
            model.Provider = authenticatedSession.Provider.Provider.Name;

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            CreateAuthenticatedSessionModel model = CreateModel<CreateAuthenticatedSessionModel>();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAuthenticatedSessionModel model)
        {
            if (ModelState.ErrorCount == 0)
            {
                Provider provider = await providerRepository.Get(model.AuthenticationProvider);
                ProviderInstance providerInstance = await providerMappingService.CreateProviderInstance(provider, model.Providers.FirstOrDefault());

                int authenticatedSessionId = await authenticatedSessionRepository.Add(model.Name, null, providerInstance);

                IAuthenticationProvider<object> authenticationProvider = await providerMappingService.CreateProvider<IAuthenticationProvider<object>>(providerInstance);
                authenticationProvider.Initialize($"http://{Request.Host}/Authentication/CompleteAuthentication?id={authenticatedSessionId}", data => StoreSession(authenticatedSessionId, data));
                string redirectUrl = await authenticationProvider.GetOAuthUrl();

                return Redirect(redirectUrl);
            }

            return View(model);
        }

        public async Task<IActionResult> CompleteAuthentication(int id)
        {
            AuthenticatedSession authenticatedSession = await authenticatedSessionRepository.Get(id);
            ProviderInstance providerInstance = await providerRepository.GetInstance(authenticatedSession.Provider.ID);

            if (providerInstance == null)
                return RedirectToAction("Index", "Home");

            IAuthenticationProvider<object> authenticationProvider = await providerMappingService.CreateProvider<IAuthenticationProvider<object>>(providerInstance);
            try
            {
                authenticationProvider.Initialize($"http://{Request.Host}/Authentication/CompleteAuthentication?id={id}", data => StoreSession(id, data));
                bool result = await authenticationProvider.Authenticate(Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString()));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to complete authentication for '{authenticatedSession.Name}'.");
            }

            return RedirectToAction("Index");
        }

        private async void StoreSession(int id, string sessionData)
        {
            byte[] encryptedSessionData = await encryptionService.Encrypt(sessionData);
            await authenticatedSessionRepository.StoreSession(id, encryptedSessionData);
        }
    }
}
