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
using TryBeta.Models;

namespace TryBeta.Controllers
{
    [RoutePrefix("api/v1/users/")]
    public class PublicProgramPlansController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();

        // GET: api/PublicProgramPlans
        public IQueryable<ProgramPlan> GetProgramPlan()
        {
            return db.ProgramPlan;
        }

        // GET: api/PublicProgramPlans/5
        //[HttpGet]
        //[Route("")]
        //public IHttpActionResult GetPublicPrograms(
        //string search = null,
        //int? industry = null,
        //int? jobtitle = null,
        //string sort = "newest",
        //int page = 1,
        //int limit = 21)
        //{
        //    try
        //    {
        //        var now = DateTime.Now;

        //        // 查詢單一計畫
        //        var plan = db.ProgramPlans.FirstOrDefault(p => p.Id == programId);

        //        if (plan == null)
        //        {
        //            return NotFound();
        //        }

        //        // 已申請人數 (假設有 ProgramApplications 表紀錄申請)
        //        var appliedCount = db.ProgramApplications.Count(a => a.ProgramPlanId == plan.Id);

        //        // 建立 DTO
        //        var dto = new ProgramPlanDto
        //        {
        //            Name = plan.Name,
        //            Intro = plan.Intro,
        //            Address = plan.Address,
        //            ProgramStartDate = plan.ProgramStartDate,
        //            ProgramEndDate = plan.ProgramEndDate,
        //            AppliedCount = appliedCount
        //        };

        //        // 判斷顯示 DaysLeft 或 IsOngoing
        //        if (now < plan.ProgramStartDate)
        //        {
        //            dto.DaysLeft = (plan.PublishEndDate - now).Days;
        //            dto.IsOngoing = null;
        //        }
        //        else if (now >= plan.ProgramStartDate && now <= plan.ProgramEndDate)
        //        {
        //            dto.IsOngoing = true;
        //            dto.DaysLeft = null;
        //        }
        //        else
        //        {
        //            dto.IsOngoing = false;
        //            dto.DaysLeft = null;
        //        }

        //        return Ok(dto);
        //    }
        //    catch (Exception ex)
        //    {
        //        return InternalServerError(ex);
        //    }
        //}

        // PUT: api/PublicProgramPlans/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutProgramPlan(int id, ProgramPlan programPlan)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != programPlan.Id)
            {
                return BadRequest();
            }

            db.Entry(programPlan).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProgramPlanExists(id))
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

        // POST: api/PublicProgramPlans
        [ResponseType(typeof(ProgramPlan))]
        public IHttpActionResult PostProgramPlan(ProgramPlan programPlan)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ProgramPlan.Add(programPlan);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = programPlan.Id }, programPlan);
        }

        // DELETE: api/PublicProgramPlans/5
        [ResponseType(typeof(ProgramPlan))]
        public IHttpActionResult DeleteProgramPlan(int id)
        {
            ProgramPlan programPlan = db.ProgramPlan.Find(id);
            if (programPlan == null)
            {
                return NotFound();
            }

            db.ProgramPlan.Remove(programPlan);
            db.SaveChanges();

            return Ok(programPlan);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProgramPlanExists(int id)
        {
            return db.ProgramPlan.Count(e => e.Id == id) > 0;
        }
    }
}