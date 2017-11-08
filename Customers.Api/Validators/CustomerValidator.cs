using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Customers.Api.Validators
{
    public class CustomerValidator
    {
        public static bool ValidateAge(int age, out string message)
        {
            if (age < 10 || age > 20)
            {
                message = $"{age} is not a valid age.  Allowed ages are 10-20, inclusive";
                return false;
            }
            message = "";
            return true;
        }

        public static bool ValidateFirstName(string firstName, out string message)
        {
            if (firstName == null || firstName.Length < 3)
            {
                message = $"{firstName} is not a valid first name.  Minimum allowed name length is 3 characters.";
                return false;
            }

            message = "";
            return true;
        }

        public static bool ValidateEmail(string email, out string message)
        {
            if (string.IsNullOrEmpty(email))
            {
                message = "email is required.";
                return false;
            }

            //regex snippet found here: http://regexlib.com/Search.aspx?k=email
            if (!Regex.IsMatch(email ?? "", @"^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$"))
            {
                message = $"{email} is not a valid email.  use the format example@mail.com";
                return false;
            }

            message = "";
            return true;
        }
    }
}