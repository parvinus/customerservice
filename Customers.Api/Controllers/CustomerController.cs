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
                    .SingleOrDefault(customer => customer.Id == customerId && customer.Address.City == city);
                var message = "";

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
            //validate the model being passed from the request
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            try
            {
                var statusCode = HttpStatusCode.OK;
                var responseModel = new CustomerResponseModel();
                Address addressDbo = null;

                //if an address_id is provided we need to validate it.
                if (newCustomer?.Address_Id != null)
                {
                    //check the database to verify the provided address id exists
                    var address =
                        CustomerServiceDb.Addresses.SingleOrDefault(addr => addr.Id == newCustomer.Address_Id);

                    //return an error response if the address wasn't found.
                    if (address == null)
                    {
                        responseModel.Message = "failed to create customer";
                        responseModel.Errors = new List<string>(){"addressId provided does not exist."};

                        return Request.CreateResponse(HttpStatusCode.BadRequest, responseModel);
                    } 
                }

                //check if the request sent an address Dto as a payload
                if (newCustomer?.Address != null)
                {
                    //at this point if we have an address payload it's a new address
                    var address = newCustomer.Address;
                    addressDbo = new Address()
                    {
                        Street = address.Street,
                        Unit = address.Unit,
                        City = address.City,
                        State = address.State,
                        PostalCode = address.PostalCode
                    };
                }

                var customerDbo = new Customer()
                {
                    FirstName = newCustomer.FirstName,
                    LastName = newCustomer.LastName,
                    Address = addressDbo,
                    Age = Convert.ToInt32(newCustomer.Age),
                    Address_Id = newCustomer.Address_Id,
                    CreatedOn = DateTime.Now.ToUniversalTime(),
                    Email = newCustomer.Email
                };

                CustomerServiceDb.Customers.Add(customerDbo);
                var rowsSaved = CustomerServiceDb.SaveChanges();

                if (rowsSaved > 0)
                    responseModel.Message = "success";

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
        public HttpResponseMessage Update(CustomerSaveDto updateCustomerDto)
        {
            var errors = new List<string>();
            var message = "";
            var statusCode = HttpStatusCode.OK;
            object result = null;

            Address updatedAddressDbo = null;
            Customer updatedCustomerDbo = null;

            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }

                //check if customer data was actually provided.
                if (updateCustomerDto == null)
                {
                    errors.Add("customer is required.");
                    statusCode = HttpStatusCode.BadRequest;
                }

                //check if the customer id is actually provided
                else if (updateCustomerDto.Id == null)
                {
                    statusCode = HttpStatusCode.BadRequest;
                    errors.Add("customer Id is required to update");
                }

                //check if the  customer id provided exists in database
                if (statusCode == HttpStatusCode.OK)
                {
                    //validate the customer
                    updatedCustomerDbo =
                        CustomerServiceDb.Customers.SingleOrDefault(cust => cust.Id == updateCustomerDto.Id);

                    //verify we found the customer
                    if (updatedCustomerDbo == null)
                    {
                        statusCode = HttpStatusCode.BadRequest;

                        return Request.CreateErrorResponse(statusCode,
                            new ArgumentException("requested customer does not exist"));
                    }

                    if (updateCustomerDto.Address_Id != null)
                    {
                        try
                        {
                            updatedAddressDbo = CustomerServiceDb.Addresses.Single(addr => addr.Id == updateCustomerDto.Address_Id);
                        }
                        catch (Exception e)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                new ArgumentException("requested address does not exist"));
                        }
                    }
                    else if (updatedCustomerDbo.Address_Id != null)
                    {
                        if (updateCustomerDto.Address == null)
                        {
                            CustomerServiceDb.Addresses.Remove(updatedCustomerDbo.Address);
                            CustomerServiceDb.Entry(updatedCustomerDbo.Address).State = EntityState.Deleted;
                            updatedCustomerDbo.Address_Id = null;
                        }
                        else 
                        try
                        {
                            updatedAddressDbo =
                                CustomerServiceDb.Addresses.Single(addr => addr.Id == updatedCustomerDbo.Address_Id);
                        }
                        catch (Exception e)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                new ArgumentException("requested address does not exist"));
                        }
                    }
                    else
                    {
                        updatedAddressDbo = new Address();
                        CustomerServiceDb.Entry(updatedAddressDbo).State = EntityState.Added;
                    }

                    if (updateCustomerDto.Address != null)
                    {
                        updatedAddressDbo.City = updateCustomerDto.Address.City;
                        updatedAddressDbo.PostalCode = updateCustomerDto.Address.PostalCode;
                        updatedAddressDbo.State = updateCustomerDto.Address.State;
                        updatedAddressDbo.Street = updateCustomerDto.Address.Street;
                        updatedAddressDbo.Unit = updateCustomerDto.Address.Unit;
                    }


                    //everything looks good, try to update the customer/address
                    if (statusCode == HttpStatusCode.OK)
                    {
                        updatedCustomerDbo.Address = updatedAddressDbo;

                        updatedCustomerDbo.Age = updateCustomerDto.Age ?? updatedCustomerDbo.Age;
                        updatedCustomerDbo.Email = updateCustomerDto.Email;
                        updatedCustomerDbo.FirstName = updateCustomerDto.FirstName;
                        updatedCustomerDbo.LastName = updateCustomerDto.LastName;

                        CustomerServiceDb.Customers.Attach(updatedCustomerDbo);
                        CustomerServiceDb.Entry(updatedCustomerDbo).State = EntityState.Modified;
                        CustomerServiceDb.SaveChanges();
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
                Message = statusCode == HttpStatusCode.OK ? "success" : "failed to save customer",
                Result = result
            };

            return Request.CreateResponse(statusCode, responseModel);
        }
    }
}
