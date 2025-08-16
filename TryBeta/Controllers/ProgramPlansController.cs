using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using TryBeta.Models;

namespace TryBeta.Controllers
{
    [RoutePrefix("api/v1/company/{companyid:int}")]
    public class ProgramPlansController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();

        // GET: api/ProgramPlans
        //public IQueryable<ProgramPlans> GetProgramPlans()
        //{
        //    return db.ProgramPlans;
        //}

        // GET: api/ProgramPlans/5 取得體驗計畫(未審核即通過)
        [HttpGet]
        [Route("programs")]
        [JwtAuthFilter] // 必須登入
        public IHttpActionResult GetProgramPlans()
        {
            try
            {
                // 1. 取得登入企業 ID
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                {
                    return Unauthorized();
                }
                int companyId = (int)userIdObj;

                // 2. 查詢該企業的所有體驗計畫
                var programs = db.ProgramPlans
                    .Where(p => p.CompanyId == companyId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Intro,
                        p.ProgramCount,
                        p.Status,
                        p.PublishStartDate,
                        p.PublishDurationDays,
                        p.ProgramStartDate,
                        p.ProgramEndDate,
                        p.CreatedAt,
                        p.UpdatedAt
                    })
                    .ToList();

                return Ok(programs);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/ProgramPlans/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutProgramPlans(int id, ProgramPlans programPlans)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != programPlans.Id)
            {
                return BadRequest();
            }

            db.Entry(programPlans).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProgramPlansExists(id))
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

        // POST: api/ProgramPlans
        public IHttpActionResult PostProgramPlans(ProgramPlans programPlans)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ProgramPlans.Add(programPlans);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = programPlans.Id }, programPlans);
        }

        // POST: api/v1/program 新增體驗計畫
        [HttpPost]
        [Route("programs")]
        [JwtAuthFilter]
        public IHttpActionResult CreateProgramPlan([FromBody] ProgramPlanDto dto)
        {
            try
            {
                // 1. 取得登入企業 ID
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                {
                    return Unauthorized();
                }
                int companyId = (int)userIdObj;

                // 2. 檢查方案是否過期並更新狀態
                var planUsageToCheck = db.PlanUsage
                    .Where(p => p.CompanyId == companyId && p.Status == "active")
                    .ToList();

                foreach (var plan in planUsageToCheck)
                {
                    if (plan.EndDate.HasValue && plan.EndDate.Value < DateTime.Now)
                    {
                        plan.Status = "expired";
                    }
                }
                db.SaveChanges();

                // 3. 再查詢仍然有效的方案
                var planUsage = db.PlanUsage
                    .FirstOrDefault(p => p.CompanyId == companyId && p.Status == "active");
                if (planUsage == null)
                {
                    return BadRequest("尚未購買方案或方案已過期，無法新增體驗計畫");
                }

                // 4. 檢查剩餘人數
                if (planUsage.RemainingPeople < dto.ProgramCount)
                {
                    return BadRequest("體驗剩餘人數已達上限");
                }

                // 5. 計算日期
                var programEndDate = dto.ProgramStartDate.AddDays(dto.ProgramDurationDays);
                var publishEndDate = dto.PublishStartDate.AddDays(dto.PublishDurationDays);

                // 6. 建立 ProgramPlan
                var programPlan = new ProgramPlans
                {
                    CompanyId = companyId,
                    Name = dto.Name,
                    Intro = dto.Intro,
                    IndustryId = dto.IndustryId,
                    JobTitleId = dto.JobTitleId,
                    Address = dto.Address,
                    ContactName = dto.ContactName,
                    ContactPhone = dto.ContactPhone,
                    StepId = dto.StepId,
                    ProgramCount = dto.ProgramCount,
                    PublishStartDate = dto.PublishStartDate,
                    PublishDurationDays = dto.PublishDurationDays,
                    ProgramStartDate = dto.ProgramStartDate,
                    ProgramEndDate = programEndDate,
                    ProgramDurationDays = dto.ProgramDurationDays,
                    Status = "Under review", // 暫扣人數
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // 7.扣掉剩餘人數 
                planUsage.RemainingPeople -= dto.ProgramCount;

                db.ProgramPlans.Add(programPlan);
                db.SaveChanges();

                // 8. 建立 DTO 回傳前端
                var responseDto = new ProgramPlanDto
                {
                    Name = programPlan.Name,
                    Intro = programPlan.Intro,
                    IndustryId = programPlan.IndustryId,
                    JobTitleId = programPlan.JobTitleId,
                    Address = programPlan.Address,
                    ContactName = programPlan.ContactName,
                    ContactPhone = programPlan.ContactPhone,
                    StepId = programPlan.StepId,
                    ProgramCount = programPlan.ProgramCount,
                    PublishStartDate = programPlan.PublishStartDate,
                    PublishDurationDays = programPlan.PublishDurationDays,
                    ProgramStartDate = programPlan.ProgramStartDate,
                    ProgramDurationDays = programPlan.ProgramDurationDays
                };

                return Ok(responseDto);
            }
            catch (DbEntityValidationException ex)
            {
                var allErrors = ex.EntityValidationErrors
                    .SelectMany(eve => eve.ValidationErrors)
                    .Select(ve => new
                    {
                        Property = ve.PropertyName,
                        Error = ve.ErrorMessage
                    })
                    .ToList();

                return Content(HttpStatusCode.BadRequest, new
                {
                    Message = "欄位驗證失敗",
                    Errors = allErrors
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/ProgramPlans/5
        [ResponseType(typeof(ProgramPlans))]
        public IHttpActionResult DeleteProgramPlans(int id)
        {
            ProgramPlans programPlans = db.ProgramPlans.Find(id);
            if (programPlans == null)
            {
                return NotFound();
            }

            db.ProgramPlans.Remove(programPlans);
            db.SaveChanges();

            return Ok(programPlans);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProgramPlansExists(int id)
        {
            return db.ProgramPlans.Count(e => e.Id == id) > 0;
        }
    }
}