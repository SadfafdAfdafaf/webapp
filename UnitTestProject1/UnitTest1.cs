using System;
using WebApplication1.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;

namespace UnitTestProject1
{
    [TestClass]
    public class HelloControllerTest
    {
        [TestMethod]
        public void IndexViewEqualIndexCshtml()
        {
            HelloController controller = new HelloController();

            ViewResult result = controller.Index() as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }

        [TestMethod]
        public void IndexViewResultNotNull()
        {
            HelloController controller = new HelloController();

            ViewResult result = controller.Index() as ViewResult;

            Assert.IsNotNull(result);
        }
    }
}
