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
        // GET: api/Booking/userName
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

        // PUT: api/Booking/
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

        /// <summary>
        /// Help method to check if it's the same user that requesting data as the owner to the bookings. 
        /// It also checks if it's Admin that requesting the data. Admin has access to all data.
        /// </summary>
        /// <param name="userName">pass the userName you want bookings from.</param>
        /// <returns>Returns true if it's the same active user as the passed userName or if it's admin.</returns>
        private bool CheckIfAuthorized(string userName)
        {
            if (HttpContext.Current.User.Identity.Name == userName || HttpContext.Current.User.IsInRole("Admin"))
            {
                return true;
            }
            else { return false; }
        }

        /// <summary>
        /// Overloaded Help method to check if it's the same user that requesting data as the owner to the bookingID. 
        /// It also checks if it's Admin that requesting the data. Admin has access to all data.
        /// </summary>
        /// <param name="bookingID">Pass the booking id that is requested.</param>
        /// <returns>Returns true if it's the same active user as the owner to the passed bookingID or if it's admin.</returns>
        private bool CheckIfAuthorized(int bookingID)
        {
            TimeBooking booking;
            using (var con = new BookingContext())
            {
                booking = con.Bookings.Where(b => b.Id == bookingID).FirstOrDefault();
            }
            if (HttpContext.Current.User.Identity.Name == booking.UserName || HttpContext.Current.User.IsInRole("Admin"))
            {
                return true;
            }
            else { return false; }
        }
    }
}
