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

        // add these contructors
        public DBController() { }

        public DBController(ICompaniesContext context)
        {
            db = context;
        }
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // GET api/DB
        public IQueryable<companies> Getcompanies()
        {
            logger.Info("Request GET");
            return db.companies;
        }

        // GET api/DB/5
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
        public async Task<IHttpActionResult> Putcompanies(int id, companies companies)
        {
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
        [ResponseType(typeof(companies))]
        public async Task<IHttpActionResult> Postcompanies(companies companies)
        {
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
        [ResponseType(typeof(companies))]
        public async Task<IHttpActionResult> Deletecompanies(int id)
        {
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