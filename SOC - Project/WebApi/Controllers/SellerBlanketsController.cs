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
    public class SellerBlanketsController : ApiController
    {
        private DBModel db = new DBModel();

        // GET: api/SellerBlankets
        public IQueryable<SellerBlanket> GetSellerBlankets()
        {
            return db.SellerBlankets;
        }

        // GET: api/SellerBlankets/5
        [ResponseType(typeof(SellerBlanket))]
        public IHttpActionResult GetSellerBlanket(int id)
        {
            SellerBlanket sellerBlanket = db.SellerBlankets.Find(id);
            if (sellerBlanket == null)
            {
                return NotFound();
            }

            return Ok(sellerBlanket);
        }

        // PUT: api/SellerBlankets/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutSellerBlanket(int id, SellerBlanket sellerBlanket)
        {

            if (id != sellerBlanket.BlanketId)
            {
                return BadRequest();
            }

            db.Entry(sellerBlanket).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SellerBlanketExists(id))
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

        // POST: api/SellerBlankets
        [ResponseType(typeof(SellerBlanket))]
        public IHttpActionResult PostSellerBlanket(SellerBlanket sellerBlanket)
        {

            db.SellerBlankets.Add(sellerBlanket);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = sellerBlanket.BlanketId }, sellerBlanket);
        }

        // DELETE: api/SellerBlankets/5
        [ResponseType(typeof(SellerBlanket))]
        public IHttpActionResult DeleteSellerBlanket(int id)
        {
            SellerBlanket sellerBlanket = db.SellerBlankets.Find(id);
            if (sellerBlanket == null)
            {
                return NotFound();
            }

            db.SellerBlankets.Remove(sellerBlanket);
            db.SaveChanges();

            return Ok(sellerBlanket);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool SellerBlanketExists(int id)
        {
            return db.SellerBlankets.Count(e => e.BlanketId == id) > 0;
        }
    }
}