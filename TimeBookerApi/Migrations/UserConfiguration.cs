namespace TimeBookerApi.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class UserConfiguration : DbMigrationsConfiguration<TimeBookerApi.Authentication.Context.UserContext>
    {
        public UserConfiguration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Authentication.Context.UserContext context)
        {
            //Step 1 Create the admin user.
            var passwordHasher = new PasswordHasher();
            var user = new IdentityUser("Administrator");
            user.PasswordHash = passwordHasher.HashPassword("Admin12345");
            user.Email = "andreas_edlund87@hotmail.com";
            user.EmailConfirmed = true;

            //Step 2 Create and add the new Roles.
            var adminRole = new IdentityRole("Admin");
            var userRole = new IdentityRole("User");
            context.Roles.Add(adminRole);
            context.Roles.Add(userRole);

            //Step 3 Create a role for a user
            var role = new IdentityUserRole();
            role.RoleId = adminRole.Id;
            role.UserId = user.Id;

            //Step 4 Add the role row and add the user to DB)
            user.Roles.Add(role);
            context.Users.Add(user);
        }
    }
}
