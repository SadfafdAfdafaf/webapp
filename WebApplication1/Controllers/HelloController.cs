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
        string token;
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

        // POST: /Account/Login
        public ActionResult Login()
        {
            return Redirect(String.Format("http://localhost:56454/Users/Authenticate?redirect_uri={0}&client_id={1}", "http://localhost:55490/Hello/lootcodes", 1));
            
        }

        public async Task<ActionResult> lootcodes(string code, string state)
        {
            WebApplication1.Models.tokenmessage aaa = new WebApplication1.Models.tokenmessage();
            WebApplication1.Models.authcodemoels bbb = new WebApplication1.Models.authcodemoels();

            bbb.code = code;
            bbb.redirect_uri = "http://localhost:55490";
            bbb.grant_type = "code";
            bbb.client_id = 1;

            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.PostAsJsonAsync(new Uri("http://localhost:56454/api/gate/code"), bbb);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        aaa = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.tokenmessage>(EmpResponse);
                        HttpContext.Response.Cookies["access_token"].Value = aaa.access_token;
                        HttpContext.Response.Cookies["refresh_token"].Value = aaa.refresh_token;
                    }
                    else
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        var sss = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                        return View("sorry", (object)sss);
                    }
                }
            }
            catch
            {
                string myString = "System is unavalieable. lol.";
                return View("sorry", (object)myString);
            }
            return View("lab4");
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
                    else
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        var sss = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                        return View("sorry", (object)sss);
                    }
                }
            }
            catch
            {
                string myString = "System is unavalieable. lol.";
                return View("sorry", (object)myString);
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
                    else
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        var sss = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                        return View("sorry", (object)sss);
                    }
                }
            }
            catch
            {
                string myString = "System is unavalieable. lol.";
                return View("sorry", (object)myString);
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
                        CompInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.companiesmodel>(EmpResponse);
                    }
                    else
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        var sss = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                        return View("sorry", (object)sss);
                    }
                }
            }
            catch
            {
                string myString = "System is unavalieable. lol.";
                return View("sorry", (object)myString);
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
                    else
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        var sss = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                        return View("sorry", (object)sss);
                    }
                }
            }
            catch
            {
                string myString = "System is unavalieable. lol.";
                return View("sorry", (object)myString);
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
                    else
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        var sss = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                        return View("sorry", (object)sss);
                    }
                }
            }
            catch
            {
                string myString = "System is unavalieable. lol.";
                return View("sorry", (object)myString);
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
                    else
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        var sss = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                        return View("sorry", (object)sss);
                    }
                }
            }
            catch
            {
                string myString = "System is unavalieable. lol.";
                return View("sorry", (object)myString);
            }

            return View(WorkInfo);
        }


        public async Task<ActionResult> getonecompany(string company_name, string ceo_name, int region_id = 0)
        {
            string req = "";
            if (ceo_name != "")
            {
                req += "/" + ceo_name;
            }
            else
            {
                req += "/_";
            }
            if (region_id != 0)
            {
                req += "/" + region_id.ToString();
            }
            else
            {
                req += "/0";
            }


            WebApplication1.Models.companiesmodel CompInfo = new WebApplication1.Models.companiesmodel();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync("http://localhost:56454/api/gate/~companies/" + company_name + req);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;                        
                        CompInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.companiesmodel>(EmpResponse);
                    }
                    else
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        var sss = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                        return View("sorry", (object)sss);
                    }
                }
            }
            catch
            {
                string myString = "System is unavalieable. lol.";
                return View("sorry", (object)myString);
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
                    else
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        var sss = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                        return View("sorry", (object)sss);
                    }
                }
            }
            catch
            {
                string myString = "System is unavalieable. lol.";
                return View("sorry", (object)myString);
            }

            ///int pageNumber = (page ?? 1);
            int pageNumber = 1;
            return View(EmpInfo.ToPagedList(pageNumber, pageSize));
        }

        public async Task<ActionResult> getoneworker(string worker_name, int compay_id = 0, int cost = 0, int region_id = 0)
        {
            string req = "";
            if (compay_id != 0)
            {
                req += "/" + compay_id;
            }
            else
            {
                req += "/0";
            }
            if (cost != 0)
            {
                req += "/" + cost.ToString();
            }
            else
            {
                req += "/0";
            }
            if (region_id != 0)
            {
                req += "/" + region_id.ToString();
            }
            else
            {
                req += "/0";
            }

            WebApplication1.Models.workermodel CompInfo = new WebApplication1.Models.workermodel();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync("http://localhost:56454/api/gate/~workers/" + worker_name + req);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        CompInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.workermodel>(EmpResponse);
                    }
                    else
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        var sss = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                        return View("sorry", (object)sss);
                    }
                }
            }
            catch
            {
                string myString = "System is unavalieable. lol.";
                return View("sorry", (object)myString);
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
                    else
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        var sss = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                        return View("sorry", (object)sss);
                    }
                }
            }
            catch
            {
                string myString = "System is unavalieable. lol.";
                return View("sorry", (object)myString);
            }

            return View();
        }

        
        public async Task<ActionResult> deletecompany(string company_name)
        {
            int i = 0;
            try
            {
                //HttpClientHandler handler = new HttpClientHandler();
                //handler.UseCookies = true;
                //handler.CookieContainer = new System.Net.CookieContainer();
                //handler.CookieContainer.Add(new System.Net.Cookie("token_name", token));

                using (HttpClient test = new HttpClient())
                {
                l1: test.DefaultRequestHeaders.Clear();
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    test.DefaultRequestHeaders.Add("Authorization", "Bearer " + HttpContext.Request.Cookies["access_token"].Value);
                    HttpResponseMessage res = await test.DeleteAsync("http://localhost:56454/api/gate/~companies/delete/" + company_name);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        string sss = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                        if (sss != null)
                        {
                            return View("sorry", (object)sss);
                        }
                        if ((res.StatusCode == System.Net.HttpStatusCode.Unauthorized) && (i == 0))
                        {
                            test.DefaultRequestHeaders.Clear();
                            test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            WebApplication1.Models.tokenmessage aaa = new WebApplication1.Models.tokenmessage();
                            WebApplication1.Models.MyModel AAAAA = new WebApplication1.Models.MyModel();
                            AAAAA.refreshtoken = HttpContext.Request.Cookies["refresh_token"].Value;
                            res = await test.PostAsJsonAsync("http://localhost:56454/api/gate/refresh", AAAAA);
                            if (res.IsSuccessStatusCode)
                            {
                                var EmpResponse2 = res.Content.ReadAsStringAsync().Result;
                                aaa = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication1.Models.tokenmessage>(EmpResponse2);
                                HttpContext.Request.Cookies["access_token"].Value = aaa.access_token;
                                HttpContext.Request.Cookies["refresh_token"].Value = aaa.refresh_token;
                                HttpContext.Response.Cookies["access_token"].Value = aaa.access_token;
                                HttpContext.Response.Cookies["refresh_token"].Value = aaa.refresh_token;
                                i++;
                                goto l1;
                            }
                            else
                            {
                                return View("sorry", (object)"Сломалось рефрешение. Иди еще раз проси код. Че как не пасан.");
                            }
                        }                        
                        return View("sorry", (object)sss);
                    }
                }
            }
            catch
            {
                string myString = "System is unavalieable. lol.";
                return View("sorry", (object)myString);
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
                    else
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        var sss = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                        return View("sorry", (object)sss);
                    }
                }
            }
            catch
            {
                string myString = "System is unavalieable. lol.";
                return View("sorry", (object)myString);
            }


            return View();
        }
        
	}
}