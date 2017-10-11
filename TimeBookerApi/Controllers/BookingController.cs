using TimeBookerApi.Authentication.Models;
using TimeBookerApi.Booking.Context;
using TimeBookerApi.Booking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;

namespace TimeBookerApi.Controllers
{
    [Authorize(Roles = "User,Admin")]
    [RoutePrefix("api/Booking")]
    public class BookingController : ApiController
    {
        // GET: api/Booking
        [HttpGet]
        public IHttpActionResult Get(string username)
        {
            if (!CheckIfAuthorized(username))
            {
                return BadRequest("You are not authorized to look at other user bookings.");
            }

            else if (ModelState.IsValid)
            {
                List<TimeBooking> bookings = new List<TimeBooking>();
                using (var con = new BookingContext())
                {
                    bookings.AddRange(con.Bookings.Where(b => b.UserName == username).ToList());
                }
                return Ok(bookings);
            }
            return BadRequest();

        }

        // POST: api/Booking
        [HttpPost]
        public IHttpActionResult Post([FromBody]TimeBooking booking)
        {
            if (!CheckIfAuthorized(booking.UserName))
            {
                return BadRequest("You are not authorized to save bookings for other users.");
            }
            try
            {
                if (ModelState.IsValid)
                {
                    using (var con = new BookingContext())
                    {
                        con.Bookings.Add(booking);
                        con.SaveChanges();
                    }
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception)
            {
                return InternalServerError();
            }
            return Ok("You have successfully saved the booking.");
        }

        // PUT: api/Booking/5
        [HttpPut]
        public IHttpActionResult Put([FromBody]TimeBooking booking)
        {
            if (!CheckIfAuthorized(booking.UserName))
            {
                return BadRequest("You are not authorized to change bookings for other users.");
            }
            try
            {
                if (ModelState.IsValid)
                {
                    using (var con = new BookingContext())
                    {
                        var bookingToReplace = con.Bookings.Where(b => b.Id == booking.Id).FirstOrDefault();
                        con.Bookings.Remove(bookingToReplace);
                        con.Bookings.Add(booking);
                        con.SaveChanges();
                    }
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception)
            {
                return InternalServerError();
            }
            return Ok("You have successfully changed the booking.");
        }

        // DELETE: api/Booking/5
        [HttpDelete]
        public IHttpActionResult Delete(int bookingID)
        {
            if (!CheckIfAuthorized(bookingID))
            {
                return BadRequest("You are not authorized to delete bookings for other users.");
            }
            try
            {
                if (ModelState.IsValid)
                {
                    using (var con = new BookingContext())
                    {
                        var booking = con.Bookings.Where(b => b.Id == bookingID).FirstOrDefault();
                        con.Bookings.Remove(booking);
                        con.SaveChanges();
                    }
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }

            catch
            {
                return InternalServerError();
            }
            return Ok("You have successfully deleted the booking.");
        }

        private bool CheckIfAuthorized(string userName)
        {
            if (HttpContext.Current.User.Identity.Name == userName || HttpContext.Current.User.Identity.Name == "Administrator")
            {
                return true;
            }
            else { return false; }
        }

        private bool CheckIfAuthorized(int bookingID)
        {
            TimeBooking booking;
            using (var con = new BookingContext())
            {
                booking = con.Bookings.Where(b => b.Id == bookingID).FirstOrDefault();
            }
            if (HttpContext.Current.User.Identity.Name == booking.UserName || HttpContext.Current.User.Identity.Name == "Administrator")
            {
                return true;
            }
            else { return false; }
        }
    }
}
