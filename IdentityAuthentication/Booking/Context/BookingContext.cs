using IdentityAuthentication.Booking.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace IdentityAuthentication.Booking.Context
{
    public class BookingContext:DbContext
    {
        public BookingContext():base("BookingDB")
        {
        }
        public DbSet<TimeBooking> Bookings { get; set; }
    }
}