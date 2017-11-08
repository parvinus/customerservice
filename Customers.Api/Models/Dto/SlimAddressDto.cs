using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Customers.Api.Models.Dto
{
    public class SlimAddressDto
    {
        public string City { get; set; }
        public string Street { get; set; }
        public string Unit { get; set; }
    }
}