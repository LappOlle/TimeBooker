using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace IdentityAuthentication.Authentication.Context
{
    public class UserContext:IdentityDbContext<IdentityUser>
    {
        public UserContext():base("UserDB")
        {
           
        }
    }
}