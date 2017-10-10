using IdentityAuthentication.Authentication.Repository;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace IdentityAuthentication.Authentication.Providers
{
    public class MyAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            //Here we only doing something when we using userId and userSecret approach.
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            using (UserRepository repo = new UserRepository())
            {
                IdentityUser user = await repo.FindUser(context.UserName, context.Password);
           
                if (user == null)
                {
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                    return;
                }
                else if(!user.EmailConfirmed)
                {
                    context.SetError("invalid_grant", "The email isn't verified.");
                    return;
                }
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                identity.AddClaim(new Claim("sub", context.UserName)); //"sub" = the identity of the user, we put it as the username.
                var roles = await repo.GetUserRoles(user.Id); //Checking for all the roles from db.

                foreach (var role in roles)//Adding all the roles, It will be inside the accesstoken.
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
                context.Validated(identity);
            }
        }
    }

}