using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using jksmartstorageService.DataObjects;
using jksmartstorageService.Models;
using Owin;
using Swashbuckle.Application;
using System.Linq;
using System.Net.Http;

namespace jksmartstorageService
{
    public partial class Startup
    {
        public static void ConfigureMobileApp(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            //#1 config swagger
            config.EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "A title for your API");
                c.ResolveConflictingActions(apiDescription => apiDescription.First());
            c.RootUrl(req =>
                req.RequestUri.GetLeftPart(UriPartial.Authority) +
                req.GetRequestContext().VirtualPathRoot.TrimEnd('/'));
                c.IncludeXmlComments(string.Format(@"{0}\bin\jksmartstorageService.XML", System.AppDomain.CurrentDomain.BaseDirectory));
            }).EnableSwaggerUi();

            //For more information on Web API tracing, see http://go.microsoft.com/fwlink/?LinkId=620686 
            config.EnableSystemDiagnosticsTracing();

            new MobileAppConfiguration()
                .UseDefaultConfiguration()
                .ApplyTo(config);

            // Use Entity Framework Code First to create database tables based on your DbContext
            Database.SetInitializer(new jksmartstorageInitializer());

            // To prevent Entity Framework from modifying your database schema, use a null database initializer
            // Database.SetInitializer<jksmartstorageContext>(null);

            MobileAppSettingsDictionary settings = config.GetMobileAppSettingsProvider().GetMobileAppSettings();

            if (string.IsNullOrEmpty(settings.HostName))
            {
                // This middleware is intended to be used locally for debugging. By default, HostName will
                // only have a value when running in an App Service application.
                app.UseAppServiceAuthentication(new AppServiceAuthenticationOptions
                {
                    SigningKey = ConfigurationManager.AppSettings["SigningKey"],
                    ValidAudiences = new[] { ConfigurationManager.AppSettings["ValidAudience"] },
                    ValidIssuers = new[] { ConfigurationManager.AppSettings["ValidIssuer"] },
                    TokenHandler = config.GetAppServiceTokenHandler()
                });
            }
            //#2 remove default routes
            config.MapHttpAttributeRoutes();
            config.Routes.Remove("DefaultApis");
            config.Routes.Remove("tables");
            //
            app.UseWebApi(config);
        }
    }

    public class jksmartstorageInitializer : CreateDatabaseIfNotExists<jksmartstorageContext>
    {
        protected override void Seed(jksmartstorageContext context)
        {
            List<TodoItem> todoItems = new List<TodoItem>
            {
                new TodoItem { Id = Guid.NewGuid().ToString(), Text = "First item", Complete = false },
                new TodoItem { Id = Guid.NewGuid().ToString(), Text = "Second item", Complete = false },
            };

            foreach (TodoItem todoItem in todoItems)
            {
                context.Set<TodoItem>().Add(todoItem);
            }

            base.Seed(context);
        }
    }
}

