using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using WebApplication5.Models;
using System.Net.Http;
using System.Net.Http.Headers;

namespace WebApplication5.Controllers
{
    public class UsersController : Controller
    {
        //
        // GET: /Users/
        public ActionResult Authenticate(string redirect_uri, int client_id = 1)
        {
            ViewBag.ReturnUrl = redirect_uri;
            ViewBag.Code = client_id;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(AuthenticationModel authenticationModel)
        {
            AuthModel aaa = new AuthModel();

            aaa.Username = authenticationModel.Username;
            aaa.Password = authenticationModel.Password;

            string name = "";
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.PostAsJsonAsync(new Uri("http://localhost:17767/oauth/checkuser"), aaa);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        name = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                    }
                    else
                    {
                        ViewBag.ReturnUrl = authenticationModel.Redirect;
                        ViewBag.Code = authenticationModel.ClientId;
                        return View("Authenticate");
                    }
                }
            }
            catch
            {
                return View("Error");
            }

            string clientname = "";

            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:17767/oauth/getclient?ClientId=" + authenticationModel.ClientId.ToString() + "&redirect=" + authenticationModel.Redirect));

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        clientname = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                    }
                    else
                    {
                        return View("Error");
                    }
                }
            }
            catch
            {
                return View("Error");
            }

            ViewBag.ReturnUrl = authenticationModel.Redirect;
            ViewBag.Code = authenticationModel.ClientId;
            ViewBag.Username = authenticationModel.Username;
            ViewBag.Password = authenticationModel.Password;
            ViewBag.Clientname = clientname;
            return View("Access");
        }

        public async Task<ActionResult> Acept(AuthenticationModel authenticationModel)
        {
            authmoels aaa = new authmoels();

            aaa.name = authenticationModel.Username;
            aaa.pass = authenticationModel.Password;
            aaa.clientid = authenticationModel.ClientId;

            string code = "";

            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.PostAsJsonAsync(new Uri("http://localhost:17767/oauth/login"), aaa);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        code = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                    }
                    else
                    {
                        return View("Error");
                    }
                }
            }
            catch
            {
                return View("Error");
            }

            if (authenticationModel.Redirect != null)
            {
                return Redirect(String.Format(authenticationModel.Redirect + "?code={0}&state=", HttpUtility.UrlEncode(code)));
            }
            return RedirectToAction("Index", "Home");
        }



	}
}