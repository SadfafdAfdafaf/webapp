using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApplication3.Controllers;
using WebApplication3.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Web.Http.Results;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void PostProduct_ShouldReturnSameProduct()
        {
            var controller = new DBController(new TestStoreAppContextGood());

            var item = GetDemoProduct();

            var result =
                controller.Postcompanies(item).Result as CreatedAtRouteNegotiatedContentResult<companies>;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.RouteName, "DefaultApi");
            Assert.AreEqual(result.RouteValues["id"], result.Content.Id);
            Assert.AreEqual(result.Content.CEO, item.CEO);
            Assert.AreEqual(result.Content.Name, item.Name);
            Assert.AreEqual(result.Content.region, item.region);
            Assert.AreEqual(result.Content.Id, item.Id);
        }

        [TestMethod]
        public void PostProduct_ShouldFail_WhenModel()
        {
            var controller = new DBController(new TestStoreAppContextGood());
            companies item = new companies
            {
                Id = 3,
                Name = "Demo name",
                CEO = string.Empty,
                region = 5
            };

            var result = controller.Postcompanies(item).Result as CreatedAtRouteNegotiatedContentResult<companies>;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void PutProduct_ShouldReturnStatusCode()
        {
            var controller = new DBController(new TestStoreAppContextGood());

            var item = GetDemoProduct();

            var result = controller.Putcompanies(item.Id, item).Result as StatusCodeResult;
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(StatusCodeResult));
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [TestMethod]
        public void PutProduct_ErrorContext()
        {
            var controller = new DBController(new TestStoreAppContextBad());
            companies item = new companies
            {
                Id = 3,
                Name = "Demo name",
                CEO = "Demo CEO",
                region = 5
            };

            var result = controller.Putcompanies(item.Id, item).Result;
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void PutProduct_ShouldFail_WhenDifferentID()
        {
            var controller = new DBController(new TestStoreAppContextGood());

            var badresult = controller.Putcompanies(999, GetDemoProduct()).Result;
            Assert.IsInstanceOfType(badresult, typeof(BadRequestResult));
        }

        [TestMethod]
        public void PutProduct_ShouldFail_WhenModel()
        {
            var context = new TestStoreAppContextGood();
            context.companies.Add(GetDemoProduct());

            var controller = new DBController(context);

            companies item = new companies
            {
                Id = 3,
                Name = "Demo name",
                CEO = string.Empty,
                region = 5
            };

            controller.Configuration = new HttpConfiguration();
            controller.Validate(item);
            var result = controller.Putcompanies(item.Id, item).Result;

            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
        }

        [TestMethod]
        public void PutProduct_ShouldFail_WhenModel2()
        {
            var context = new TestStoreAppContextBad();
            context.companies.Add(GetDemoProduct());

            var controller = new DBController(context);

            companies item = new companies
            {
                Id = 3,
                Name = "Demo name",
                CEO = "Demo CEO",
                region = 5
            };

            controller.Configuration = new HttpConfiguration();
            controller.Validate(item);
            var result = controller.Putcompanies(3, item).Result as StatusCodeResult;

            Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [TestMethod]
        public void GetProduct_ShouldReturnProductWithSameID()
        {
            var context = new TestStoreAppContextGood();
            context.companies.Add(GetDemoProduct());

            var controller = new DBController(context);
            var result = controller.Getcompanies(3).Result as OkNegotiatedContentResult<companies>;

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Content.Id);
        }

        [TestMethod]
        public void GetProduct_ShouldFail_WhenNoID()
        {
            var context = new TestStoreAppContextGood();
            context.companies.Add(GetDemoProduct());

            var controller = new DBController(context);
            var result = controller.Getcompanies(2).Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.GetType(), typeof(NotFoundResult));
        }

        [TestMethod]
        public void GetProducts_ShouldReturnAllProducts()
        {
            var context = new TestStoreAppContextGood();
            context.companies.Add(new companies { Id = 1, Name = "Demo name1", CEO = "Demo CEO1", region = 1});
            context.companies.Add(new companies { Id = 2, Name = "Demo name2", CEO = "Demo CEO2", region = 2});
            context.companies.Add(new companies { Id = 3, Name = "Demo name3", CEO = "Demo CEO3", region = 3});

            var controller = new DBController(context);
            var result = controller.Getcompanies() as TestProductDbSetComp;

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Local.Count);
        }

        [TestMethod]
        public void DeleteProduct_ShouldReturnOK()
        {
            var context = new TestStoreAppContextGood();
            var item = GetDemoProduct();
            context.companies.Add(item);

            var controller = new DBController(context);
            var result = controller.Deletecompanies(3).Result as OkNegotiatedContentResult<companies>;

            Assert.IsNotNull(result);
            Assert.AreEqual(item.Id, result.Content.Id);
        }

        [TestMethod]
        public void DeleteProduct_ShouldNotFound()
        {
            var context = new TestStoreAppContextGood();
            var item = GetDemoProduct();
            context.companies.Add(item);

            var controller = new DBController(context);
            var result = controller.Deletecompanies(555).Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.GetType(), typeof(NotFoundResult));
        }

        [TestMethod]
        public void test_validation()
        {
            var context = new TestStoreAppContextGood();
            context.companies.Add(GetDemoProduct());
            companies item = new companies 
            { 
                Id = 3, 
                Name = "Demo name",
                CEO = string.Empty, 
                region = 5 
            };

            var controller = new DBController();
            controller.Configuration = new HttpConfiguration();

            controller.Validate(item);
            var result = controller.Postcompanies(item).Result;

            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
            Assert.AreEqual(1, context.companies.Count());
        }

        companies GetDemoProduct()
        {
            return new companies() { Id = 3, Name = "Demo name", CEO = "Demo CEO", region = 5};
        }
    }
}
