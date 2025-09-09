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
    public class DistributorProductsController : ApiController
    {
        private DBModel db = new DBModel();

        // GET: api/DistributorProducts
        public IQueryable<BDistributorProduct> GetBDistributorProducts()
        {
            return db.BDistributorProducts;
        }

        // GET: api/DistributorProducts/5
        [ResponseType(typeof(BDistributorProduct))]
        public IHttpActionResult GetBDistributorProduct(int id)
        {
            BDistributorProduct bDistributorProduct = db.BDistributorProducts.Find(id);
            if (bDistributorProduct == null)
            {
                return NotFound();
            }

            return Ok(bDistributorProduct);
        }

        // PUT: api/DistributorProducts/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutBDistributorProduct(int id, BDistributorProduct bDistributorProduct)
        {

            if (id != bDistributorProduct.BlanketId)
            {
                return BadRequest();
            }

            db.Entry(bDistributorProduct).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BDistributorProductExists(id))
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

        // POST: api/DistributorProducts
        [ResponseType(typeof(BDistributorProduct))]
        public IHttpActionResult PostBDistributorProduct(BDistributorProduct bDistributorProduct)
        {
            db.BDistributorProducts.Add(bDistributorProduct);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = bDistributorProduct.BlanketId }, bDistributorProduct);
        }

        // DELETE: api/DistributorProducts/5
        [ResponseType(typeof(BDistributorProduct))]
        public IHttpActionResult DeleteBDistributorProduct(int id)
        {
            BDistributorProduct bDistributorProduct = db.BDistributorProducts.Find(id);
            if (bDistributorProduct == null)
            {
                return NotFound();
            }

            db.BDistributorProducts.Remove(bDistributorProduct);
            db.SaveChanges();

            return Ok(bDistributorProduct);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BDistributorProductExists(int id)
        {
            return db.BDistributorProducts.Count(e => e.BlanketId == id) > 0;
        }
    }
}