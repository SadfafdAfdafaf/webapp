using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http.Headers;
using WebApplication5.Models;
using PagedList.Mvc;
using PagedList;
using NLog;
using System.Messaging;
using DotNetOpenAuth.OAuth2;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace WebApplication5.Controllers
{
    [RoutePrefix("api/gate")]
    public class GateController : ApiController
    {
        private FuckContext db = new FuckContext();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static string[] tokens = new string[3];
        public GateController()
        {        
            Task.Run(() => backwork());
            Task.Run(() => sendstat());            
        }

        private async void recconf(FuckContext db)
        {
            Logger logger = LogManager.GetCurrentClassLogger();            
            MessageQueue queue;
            if (MessageQueue.Exists(@".\private$\OutStat"))
            {
                queue = new MessageQueue(@".\private$\OutStat");
            }
            else
            {
                queue = MessageQueue.Create(".\\private$\\OutStat");
            }

            using (queue)
            {
                queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(outstatmes) });

                Message[] msgs = queue.GetAllMessages();

                List<string> msgsuid = new List<string>();
                List<outstatmes> bodylst = new List<outstatmes>();

                foreach (var m in msgs)
                {
                    if (m.Label == "ANGATE")
                    {
                        queue.ReceiveById(m.Id);
                        bodylst.Add((outstatmes)m.Body);
                    }                    
                }

                foreach (var m in bodylst)
                {
                    switch (m.status)
                    {
                        case 0:
                            var eee = db.instatmes.Find(m.mes.Id);
                            if (eee != null)
                            {
                                db.instatmes.Remove(eee);
                                try
                                {
                                    await db.SaveChangesAsync();
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    ex.Entries.Single().Reload();
                                }
                            }
                            break;
                        case -1:
                            var eee1 = db.instatmes.Find(m.mes.Id);
                            if (eee1 != null)
                            {
                                logger.Error("ERR in Stat. Deleted data from db. Mess:" + m.Error);
                                db.instatmes.Remove(eee1);
                                try
                                {
                                    await db.SaveChangesAsync();
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    ex.Entries.Single().Reload();
                                }
                                
                            }
                            break;
                        default:
                            break;
                    }                    
                }

                await db.SaveChangesAsync();
                return;
            }
        }

        private async void sendstat()
        {
            using (HttpClient test = new HttpClient())
            {
                await test.GetAsync("http://localhost:2100/stat/start");
            }

            FuckContext db = new FuckContext();
            MessageQueue queue;
            if (MessageQueue.Exists(@".\private$\InStat"))
            {
                queue = new MessageQueue(@".\private$\InStat");
            }
            else
            {
                queue = MessageQueue.Create(".\\private$\\InStat");
            }
                        
            using (queue)
            {
                queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(instatmes) });
                while (true)
                {
                    
                    await db.SaveChangesAsync();
                    List<instatmes> sss = new List<instatmes>();
                    try
                    {
                        await Task.Run(() => recconf(db));
                        
                        TimeSpan interval = new TimeSpan(0, 2, 30);
                        System.Threading.Thread.Sleep(interval);

                        sss = db.instatmes.ToList();
                        foreach (var qwe in sss)
                        {
                            if (qwe.Np < 3)
                            {
                                qwe.Np++;
                                queue.Send(qwe);
                                try
                                {
                                    db.Entry(qwe).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    continue;
                                }
                            }                            
                        }

                    }
                    catch
                    {
                        TimeSpan interval = new TimeSpan(0, 2, 0);
                        System.Threading.Thread.Sleep(interval);
                    }
                          
                }

            }
        }

        private async void putdata(string inf, request_type ttt)
        {
            FuckContext db = new FuckContext();
            instatmes m = new instatmes();
            m.detail = inf;
            m.request_type = ttt;
            m.server_name = server_name.GATE;
            m.Time = DateTime.Now;
            m.state = Guid.NewGuid();
            m.Np = 0;

            try
            {
                db.instatmes.Add(m);
                await db.SaveChangesAsync();
            }
            catch
            {
                return;
            }

        }

        private async void backwork()
        {
            MessageQueue queue;
            if (MessageQueue.Exists(@".\private$\MyNewPrivateQueue"))
            {
                queue = new MessageQueue(@".\private$\MyNewPrivateQueue");
            }
            else
            {
                queue = MessageQueue.Create(".\\private$\\MyNewPrivateQueue");
            }
            using (queue)
            {
                queue.Formatter = new XmlMessageFormatter(new Type[] {typeof(string)});
                while (queue.CanRead)
                {
                    Message m = queue.Receive();
                    int r = await deleteentyti(m.Body.ToString());
                    if (r<0)
                    {
                        queue.Send(m);
                    }
                }
            }
        }

        private async Task<int> deleteentyti(string source)
        {
            try
            {
                using (HttpClient test = new HttpClient())
                {

                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    string v = source.Substring(0, 23);
                    switch (v)
                    {
                        case "http://localhost:2051/":
                            test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[0]);
                            break;
                        case "http://localhost:29443/":
                            test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[1]);
                            break;
                        case "http://localhost:46487/":
                            test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[2]);
                            break;
                        default:
                            break;
                    }

                    HttpResponseMessage res = await test.DeleteAsync(source);
                    if (res.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        gettokens();
                        return -1;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request DELETE http://localhost:46487/api/DB/ with filters. Error message: {0}", ex.Message);

                return -1;
            }
            return 0;
        }

        private async Task<int> deleteregion(personalinfmodel inf)
        {
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    int suka = -1;
                    while (suka != 0)
                    {
                        test.DefaultRequestHeaders.Clear();
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[2]);
                        HttpResponseMessage res = await test.DeleteAsync("http://localhost:46487/api/DB/" + inf.Id);
                        if (res.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            gettokens();                            
                        }
                        else
                        {
                            suka = 0;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request DELETE http://localhost:46487/api/DB/ with filters. Error message: {0}", ex.Message);

                return -1;
            }
            return 0;
        }

        private async Task<int> deletecompany(companiesmodel inf)
        {
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    int suka = -1;
                    while (suka != 0)
                    {
                        test.DefaultRequestHeaders.Clear();
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[1]);
                        HttpResponseMessage res = await test.DeleteAsync("http://localhost:29443/api/DB/" + inf.Id);
                        if (res.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            gettokens();                            
                        }
                        else
                        {
                            suka = 0;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request DELETE http://localhost:29443/api/DB/ with filters. Error message: {0}", ex.Message);

                return -1;
            }
            return 0;
        }

        private int gettokens()
        {

            string[] adress = new string[3];
            adress[0] = "http://localhost:2051/Token";
            adress[1] = "http://localhost:29443/Token";
            adress[2] = "http://localhost:46487/Token";
            int i, j;
            j = 0;
            i = 0;
            try
            {
                for (i = j; i < 3; i++)
                    using (HttpClient test = new HttpClient())
                    {
                        var form = new Dictionary<string, string>  
                        {  
                           {"grant_type", "password"},  
                           {"username", "ANUS" + (1+i).ToString()},  
                           {"password", "111111"},  
                        };
                        //test.DefaultRequestHeaders.Accept.Clear();
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var loginContent = new FormUrlEncodedContent(form);
                        HttpResponseMessage res = test.PostAsync(adress[i], loginContent).Result;
                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            EmpResponse = EmpResponse.Remove(0, 17);
                            EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(",") - 1, EmpResponse.Length - EmpResponse.IndexOf(",") + 1);
                            tokens[i] = EmpResponse;
                            j = i;
                        }
                        else
                        {
                            j = i;
                            return -1;
                        }

                    }
            }
            catch (HttpRequestException ex)
            {
                return -1;
            }

            return 0;

        }

        // GET api/gate/login
        [Route("login")]
        public async Task<IHttpActionResult> Postlogin([FromBody]LoginViewModel2 mmm)
        {
            mmm.grant_type = "password";
            using (HttpClient test = new HttpClient())
            {
                var form = new Dictionary<string, string>  
                {  
                   {"grant_type", "password"},  
                   {"username", mmm.username},  
                   {"password", mmm.password},  
                }; 
                //test.DefaultRequestHeaders.Accept.Clear();
                test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var loginContent = new FormUrlEncodedContent(form);
                HttpResponseMessage res = await test.PostAsync("http://localhost:1804/Token", loginContent);
                if (res.IsSuccessStatusCode)
                {
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    EmpResponse = EmpResponse.Remove(0, 17);
                    EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(",") - 1, EmpResponse.Length - EmpResponse.IndexOf(",") + 1);
                    return Ok(EmpResponse);
                }
                
            }
            return Unauthorized();
        }

        // GET: api/Gate/companies
        [Route("inf/{service:maxlength(32)}")]
        public async Task<IHttpActionResult> Get([FromUri]string service)
        {
            await  Task.Run(() => putdata("GET COMPANIES", request_type.GET));
#if(DEBUG==true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            logger.Info("Request from {1} with parametr 'service'= {0}", service, ip);
            switch (service)
            {
                case "companies":                                
                    string CompInfo = "";
                    try
                    {
                        using (HttpClient test = new HttpClient())
                        {
                            int suka = -1;
                            while (suka != 0)
                            {
                                test.DefaultRequestHeaders.Clear();
                                test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[1]);
                                HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:29443/api/DB"));

                                if (res.IsSuccessStatusCode)
                                {
                                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                                    CompInfo = EmpResponse;
                                }
                                if (res.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    gettokens();                                    
                                }
                                else
                                {
                                    suka = 0;
                                }
                                logger.Info("Request  http://localhost:29443/api/DB. Answer status = {0} and Reason = {1}", res.StatusCode, res.ReasonPhrase);
                            }
                            
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        logger.Error("Error with request http://localhost:29443/api/DB Answer status = {0} and Reason = {1}", ex.InnerException, ex.Message);
                        return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
                    }
                    logger.Info("Succsess request from {1} with parametr 'service'= {0}", service, ip);
                    return Ok(CompInfo);
                case "workers":
                    string WorkersInfo = "";
                    try
                    {
                        using (HttpClient test = new HttpClient())
                        {
                            int suka = -1;
                            while (suka != 0)
                            {
                                test.DefaultRequestHeaders.Clear();
                                test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[0]);
                                HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:2051/api/DB"));

                                if (res.IsSuccessStatusCode)
                                {
                                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                                    WorkersInfo = EmpResponse;
                                }
                                if (res.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    gettokens();                                    
                                }
                                else
                                {
                                    suka = 0;
                                }
                                logger.Info("Request  http://localhost:2051/api/DB. Answer status = {0} and Reason = {1}", res.StatusCode, res.ReasonPhrase);
                            }
                            
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        logger.Error("Error with request http://localhost:2051/api/DB Answer status = {0} and Reason = {1}", ex.InnerException, ex.Message);
                        return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
                    }
                    logger.Info("Succsess request from {1} with parametr 'service'= {0}", service, ip);
                    return Ok(WorkersInfo);
                case "regions":
                    string RegionsInfo = "";
                    try
                    {
                        using (HttpClient test = new HttpClient())
                        {
                            int suka = -1;
                            while (suka != 0)
                            {
                                test.DefaultRequestHeaders.Clear();
                                test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[2]);
                                HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:46487/api/DB"));

                                if (res.IsSuccessStatusCode)
                                {
                                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                                    RegionsInfo = EmpResponse;
                                }
                                if (res.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    gettokens();                                    
                                }
                                else
                                {
                                    suka = 0;
                                }
                                logger.Info("Request  http://localhost:46487/api/DB. Answer status = {0} and Reason = {1}", res.StatusCode, res.ReasonPhrase);
                            }
                            
                        }                        
                    }
                    catch (HttpRequestException ex)
                    {
                        logger.Error("Error with request http://localhost:46487/api/DB Answer status = {0} and Reason = {1}", ex.InnerException, ex.Message);
                        return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
                    }
                    logger.Info("Succsess request from {1} with parametr 'service'= {0}", service, ip);
                    return Ok(RegionsInfo);
            }
            return StatusCode(HttpStatusCode.NotImplemented);
        }

        // GET: api/Gate/companies
        [Route("~companies/all/page/{page}/{pageSize:int=3}")]
        public async Task<IPagedList<companiesmodel>> Get(int? page, int pageSize)
        {
#if(DEBUG==true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            logger.Info("Request from {2} with parametr 'page'= {0} 'pageSize'= {1}", page, pageSize, ip);
            int pageNumber = (page ?? 1);
            List<WebApplication5.Models.companiesmodel> CompInfo = new List<WebApplication5.Models.companiesmodel>();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    int suka = -1;
                    while (suka != 0)
                    {
                        test.DefaultRequestHeaders.Clear();
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[1]);
                        HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:29443/api/DB"));

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            CompInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WebApplication5.Models.companiesmodel>>(EmpResponse);
                        }
                        if (res.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            gettokens();
                        }
                        else
                        {
                            suka = 0;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request http://localhost:29443/api/DB Answer status = {0} and Reason = {1}", ex.InnerException, ex.Message);
                return CompInfo.ToPagedList(pageNumber, pageSize);
            }
            logger.Info("Succsess request from {2} with parametr 'page'= {0} 'pageSize'= {1}", page, pageSize, ip);
            return CompInfo.ToPagedList(pageNumber, pageSize);
        }

        [Route("~workers/all/page/{page}/{pageSize:int=3}")]
        public async Task<IPagedList<workermodel>> GetAllWorkers(int? page, int pageSize)
        {
#if(DEBUG==true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            logger.Info("Request from {2} with parametr 'page'= {0} 'pageSize'= {1}", page, pageSize, ip);
            int pageNumber = (page ?? 1);
            List<WebApplication5.Models.workermodel> CompInfo = new List<WebApplication5.Models.workermodel>();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    int suka = -1;
                    while (suka != 0)
                    {
                        test.DefaultRequestHeaders.Clear();
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[0]);
                        HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:2051/api/DB"));

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            CompInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WebApplication5.Models.workermodel>>(EmpResponse);
                        }
                        if (res.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            gettokens();
                        }
                        else
                        {
                            suka = 0;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request http://localhost:2051/api/DB Answer status = {0} and Reason = {1}", ex.InnerException, ex.Message);
                return CompInfo.ToPagedList(pageNumber, pageSize);
            }
            logger.Info("Succsess request from {2} with parametr 'page'= {0} 'pageSize'= {1}", page, pageSize, ip);
            return CompInfo.ToPagedList(pageNumber, pageSize);
        }

        [Route("inf/{service:maxlength(32)}/{id:int}")]
        public async Task<IHttpActionResult> Get([FromUri]string service, [FromUri]int id)
        {
#if(DEBUG==true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            logger.Info("Request from {3} with parametr 'service'= {0} 'id'={1}", service, id, ip);
            switch (service)
            {
                case "companies":
                    string CompInfo = "";
                    try
                    {
                        using (HttpClient test = new HttpClient())
                        {
                            int suka = -1;
                            while (suka != 0)
                            {
                                test.DefaultRequestHeaders.Clear();
                                test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[1]);
                                HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:29443/api/DB/" + id.ToString()));

                                if (res.IsSuccessStatusCode)
                                {
                                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                                    CompInfo = EmpResponse;
                                }
                                if (res.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    gettokens();
                                }
                                else
                                {
                                    suka = 0;
                                }
                            }
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        logger.Error("Error with request http://localhost:29443/api/DB Answer status = {0} and Reason = {1}", ex.InnerException, ex.Message);
                        return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
                    }
                    logger.Info("Compliete request from {3} with parametr 'service'= {0} 'id'={1}", service, id, ip);
                    return Ok(CompInfo);
                case "workers":
                    string WorkersInfo = "";
                    try
                    {
                        using (HttpClient test = new HttpClient())
                        {
                            int suka = -1;
                            while (suka != 0)
                            {
                                test.DefaultRequestHeaders.Clear();
                                test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[0]);
                                HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:2051/api/DB/" + id.ToString()));

                                if (res.IsSuccessStatusCode)
                                {
                                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                                    WorkersInfo = EmpResponse;
                                }
                                if (res.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    gettokens();
                                }
                                else
                                {
                                    suka = 0;
                                }
                            }
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        logger.Error("Error with request http://localhost:2051/api/DB Answer status = {0} and Reason = {1}", ex.InnerException, ex.Message);
                        return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
                    }
                    logger.Info("Compliete request from {3} with parametr 'service'= {0} 'id'={1}", service, id, ip);
                    return Ok(WorkersInfo);
                case "regions":
                    string RegionsInfo = "";
                    try
                    {
                        using (HttpClient test = new HttpClient())
                        {
                            int suka = -1;
                            while (suka != 0)
                            {
                                test.DefaultRequestHeaders.Clear();
                                test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[2]);
                                HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:46487/api/DB/" + id.ToString()));

                                if (res.IsSuccessStatusCode)
                                {
                                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                                    RegionsInfo = EmpResponse;
                                }
                                if (res.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    gettokens();
                                }
                                else
                                {
                                    suka = 0;
                                }
                            }
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        logger.Error("Error with request http://localhost:46487/api/DB Answer status = {0} and Reason = {1}", ex.InnerException, ex.Message);
                        return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
                    }
                    logger.Info("Compliete request from {3} with parametr 'service'= {0} 'id'={1}", service, id, ip);
                    return Ok(RegionsInfo);
            }
            throw new HttpException(500, "Not implemented");
        }

        [Route("~companies/{Name}/{CEO:maxlength(32)=_}/{region:int=0}")]
        public async Task<IHttpActionResult> Get([FromUri] companiesmodel company)
        {
#if(DEBUG==true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            logger.Info("Request GET from {3} with parametrs 'Name'= {0} 'CEO'= {1} 'region'= {2}", company.Name, company.CEO, company.region, ip);
            string requeststr = "";
            if (company.Name != null)
            {
                requeststr = requeststr + "?$filter=Name eq '" + company.Name + "'";
            }
            if (company.CEO != "_")
            {
                requeststr = requeststr + " and CEO eq '" + company.CEO + "'";
            }

            if (company.region != 0)
            {
                requeststr = requeststr + " and region eq " + company.region;
            }
            companiesmodel CompInfo = new companiesmodel();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    int suka = -1;
                    while (suka != 0)
                    {
                        test.DefaultRequestHeaders.Clear();
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[1]);
                        HttpResponseMessage res = await test.GetAsync("http://localhost:29443/odata/CompInf" + requeststr);

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                            EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                            CompInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<companiesmodel>(EmpResponse);
                        }
                        if (res.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            gettokens();
                        }
                        else
                        {
                            suka = 0;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Warn("Error GET http://localhost:29443/odata/CompInfwith + {0} Error message: {1}", requeststr, ex.Message);
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }
            logger.Info("Success compliete request GET from {3} with parametrs 'Name'= {0} 'CEO'= {1} 'region'= {2}", company.Name, company.CEO, company.region, ip);
            if (CompInfo == null)
            {
                return Ok();
            }
            return Ok(CompInfo);
        }

        [Route("~workers/{FIO}/{Company:int=0}/{Cost:int=0}/{RegionOffice:int=0}")]
        public async Task<IHttpActionResult> Get([FromUri] workermodel worker)
        {
#if(DEBUG==true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            logger.Info("Request GET from {4} with parametrs 'FIO'= {0} 'Company'= {1} 'Cost'= {2} 'RegionOffice'= {3}", worker.FIO, worker.Company, worker.Cost, worker.RegionOffice, ip);
            string requeststr = "";
            if (worker.FIO != null)
            {
                requeststr = requeststr + "?$filter=FIO eq '" + worker.FIO + "'";
            }
            if (worker.Company != 0)
            {
                requeststr = requeststr + " and Company eq " + worker.Company;
            }

            if (worker.Cost != 0)
            {
                requeststr = requeststr + " and Cost eq " + worker.Cost;
            }
            if (worker.RegionOffice != 0)
            {
                requeststr = requeststr + " and RegionOffice eq " + worker.RegionOffice;
            }
            workermodel WorkInfo = new workermodel();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    int suka = -1;
                    while (suka != 0)
                    {
                        test.DefaultRequestHeaders.Clear();
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[0]);
                        HttpResponseMessage res = await test.GetAsync("http://localhost:2051/odata/CompInf" + requeststr);

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                            EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                            WorkInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<workermodel>(EmpResponse);
                        }
                        if (res.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            gettokens();
                        }
                        else
                        {
                            suka = 0;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Warn("Error GET http://localhost:2051/odata/CompInfwith + {0} Error message: {1}", requeststr, ex.Message);
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            if (WorkInfo == null)
            {
                return Content(HttpStatusCode.BadRequest, "Worker is not found.");
            }

            logger.Info("Success compliete request GET from {4} with parametrs 'FIO'= {0} 'Company'= {1} 'Cost'= {2} 'RegionOffice'= {3}", worker.FIO, worker.Company, worker.Cost, worker.RegionOffice, ip);
            return Ok(WorkInfo);
        }

        [Route("~workers/{CompanyName:maxlength(32)=_}")]
        public async Task<IHttpActionResult> GetAll([FromUri]string CompanyName)
        {
#if(DEBUG==true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            
            logger.Info("Request GET from {1} with parametr 'CompanyName'= {0}", CompanyName, ip);
            string requeststr = "";
            if (!(CompanyName == "_"))
            {                
                requeststr = requeststr + "?$filter=Name eq '" + CompanyName + "'";
            }
            else
            {
                logger.Warn("Request GET from {1} with parametr 'CompanyName'= {0} aborted by parameters", CompanyName, ip);
                return Content(HttpStatusCode.BadRequest, "Bad DATA AAAAAAAA. Many companies. But need one company.");
            }

            List<WebApplication5.Models.workermodel> WorkInfo = new List<WebApplication5.Models.workermodel>();
            WebApplication5.Models.companiesmodel CompInfo = new WebApplication5.Models.companiesmodel();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    int suka = -1;
                    while (suka != 0)
                    {
                        test.DefaultRequestHeaders.Clear();
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[1]);
                        HttpResponseMessage res = await test.GetAsync("http://localhost:29443/odata/CompInf" + requeststr);

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                            EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                            CompInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication5.Models.companiesmodel>(EmpResponse);
                        }
                        if (res.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            gettokens();
                        }
                        else
                        {
                            suka = 0;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Warn("Error GET http://localhost:29443/odata/CompInfwith + {0} Error message: {1}", requeststr, ex.Message);
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            if (CompInfo == null)
            {
                return Content(HttpStatusCode.BadRequest, "Company is not found.");
            }

            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.BaseAddress = new Uri("http://localhost:2051/");
                    int suka = -1;
                    while (suka != 0)
                    {
                        test.DefaultRequestHeaders.Clear();
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[0]);
                        HttpResponseMessage res = await test.GetAsync("odata/CompInf?$filter=Company eq " + CompInfo.Id.ToString());

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('['));
                            EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']') + 1, 1);
                            WorkInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WebApplication5.Models.workermodel>>(EmpResponse);
                        }
                        if (res.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            gettokens();
                        }
                        else
                        {
                            suka = 0;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Warn("Error GET http://localhost:2051/odata/CompInfwith?$filter=Company eq {0} Error message: {1}", CompInfo.Id, ex.Message);
                WebApplication5.Models.detailedworkermodel temp = new WebApplication5.Models.detailedworkermodel();
                temp.FIO = string.Empty;
                temp.Cost = 0;
                temp.Name = CompInfo.Name;
                temp.CEO = CompInfo.CEO;
                temp.region = CompInfo.region;
                temp.RegionOffice = 0;
                logger.Info("DEGRADATE compliete request GET from {1} with parametr 'CompanyName'= {0}", CompanyName, ip);
                return Ok(temp);
            }

            List<WebApplication5.Models.detailedworkermodel> DWorkInfo = new List<WebApplication5.Models.detailedworkermodel>();

            foreach (var t in WorkInfo)
            {
                WebApplication5.Models.detailedworkermodel temp = new Models.detailedworkermodel();

                temp.FIO = t.FIO;
                temp.Cost = t.Cost;
                temp.Name = CompInfo.Name;
                temp.CEO = CompInfo.CEO;
                temp.region = CompInfo.region;
                temp.RegionOffice = t.RegionOffice;

                DWorkInfo.Add(temp);
            }
            logger.Info("Success compliete request GET from {1} with parametr 'CompanyName'= {0}", CompanyName, ip);
            return Ok(DWorkInfo);
        }

        [Route("~regions/{region:maxlength(32)=_}")]
        public async Task<IHttpActionResult> GetAllRegion([FromUri] string region)
        {
#if(DEBUG==true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            logger.Info("Request GET from {1} with parametr 'region'= {0}", region, ip);
            string requeststr = "";
            if (!(region == "_"))
            {
                requeststr = requeststr + "?$filter=claster eq '" + region + "'";
            }
            string CompInfo = "";
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    int suka = -1;
                    while (suka != 0)
                    {
                        test.DefaultRequestHeaders.Clear();
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[2]);
                        HttpResponseMessage res = await test.GetAsync("http://localhost:46487/odata/CompInf" + requeststr);

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('['));
                            EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']') + 1, 1);
                            CompInfo = EmpResponse;
                        }
                        if (res.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            gettokens();
                        }
                        else
                        {
                            suka = 0;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Warn("Error GET http://localhost:46487/odata/CompInfwith + {0} Error message: {1}", requeststr, ex.Message);
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }
            logger.Info("Success compliete request GET from {1} with parametr 'region'= {0}", region, ip);
            return Ok(CompInfo);
        }

        [Route("~regions/{region:maxlength(32)=_}/page/{page}/{pageSize:int=3}")]
        public async Task<IPagedList<personalinfmodel>> GetAllRegion([FromUri] string region, int? page, int pageSize)
        {
            int pageNumber = (page ?? 1);
#if(DEBUG==true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            logger.Info("Request GET from {1} with parametr 'region'= {0}", region, ip);
            string requeststr = "";
            if (!(region == "_"))
            {
                requeststr = requeststr + "?$filter=claster eq '" + region + "'";
            }
            List<WebApplication5.Models.personalinfmodel> CompInfo = new List<WebApplication5.Models.personalinfmodel>();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    int suka = -1;
                    while (suka != 0)
                    {
                        test.DefaultRequestHeaders.Clear();
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[2]);
                        HttpResponseMessage res = await test.GetAsync("http://localhost:46487/odata/CompInf" + requeststr);

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('['));
                            EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']') + 1, 1);
                            CompInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WebApplication5.Models.personalinfmodel>>(EmpResponse);
                        }
                        if (res.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            gettokens();
                        }
                        else
                        {
                            suka = 0;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Warn("Error GET http://localhost:46487/odata/CompInfwith + {0} Error message: {1}", requeststr, ex.Message);
                return CompInfo.ToPagedList(pageNumber, pageSize);
            }
            logger.Info("Success compliete request GET from {1} with parametr 'region'= {0}", region, ip);
            return CompInfo.ToPagedList(pageNumber, pageSize);
        }

        // POST: api/Gate

        [Route("~companies/add")]
        public async Task<IHttpActionResult> Post([FromBody]detailedCEOmodel baseinf)
        {
#if(DEBUG==true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            await Task.Run(() => putdata("POST COMPANIE", request_type.CHANGE));
            logger.Info("Request POST from {4} with parametrs 'CEO'= {0}, 'Name'= {1}, 'Cost'= {2}, 'region'= {3}", baseinf.CEO, baseinf.Name, baseinf.Cost, baseinf.region, ip);

            if ((baseinf.CEO == null) || (baseinf.Cost == 0) || (baseinf.Name == null) || (baseinf.region == null))
            {
                logger.Warn("ABORTED POST from {4} with parametrs 'CEO'= {0}, 'Name'= {1}, 'Cost'= {2}, 'region'= {3}. Invalid parametrs.", baseinf.CEO, baseinf.Name, baseinf.Cost, baseinf.region, ip);
                return Content(HttpStatusCode.BadRequest, "Bad DATA AAAAAAAA.");
            }

            WebApplication5.Models.personalinfmodel regID = new Models.personalinfmodel();
            WebApplication5.Models.personalinfmodel regIDbuf = new Models.personalinfmodel();
            WebApplication5.Models.companiesmodel buf_t = new Models.companiesmodel();
            WebApplication5.Models.companiesmodel buf = new Models.companiesmodel();
            WebApplication5.Models.workermodel buf2_t = new Models.workermodel();
            WebApplication5.Models.workermodel buf2 = new Models.workermodel();
            bool flag = true;
            
            buf.CEO = baseinf.CEO;
            buf.Name = baseinf.Name;
            buf.region = 0;

            buf2.Company = 0;
            buf2.Cost = baseinf.Cost;
            buf2.FIO = buf.CEO;
            buf2.RegionOffice = 0;

            regID.claster = baseinf.region;
            
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    int suka = -1;
                    while (suka != 0)
                    {
                        test.DefaultRequestHeaders.Clear();
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[2]);
                        HttpResponseMessage res = await test.GetAsync("http://localhost:46487/odata/CompInf?$filter=claster eq '" + regID.claster + "'");

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                            EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                            regIDbuf = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication5.Models.personalinfmodel>(EmpResponse);
                        }
                        if (res.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            gettokens();
                        }
                        else
                        {
                            suka = 0;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request GET http://localhost:46487/odata/CompInf?$filter=claster eq '{0}' . Error message: {1}", regID.claster, ex.Message);
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            try
            {
                if (regIDbuf == null)
                {
                    using (HttpClient test = new HttpClient())
                    {
                        int suka = -1;
                        while (suka != 0)
                        {
                            test.DefaultRequestHeaders.Clear();
                            test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[2]);
                            HttpResponseMessage res = await test.PostAsJsonAsync("http://localhost:46487/api/DB", regID);

                            if (res.IsSuccessStatusCode)
                            {
                                var EmpResponse = res.Content.ReadAsStringAsync().Result;
                                regID = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication5.Models.personalinfmodel>(EmpResponse);
                            }
                            else
                            {
                                return Content(res.StatusCode, "Bad DATA. AAAAAAAA.");
                            }
                            if (res.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                gettokens();
                            }
                            else
                            {
                                suka = 0;
                            }
                        }
                    }
                }
                else
                {
                    regID = regIDbuf;
                    flag = false;
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request POST http://localhost:46487/api/DB. Error message: {0}", ex.Message);
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            buf.region = regID.Id;

            try
            {
                using (HttpClient test = new HttpClient())
                {
                    int suka = -1;
                    while (suka != 0)
                    {
                        test.DefaultRequestHeaders.Clear();
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[1]);
                        HttpResponseMessage res = await test.GetAsync("http://localhost:29443/odata/CompInf?$filter=region eq " + buf.region
                            + " and " + "CEO eq '" + buf.CEO + "' and Name eq '" + buf.Name + "'");

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                            EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                            buf_t = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication5.Models.companiesmodel>(EmpResponse);
                        }
                        if (res.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            gettokens();
                        }
                        else
                        {
                            suka = 0;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request GET http://localhost:29443/odata/CompInf with filters. Error message: {0}", ex.Message);
                if (flag)
                    if (deleteregion(regID).Result < 0)
                    {
                        return Content((HttpStatusCode)418, "Global system error. Sorry.");
                    }
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            try
            {
                if (buf_t == null)
                {
                    using (HttpClient test = new HttpClient())
                    {
                        int suka = -1;
                        while (suka != 0)
                        {
                            test.DefaultRequestHeaders.Clear();
                            test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[1]);
                            HttpResponseMessage res = await test.PostAsJsonAsync("http://localhost:29443/api/DB", buf);

                            if (res.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                gettokens();
                            }
                            else
                            {
                                suka = 0;
                            }


                            if (res.IsSuccessStatusCode)
                            {
                                var EmpResponse = res.Content.ReadAsStringAsync().Result;
                                buf = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication5.Models.companiesmodel>(EmpResponse);
                            }
                            else
                            {
                                if (flag)
                                    if (deleteregion(regID).Result < 0)
                                    {
                                        return Content((HttpStatusCode)418, "Global system error. Sorry.");
                                    }
                                return Content(res.StatusCode, "Bad DATA. AAAAAAAA.");
                            }
                        }

                    }
                }
                else
                {
                    buf = buf_t;
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request POST http://localhost:29443/api/DB. Error message: {0}", ex.Message);
                if (flag)
                    if (deleteregion(regID).Result < 0)
                    {
                        return Content((HttpStatusCode)418, "Global system error. Sorry.");
                    }
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            buf2.Company = buf.Id;
            buf2.RegionOffice = regID.Id;

            try
            {
                using (HttpClient test = new HttpClient())
                {
                    int suka = -1;
                    while (suka != 0)
                    {
                        test.DefaultRequestHeaders.Clear();
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[0]);
                        HttpResponseMessage res = await test.GetAsync("http://localhost:2051/odata/CompInf?$filter=FIO eq '" + buf2.FIO + "' and " +
                            "Cost eq " + buf2.Cost + " and RegionOffice eq " + buf2.RegionOffice + " and Company eq " + buf2.Company);

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                            EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                            buf2_t = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication5.Models.workermodel>(EmpResponse);
                        }
                        if (res.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            gettokens();
                        }
                        else
                        {
                            suka = 0;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request GET http://localhost:2051/odata/CompInf with filters. Error message: {1}", regID.claster, ex.Message);
                if (flag)
                    if (deleteregion(regID).Result < 0)
                    {
                        return Content((HttpStatusCode)418, "Global system error. Sorry.");
                    }
                if (deletecompany(buf).Result < 0)
                {
                    return Content((HttpStatusCode)418, "Global system error. Sorry.");
                }
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            try
            {
                if (buf2_t == null)
                {
                    using (HttpClient test = new HttpClient())
                    {
                        int suka = -1;
                        while (suka != 0)
                        {
                            test.DefaultRequestHeaders.Clear();
                            test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[0]);
                            HttpResponseMessage res = await test.PostAsJsonAsync("http://localhost:2051/api/DB", buf2);

                            if (res.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                gettokens();
                            }
                            else
                            {
                                suka = 0;
                            }

                            if (res.IsSuccessStatusCode)
                            {
                                var EmpResponse = res.Content.ReadAsStringAsync().Result;
                                buf2 = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication5.Models.workermodel>(EmpResponse);
                            }
                            else
                            {
                                if (flag)
                                    if (deleteregion(regID).Result < 0)
                                    {
                                        return Content((HttpStatusCode)418, "Global system error. Sorry.");
                                    }
                                if (deletecompany(buf).Result < 0)
                                {
                                    return Content((HttpStatusCode)418, "Global system error. Sorry.");
                                }
                                return Content(res.StatusCode, "Dab DATA. AAAAA.");
                            }
                        }
                    }
                }
                else
                {
                    buf2 = buf2_t;
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request POST http://localhost:2051/api/DB . Error message: {0}", ex.Message);
                if (flag)
                    if (deleteregion(regID).Result < 0)
                    {
                        return Content((HttpStatusCode)418, "Global system error. Sorry.");
                    }
                if (deletecompany(buf).Result < 0)
                {
                    return Content((HttpStatusCode)418, "Global system error. Sorry.");
                }
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            logger.Info("Success compliete request POST from {4} with parametrs 'CEO'= {0}, 'Name'= {1}, 'Cost'= {2}, 'region'= {3}", baseinf.CEO, baseinf.Name, baseinf.Cost, baseinf.region, ip);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // PUT: api/Gate/5

        [Route("~companies/edit/{id:int=1}")]
        public async Task<IHttpActionResult> Put([FromUri] int id, [FromBody]companiesmodel value)
        {
#if(DEBUG==true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            await Task.Run(() => putdata("PUT COMPANIES", request_type.CHANGE));
            logger.Info("Request PUT from {4} with parametrs 'ID'= {0}, 'CEO'= {1}, 'Name'= {2}, 'region'= {3}", id, value.CEO, value.Name, value.region, ip);
            if ((value.CEO == null) || (value.Name == null) || (value.region == 0))
            {
                logger.Warn("ABORTED PUT from {4} with parametrs 'ID'= {0}, 'CEO'= {1}, 'Name'= {2}, 'region'= {3}. Invalid parametrs.", id, value.CEO, value.Name, value.region, ip);
                return Content(HttpStatusCode.BadRequest, "Bad DATA AAAAAAAA.");
            }

            try
            {
                using (HttpClient test = new HttpClient())
                {
                    int suka = -1;
                    while (suka != 0)
                    {
                        test.DefaultRequestHeaders.Clear();
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[1]);
                        HttpResponseMessage res = await test.PutAsJsonAsync("http://localhost:29443/api/DB/" + id.ToString(), value);
                        if (res.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            gettokens();
                        }
                        else
                        {
                            suka = 0;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request PUT http://localhost:29443/api/DB + {4}. Parametrs 'ID'= {0}, 'CEO'= {1}, 'Name'= {2}, 'region'= {3}. Error message: {5}", id, value.CEO, value.Name, value.region, id.ToString(), ex.Message);
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            logger.Info("Success compliete PUT from {4}. Parametrs 'ID'= {0}, 'CEO'= {1}, 'Name'= {2}, 'region'= {3}.", id, value.CEO, value.Name, value.region, ip);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/Gate/5
        [Route("~companies/delete/{companyname:maxlength(32)=_}")]
        public async Task<IHttpActionResult> Delete([FromUri] string companyname)
        {
            
#if(DEBUG==true)
            int ip = 0;
#else
            var ip = Request.GetOwinContext().Request.RemoteIpAddress;
#endif
            using (HttpClient test = new HttpClient())
            {
                test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (Request.Headers.Authorization.Scheme != "Bearer")
                {
                    await  Task.Run(() => putdata("TOKEN NOT BEARER", request_type.LOGIN));
                    return Content(HttpStatusCode.Unauthorized, "Bad Authorization.Scheme. Go away, Niger.");
                }
                var sss = Request.Headers.Authorization.Parameter;
                authcheckmoels aaaa = new authcheckmoels();
                aaaa.token = sss;
                aaaa.requedrole = "ADMIN";

                HttpResponseMessage res = await test.PostAsJsonAsync("http://localhost:17767/oauth/check", aaaa);
                if(res.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await Task.Run(() => putdata("UNAUTHORIZED TRY ACCESS", request_type.LOGIN));
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    string asd = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse); 
                    return Content(HttpStatusCode.Unauthorized, asd);
                }
            }
            await  Task.Run(() => putdata("ACCESS GRANTED", request_type.LOGIN));
            await Task.Run(() => putdata("DELETE COMPANIE", request_type.CHANGE));

            logger.Info("Request DELETE from {1} with parametr 'Name'= {0}", companyname, ip);
            var queue = new MessageQueue(@".\private$\MyNewPrivateQueue");
            string requeststr = "";
            if (!(companyname == "_"))
            {
                requeststr = requeststr + "?$filter=Name eq '" + companyname + "'";
            }
            else
            {
                logger.Info("Request DELETE from {1} with parametr 'Name'= {0} CANCELED by fitler. Bad parametr value.", companyname, ip);
                return Content(HttpStatusCode.BadRequest, "Bad DATA AAAAAAAA.");
            }

            List<WebApplication5.Models.workermodel> WorkInfo = new List<WebApplication5.Models.workermodel>();
            WebApplication5.Models.companiesmodel CompInfo = new WebApplication5.Models.companiesmodel();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    int suka = -1;
                    while (suka != 0)
                    {
                        test.DefaultRequestHeaders.Clear();
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[1]);
                        HttpResponseMessage res = await test.GetAsync("http://localhost:29443/odata/CompInf" + requeststr);

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('[') + 1);
                            EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']'), 2);
                            CompInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApplication5.Models.companiesmodel>(EmpResponse);
                        }
                        if (res.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            gettokens();
                        }
                        else
                        {
                            suka = 0;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request GET http://localhost:29443/odata/CompInf + {0}. Error message: {1}", requeststr, ex.Message);
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            if (CompInfo == null)
            {
                logger.Warn("Aborted DELETE from {1} with parametr 'Name'= {0}. No such company.", companyname, ip);
                return Content(HttpStatusCode.Conflict, "Company in invalid");
            }

            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.BaseAddress = new Uri("http://localhost:29443/");
                    int suka = -1;
                    while (suka != 0)
                    {
                        test.DefaultRequestHeaders.Clear();
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[1]);
                        HttpResponseMessage res = await test.DeleteAsync("api/DB/" + CompInfo.Id);
                        if (res.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            gettokens();
                        }
                        else
                        {
                            suka = 0;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request DELETE http://localhost:29443/api/DB/ . Error message: {1}", requeststr, ex.Message);
                queue.Send("http://localhost:29443/api/DB/" + CompInfo.Id);
                return StatusCode(HttpStatusCode.OK);
            }

            try
            {
                using (HttpClient test = new HttpClient())
                {
                    int suka = -1;
                    while (suka != 0)
                    {
                        test.DefaultRequestHeaders.Clear();
                        test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[0]);
                        HttpResponseMessage res = await test.GetAsync("http://localhost:2051/odata/CompInf?$filter=Company eq " + CompInfo.Id.ToString());

                        if (res.IsSuccessStatusCode)
                        {
                            var EmpResponse = res.Content.ReadAsStringAsync().Result;
                            EmpResponse = EmpResponse.Remove(0, EmpResponse.IndexOf('['));
                            EmpResponse = EmpResponse.Remove(EmpResponse.IndexOf(']') + 1, 1);
                            WorkInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WebApplication5.Models.workermodel>>(EmpResponse);
                        }
                        if (res.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            gettokens();
                        }
                        else
                        {
                            suka = 0;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request GET http://localhost:2051/odata/CompInf?$filter=Company eq  + {0}. Error message: {1}", CompInfo.Id, ex.Message);
                return Content(HttpStatusCode.BadGateway, "Error in system. Sorry.");
            }

            try
            {
                foreach (var t in WorkInfo)
                    using (HttpClient test = new HttpClient())
                    {
                        int suka = -1;
                        while (suka != 0)
                        {
                            test.DefaultRequestHeaders.Clear();
                            test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            test.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens[0]);
                            HttpResponseMessage res = await test.DeleteAsync("http://localhost:2051/api/DB/" + t.Id);
                            if (res.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                gettokens();
                            }
                            else
                            {
                                suka = 0;
                            }
                        }
                    }
            }
            catch (HttpRequestException ex)
            {
                logger.Error("Error with request DELETE http://localhost:2051/api/DB/. Error message: {0}",ex.Message);
                foreach (var t in WorkInfo)
                    queue.Send("http://localhost:2051/api/DB/" + t.Id.ToString());
                return StatusCode(HttpStatusCode.OK);
            }

            logger.Info("Success compliete DELETE from {1} with parametr 'Name'= {0}", companyname, ip);
            return Ok(CompInfo);
        }

        [Route("code")]
        public async Task<IHttpActionResult> Postcode([FromBody] authcodemoels AAAA)
        {
            await  Task.Run(() => putdata("TOKEN REQUEST", request_type.LOGIN));
            tokenmessage code = new tokenmessage();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.PostAsJsonAsync(new Uri("http://localhost:17767/oauth/gettokens"), AAAA);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        code = Newtonsoft.Json.JsonConvert.DeserializeObject<tokenmessage>(EmpResponse);
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }
            }
            catch
            {
                return InternalServerError();
            }


            return Ok<tokenmessage>(code);                
        }

        [Route("refresh")]
        public async Task<IHttpActionResult> Postrefresh([FromBody] MyModel AAAAAAAAAA)
        {
            await  Task.Run(() => putdata("REFRESH TOKEN REQUEST", request_type.LOGIN));
            tokenmessage code = new tokenmessage();
            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.PostAsJsonAsync(new Uri("http://localhost:17767/oauth/refresh"), AAAAAAAAAA);

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        code = Newtonsoft.Json.JsonConvert.DeserializeObject<tokenmessage>(EmpResponse);
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }
            }
            catch
            {
                return InternalServerError();
            }


            return Ok<tokenmessage>(code);
        }

        [Route("GETFUCKINGSTATISTIC")]
         public async Task<IHttpActionResult> Getstat()
        {

            using (HttpClient test = new HttpClient())
            {
                test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (Request.Headers.Authorization.Scheme != "Bearer")
                {
                    await Task.Run(() => putdata("TOKEN NOT BEARER", request_type.LOGIN));
                    return Content(HttpStatusCode.Unauthorized, "Bad Authorization.Scheme. Go away, Niger.");
                }
                var sss = Request.Headers.Authorization.Parameter;
                authcheckmoels aaaa = new authcheckmoels();
                aaaa.token = sss;
                aaaa.requedrole = "ADMIN";

                HttpResponseMessage res = await test.PostAsJsonAsync("http://localhost:17767/oauth/check", aaaa);
                if (res.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await Task.Run(() => putdata("UNAUTHORIZED TRY ACCESS", request_type.LOGIN));
                    var EmpResponse = res.Content.ReadAsStringAsync().Result;
                    string asd = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(EmpResponse);
                    return Content(HttpStatusCode.Unauthorized, asd);
                }
            }
            await Task.Run(() => putdata("ACCESS GRANTED", request_type.LOGIN));

            statinf ANUS = new statinf();

            try
            {
                using (HttpClient test = new HttpClient())
                {
                    test.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = await test.GetAsync(new Uri("http://localhost:2100/stat/all"));

                    if (res.IsSuccessStatusCode)
                    {
                        var EmpResponse = res.Content.ReadAsStringAsync().Result;
                        ANUS = Newtonsoft.Json.JsonConvert.DeserializeObject<statinf>(EmpResponse);
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }
            }
            catch
            {
                return InternalServerError();
            }

            return Ok<statinf>(ANUS);
        }
    }
}
