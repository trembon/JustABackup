using JustABackup.Base;
using JustABackup.Core.Extensions;
using JustABackup.Core.Services;
using JustABackup.Database;
using JustABackup.Database.Entities;
using JustABackup.Database.Enum;
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

        private DefaultContext dbContext;
        private IProviderMappingService typeMappingService;

        public AuthenticationController(DefaultContext dbContext, IProviderMappingService typeMappingService)
        {
            this.dbContext = dbContext;
            this.typeMappingService = typeMappingService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ListAuthenticatedSessionModel model = CreateModel<ListAuthenticatedSessionModel>("Authenticated Sessions");

            model.Sessions = await dbContext
                .AuthenticatedSessions
                .Include(api => api.Provider)
                .ThenInclude(pi => pi.Provider)
                .OrderBy(api => api.Name)
                .Select(api => new AuthenticatedSessionModel
                {
                    ID = api.ID,
                    Name = api.Name,
                    Provider = api.Provider.Provider.Name,
                    HasChangedModel = api.HasChangedModel
                })
                .ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            CreateAuthenticatedSessionModel model = CreateModel<CreateAuthenticatedSessionModel>("Create Authenticated Session");

            var authenticationProviders = await dbContext.Providers.Where(p => p.Type == ProviderType.Authentication).Select(p => new { p.ID, p.Name }).ToListAsync();
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
            
            Provider provider = await dbContext.Providers.Include(x => x.Properties).FirstOrDefaultAsync(p => p.ID == createSession.Base.AuthenticationProvider);

            CreateProviderModel model = CreateModel<CreateProviderModel>("Create Authenticated Session");
            
            model.ProviderName = provider.Name;
            model.Properties = provider.Properties.Select(x => new Models.ProviderPropertyModel // TODO: place in a common place?
            {
                Name = x.Name,
                Description = x.Description,
                Template = typeMappingService.GetTemplateFromType(x.Type),
                ViewData = x.GenericType != null ? new { Type = x.GenericType } : null
            }).ToList();

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

                Provider provider = await dbContext.Providers.Include(x => x.Properties).FirstOrDefaultAsync(p => p.ID == createSession.Base.AuthenticationProvider);
                providerInstance = createSession.ProviderInstance.CreateProviderInstance(provider);
            }
            //else if(id > 0)
            //{
            //}

            if (providerInstance == null)
                return RedirectToAction("Index", "Home");

            IAuthenticationProvider<object>  authenticationProvider = ConvertToProvider<IAuthenticationProvider<object>>(providerInstance);
            
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
                Provider provider = await dbContext.Providers.Include(x => x.Properties).FirstOrDefaultAsync(p => p.ID == createSession.Base.AuthenticationProvider);
                providerInstance = createSession.ProviderInstance.CreateProviderInstance(provider);
            }
            //else if (id > 0)
            //{
            //    redirectId = id.ToString();
            //}

            if (providerInstance == null)
                return RedirectToAction("Index", "Home");

            IAuthenticationProvider<object>  authenticationProvider = ConvertToProvider<IAuthenticationProvider<object>>(providerInstance);
            
            authenticationProvider.Initialize($"http://localhost:53178/Authentication/CompleteAuthentication?sessionId={sessionId}", data => StoreSession(providerInstance.ID, data));
            bool result = await authenticationProvider.Authenticate(Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString()));

            if(result && createSession != null)
            {
                // refresh the object
                createSession = HttpContext.Session.GetObject<CreateAuthenicatedSession>(AUTHENTICATED_SESSION_KEY);

                AuthenticatedSession session = new AuthenticatedSession();
                session.Name = createSession.Base.Name;
                session.SessionData = createSession.SessionData;
                session.Provider = providerInstance;

                await dbContext.AuthenticatedSessions.AddAsync(session);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        private void StoreSession(long id, string sessionData)
        {
            CreateAuthenicatedSession createSession = HttpContext.Session.GetObject<CreateAuthenicatedSession>(AUTHENTICATED_SESSION_KEY);
            if (createSession != null)
            {
                createSession.SessionData = sessionData;
                HttpContext.Session.SetObject(AUTHENTICATED_SESSION_KEY, createSession);
            }
            else
            {
                var authenticated = dbContext.AuthenticatedSessions.FirstOrDefault(api => api.ID == id);
                if (authenticated != null)
                {
                    authenticated.SessionData = sessionData;
                    dbContext.SaveChanges();
                }
            }
        }

        // TODO: place in service
        private T ConvertToProvider<T>(ProviderInstance providerInstance) where T : class
        {
            Type providerType = Type.GetType(providerInstance.Provider.Namespace);
            T convertedProvider = Activator.CreateInstance(providerType) as T;

            foreach (var property in providerInstance.Values)
            {
                PropertyInfo propertyInfo = providerType.GetProperty(property.Property.TypeName);
                object originalValueType = Convert.ChangeType(property.Value, propertyInfo.PropertyType);

                propertyInfo.SetValue(convertedProvider, originalValueType);
            }

            return convertedProvider;
        }
    }
}
