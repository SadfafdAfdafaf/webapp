using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using WebApplication2.Models;
using System.Web.OData;
using NLog;
using System.Messaging;

namespace WebApplication2.Controllers
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

        [EnableQuery]
        [Authorize]
        public IQueryable<Worker> Get()
        {
            logger.Info("Request GET with OData");
            return db.Workers.AsQueryable();
        }

        [EnableQuery]
        public SingleResult<Worker> Get([FromODataUri] int key)
        {
            logger.Info("Request GET with OData with ID = {0}", key);
            IQueryable<Worker> result = db.Workers.Where(p => p.Id == key);
            return SingleResult.Create(result);
        }
    }

    public class DBController : ApiController
    {
        private IWorkerContext db = new DBContext();        

        // add these contructors
        public DBController()
        {
            Task.Run(() => sendstat());
        }

        private async void recconf(Fuck2Context db4)
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
                    if (m.Label == "ANPERS")
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
                            var eee = db4.instatmes.Find(m.mes.Id);
                            if (eee != null)
                            {
                                db4.instatmes.Remove(eee);
                                try
                                {
                                    await db4.SaveChangesAsync();
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    ex.Entries.Single().Reload();
                                }
                            }
                            break;
                        case -1:
                            var eee1 = db4.instatmes.Find(m.mes.Id);
                            if (eee1 != null)
                            {
                                logger.Error("ERR in Stat. Deleted data from db. Mess:" + m.Error);
                                db4.instatmes.Remove(eee1);
                                try
                                {
                                    await db4.SaveChangesAsync();
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

                await db4.SaveChangesAsync();
                return;
            }
        }

        private async void sendstat()
        {
            Fuck2Context db4 = new Fuck2Context();
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
                    await db4.SaveChangesAsync();
                    List<instatmes> sss = new List<instatmes>();
                    try
                    {
                        await Task.Run(() => recconf(db4));

                        TimeSpan interval = new TimeSpan(0, 2, 30);
                        System.Threading.Thread.Sleep(interval);

                        sss = db4.instatmes.ToList();
                        foreach (var qwe in sss)
                        {
                            if (qwe.Np < 3)
                            {
                                qwe.Np++;
                                queue.Send(qwe);
                                try
                                {
                                    db4.Entry(qwe).State = EntityState.Modified;
                                    db4.SaveChanges();
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
            Fuck2Context db4 = new Fuck2Context();
            instatmes m = new instatmes();
            m.detail = inf;
            m.request_type = ttt;
            m.server_name = server_name.PERS;
            m.Time = DateTime.Now;
            m.state = Guid.NewGuid();

            try
            {
                db4.instatmes.Add(m);
                await db4.SaveChangesAsync();
            }
            catch
            {
                return;
            }

        }

        public DBController(IWorkerContext context)
        {
            db = context;
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        // GET api/DB
        [Authorize]
        public IQueryable<Worker> GetWorkers()
        {
            logger.Info("Request GET");
            return db.Workers;
        }

        // GET api/DB/5
        [Authorize]
        [ResponseType(typeof(Worker))]
        public async Task<IHttpActionResult> GetWorker(int id)
        {
            logger.Info("Request GET with ID = {0}", id);
            Worker worker = await db.Workers.FindAsync(id);
            if (worker == null)
            {
                logger.Error("ERROR request PUT with ID = {0}. Not found ID", id);
                return NotFound();
            }

            logger.Info("Success request PUT with ID = {0}", id);
            return Ok(worker);
        }

        // PUT api/DB/5
        [Authorize]
        public async Task<IHttpActionResult> PutWorker(int id, Worker worker)
        {
            await Task.Run(() => putdata("PUT WORKER", request_type.CHANGE));
            logger.Info("Request PUT with ID = {0} FIO = {1} Company = {2} Cost = {3} RegionOffice = {4}", id, worker.FIO, worker.Company, worker.Cost, worker.RegionOffice);
            if (!ModelState.IsValid)
            {
                logger.Warn("ABORTED request PUT with ID = {0} FIO = {1} Company = {2} Cost = {3} RegionOffice = {4}. Bad model.", id, worker.FIO, worker.Company, worker.Cost, worker.RegionOffice);
                return BadRequest(ModelState);
            }

            if (id != worker.Id)
            {
                logger.Warn("ABORTED request PUT with ID = {0} FIO = {1} Company = {2} Cost = {3} RegionOffice = {4}. Bad ID and data.ID.", id, worker.FIO, worker.Company, worker.Cost, worker.RegionOffice);
                return BadRequest();
            }

            //db.Entry(worker).State = EntityState.Modified;
            db.MarkAsModified(worker);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WorkerExists(id))
                {
                    logger.Error("ERROR request PUT with ID = {0} FIO = {1} Company = {2} Cost = {3} RegionOffice = {4}. Not found ID", id, worker.FIO, worker.Company, worker.Cost, worker.RegionOffice);
                    return NotFound();
                }
                else
                {
                    logger.Error("ERROR request PUT with ID = {0} FIO = {1} Company = {2} Cost = {3} RegionOffice = {4}. BD error.", id, worker.FIO, worker.Company, worker.Cost, worker.RegionOffice);
                    return StatusCode(HttpStatusCode.InternalServerError);
                }
            }

            logger.Info("Success request PUT with ID = {0} FIO = {1} Company = {2} Cost = {3} RegionOffice = {4}", id, worker.FIO, worker.Company, worker.Cost, worker.RegionOffice);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/DB
        [Authorize]
        [ResponseType(typeof(Worker))]
        public async Task<IHttpActionResult> PostWorker(Worker worker)
        {
            await Task.Run(() => putdata("POST WORKER", request_type.CHANGE));
            logger.Info("Request POST with FIO = {0} Company = {1} Cost = {2} RegionOffice = {3}", worker.FIO, worker.Company, worker.Cost, worker.RegionOffice);
            if (!ModelState.IsValid)
            {
                logger.Warn("ABORTED request PUT with FIO = {0} Company = {1} Cost = {2} RegionOffice = {3}. Bad model.", worker.FIO, worker.Company, worker.Cost, worker.RegionOffice);
                return BadRequest(ModelState);
            }

            db.Workers.Add(worker);
            await db.SaveChangesAsync();
            

            logger.Info("Success request PUT with FIO = {0} Company = {1} Cost = {2} RegionOffice = {3}", worker.FIO, worker.Company, worker.Cost, worker.RegionOffice);
            return CreatedAtRoute("DefaultApi", new { id = worker.Id }, worker);
        }

        // DELETE api/DB/5
        [Authorize]
        [ResponseType(typeof(Worker))]
        public async Task<IHttpActionResult> DeleteWorker(int id)
        {
            await Task.Run(() => putdata("DELETE WORKER", request_type.CHANGE));
            logger.Info("Request DELETE with ID = {0}", id);
            Worker worker = await db.Workers.FindAsync(id);
            if (worker == null)
            {
                logger.Error("ERROR request DELETE with ID = {0}. Not found ID", id);
                return NotFound();
            }

            db.Workers.Remove(worker);
            await db.SaveChangesAsync();

            logger.Info("Success request DELETE with ID = {0}", id);
            return Ok(worker);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool WorkerExists(int id)
        {
            return db.Workers.Count(e => e.Id == id) > 0;
        }

    }
}