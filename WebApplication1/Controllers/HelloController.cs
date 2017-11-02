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



        public async Task<ActionResult> getworkers()
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

        public async Task<ActionResult> getsomeworkers()
        {
            List<WebApplication1.Models.workermodel> WorkInfo = new List<WebApplication1.Models.workermodel>();
            WebApplication1.Models.companiesmodel CompInfo = new WebApplication1.Models.companiesmodel();
            using (HttpClient test = new HttpClient())
            {
                test.BaseAddress = new Uri("http://localhost:29443/");
                test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage res = await test.GetAsync("odata/CompInf?$filter=Name eq 'Microsort'");

                if (res.IsSuccessStatusCode)
                {
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[')+1);
                    EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'),2);
                    CompInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.companiesmodel>(EmpResponse);
                } 
            }

            using (HttpClient test = new HttpClient())
            {
                test.BaseAddress = new Uri("http://localhost:2051/");
                test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage res = await test.GetAsync("odata/CompInf?$filter=Company eq " + CompInfo.Id.ToString());

                if (res.IsSuccessStatusCode)
                {
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('['));
                    EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']')+1, 1);
                    WorkInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WebApplication1.Models.workermodel>>(EmpResponse);
                }

            }

            List<WebApplication1.Models.detailedworkermodel> DWorkInfo = new List<WebApplication1.Models.detailedworkermodel>();

            foreach (var t in WorkInfo)
            {
                WebApplication1.Models.detailedworkermodel temp = new Models.detailedworkermodel();

                temp.FIO = t.FIO;
                temp.Cost = t.Cost;
                temp.Name = CompInfo.Name;
                temp.CEO = CompInfo.CEO;
                temp.region = CompInfo.region;
                temp.RegionOffice = t.RegionOffice;

                DWorkInfo.Add(temp);
            }

            return View(DWorkInfo);
        }

        public async Task<ActionResult> deletecompany()
        {
            List<WebApplication1.Models.workermodel> WorkInfo = new List<WebApplication1.Models.workermodel>();
            WebApplication1.Models.companiesmodel CompInfo = new WebApplication1.Models.companiesmodel();
            using (HttpClient test = new HttpClient())
            {
                test.BaseAddress = new Uri("http://localhost:29443/");
                test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage res = await test.GetAsync("odata/CompInf?$filter=Name eq 'Microsort'");

                if (res.IsSuccessStatusCode)
                {
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                    EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                    CompInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.companiesmodel>(EmpResponse);
                }
            }

            using (HttpClient test = new HttpClient())
            {
                test.BaseAddress = new Uri("http://localhost:29443/");
                test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                await test.DeleteAsync("api/DB/" + CompInfo.Id);
            }

            using (HttpClient test = new HttpClient())
            {
                test.BaseAddress = new Uri("http://localhost:2051/");
                test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage res = await test.GetAsync("odata/CompInf?$filter=Company eq " + CompInfo.Id.ToString());

                if (res.IsSuccessStatusCode)
                {
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('['));
                    EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']') + 1, 1);
                    WorkInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WebApplication1.Models.workermodel>>(EmpResponse);
                }
            }

            foreach (var t in WorkInfo)
                using (HttpClient test = new HttpClient())
                {
                    test.BaseAddress = new Uri("http://localhost:2051/");
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    await test.DeleteAsync("api/DB/" + t.Id);
                }


            return View();
        }

        public async Task<ActionResult> addcompany()
        {
            string region = "UA5";
            WebApplication1.Models.personalinfmodel regID = new Models.personalinfmodel();

            WebApplication1.Models.companiesmodel buf = new Models.companiesmodel();
            buf.CEO = "Meme JD";
            buf.Name = "Microsoft";
            buf.region = 0;
            WebApplication1.Models.workermodel buf2 = new Models.workermodel();
            buf2.Company = 0;
            buf2.Cost = 450;
            buf2.FIO = buf.CEO;
            buf2.RegionOffice = 0;
            WebApplication1.Models.companiesmodel CompInfo = new WebApplication1.Models.companiesmodel();

            using (HttpClient test = new HttpClient())
            {
                test.BaseAddress = new Uri("http://localhost:46487/");
                test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage res = await test.GetAsync("odata/CompInf?$filter=claster eq '" + region + "'");

                if (res.IsSuccessStatusCode)
                {
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                    EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                    regID = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.personalinfmodel>(EmpResponse);
                }
            }

            if (regID == null)
            {
                using (HttpClient test = new HttpClient())
                {
                    test.BaseAddress = new Uri("http://localhost:46487/");
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync("odata/CompInf?$filter=claster eq '" + region + "'");

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                        regID = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.personalinfmodel>(EmpResponse);
                    }
                }
            }

            buf.region = regID.Id;
            using (HttpClient test = new HttpClient())
            {
                test.BaseAddress = new Uri("http://localhost:29443/");
                test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpContent requestMessage = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(buf));
                await test.PostAsync("api/DB", requestMessage);
            }

            using (HttpClient test = new HttpClient())
            {
                test.BaseAddress = new Uri("http://localhost:29443/");
                test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage res = await test.GetAsync("odata/CompInf?$filter=Name eq '"+buf.Name+"'" );

                if (res.IsSuccessStatusCode)
                {
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                    EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                    CompInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.companiesmodel>(EmpResponse);
                }
            }



            return View();
        }
	}
}