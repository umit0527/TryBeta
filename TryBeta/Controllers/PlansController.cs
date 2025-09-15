using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Web.Http;
using System.Web.Http.Description;
using TryBeta.Models;

namespace TryBeta.Controllers
{
    [RoutePrefix("api/v1/plans")]
    public class PlansController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();

        // GET: api/Plans
        public IQueryable<Plan> GetPlan()
        {
            return db.Plan;
        }
        // GET: api/v1/plan
        [HttpGet]
        [Route("")]
        [ResponseType(typeof(IEnumerable<Plan>))]
        [JwtAuthFilter]
        public IHttpActionResult GetPlans()
        {

            var plans = db.Plan.ToList();
            if (plans == null)
            {
                return NotFound();
            }
            return Ok(plans);
        }

        // GET: api/Plans/5
        [ResponseType(typeof(Plan))]
        public IHttpActionResult GetPlan(int id)
        {
            Plan plan = db.Plan.Find(id);
            if (plan == null)
            {
                return NotFound();
            }

            return Ok(plan);
        }

        // PUT: api/Plans/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutPlan(int id, Plan plan)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != plan.Id)
            {
                return BadRequest();
            }

            db.Entry(plan).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlanExists(id))
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

        // POST: api/Plans
        [ResponseType(typeof(Plan))]
        public IHttpActionResult PostPlan(Plan plan)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Plan.Add(plan);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = plan.Id }, plan);
        }

        // DELETE: api/Plans/5
        [ResponseType(typeof(Plan))]
        public IHttpActionResult DeletePlan(int id)
        {
            Plan plan = db.Plan.Find(id);
            if (plan == null)
            {
                return NotFound();
            }

            db.Plan.Remove(plan);
            db.SaveChanges();

            return Ok(plan);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PlanExists(int id)
        {
            return db.Plan.Count(e => e.Id == id) > 0;
        }
    }
}