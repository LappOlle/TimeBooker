using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TimeBookerApi.Booking.Models
{
    public class TimeBooking:ITimeBooking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name ="From")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime From { get; set; }

        [Required]
        [Display(Name = "To")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime To { get; set; }

        [Required]
        [Display(Name ="Username")]
        public string UserName { get; set; }
    }
}