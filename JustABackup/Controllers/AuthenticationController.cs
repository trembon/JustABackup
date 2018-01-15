using JustABackup.Base;
using JustABackup.Core.Extensions;
using JustABackup.Core.Services;
using JustABackup.Database;
using JustABackup.Database.Entities;
using JustABackup.Database.Enum;
using JustABackup.Database.Repositories;
using JustABackup.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace JustABackup.Controllers
{
    public class AuthenticationController : ControllerBase
    {
        private const string AUTHENTICATED_SESSION_KEY = "CreateAuthenticatedSession";
        
        private IProviderRepository providerRepository;
        private IProviderMappingService providerMappingService;
        private IAuthenticatedSessionRepository authenticatedSessionRepository;

        public AuthenticationController(IAuthenticatedSessionRepository authenticatedSessionRepository, IProviderRepository providerRepository, IProviderMappingService providerMappingService)
        {
            this.providerRepository = providerRepository;
            this.providerMappingService = providerMappingService;
            this.authenticatedSessionRepository = authenticatedSessionRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await providerRepository.GetInstance(1);

            ListAuthenticatedSessionModel model = CreateModel<ListAuthenticatedSessionModel>("Authenticated Sessions");

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
        public async Task<IActionResult> Create()
        {
            CreateAuthenticatedSessionModel model = CreateModel<CreateAuthenticatedSessionModel>("Create Authenticated Session");

            var authenticationProviders = await providerRepository.Get(ProviderType.Authentication);
            model.AuthenticationProviders = new SelectList(authenticationProviders, "ID", "Name");

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(CreateAuthenticatedSessionModel model)
        {
            if (ModelState.IsValid)
            {
                CreateAuthenicatedSession createSession = new CreateAuthenicatedSession { Base = model };
                HttpContext.Session.SetObject(AUTHENTICATED_SESSION_KEY, createSession);
                
                return RedirectToAction("ConfigureProvider");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfigureProvider()
        {
            CreateAuthenicatedSession createSession = HttpContext.Session.GetObject<CreateAuthenicatedSession>(AUTHENTICATED_SESSION_KEY);
            if (createSession == null)
                return RedirectToAction("Index", "Home");
            
            Provider provider = await providerRepository.Get(createSession.Base.AuthenticationProvider);
            CreateProviderModel model = CreateModel<CreateProviderModel>("Create Authenticated Session");
            
            model.ProviderName = provider.Name;
            model.Properties = provider.Properties.Select(x => new ProviderPropertyModel(x, providerMappingService)).ToList();

            return View(model);
        }

        [HttpPost]
        public IActionResult ConfigureProvider(CreateProviderModel model)
        {
            CreateAuthenicatedSession createSession = HttpContext.Session.GetObject<CreateAuthenicatedSession>(AUTHENTICATED_SESSION_KEY);
            if (createSession == null)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                createSession.ProviderInstance = model;
                HttpContext.Session.SetObject(AUTHENTICATED_SESSION_KEY, createSession);

                return RedirectToAction("Authenticate");
            }

            return View(model);
        }
        
        public async Task<IActionResult> Authenticate(long id = 0)
        {
            string redirectId = null;
            ProviderInstance providerInstance = null;

            CreateAuthenicatedSession createSession = HttpContext.Session.GetObject<CreateAuthenicatedSession>(AUTHENTICATED_SESSION_KEY);
            if (createSession != null)
            {
                redirectId = Guid.NewGuid().ToString(); // TODO: move to create and store this in db

                Provider provider = await providerRepository.Get(createSession.Base.AuthenticationProvider);
                providerInstance = createSession.ProviderInstance.CreateProviderInstance(provider);
            }
            //else if(id > 0)
            //{
            //}

            if (providerInstance == null)
                return RedirectToAction("Index", "Home");

            IAuthenticationProvider<object>  authenticationProvider = await providerMappingService.CreateProvider<IAuthenticationProvider<object>>(providerInstance);
            
            authenticationProvider.Initialize($"http://localhost:53178/Authentication/CompleteAuthentication?sessionId={redirectId}", data => StoreSession(providerInstance.ID, data));
            string redirectUrl = await authenticationProvider.GetOAuthUrl();

            return Redirect(redirectUrl);
        }

        public async Task<IActionResult> CompleteAuthentication(string sessionId)
        {
            ProviderInstance providerInstance = null;

            CreateAuthenicatedSession createSession = HttpContext.Session.GetObject<CreateAuthenicatedSession>(AUTHENTICATED_SESSION_KEY);
            if (createSession != null)
            {
                Provider provider = await providerRepository.Get(createSession.Base.AuthenticationProvider);
                providerInstance = createSession.ProviderInstance.CreateProviderInstance(provider);
            }
            //else if (id > 0)
            //{
            //    redirectId = id.ToString();
            //}

            if (providerInstance == null)
                return RedirectToAction("Index", "Home");

            IAuthenticationProvider<object>  authenticationProvider = await providerMappingService.CreateProvider<IAuthenticationProvider<object>>(providerInstance);
            
            authenticationProvider.Initialize($"http://localhost:53178/Authentication/CompleteAuthentication?sessionId={sessionId}", data => StoreSession(providerInstance.ID, data));
            bool result = await authenticationProvider.Authenticate(Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString()));

            if(result && createSession != null)
            {
                // refresh the object
                createSession = HttpContext.Session.GetObject<CreateAuthenicatedSession>(AUTHENTICATED_SESSION_KEY);

                await authenticatedSessionRepository.Add(createSession.Base.Name, createSession.SessionData, providerInstance);

                HttpContext.Session.Clear();
            }

            return RedirectToAction("Index");
        }

        private void StoreSession(int id, string sessionData)
        {
            CreateAuthenicatedSession createSession = HttpContext.Session.GetObject<CreateAuthenicatedSession>(AUTHENTICATED_SESSION_KEY);
            if (createSession != null)
            {
                createSession.SessionData = sessionData;
                HttpContext.Session.SetObject(AUTHENTICATED_SESSION_KEY, createSession);
            }
            else
            {
                authenticatedSessionRepository.StoreSession(id, sessionData).Wait();
            }
        }
    }
}
