using TimeBookerApi.Authentication.Repository;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace TimeBookerApi.Authentication.Providers
{
    public class MyAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            //Here we only doing something before validating when we using userId and userSecret approach.
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            /*For requests without credentials, we have to add "Access-Control-Allow-Origin" 
             * and specify "*" as a wildcard, thereby allowing any origin to access the resource.*/
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            using (UserRepository repo = new UserRepository())
            {
                //"Repo.FindUser" returns a IdentityUser If it's success and the username and password is valid.
                IdentityUser user = await repo.FindUser(context.UserName, context.Password);
           
                if (user == null)
                {
                    //This error will be added to the BadRequest response if there is no user with the given username and password.
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                    return;
                }
                else if(!user.EmailConfirmed)
                {
                    //This error will be added to the BadRequest response to client if the user email isn't confirmed.
                    context.SetError("invalid_grant", "The email isn't verified.");
                    return;
                }
                /*If everthing went fine in the validation above we adding Claims to a ClaimsIdentity "The bearer token for the user"
                 And give the IdentityClaim a Name same as the requesting username. That makes it very easy to validate who the
                 requesting user is in the BookingController with the method "HttpContext.Current.User.Identity.Name == userName" */
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName)); 
                var roles = await repo.GetUserRoles(user.Id); //Checking for all the roles from db.

                foreach (var role in roles)//Adding all the roles, It will be inside the bearer access_token.
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
                context.Validated(identity);//IsValidated becomes true and HasError becomes false.
            }
        }
    }

}