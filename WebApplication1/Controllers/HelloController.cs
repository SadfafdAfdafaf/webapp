using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers; 
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class HelloController : Controller
    {
        //
        // GET: /Hello/
        public ActionResult Index()
        {
            return View("Index");
        }

        public async Task<ActionResult> lab2()
        {

            return View();
        }

        public async Task<ActionResult> ass()
        {
            List<WebApplication1.Models.workermodel> EmpInfo = new List<WebApplication1.Models.workermodel>();  
            using (HttpClient test = new HttpClient())
            {
                test.BaseAddress = new Uri("http://localhost:2051/");
                test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage res = await test.GetAsync("api/DB");

                if (res.IsSuccessStatusCode)
                {  
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    EmpInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WebApplication1.Models.workermodel>>(EmpResponse);
                }  
            }


            return View(EmpInfo);
        }
	}
}