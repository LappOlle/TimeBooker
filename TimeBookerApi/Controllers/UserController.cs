using TimeBookerApi.Authentication.Models;
using TimeBookerApi.Authentication.Repository;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.IO;
using RazorEngine;
using System.Net.Http.Headers;

namespace TimeBookerApi.Controllers
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

        [Authorize(Roles = "Admin,User")]
        [Route("ChangePassword")]
        [HttpPut]
        public async Task<IHttpActionResult>ChangePassword(UpdateUserModel model)
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
        public async Task<HttpResponseMessage> ConfirmEmail(string userID, string token)
        {
            dynamic model = new { Name = HttpContext.Current.User.Identity.Name };
            HttpResponseMessage responseMessage;

            if (userID == null || token == null)
            {
                responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                return responseMessage;
                
            }

            IdentityResult result = await repo.ValidateEmail(userID,  token);

            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                return responseMessage;
            }
            responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            string viewPath = HttpContext.Current.Server.MapPath(@"~/MessageViews/ViewEmailMessage.cshtml");
            var template = File.ReadAllText(viewPath);
            string parsedView = Razor.Parse(template,model);
            responseMessage.Content = new StringContent(parsedView);
            responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return responseMessage;
        }

        [AllowAnonymous]
        [Route("ResetPassword")]
        [HttpGet]
        public async Task<IHttpActionResult>ResetPassword(string userName)
        {
            if(userName == null)
            {
                return BadRequest("You have not passed a username.");
            }
            await repo.SendResetPasswordToken(userName);

            return Ok("Check for your token in your email.");
        }

        [AllowAnonymous]
        [Route("ResetPassword")]
        [HttpPost]
        public async Task<IHttpActionResult> ResetPassword(string userName,string token,[FromBody]string newPassword)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            IdentityResult result = await repo.ValidatePasswordToken(userName, token, newPassword);

            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }

            return Ok("Your new password is saved, now you can log in.");
        }

        /// <summary>
        /// Method to help adding errors to ModelState.
        /// </summary>
        /// <param name="result">Pass the IdentityResult before you send the response to the client.</param>
        /// <returns>Returns a error if there is any else it returns null.</returns>
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
