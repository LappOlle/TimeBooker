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
    public class UserRepository:IDisposable
    {
        private UserContext context;
        private UserManager<IdentityUser> userManager;

        public UserRepository()
        {
            context = new UserContext();
            userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(context));
            var provider = new DpapiDataProtectionProvider("TimeBooker");

            userManager.UserTokenProvider = new DataProtectorTokenProvider<IdentityUser>(
                provider.Create("EmailConfirmation"));

            userManager.EmailService = new EmailService();

            userManager.UserValidator = new UserValidator<IdentityUser>(userManager)
            {
                AllowOnlyAlphanumericUserNames = true,
                RequireUniqueEmail = true
            };

            userManager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };
        }

        public async Task<IList<string>> GetUserRoles(string userId)
        {
            IList<string> roles = await userManager.GetRolesAsync(userId);
            
            return roles;
        }

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

        public async Task<IdentityResult> ChangePassword(UserViewModel userViewModel)
        {
            var userId = context.Users.Where(u => u.UserName == userViewModel.UserName).FirstOrDefault().Id;
            var result = await userManager.ChangePasswordAsync(userId, userViewModel.Password, userViewModel.NewPassword);
            return result;
        }

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

        public async Task<IdentityResult> ValidateEmail(string userId,string token)
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

        public async Task<IdentityUser> FindUser(string userName, string password)
        {
            IdentityUser user = await userManager.FindAsync(userName, password);
            return user;

        }

        public void Dispose()
        {
            context.Dispose();
            userManager.Dispose();
        }
    }
}