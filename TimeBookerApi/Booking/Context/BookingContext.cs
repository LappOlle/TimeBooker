using TimeBookerApi.Booking.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace TimeBookerApi.Booking.Context
{
    public class BookingContext:DbContext
    {
        public BookingContext():base("BookingDB")
        {
        }
        public DbSet<TimeBooking> Bookings { get; set; }
    }
}