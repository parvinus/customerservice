using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Customers.Api.Models.Data;

namespace Customers.Api.Models.Dto
{
    public class CustomerSaveDto
    {
        [Required(ErrorMessage = "Age is required")]
        [Range(10,20, ErrorMessage = "age must be between 10 and 20")]
        public int? Age { get; set; }

        [StringLength(50, MinimumLength = 3, ErrorMessage = "first name must be at least 3 characters")]
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "Not a valid email address")]
        public string Email { get; set; }

        //[Required(ErrorMessage = "Address ID is required")]
        //public int? Address_Id { get; set; }
        [Required(ErrorMessage = "An Address is required.")]
        public AddressSaveDto Address { get; set; }
    }
}