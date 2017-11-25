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

        public ActionResult lab4()
        {

            return View();
        }



        public async Task<ActionResult> getworkers()
        {
            List<WebApplication1.Models.personalinfmodel> EmpInfo = new List<WebApplication1.Models.personalinfmodel>();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:56454/api/gate/inf/regions"));

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        var sss = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                        EmpInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WebApplication1.Models.personalinfmodel>>(sss);
                    }
                }
            }
            catch (HttpException e)
            {
                throw new HttpException(400, "Bad Request", e);
            }
            return View(EmpInfo);
        }

        public async Task<ActionResult> getregionbyid(int region_number)
        {
            WebApplication1.Models.personalinfmodel EmpInfo = new WebApplication1.Models.personalinfmodel();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:56454/api/gate/inf/regions/" + region_number.ToString()));

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        var sss = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                        EmpInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.personalinfmodel>(sss);
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            return View(EmpInfo);
        }
        //когда ничего не нашло возвращается пустая коллекция
        public async Task<ActionResult> getcompanybyname(string company_name)
        {
            WebApplication1.Models.companiesmodel CompInfo = new WebApplication1.Models.companiesmodel();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync("http://localhost:56454/api/gate/~companies/" + company_name);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        var sss = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                        CompInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.companiesmodel>(sss);
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            return View(CompInfo);
        }

        public async Task<ActionResult> getworkerspage(int? page, int pageSize=3)
        {
            if (pageSize <= 0)
                pageSize = 3;
            List<WebApplication1.Models.workermodel> EmpInfo = new List<WebApplication1.Models.workermodel>();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:56454/api/gate/~workers/all/page/" + page.ToString() + "/" + pageSize.ToString()));

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

            ///int pageNumber = (page ?? 1);
            int pageNumber = 1;
            return View(EmpInfo.ToPagedList(pageNumber, pageSize));
        }

        public async Task<ActionResult> getcompaniespage(int? page, int pageSize = 3)
        {
            if (pageSize<=0)
                pageSize = 3;

            List<WebApplication1.Models.companiesmodel> EmpInfo = new List<WebApplication1.Models.companiesmodel>();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:56454/api/gate/~companies/all/page/" + page.ToString() + "/" + pageSize.ToString()));

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WebApplication1.Models.companiesmodel>>(EmpResponse);
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            ///int pageNumber = (page ?? 1);
            int pageNumber = 1;
            return View(EmpInfo.ToPagedList(pageNumber, pageSize));
        }

        public async Task<ActionResult> getfilteredworkers(string company_name)
        {
            List<WebApplication1.Models.detailedworkermodel> WorkInfo = new List<WebApplication1.Models.detailedworkermodel>();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync("http://localhost:56454/api/gate/~workers/" + company_name);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        WorkInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WebApplication1.Models.detailedworkermodel>>(EmpResponse);
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            return View(WorkInfo);
        }


        public async Task<ActionResult> getonecompany(string company_name, string ceo_name, int region_id)
        {
            WebApplication1.Models.companiesmodel CompInfo = new WebApplication1.Models.companiesmodel();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync("http://localhost:56454/api/gate/~companies/" + company_name + "/" + ceo_name + "/" + region_id.ToString());

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;                        
                        CompInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.companiesmodel>(EmpResponse);
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            return View(CompInfo);
        }



        public async Task<ActionResult> getregionspage(int? page, int pageSize = 3)
        {
            if (pageSize<=0)
                pageSize = 3;

            List<WebApplication1.Models.personalinfmodel> EmpInfo = new List<WebApplication1.Models.personalinfmodel>();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:56454/api/gate/~regions/_/page/" + page.ToString() + "/" + pageSize.ToString()));

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        EmpInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WebApplication1.Models.personalinfmodel>>(EmpResponse);
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            ///int pageNumber = (page ?? 1);
            int pageNumber = 1;
            return View(EmpInfo.ToPagedList(pageNumber, pageSize));
        }

        public async Task<ActionResult> getoneworker(string worker_name, int compay_id, int cost, int region_id)
        {
            WebApplication1.Models.workermodel CompInfo = new WebApplication1.Models.workermodel();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync("http://localhost:56454/api/gate/~companies/" + worker_name + "/" + compay_id + "/" +
                        cost .ToString() + "/" + region_id.ToString());

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        CompInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.workermodel>(EmpResponse);
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            return View(CompInfo);
        }

        public async Task<ActionResult> addcompany(WebApplication1.Models.detailedCEOmodel aaaasssss)
        {
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.PostAsJsonAsync("http://localhost:56454/api/gate/~companies/add", aaaasssss);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;                        
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            return View();
        }

        
        public async Task<ActionResult> deletecompany(string company_name)
        {
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.DeleteAsync("http://localhost:56454/api/gate/~companies/delete/" + company_name);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    }
                }
            }
            catch
            {
                throw new HttpException(400, "Bad Request");
            }

            
            return View();
        }

        public async Task<ActionResult> editcompany(int company_id, string company_name, string company_ceo, int company_region_id)
        {
            WebApplication1.Models.companiesmodel asadsad = new Models.companiesmodel();
            asadsad.Name = company_name;
            asadsad.CEO = company_ceo;
            asadsad.region = company_region_id;
            asadsad.Id = company_id;

            if (!ModelState.IsValid)
            {
                return View();
            }

            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.PutAsJsonAsync("http://localhost:56454/api/gate/~companies/edit/" + company_id.ToString(), asadsad);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    }
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