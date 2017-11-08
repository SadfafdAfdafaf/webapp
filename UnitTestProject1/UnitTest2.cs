using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApplication2.Controllers;
using WebApplication2.Models;
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
    public class UnitTest2
    {
        [TestMethod]
        public void PostProduct_ShouldReturnSameProduct()
        {
            var controller = new DBController(new TestStoreAppContext());

            var item = GetDemoProduct();

            var result =
                controller.PostWorker(item).Result as CreatedAtRouteNegotiatedContentResult<Worker>;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.RouteName, "DefaultApi");
            Assert.AreEqual(result.RouteValues["id"], result.Content.Id);
            Assert.AreEqual(result.Content.FIO, item.FIO);
            Assert.AreEqual(result.Content.Cost, item.Cost);
            Assert.AreEqual(result.Content.Company, item.Company);
            Assert.AreEqual(result.Content.RegionOffice, item.RegionOffice);
        }

        [TestMethod]
        public void PostProduct_ShouldFail_WhenModel()
        {
            var controller = new DBController(new TestStoreAppContext());
            var item = new Worker
            {
                Cost = 1,
                Company = 1,
                RegionOffice = 1
            };

            var result = controller.PostWorker(item).Result as CreatedAtRouteNegotiatedContentResult<Worker>;

            Assert.IsNotNull(result);            
        }

        [TestMethod]
        public void PutProduct_ShouldReturnStatusCode()
        {
            var controller = new DBController(new TestStoreAppContext());

            var item = GetDemoProduct();

            var result = controller.PutWorker(item.Id, item).Result as StatusCodeResult;
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(StatusCodeResult));
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [TestMethod]
        public void PutProduct_ErrorContext()
        {
            var controller = new DBController(new TestStoreAppContext2());

            Worker item = new Worker
            {
                Id = 1,
                Cost = 5,
                Company = 6,
                RegionOffice = 3,
                FIO = null
            };

            var result = controller.PutWorker(item.Id, item).Result;
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void PutProduct_ShouldFail_WhenDifferentID()
        {
            var controller = new DBController(new TestStoreAppContext());

            var badresult = controller.PutWorker(999, GetDemoProduct()).Result;
            Assert.IsInstanceOfType(badresult, typeof(BadRequestResult));
        }

        [TestMethod]
        public void PutProduct_ShouldFail_WhenModel()
        {
            var context = new TestStoreAppContext();
            context.Workers.Add(GetDemoProduct());

            var controller = new DBController(context);

            Worker item = new Worker
            {
                Id = 1,
                Cost = 5,
                Company = 6,
                RegionOffice = 3,
                FIO = null
            };

            controller.Configuration = new HttpConfiguration();
            controller.Validate(item);
            var result = controller.PutWorker(item.Id, item).Result;

            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
        }

        [TestMethod]
        public void PutProduct_ShouldFail_WhenModel2()
        {
            var context = new TestStoreAppContext2();
            context.Workers.Add(GetDemoProduct());

            var controller = new DBController(context);

            Worker item = new Worker
            {
                Id = 3,
                Cost = 5,
                Company = 6,
                RegionOffice = 3,
                FIO = "fff" 
            };

            controller.Configuration = new HttpConfiguration();
            controller.Validate(item);
            var result = controller.PutWorker(3, item).Result as StatusCodeResult;

            Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [TestMethod]
        public void GetProduct_ShouldReturnProductWithSameID()
        {
            var context = new TestStoreAppContext();
            context.Workers.Add(GetDemoProduct());

            var controller = new DBController(context);
            var result = controller.GetWorker(3).Result as OkNegotiatedContentResult<Worker>;

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Content.Id);
        }

        [TestMethod]
        public void GetProduct_ShouldFail_WhenNoID()
        {
            var context = new TestStoreAppContext();
            context.Workers.Add(GetDemoProduct());

            var controller = new DBController(context);
            var result = controller.GetWorker(2).Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.GetType(), typeof(NotFoundResult));
        }

        [TestMethod]
        public void GetProducts_ShouldReturnAllProducts()
        {
            var context = new TestStoreAppContext();
            context.Workers.Add(new Worker { Id = 1, FIO = "Demo1", Cost = 20, Company = 1, RegionOffice = 1 });
            context.Workers.Add(new Worker { Id = 2, FIO = "Demo2", Cost = 30, Company = 2, RegionOffice = 2 });
            context.Workers.Add(new Worker { Id = 3, FIO = "Demo3", Cost = 40, Company = 3, RegionOffice = 3 });

            var controller = new DBController(context);
            var result = controller.GetWorkers() as TestProductDbSet;

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Local.Count);
        }

        [TestMethod]
        public void DeleteProduct_ShouldReturnOK()
        {
            var context = new TestStoreAppContext();
            var item = GetDemoProduct();
            context.Workers.Add(item);

            var controller = new DBController(context);
            var result = controller.DeleteWorker(3).Result as OkNegotiatedContentResult<Worker>;

            Assert.IsNotNull(result);
            Assert.AreEqual(item.Id, result.Content.Id);
        }

        [TestMethod]
        public void DeleteProduct_ShouldNotFound()
        {
            var context = new TestStoreAppContext();
            var item = GetDemoProduct();
            context.Workers.Add(item);

            var controller = new DBController(context);
            var result = controller.DeleteWorker(555).Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.GetType(), typeof(NotFoundResult));
        }

        [TestMethod]
        public void test_validation()
        {
            var context = new TestStoreAppContext();
            context.Workers.Add(GetDemoProduct());
            Worker item = new Worker
            {
                Id = 1,
                Cost = 5,
                Company = 6,
                RegionOffice = 3,
                FIO = string.Empty
            };
            var controller = new DBController();
            controller.Configuration = new HttpConfiguration();

            controller.Validate(item);
            var result = controller.PostWorker(item).Result;

            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
            Assert.AreEqual(1, context.Workers.Count());
        }

        Worker GetDemoProduct()
        {
            return new Worker() { Id = 3, FIO = "Demo name", Cost = 5, Company = 5, RegionOffice = 5 };
        }
    }
}
