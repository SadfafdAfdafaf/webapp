using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers; 
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;

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

        public ActionResult lab2()
        {

            return View();
        }



        public async Task<ActionResult> getworkers()
        {
            List<WebApplication1.Models.workermodel> EmpInfo = new List<WebApplication1.Models.workermodel>();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:2051/api/DB"));

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WebApplication1.Models.workermodel>>(EmpResponse);
                    }
                }
            }
            catch (HttpException e)
            {
                throw new HttpException(400, "Bad Request", e);
            }
            return View(EmpInfo);
        }

        public async Task<ActionResult> getworkerspage(int? page)
        {
            List<WebApplication1.Models.workermodel> EmpInfo = new List<WebApplication1.Models.workermodel>();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:2051/api/DB"));

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WebApplication1.Models.workermodel>>(EmpResponse);
                    }
                }
            }
            catch{
                throw new HttpException(400, "Bad Request");
            }

            int pageSize = 3;
            int pageNumber = (page ?? 1);
            return View(EmpInfo.ToPagedList(pageNumber, pageSize));
        }

        public async Task<ActionResult> getsomeworkers()
        {
            List<WebApplication1.Models.workermodel> WorkInfo = new List<WebApplication1.Models.workermodel>();
            WebApplication1.Models.companiesmodel CompInfo = new WebApplication1.Models.companiesmodel();
            try
            {
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
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            try
            {
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
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
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
            try
            {
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
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.BaseAddress = new Uri("http://localhost:29443/");
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    await test.DeleteAsync("api/DB/" + CompInfo.Id);
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            try
            {
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
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            try
            {
                foreach (var t in WorkInfo)
                    using (HttpClient test = new HttpClient())
                    {
                        test.BaseAddress = new Uri("http://localhost:2051/");
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        await test.DeleteAsync("api/DB/" + t.Id);
                    }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }


            return View();
        }

        public async Task<ActionResult> addcompany()
        {
            string region = "UA7";
            WebApplication1.Models.personalinfmodel regID = new Models.personalinfmodel();
            WebApplication1.Models.personalinfmodel regIDbuf = new Models.personalinfmodel();
            regID.claster = region;
            WebApplication1.Models.companiesmodel buf_t = new Models.companiesmodel();
            WebApplication1.Models.companiesmodel buf = new Models.companiesmodel();
            buf.CEO = "Meme JD";
            buf.Name = "Microsoft";
            buf.region = 0;
            WebApplication1.Models.workermodel buf2_t = new Models.workermodel();
            WebApplication1.Models.workermodel buf2 = new Models.workermodel();
            buf2.Company = 0;
            buf2.Cost = 450;
            buf2.FIO = buf.CEO;
            buf2.RegionOffice = 0;

            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.BaseAddress = new Uri("http://localhost:46487/");
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync("odata/CompInf?$filter=claster eq '" + regID.claster + "'");

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                        regIDbuf = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.personalinfmodel>(EmpResponse);
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            try
            {
                if (regIDbuf == null)
                {
                    using (HttpClient test = new HttpClient())
                    {
                        test.BaseAddress = new Uri("http://localhost:46487/");
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage res = await test.PostAsJsonAsync("api/DB", regID);

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            regID = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.personalinfmodel>(EmpResponse);
                        }
                    }
                }
                else
                {
                    regID = regIDbuf;
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }
                                    
            buf.region = regID.Id;

            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.BaseAddress = new Uri("http://localhost:29443/");
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync("odata/CompInf?$filter=region eq " + buf.region + " and " +
                        "CEO eq '" + buf.CEO + "' and Name eq '" + buf.Name + "'");

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                        buf_t = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.companiesmodel>(EmpResponse);
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            try
            {
                if (buf_t == null)
                {
                    using (HttpClient test = new HttpClient())
                    {
                        test.BaseAddress = new Uri("http://localhost:29443/");
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage res = await test.PostAsJsonAsync("api/DB", buf);

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            buf = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.companiesmodel>(EmpResponse);
                        }
                    }
                }
                else
                {
                    buf = buf_t;
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            buf2.Company = buf.Id;
            buf2.RegionOffice = regID.Id;

            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.BaseAddress = new Uri("http://localhost:2051/");
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync("odata/CompInf?$filter=FIO eq '" + buf2.FIO + "' and " +
                        "Cost eq " + buf2.Cost + " and RegionOffice eq " + buf2.RegionOffice + " and Company eq " + buf2.Company);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                        EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                        buf2_t = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.workermodel>(EmpResponse);
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            try
            {
                if (buf2_t == null)
                {
                    using (HttpClient test = new HttpClient())
                    {
                        test.BaseAddress = new Uri("http://localhost:2051/");
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage res = await test.PostAsJsonAsync("api/DB", buf2);

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            buf2 = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.workermodel>(EmpResponse);
                        }
                    }
                }
                else
                {
                    buf2 = buf2_t;
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }
            return View();
        }
	}
}