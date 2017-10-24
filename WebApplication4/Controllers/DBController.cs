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

namespace WebApplication4.Controllers
{
    public class DBController : ApiController
    {
        private personalContext db = new personalContext();

        // GET api/DB
        public IQueryable<personalinf> Getpersonalinfs()
        {
            return db.personalinfs;
        }

        // GET api/DB/5
        [ResponseType(typeof(personalinf))]
        public async Task<IHttpActionResult> Getpersonalinf(int id)
        {
            personalinf personalinf = await db.personalinfs.FindAsync(id);
            if (personalinf == null)
            {
                return NotFound();
            }

            return Ok(personalinf);
        }

        // PUT api/DB/5
        public async Task<IHttpActionResult> Putpersonalinf(int id, personalinf personalinf)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != personalinf.Id)
            {
                return BadRequest();
            }

            db.Entry(personalinf).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!personalinfExists(id))
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
        [ResponseType(typeof(personalinf))]
        public async Task<IHttpActionResult> Postpersonalinf(personalinf personalinf)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.personalinfs.Add(personalinf);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = personalinf.Id }, personalinf);
        }

        // DELETE api/DB/5
        [ResponseType(typeof(personalinf))]
        public async Task<IHttpActionResult> Deletepersonalinf(int id)
        {
            personalinf personalinf = await db.personalinfs.FindAsync(id);
            if (personalinf == null)
            {
                return NotFound();
            }

            db.personalinfs.Remove(personalinf);
            await db.SaveChangesAsync();

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