using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using WebApplication3.Models;
using System.Web.OData;
using NLog;
using System.Messaging;

namespace WebApplication3.Controllers
{

    public class CompInfController : ODataController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private DBContext db = new DBContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [Authorize]
        [EnableQuery]
        public IQueryable<companies> Get()
        {
            logger.Info("Request GET with OData");
            return db.companies.AsQueryable();
        }

        [EnableQuery]
        public SingleResult<companies> Get([FromODataUri] int key)
        {
            logger.Info("Request GET with OData with ID = {0}", key);
            IQueryable<companies> result = db.companies.Where(p => p.Id == key);
            return SingleResult.Create(result);
        }
    }

    public class DBController : ApiController
    {
        private ICompaniesContext db = new DBContext();
        private FUUUUUUKContext db3 = new FUUUUUUKContext();
        // add these contructors
        public DBController()
        {
            Task.Run(() => sendstat());
        }

        private async void recconf(FUUUUUUKContext db3)
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
                    if (m.Label == "ANCOMP")
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
                            var eee = db3.instatmes.Find(m.mes.Id);
                            if (eee != null)
                            {
                                db3.instatmes.Remove(eee);
                                try
                                {
                                    await db3.SaveChangesAsync();
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    ex.Entries.Single().Reload();
                                }
                            }
                            break;
                        case -1:
                            var eee1 = db3.instatmes.Find(m.mes.Id);
                            if (eee1 != null)
                            {
                                logger.Error("ERR in Stat. Deleted data from db. Mess:" + m.Error);
                                db3.instatmes.Remove(eee1);
                                try
                                {
                                    await db3.SaveChangesAsync();
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

                await db3.SaveChangesAsync();
                return;
            }
        }

        private async void sendstat()
        {
            FUUUUUUKContext db3 = new FUUUUUUKContext();
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

                    await db3.SaveChangesAsync();
                    List<instatmes> sss = new List<instatmes>();
                    try
                    {
                        await Task.Run(() => recconf(db3));

                        TimeSpan interval = new TimeSpan(0, 2, 30);
                        System.Threading.Thread.Sleep(interval);

                        sss = db3.instatmes.ToList();
                        foreach (var qwe in sss)
                        {
                            if (qwe.Np < 3)
                            {
                                qwe.Np++;
                                queue.Send(qwe);
                                try
                                {
                                    db3.Entry(qwe).State = EntityState.Modified;
                                    db3.SaveChanges();
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
            FUUUUUUKContext db3 = new FUUUUUUKContext();
            instatmes m = new instatmes();
            m.detail = inf;
            m.request_type = ttt;
            m.server_name = server_name.COMP;
            m.Time = DateTime.Now;
            m.state = Guid.NewGuid();

            try
            {
                db3.instatmes.Add(m);
                await db3.SaveChangesAsync();
            }
            catch
            {
                return;
            }

        }

        public DBController(ICompaniesContext context)
        {
            db = context;
        }
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // GET api/DB
        [Authorize]
        public IQueryable<companies> Getcompanies()
        {
            logger.Info("Request GET");
            return db.companies;
        }

        // GET api/DB/5
        [Authorize]
        [ResponseType(typeof(companies))]
        public async Task<IHttpActionResult> Getcompanies(int id)
        {
            logger.Info("Request GET with ID = {0}", id);
            companies companies = await db.companies.FindAsync(id);
            if (companies == null)
            {
                logger.Error("ERROR request PUT with ID = {0}. Not found ID", id);
                return NotFound();
            }
            logger.Info("Success request PUT with ID = {0}", id);
            return Ok(companies);
        }

        // PUT api/DB/5
        [Authorize]
        public async Task<IHttpActionResult> Putcompanies(int id, companies companies)
        {
            await Task.Run(() => putdata("PUT COMPANIE", request_type.CHANGE));
            logger.Info("Request PUT with ID = {0} Name = {1} CEO = {2} region = {3}", id, companies.Name, companies.CEO, companies.region);
            if (!ModelState.IsValid)
            {
                logger.Warn("ABORTED request PUT with ID = {0} Name = {1} CEO = {2} region = {3}. Bad model.", id, companies.Name, companies.CEO, companies.region);
                return BadRequest(ModelState);
            }

            if (id != companies.Id)
            {
                logger.Warn("ABORTED request PUT with ID = {0} Name = {1} CEO = {2} region = {3}. Bad ID and data.ID.", id, companies.Name, companies.CEO, companies.region);
                return BadRequest();
            }

            //db.Entry(companies).State = EntityState.Modified;
            db.MarkAsModified(companies);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!companiesExists(id))
                {
                    logger.Error("ERROR request PUT with ID = {0} Name = {1} CEO = {2} region = {3}. Not found ID", id, companies.Name, companies.CEO, companies.region);
                    return NotFound();
                }
                else
                {
                    logger.Error("ERROR request PUT with ID = {0} Name = {1} CEO = {2} region = {3}. BD error.", id, companies.Name, companies.CEO, companies.region);
                    return StatusCode(HttpStatusCode.InternalServerError);
                }
            }
            logger.Info("Success request PUT with ID = {0} Name = {1} CEO = {2} region = {3}", id, companies.Name, companies.CEO, companies.region);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/DB
        [Authorize]
        [ResponseType(typeof(companies))]
        public async Task<IHttpActionResult> Postcompanies(companies companies)
        {
            await Task.Run(() => putdata("POST COMPANIE", request_type.CHANGE));
            logger.Info("Request POST with Name = {0} CEO = {1} region = {2}", companies.Name, companies.CEO, companies.region);
            if (!ModelState.IsValid)
            {
                logger.Warn("ABORTED request PUT with Name = {0} CEO = {1} region = {2}. Bad model.", companies.Name, companies.CEO, companies.region);
                return BadRequest(ModelState);
            }

            db.companies.Add(companies);
            await db.SaveChangesAsync();

            logger.Info("Success request PUT with Name = {0} CEO = {1} region = {2}", companies.Name, companies.CEO, companies.region);
            return CreatedAtRoute("DefaultApi", new { id = companies.Id }, companies);
        }

        // DELETE api/DB/5
        [Authorize]
        [ResponseType(typeof(companies))]
        public async Task<IHttpActionResult> Deletecompanies(int id)
        {
            await Task.Run(() => putdata("DELETE COMPANIE", request_type.CHANGE));
            logger.Info("Request DELETE with ID = {0}", id);
            companies companies = await db.companies.FindAsync(id);
            if (companies == null)
            {
                logger.Error("ERROR request DELETE with ID = {0}. Not found ID", id);
                return NotFound();
            }

            db.companies.Remove(companies);
            await db.SaveChangesAsync();

            logger.Info("Success request DELETE with ID = {0}", id);
            return Ok(companies);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool companiesExists(int id)
        {
            return db.companies.Count(e => e.Id == id) > 0;
        }
    }
}