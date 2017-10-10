using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IdentityAuthentication.Booking.Models
{
    public interface ITimeBooking
    {
        int Id { get; set; }
        DateTime From { get; set; }
        DateTime To { get; set; }
        string UserName { get; set; }
    }
}