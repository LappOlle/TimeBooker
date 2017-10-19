using TimeBookerApi.Authentication.Context;
using TimeBookerApi.Authentication.Models;
using TimeBookerApi.Authentication.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace TimeBookerApi.Authentication.Repository
{
    public class UserRepository : IDisposable
    {
        private UserContext context;
        private UserManager<IdentityUser> userManager;

        public UserRepository()
        {
            context = new UserContext();
            userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(context));
            var provider = new DpapiDataProtectionProvider("TimeBooker");

            //Setting up what token provider i use. "Owin"
            userManager.UserTokenProvider = new DataProtectorTokenProvider<IdentityUser>(
                provider.Create("EmailConfirmation","PasswordReset"));

            //Setting the EmailService to my own custom emailservice.
            userManager.EmailService = new EmailService();

            //Setting up rules for validation of user.
            userManager.UserValidator = new UserValidator<IdentityUser>(userManager)
            {
                AllowOnlyAlphanumericUserNames = true,
                RequireUniqueEmail = true
            };

            //Setting up rules for validation of password.
            userManager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            //Enable UserLockOut And setting up the rules.
            userManager.UserLockoutEnabledByDefault = true;
            userManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            userManager.MaxFailedAccessAttemptsBeforeLockout = 5;
        }

        /// <summary>
        /// Method to check what roles a user have from DB.
        /// </summary>
        /// <param name="userId">Pass the userId.</param>
        /// <returns>Returns a list of all the roles.</returns>
        public async Task<IList<string>> GetUserRoles(string userId)
        {
            IList<string> roles = await userManager.GetRolesAsync(userId);
            return roles;
        }

        /// <summary>
        /// Method to register a new user.
        /// </summary>
        /// <param name="userModel">Pass a valid userModel.</param>
        /// <returns>Returns a IdentityResult which contains either succeeded or an errorResult.</returns>
        public async Task<IdentityResult> RegisterUser(User userModel)
        {
            IdentityUser user = new IdentityUser
            {
                UserName = userModel.UserName,
                Email = userModel.Email
            };
            var result = await userManager.CreateAsync(user, userModel.Password);
            return result;
        }

        /// <summary>
        /// Method to Change Password.
        /// </summary>
        /// <param name="updateUserModel">Pass a valid updateUserModel.</param>
        /// <returns>Returns a IdentityResult which contains either succeeded or an errorResult.</returns>
        public async Task<IdentityResult> ChangePassword(UpdateUserModel updateUserModel)
        {
            var userId = context.Users.Where(u => u.UserName == updateUserModel.User.UserName).FirstOrDefault().Id;
            var result = await userManager.ChangePasswordAsync(userId, updateUserModel.User.Password, updateUserModel.NewPassword);
            return result;
        }

        /// <summary>
        /// Method to send ConfirmationEmail to the registered email.
        /// </summary>
        /// <param name="userName">Pass a valid userName.</param>
        /// <returns></returns>
        public async Task SendConfirmationEmail(string userName)
        {
            var user = userManager.FindByName(userName);
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user.Id);
            var callbackUrl = Properties.Settings.Default.ConfirmEmailUrl + "api/User/ConfirmEmail" + "?userID=" + user.Id + "&token=" + HttpUtility.UrlEncode(token);

            var message = new IdentityMessage
            {
                Subject = "Confirm your Account",
                Destination = user.Email,
                Body = callbackUrl
            };
            await userManager.EmailService.SendAsync(message);
        }

        /// <summary>
        /// Method to reset password and send a reset token to the registered email.
        /// ***IMPORTANT***We have to set subject in the IdentityMessage to "Reset Password" for send the token.
        /// </summary>
        /// <param name="userName">Pass a valid userName.</param>
        /// <returns></returns>
        public async Task SendResetPasswordToken(string userName)
        {
            var user = userManager.FindByName(userName);
            var token = await userManager.GeneratePasswordResetTokenAsync(user.Id);
            token = HttpUtility.UrlEncode(token);
            var message = new IdentityMessage
            {
                Subject = "Reset Password",
                Destination = user.Email,
                Body = "Here is the token you should copy and past into the specified token field:" + token
            };
            await userManager.EmailService.SendAsync(message);
        }

        /// <summary>
        /// Method to validate the reset password token and set a new password for the specified user.
        /// </summary>
        /// <param name="userName">Pass a valid userName.</param>
        /// <param name="token">Pass a valid token (from the email).</param>
        /// <param name="newPassword">Pass a valid new password.</param>
        /// <returns>Returns a IdentityResult which contains either succeeded or an errorResult.</returns>
        public async Task<IdentityResult> ValidatePasswordToken(string userName, string token, string newPassword)
        {
            var userId = context.Users.Where(u => u.UserName == userName).FirstOrDefault().Id;
            var result = await userManager.ResetPasswordAsync(userId, token, newPassword);
            return result;
        }

        /// <summary>
        /// Method to validate the confirm email token. Before the email is confirmed user can't login.
        /// </summary>
        /// <param name="userId">Pass a valid userId.</param>
        /// <param name="token">Pass a valid token.</param>
        /// <returns>Returns a IdentityResult which contains either succeeded or an errorResult.</returns>
        public async Task<IdentityResult> ValidateEmail(string userId, string token)
        {
            var result = await userManager.ConfirmEmailAsync(userId, token);
            if (result.Succeeded)
            {
                var userRoleId = context.Roles.Where(r => r.Name == "User").FirstOrDefault().Id;
                var userRoleToAdd = new IdentityUserRole();
                userRoleToAdd.RoleId = userRoleId;
                context.Users.Where(u => u.Id == userId).FirstOrDefault().Roles.Add(userRoleToAdd);
                await context.SaveChangesAsync();
            }
            return result;
        }

        /// <summary>
        /// Method to check if there is a user with the specified userName and password.
        /// </summary>
        /// <param name="userName">Pass a valid userName.</param>
        /// <param name="password">Pass a valid password.</param>
        /// <returns>Returns a IdentityUser if there is any with the specified credentials.</returns>
        public async Task<IdentityUser> FindUser(string userName, string password)
        {
            IdentityUser user = await userManager.FindAsync(userName, password);
            return user;
        }

        /// <summary>
        /// Method to Dispose the db and usermanager.
        /// </summary>
        public void Dispose()
        {
            context.Dispose();
            userManager.Dispose();
        }
    }
}