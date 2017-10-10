using IdentityAuthentication.Authentication.Models;
using IdentityAuthentication.Authentication.Repository;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace IdentityAuthentication.Controllers
{
    [RoutePrefix("api/User")]
    public class UserController : ApiController
    {
        private UserRepository repo = null;

        public UserController()
        {
            repo = new UserRepository();
        }

        // POST api/User/Register
        [AllowAnonymous]
        [Route("Register")]
        [HttpPost]
        public async Task<IHttpActionResult> Register(User userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await repo.RegisterUser(userModel);
            if (result.Succeeded)
            {
                await repo.SendConfirmationEmail(userModel.UserName);
            }
            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }

            return Ok("You have succesfully created an account, check your email to confirm your account.");
        }

        [AllowAnonymous]
        [Route("ChangePassword")]
        [HttpPut]
        public async Task<IHttpActionResult>ChangePassword(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            IdentityResult result = await repo.ChangePassword(model);

            IHttpActionResult errorResult = GetErrorResult(result);
            if(errorResult != null)
            {
                return errorResult;
            }

            return Ok("You have succesfully changed password.");
        }

        [AllowAnonymous]
        [Route("ConfirmEmail")]
        [HttpGet]
        public async Task<IHttpActionResult> ConfirmEmail(string userID, string token)
        {
            if (userID == null || token == null)
            {
                return BadRequest("You may have changed the url in the confirmation link, please try again.");
            }

            IdentityResult result = await repo.ValidateEmail(userID,  token);

            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }

            return Ok("Your email is now confirmed, now you can log in.");
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                repo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
