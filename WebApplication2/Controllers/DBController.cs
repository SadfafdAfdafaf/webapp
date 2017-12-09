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
        public DBController() { }

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