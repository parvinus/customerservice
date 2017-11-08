using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Customers.Api.Models.Dto
{
    public class SlimCustomerDto
    {
        public string FirstName { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public SlimAddressDto Address { get; set; }
    }
}