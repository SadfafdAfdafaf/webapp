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

namespace WebApplication3.Controllers
{
    public class DBController : ApiController
    {
        private CompaniesContext db = new CompaniesContext();

        // GET api/DB
        [Queryable]
        public IQueryable<companies> Getcompanies()
        {
            return db.companies;
        }

        // GET api/DB/5
        [ResponseType(typeof(companies))]
        public async Task<IHttpActionResult> Getcompanies(int id)
        {
            companies companies = await db.companies.FindAsync(id);
            if (companies == null)
            {
                return NotFound();
            }

            return Ok(companies);
        }

        // PUT api/DB/5
        public async Task<IHttpActionResult> Putcompanies(int id, companies companies)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != companies.Id)
            {
                return BadRequest();
            }

            db.Entry(companies).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!companiesExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/DB
        [ResponseType(typeof(companies))]
        public async Task<IHttpActionResult> Postcompanies(companies companies)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.companies.Add(companies);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = companies.Id }, companies);
        }

        // DELETE api/DB/5
        [ResponseType(typeof(companies))]
        public async Task<IHttpActionResult> Deletecompanies(int id)
        {
            companies companies = await db.companies.FindAsync(id);
            if (companies == null)
            {
                return NotFound();
            }

            db.companies.Remove(companies);
            await db.SaveChangesAsync();

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