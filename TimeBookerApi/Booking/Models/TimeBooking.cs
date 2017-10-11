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
        public int? Id { get; set; }

        [Required]
        [Display(Name ="Title")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        public string Title { get; set; }

        [Display(Name ="Details")]
        public string Details { get; set; }

        [Display(Name ="Location")]
        [StringLength(maximumLength:50)]
        public string Location { get; set; }

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