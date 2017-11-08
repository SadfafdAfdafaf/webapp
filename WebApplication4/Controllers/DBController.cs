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

        // add these contructors
        public DBController() { }

        public DBController(IRegionContext context)
        {
            db = context;
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        // GET api/DB
        public IQueryable<personalinf> Getpersonalinfs()
        {
            logger.Info("Request GET");
            return db.personalinfs;
        }

        // GET api/DB/5
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
        public async Task<IHttpActionResult> Putpersonalinf(int id, personalinf personalinf)
        {
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
        [ResponseType(typeof(personalinf))]
        public async Task<IHttpActionResult> Postpersonalinf(personalinf personalinf)
        {
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
        [ResponseType(typeof(personalinf))]
        public async Task<IHttpActionResult> Deletepersonalinf(int id)
        {
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