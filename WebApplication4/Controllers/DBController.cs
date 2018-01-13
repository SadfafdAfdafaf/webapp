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
using WebApplication4.Models;
using System.Web.OData;
using NLog;
using System.Messaging;


namespace WebApplication4.Controllers
{
    public class CompInfController : ODataController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private PersonalContext db = new PersonalContext();

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [Authorize]
        [EnableQueryAttribute]
        public IQueryable<personalinf> Get()
        {
            logger.Info("Request GET with OData");
            return db.personalinfs.AsQueryable();
        }

        [EnableQuery]
        public SingleResult<personalinf> Get([FromODataUri] int key)
        {
            logger.Info("Request GET with OData with ID = {0}", key);
            IQueryable<personalinf> result = db.personalinfs.Where(p => p.Id == key);
            return SingleResult.Create(result);
        }
    }
    public class DBController : ApiController
    {
        private IRegionContext db = new PersonalContext();
        private FUCKContext db5 = new FUCKContext();

        // add these contructors
        public DBController()
        {
            Task.Run(() => sendstat());
        }

        private async void recconf(FUCKContext db5)
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
                    if (m.Label == "ANREG")
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
                            var eee = db5.instatmes.Find(m.mes.Id);
                            if (eee != null)
                            {
                                db5.instatmes.Remove(eee);
                                try
                                {
                                    await db5.SaveChangesAsync();
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    ex.Entries.Single().Reload();
                                }
                            }
                            break;
                        case -1:
                            var eee1 = db5.instatmes.Find(m.mes.Id);
                            if (eee1 != null)
                            {
                                logger.Error("ERR in Stat. Deleted data from db. Mess:" + m.Error);
                                db5.instatmes.Remove(eee1);
                                try
                                {
                                    await db5.SaveChangesAsync();
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

                await db5.SaveChangesAsync();
                return;
            }
        }

        private async void sendstat()
        {
            FUCKContext db5 = new FUCKContext();
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
                    await db5.SaveChangesAsync();
                    List<instatmes> sss = new List<instatmes>();
                    try
                    {
                        await Task.Run(() => recconf(db5));

                        TimeSpan interval = new TimeSpan(0, 2, 30);
                        System.Threading.Thread.Sleep(interval);

                        sss = db5.instatmes.ToList();
                        foreach (var qwe in sss)
                        {
                            if (qwe.Np < 3)
                            {
                                qwe.Np++;
                                queue.Send(qwe);
                                try
                                {
                                    db5.Entry(qwe).State = EntityState.Modified;
                                    db5.SaveChanges();
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
            FUCKContext db5 = new FUCKContext();
            instatmes m = new instatmes();
            m.detail = inf;
            m.request_type = ttt;
            m.server_name = server_name.REG;
            m.Time = DateTime.Now;
            m.state = Guid.NewGuid();

            try
            {
                db5.instatmes.Add(m);
                await db5.SaveChangesAsync();
            }
            catch
            {
                return;
            }

        }

        public DBController(IRegionContext context)
        {
            db = context;
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        // GET api/DB
        [Authorize]
        public IQueryable<personalinf> Getpersonalinfs()
        {
            logger.Info("Request GET");
            return db.personalinfs;
        }

        // GET api/DB/5
        [Authorize]
        [ResponseType(typeof(personalinf))]
        public async Task<IHttpActionResult> Getpersonalinf(int id)
        {
            logger.Info("Request GET with ID = {0}", id);
            personalinf personalinf = await db.personalinfs.FindAsync(id);
            if (personalinf == null)
            {
                logger.Error("ERROR request PUT with ID = {0}. Not found ID", id);
                return NotFound();
            }
            logger.Info("Success request PUT with ID = {0}", id);
            return Ok(personalinf);
        }

        // PUT api/DB/5
        [Authorize]
        public async Task<IHttpActionResult> Putpersonalinf(int id, personalinf personalinf)
        {
            await Task.Run(() => putdata("PUT REGION", request_type.CHANGE));
            logger.Info("Request PUT with ID = {0} claster = {1}", id, personalinf.claster);
            if (!ModelState.IsValid)
            {
                logger.Warn("ABORTED request PUT with ID = {0} claster = {1}. Bad model.", id, personalinf.claster);
                return BadRequest(ModelState);
            }

            if (id != personalinf.Id)
            {
                logger.Warn("ABORTED request PUT with ID = {0} claster = {1}. Bad ID and data.ID.", id, personalinf.claster);
                return BadRequest();
            }

            //db.Entry(personalinf).State = EntityState.Modified;
            db.MarkAsModified(personalinf);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {                
                if (!personalinfExists(id))
                {
                    logger.Error("ERROR request PUT with ID = {0} claster = {1}. Not found ID", id, personalinf.claster);
                    return NotFound();
                }
                else
                {
                    logger.Error("ERROR request PUT with ID = {0} claster = {1}. BD error.", id, personalinf.claster);
                    return StatusCode(HttpStatusCode.InternalServerError);
                }
            }
            logger.Info("Success request PUT with ID = {0} claster = {1}", id, personalinf.claster);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/DB
        [Authorize]
        [ResponseType(typeof(personalinf))]
        public async Task<IHttpActionResult> Postpersonalinf(personalinf personalinf)
        {
            await Task.Run(() => putdata("POST REGION", request_type.CHANGE));
            logger.Info("Request POST with claster = {0}", personalinf.claster);

            if (!ModelState.IsValid)
            {
                logger.Warn("ABORTED request PUT with claster = {0}. Bad model.", personalinf.claster);
                return BadRequest(ModelState);
            }

            db.personalinfs.Add(personalinf);
            await db.SaveChangesAsync();

            logger.Info("Success request PUT with claster = {0}", personalinf.claster);
            return CreatedAtRoute("DefaultApi", new { id = personalinf.Id }, personalinf);
        }

        // DELETE api/DB/5
        [Authorize]
        [ResponseType(typeof(personalinf))]
        public async Task<IHttpActionResult> Deletepersonalinf(int id)
        {
            await Task.Run(() => putdata("DELETE REGION", request_type.CHANGE));
            logger.Info("Request DELETE with ID = {0}", id);

            personalinf personalinf = await db.personalinfs.FindAsync(id);
            if (personalinf == null)
            {
                logger.Error("ERROR request DELETE with ID = {0}. Not found ID", id);
                return NotFound();
            }

            db.personalinfs.Remove(personalinf);
            await db.SaveChangesAsync();

            logger.Info("Success request DELETE with ID = {0}", id);
            return Ok(personalinf);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool personalinfExists(int id)
        {
            return db.personalinfs.Count(e => e.Id == id) > 0;
        }
    }
}