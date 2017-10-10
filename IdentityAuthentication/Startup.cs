using IdentityAuthentication.Authentication.Providers;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

/*I adding this attribute so the server we host this webapi will now where to fire up the API!
 There is no use for Global.asax anymore, so i have deleted that.*/
[assembly:OwinStartup(typeof(IdentityAuthentication.Startup))]
namespace IdentityAuthentication
{
    public class Startup
    {
        public static IDataProtectionProvider DataProtectionProvider { get; set; }

        public void Configuration(IAppBuilder app)
        {
            ConfigureOAuth(app);

            /*This HttpConfiguration config is used to configure API routes. 
             * We will pass this object to method Register in WebApiConfig class.*/
            HttpConfiguration config = new HttpConfiguration();

            //Here we passing it.
            WebApiConfig.Register(config);

            //Adding this so we accept request from any origin not only the front-end "if I had one". Cross Origin Resource Sharing
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            /*This UseWebApi is an interface that will be used by the server when 
             * we wire up ASP.NET Web API to our Owin server pipeline*/
            app.UseWebApi(config);
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(5),
                Provider = new MyAuthorizationServerProvider()
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            //Get and set DataProtectionProvider
            DataProtectionProvider = app.GetDataProtectionProvider();

        }
    }
}