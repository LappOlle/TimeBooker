namespace TimeBookerApi.Migrations
{
    using TimeBookerApi.Booking.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class BookingConfiguration : DbMigrationsConfiguration<TimeBookerApi.Booking.Context.BookingContext>
    {
        public BookingConfiguration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(TimeBookerApi.Booking.Context.BookingContext context)
        {
            var booking = new TimeBooking();
            booking.From = DateTime.Now;
            booking.To = DateTime.Now.AddDays(3);
            booking.Title = "M�te";
            booking.Location = "Mellansel framtidens hus.";
            booking.Details = "M�te d�r vi ska diskutera hanteringen utav ogr�s i byns rabatter";
            booking.UserName = "Administrator";
            context.Bookings.Add(booking);
            context.SaveChanges();
        }
    }
}
