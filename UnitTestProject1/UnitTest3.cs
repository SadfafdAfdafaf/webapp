using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApplication4.Controllers;
using WebApplication4.Models;
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
    public class UnitTest3
    {
        [TestMethod]
        public void PostProduct_ShouldReturnSameProduct()
        {
            var controller = new DBController(new TestStoreAppContextGood2());

            var item = GetDemoProduct();

            var result =
                controller.Postpersonalinf(item).Result as CreatedAtRouteNegotiatedContentResult<personalinf>;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.RouteName, "DefaultApi");
            Assert.AreEqual(result.RouteValues["id"], result.Content.Id);
            Assert.AreEqual(result.Content.claster, item.claster);
            Assert.AreEqual(result.Content.Id, item.Id);
        }

        [TestMethod]
        public void PostProduct_ShouldFail_WhenModel()
        {
            var controller = new DBController(new TestStoreAppContextGood2());
            var item = new personalinf { Id = 3, claster = "Demo" };

            var result = controller.Postpersonalinf(item).Result as CreatedAtRouteNegotiatedContentResult<personalinf>;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void PutProduct_ShouldReturnStatusCode()
        {
            var controller = new DBController(new TestStoreAppContextGood2());

            var item = GetDemoProduct();

            var result = controller.Putpersonalinf(item.Id, item).Result as StatusCodeResult;
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(StatusCodeResult));
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [TestMethod]
        public void PutProduct_ErrorContext()
        {
            var controller = new DBController(new TestStoreAppContextBad2());

            personalinf item = new personalinf { Id = 3, claster = string.Empty};

            var result = controller.Putpersonalinf(item.Id, item).Result;
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void PutProduct_ShouldFail_WhenDifferentID()
        {
            var controller = new DBController(new TestStoreAppContextGood2());

            var badresult = controller.Putpersonalinf(999, GetDemoProduct()).Result;
            Assert.IsInstanceOfType(badresult, typeof(BadRequestResult));
        }

        [TestMethod]
        public void PutProduct_ShouldFail_WhenModel()
        {
            var context = new TestStoreAppContextGood2();
            context.personalinfs.Add(GetDemoProduct());

            var controller = new DBController(context);

            personalinf item = new personalinf { Id = 3, claster = string.Empty };

            controller.Configuration = new HttpConfiguration();
            controller.Validate(item);
            var result = controller.Putpersonalinf(item.Id, item).Result;

            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
        }

        [TestMethod]
        public void PutProduct_ShouldFail_WhenModel2()
        {
            var context = new TestStoreAppContextBad2();
            context.personalinfs.Add(GetDemoProduct());

            var controller = new DBController(context);

            personalinf item = new personalinf
            { 
                Id = 3, 
                claster = "Demo" 
            };

            controller.Configuration = new HttpConfiguration();
            controller.Validate(item);
            var result = controller.Putpersonalinf(3, item).Result as StatusCodeResult;

            Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [TestMethod]
        public void GetProduct_ShouldReturnProductWithSameID()
        {
            var context = new TestStoreAppContextGood2();
            context.personalinfs.Add(GetDemoProduct());

            var controller = new DBController(context);
            var result = controller.Getpersonalinf(3).Result as OkNegotiatedContentResult<personalinf>;

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Content.Id);
        }

        [TestMethod]
        public void GetProduct_ShouldFail_WhenNoID()
        {
            var context = new TestStoreAppContextGood2();
            context.personalinfs.Add(GetDemoProduct());

            var controller = new DBController(context);
            var result = controller.Getpersonalinf(2).Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.GetType(), typeof(NotFoundResult));
        }

        [TestMethod]
        public void GetProducts_ShouldReturnAllProducts()
        {
            var context = new TestStoreAppContextGood2();
            context.personalinfs.Add(new personalinf { Id = 1, claster = "Demo1" });
            context.personalinfs.Add(new personalinf { Id = 2, claster = "Demo2" });
            context.personalinfs.Add(new personalinf { Id = 3, claster = "Demo3" });

            var controller = new DBController(context);
            var result = controller.Getpersonalinfs() as TestProductDbSetReg;

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Local.Count);
        }

        [TestMethod]
        public void DeleteProduct_ShouldReturnOK()
        {
            var context = new TestStoreAppContextGood2();
            var item = GetDemoProduct();
            context.personalinfs.Add(item);

            var controller = new DBController(context);
            var result = controller.Deletepersonalinf(3).Result as OkNegotiatedContentResult<personalinf>;

            Assert.IsNotNull(result);
            Assert.AreEqual(item.Id, result.Content.Id);
        }

        [TestMethod]
        public void DeleteProduct_ShouldNotFound()
        {
            var context = new TestStoreAppContextGood2();
            var item = GetDemoProduct();
            context.personalinfs.Add(item);

            var controller = new DBController(context);
            var result = controller.Deletepersonalinf(555).Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.GetType(), typeof(NotFoundResult));
        }

        [TestMethod]
        public void test_validation()
        {
            var context = new TestStoreAppContextGood2();
            context.personalinfs.Add(GetDemoProduct());
            personalinf item = new personalinf { Id = 3, claster = string.Empty };
            var controller = new DBController();
            controller.Configuration = new HttpConfiguration();

            controller.Validate(item);
            var result = controller.Postpersonalinf(item).Result;

            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
            Assert.AreEqual(1, context.personalinfs.Count());
        }

        personalinf GetDemoProduct()
        {
            return new personalinf() { Id = 3, claster = "Demo"};
        }
    }
}
