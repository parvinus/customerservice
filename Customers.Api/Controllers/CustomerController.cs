using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Customers.Api.Handlers;
using Customers.Api.Models;
using Customers.Api.Models.Data;
using Customers.Api.Models.Dto;
using Customers.Api.Validators;
using Newtonsoft.Json;

namespace Customers.Api.Controllers
{
    [RoutePrefix("api/Customer")]
    public class CustomerController : ApiController
    {
        private CustomerServiceDbEntity _customerServiceDb;
        private CustomerServiceDbEntity CustomerServiceDb
        {
            get
            {
                if(_customerServiceDb == null)
                    _customerServiceDb = new CustomerServiceDbEntity();
                return _customerServiceDb;
            }
        }

        [Route("GetAllCustomers")]
        [HttpGet]
        public IHttpActionResult GetAllCustomers()
        {
            var allCustomers = CustomerServiceDb.Customers.AsEnumerable();

            return new ResponseHandler(new HttpRequestMessage(Request.Method, Request.RequestUri), HttpStatusCode.OK,
                null, "success", allCustomers);
        }

        //TODO: rewrite getcustomerbyId to use a DTO instead of a direct data object

        [Route("GetCustomerById")]
        [HttpGet]
        public HttpResponseMessage GetCustomerById(int customerId)
        {
            try
            {
                //attempt to get the requested customer
                var results = CustomerServiceDb.Customers.Where(customer => customer.Id == customerId).SingleOrDefault();
                //initialize the model to pass back in our response
                var customerResponse = new CustomerResponseModel();

                //check if we got results back
                if (results == null)
                {
                    //populate our model to reflect no results found
                    customerResponse.Message = "no results found.";
                    customerResponse.Result = null;
                    customerResponse.Errors = null;
                }
                else
                {
                    //extract the customer's address 
                    var address = results.Address;
                    
                    var slimAddressDto = address == null 
                        ? null 
                        : new SlimAddressDto()
                        {
                            City = address.City,
                            Unit = address.Unit,
                            Street = address.Street
                        };

                    customerResponse.Result = new SlimCustomerDto()
                    {
                        FirstName = results.FirstName,
                        Age = results.Age,
                        Email = results.Email,
                        Address =  slimAddressDto
                    };
                }
                return Request.CreateResponse(HttpStatusCode.OK, customerResponse);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [Route("GetCustomerByIdAndCity")]
        [HttpGet]
        public HttpResponseMessage GetCustomerByIdAndCity(int customerId, string city)
        {
            try
            {
                var result = CustomerServiceDb.Customers
                    .Where(customer => customer.Id == customerId && customer.Address.City == city).SingleOrDefault();
                var message = "";
                var errors = new List<string>();

                if (result == null)
                    message = "no results found";
                var customerResponse = new CustomerResponseModel()
                {
                    Message = message,
                    Errors = null,
                    Result = result
                };

                return Request.CreateResponse(HttpStatusCode.OK, customerResponse);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [Route("Create")]
        [HttpPost]
        public HttpResponseMessage Create(CustomerSaveDto newCustomer)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            try
            {
                var statusCode = HttpStatusCode.OK;
                var responseModel = new CustomerResponseModel();

                var address = newCustomer.Address;

                var addressDbo = new Address()
                {
                    Street = address.Street,
                    Unit = address.Unit,
                    City = address.City,
                    State = address.State,
                    PostalCode = address.PostalCode
                };

                var customerDbo = new Customer()
                {
                    FirstName = newCustomer.FirstName,
                    LastName = newCustomer.LastName,
                    Address = addressDbo,
                    Age = Convert.ToInt32(newCustomer.Age),
                    //Address_Id = Convert.ToInt32(newCustomer.Address_Id),
                    CreatedOn = DateTime.Now.ToUniversalTime(),
                    Email = newCustomer.Email
                };

                CustomerServiceDb.Customers.Add(customerDbo);
                var rowsSaved = CustomerServiceDb.SaveChanges();

                if (rowsSaved > 0)
                    responseModel.Message = "success";

                //if(ModelState.IsValid)
             
                // var errors = new List<string>();
                //if (newCustomer == null)
                //{
                //    errors.Add("invalid customer.  customer was not added.");
                // }
                //             else
                //           {
                //   Validate(newCustomer);

                //                   if(ModelState.IsValid)

                //var message = "";
                //var isAgeValid = CustomerValidator.ValidateAge(newCustomer.Age, out message);

                //if (!isAgeValid)
                //{
                //    errors.Add(message);
                //}

                //var isFirstNameValid =
                //    CustomerValidator.ValidateFirstName(newCustomer.FirstName, out message);

                //if (!isFirstNameValid)
                //{
                //    errors.Add(message);
                //}

                //var isEmailValid = CustomerValidator.ValidateEmail(newCustomer.Email, out message);

                //if (!isEmailValid)
                //{
                //    errors.Add(message);
                //}

                //Validate

                //if(statusCode == HttpStatusCode.OK)
                {
                        
                        //else errors.Add("unknown error.");
                    }                      
                //}

                //if (errors.Count > 0)
                //{
                //    responseModel.Message = "failed to create customer";
                //    statusCode = HttpStatusCode.BadRequest;
                //}
                //responseModel.Errors = errors;
                return Request.CreateResponse(statusCode, responseModel);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [Route("Remove")]
        [HttpDelete]
        public HttpResponseMessage Remove(int customerId)
        {
            var errors = new List<string>();
            var message = "";
            var statusCode = HttpStatusCode.OK;

            try
            {
                var customerToRemove =
                    CustomerServiceDb.Customers.SingleOrDefault(customer => customer.Id == customerId);

                if (customerToRemove == null)
                {
                    message = "customer id not found";
                }
                else
                {
                    CustomerServiceDb.Customers.Remove(customerToRemove);
                    CustomerServiceDb.SaveChanges();
                    message = "success";
                }
            }
            catch (Exception e)
            {
                statusCode = HttpStatusCode.InternalServerError;
                message = "failed to remove customer";
                errors.Add(e.Message);
            }

            var response = new CustomerResponseModel()
            {
                Message = message,
                Errors = errors,
                Result = null
            };
            return Request.CreateResponse(statusCode, response);
        }

        [Route("Update")]
        [HttpPut]
        public HttpResponseMessage Update(CustomerUpdateDto updatedCustomer)
        {
            var errors = new List<string>();
            var message = "";
            var statusCode = HttpStatusCode.OK;
            object result = null;

            try
            {
                if (updatedCustomer == null)
                {
                    message = "valid customer was not provided.";
                    statusCode = HttpStatusCode.BadRequest;
                }else if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
                else
                {
                    var customer =
                        CustomerServiceDb.Customers.SingleOrDefault(cust => cust.Id == updatedCustomer.Id);

                    if (customer == null || customer.Id == 0)
                    {
                        message = "customer not found";
                    }
                    else
                    {
                        var address = customer.Address;
                        var updatedAddress = updatedCustomer.Address;

                        address.City = updatedAddress.City;
                        address.Street = updatedAddress.Street;
                        address.Unit = updatedAddress.Unit;
                        address.State = updatedAddress.State;
                        address.PostalCode = updatedAddress.PostalCode;

                        customer.Address = address;
                        customer.Age = Convert.ToInt32(updatedCustomer.Age);
                        customer.Email = updatedCustomer.Email;
                        customer.FirstName = updatedCustomer.FirstName;
                        customer.LastName = updatedCustomer.LastName;

                        CustomerServiceDb.Customers.Attach(customer);
                        CustomerServiceDb.Entry(customer).State = EntityState.Modified;
                        var updateCount = CustomerServiceDb.SaveChanges();

                        if (updateCount > 0)
                        {
                            message = "success";
                            result = updatedCustomer;
                        }
                        else
                        {
                            message = "no records were updated.";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                statusCode = HttpStatusCode.InternalServerError;
                errors.Add(e.Message);
            }

            var responseModel = new CustomerResponseModel()
            {
                Errors = errors,
                Message = message,
                Result = result
            };

            return Request.CreateResponse(statusCode, responseModel);
        }
    }
}
