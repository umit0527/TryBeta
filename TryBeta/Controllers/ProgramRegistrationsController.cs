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
    public class ProgramRegistrationsController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();

        // GET: api/ProgramRegistrations
        public IQueryable<ProgramRegistration> GetProgramRegistrations()
        {
            return db.ProgramRegistrations;
        }

        // GET: api/ProgramRegistrations/5
        [ResponseType(typeof(ProgramRegistration))]
        public IHttpActionResult GetProgramRegistration(int id)
        {
            ProgramRegistration programRegistration = db.ProgramRegistrations.Find(id);
            if (programRegistration == null)
            {
                return NotFound();
            }

            return Ok(programRegistration);
        }

        // PUT: api/ProgramRegistrations/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutProgramRegistration(int id, ProgramRegistration programRegistration)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != programRegistration.Id)
            {
                return BadRequest();
            }

            db.Entry(programRegistration).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProgramRegistrationExists(id))
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

        // POST: api/ProgramRegistrations
        [HttpPost]
        [Route("programs/{programId}/register")]
        [JwtAuthFilter]
        public IHttpActionResult RegisterProgram(int programId)
        {
            try
            {
                // 1. 取得使用者 ID
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                    return Unauthorized();
                int userId = (int)userIdObj;

                // 2. 取得活動
                var program = db.ProgramPlan.Find(programId);
                if (program == null)
                    return NotFound();

                // 3. 計算目前報名人數
                var currentCount = db.ProgramRegistrations
                                     .Count(r => r.ProgramId == programId && r.Status == "registered");

                // 4. 檢查是否超過最大人數
                if (currentCount >= program.MaxPeople)
                    return BadRequest("此活動報名已達上限");

                // 5. 新增報名紀錄
                var registration = new ProgramRegistration
                {
                    ProgramId = programId,
                    UserId = userId,
                    Status = "registered"
                };
                db.ProgramRegistrations.Add(registration);
                db.SaveChanges();

                //// 6. 判斷是否成團
                //bool isConfirmed = currentCount + 1 >= program.MinPeople;
                //if (isConfirmed)
                //{
                //    program.Status = "Confirmed"; // 可選：更新成團狀態
                //    db.SaveChanges();
                //}

                return Ok(new
                {
                    message = "報名成功",
                    //isConfirmed
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/ProgramRegistrations/5
        [ResponseType(typeof(ProgramRegistration))]
        public IHttpActionResult DeleteProgramRegistration(int id)
        {
            ProgramRegistration programRegistration = db.ProgramRegistrations.Find(id);
            if (programRegistration == null)
            {
                return NotFound();
            }

            db.ProgramRegistrations.Remove(programRegistration);
            db.SaveChanges();

            return Ok(programRegistration);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProgramRegistrationExists(int id)
        {
            return db.ProgramRegistrations.Count(e => e.Id == id) > 0;
        }
    }
}