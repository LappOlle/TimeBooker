﻿using TimeBookerApi.Authentication.Models;
using TimeBookerApi.Booking.Context;
using TimeBookerApi.Booking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace TimeBookerApi.Controllers
{
    [Authorize(Roles ="User,Admin")]
    [RoutePrefix("api/Booking")]
    public class BookingController : ApiController
    {
        // GET: api/Booking
        [HttpGet]
        public IHttpActionResult Get(string username)
        {
            if(ModelState.IsValid)
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

        // GET: api/Booking/5
        [HttpGet]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Booking
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Booking/5
        [HttpPut]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Booking/5
        [HttpDelete]
        public void Delete(int id)
        {
        }
    }
}
