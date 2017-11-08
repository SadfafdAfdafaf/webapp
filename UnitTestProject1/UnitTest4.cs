using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApplication5.Controllers;
using WebApplication5.Models;
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
    public class UnitTest4
    {
        [TestMethod]
        public void TestMethod1()
        {
            var controller = new GateController();
            var result = controller.Get("companies").Result;

            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void TestMethod2()
        {
            var controller = new GateController();
            var result = controller.Get("workers").Result;

            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void TestMethod3()
        {
            var controller = new GateController();
            var result = controller.Get("regions").Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod4()
        {
            var controller = new GateController();
            var result = controller.GetAll("_").Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod5()
        {
            var controller = new GateController();
            var result = controller.GetAll("Gooogle").Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod6()
        {
            var controller = new GateController();
            var result = controller.GetAllRegion("_").Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod7()
        {
            var controller = new GateController();
            var result = controller.GetAllRegion("UA").Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod8()
        {
            var controller = new GateController();
            var result = controller.GetAllWorkers(1,3);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod9()
        {
            var controller = new GateController();
            var result = controller.Get("companies", 1).Result;

            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void TestMethod10()
        {
            var controller = new GateController();
            var result = controller.Get("workers", 1).Result;

            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void TestMethod11()
        {
            var controller = new GateController();
            var result = controller.Get("regions", 1).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod12()
        {
            var controller = new GateController();
            var result = controller.Get("companies", 0).Result;

            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void TestMethod13()
        {
            var controller = new GateController();
            var result = controller.Get("workers", 0).Result;

            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void TestMethod14()
        {
            var controller = new GateController();
            var result = controller.Get("regions", 0).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod15()
        {
            var controller = new GateController();
            var result = controller.Get(1, 1).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod16()
        {
            var controller = new GateController();
            var result = controller.GetAllRegion("EU", 1, 1).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod17()
        {
            var controller = new GateController();
            var result = controller.GetAllRegion("EEE", 1, 1).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod18()
        {
            var controller = new GateController();
            var item = new companiesmodel
            {
                Name = "Gooogle",
                CEO = "Adam Jenkins",
                region = 1
            };
            var result = controller.Get(item).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod19()
        {
            var controller = new GateController();
            var item = new workermodel
            {
                FIO = "Adam Jenkins",
                Cost = 666666,
                Company = 1,
                RegionOffice = 1
            };
            var result = controller.Get(item).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod20()
        {
            var controller = new GateController();
            var item = new detailedCEOmodel
            {
                CEO = "Demo CEO",
                Cost = 666666,
                Name = "Demo Name",
                region = "Demo Region"
            };
            var result = controller.Post(item).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod21()
        {
            var controller = new GateController();
            var item = new companiesmodel
            {
                Name = "Demo Name",
                CEO = "Demo CEO"
            };

            var result1 = controller.Get(item).Result as OkNegotiatedContentResult<companiesmodel>;
            result1.Content.Name = "Demo Name 1";
            var result = controller.Put(result1.Content.Id, result1.Content).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod22()
        {
            var controller = new GateController();
            var result = controller.Delete("Demo Name 1").Result;

            Assert.IsNotNull(result);
        }
    }
}
