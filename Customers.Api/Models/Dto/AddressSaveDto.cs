using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Customers.Api.Models.Dto
{
    public class AddressSaveDto
    {
        //public int? Id { get; set; }

        [Required(ErrorMessage = "Address street is required.", AllowEmptyStrings = false)]
        public string Street { get; set; }

        public string Unit { get; set; }

        [Required(ErrorMessage = "Address city is required.", AllowEmptyStrings = false)]
        public string City { get; set; }

        public string State { get; set; }

        [Required(ErrorMessage = "Address postal code is required.")]
        [StringLength(10, ErrorMessage = "Postal code must be contain between 3 and 10 characters", MinimumLength = 3)]
        public string PostalCode { get; set; }
    }
}