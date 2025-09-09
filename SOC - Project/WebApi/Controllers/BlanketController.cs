using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using WebApi.Models;

namespace WebApi.Controllers
{
    public class BlanketController : ApiController
    {
        private DBModel db = new DBModel();

        // GET: api/Blanket
        public IQueryable<Blanket> GetBlankets()
        {
            return db.Blankets;
        }

        // GET: api/Blanket/5
        [ResponseType(typeof(Blanket))]
        public IHttpActionResult GetBlanket(int id)
        {
            Blanket blanket = db.Blankets.Find(id);
            if (blanket == null)
            {
                return NotFound();
            }

            return Ok(blanket);
        }

        // PUT: api/Blanket/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutBlanket(int id, Blanket blanket)
        {

            if (id != blanket.BlanketId)
            {
                return BadRequest();
            }

            db.Entry(blanket).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlanketExists(id))
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

        // POST: api/Blanket
        [ResponseType(typeof(Blanket))]
        public IHttpActionResult PostBlanket(Blanket blanket)
        {
            db.Blankets.Add(blanket);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = blanket.BlanketId }, blanket);
        }

        // DELETE: api/Blanket/5
        [ResponseType(typeof(Blanket))]
        public IHttpActionResult DeleteBlanket(int id)
        {
            Blanket blanket = db.Blankets.Find(id);
            if (blanket == null)
            {
                return NotFound();
            }

            db.Blankets.Remove(blanket);
            db.SaveChanges();

            return Ok(blanket);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BlanketExists(int id)
        {
            return db.Blankets.Count(e => e.BlanketId == id) > 0;
        }
    }
}