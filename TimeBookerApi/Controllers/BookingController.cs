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
        public IHttpActionResult Get()
        {
            
            var requestingUsername = HttpContext.Current.User.Identity.Name;//We take the username from the token.

            if (ModelState.IsValid)
            {
                List<TimeBooking> bookings = new List<TimeBooking>();

                //If it's admin return all the bookings with all data.
                if (requestingUsername == "Administrator")
                {
                    using (var con = new BookingContext())
                    {
                        bookings.AddRange(con.Bookings.ToList());
                    }
                }

                /*Else when it's a user we only return all data in the booking when user is the owner
                 else we only give data about the booking datetime (From, To).*/
                else
                {
                    using (var con = new BookingContext())
                    {
                        foreach (var booking in con.Bookings.ToList())
                        {
                            if (booking.UserName == requestingUsername)
                            {
                                bookings.Add(booking);
                            }
                            else
                            {
                                bookings.Add(new TimeBooking()
                                {
                                    From = booking.From,
                                    To = booking.To
                                });
                            }
                        }
                    }
                }
                return Ok(bookings);
            }
            return BadRequest();
        }

        // POST: api/Booking
        [HttpPost]
        public IHttpActionResult Post([FromBody]TimeBooking booking)
        {
            if (!isBookingHaveEverythingExceptUsername(booking))
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (!String.IsNullOrEmpty(booking.UserName) && HttpContext.Current.User.IsInRole("Admin"))
                {
                    using (var con = new BookingContext())
                    {
                        con.Bookings.Add(booking);
                        con.SaveChanges();
                    }
                }
                else
                {
                    using (var con = new BookingContext())
                    {
                        booking.UserName = HttpContext.Current.User.Identity.Name;
                        con.Bookings.Add(booking);
                        con.SaveChanges();
                    }
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
            if (ModelState.IsValid)
            {
                try
                {
                    using (var con = new BookingContext())
                    {
                        var bookingToDelete = con.Bookings.Where(b => b.Id == booking.Id).FirstOrDefault();
                        var bookingToAdd = new TimeBooking();
                        if (bookingToDelete != null)
                        {
                            con.Bookings.Remove(bookingToDelete);
                            bookingToAdd.Id = booking.Id;
                            bookingToAdd.Location = booking.Location;
                            bookingToAdd.Title = booking.Title;
                            bookingToAdd.Details = booking.Details;
                            bookingToAdd.From = booking.From;
                            bookingToAdd.To = booking.To;
                            bookingToAdd.UserName = booking.UserName;
                            con.Bookings.Add(bookingToAdd);
                            con.SaveChanges();
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
                catch (Exception)
                {
                    return InternalServerError();
                }
            }
            else
            {
                return BadRequest(ModelState);
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
        /// Help method to check if it's the same user that requesting to delete data as the owner to the bookingID. 
        /// It also checks if it's Admin that requesting to delete the data. Admin has access to delete all data.
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

        private bool isBookingHaveEverythingExceptUsername(TimeBooking bookingToCheck)
        {
            if (bookingToCheck.To != null && bookingToCheck.From != null && bookingToCheck.Title != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
