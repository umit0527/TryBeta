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

        // GET: api/ProgramPlans/5 取得所有體驗計畫(未審核即通過)
        [HttpGet]
        [Route("programs")]
        [JwtAuthFilter] // 必須登入
        public IHttpActionResult GetProgramPlans(int page = 1, int pageSize = 21)
        {
            try
            {
                // 1. 取得登入企業 ID
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                {
                    return Unauthorized();
                }
                int companyId = (int)userIdObj;


                // 2.保護 page/pageSize
                if (page <= 0) page = 1;
                if (pageSize <= 0 || pageSize > 100) pageSize = 21;

                // 3. 計算總筆數
                var totalCount = db.ProgramPlan.Count(p => p.CompanyId == companyId);

                // 4.分頁查詢
                var programs = db.ProgramPlan
                    .Where(p => p.CompanyId == companyId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)   // 跳過前面頁數的資料
                    .Take(pageSize)                // 只取一頁大小
                    .Select(p => new
                    {
                        p.Id,
                        p.Status,
                        p.Name,
                        p.Intro,
                        
                        
                        p.PublishStartDate,
                        p.PublishDurationDays,
                        p.ProgramStartDate,
                        p.ProgramEndDate,
                        p.CreatedAt,
                        p.UpdatedAt,
                    // 成團判斷，假設你有一個方法計算當前報名人數
                IsConfirmed = db.ProgramRegistrations
                                .Count(r => r.ProgramId == p.Id) >= p.MinPeople
                    })
            .ToList();

                // 回傳分頁資訊
                return Ok(new
                {
                    total = totalCount,     // 總筆數
                    page,
                    pageSize,
                    data = programs
                });               
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/v1/company/{companyid}/programs/{programId} 取得單一體驗計畫
        [HttpGet]
        [Route("programs/{programId:int}")]
        [JwtAuthFilter]
        public IHttpActionResult GetProgramPlan(int companyid, int programId)
        {
            try
            {
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                {
                    return Unauthorized();
                }
                int companyId = (int)userIdObj;

                // 確認登入的企業ID與route companyid一致，可做額外保護
                if (companyId != companyid)
                {
                    return Unauthorized();
                }

                var programPlan = db.ProgramPlan
                    .Where(p => p.Id == programId && p.CompanyId == companyId)
                    .FirstOrDefault();

                if (programPlan == null)
                {
                    return NotFound();
                }

                var steps = db.ProgramStep
                    .Where(s => s.ProgramPlanId == programPlan.Id)
                    .OrderBy(s => s.Id)
                    .Select(s => new
                    {
                        s.Id,
                        s.Name,
                        s.Description
                    })
                    .ToList();

                // 取得產業與職務名稱
                var industry = db.Industries
                    .Where(i => i.Id == programPlan.IndustryId)
                    .Select(i => new { i.Id, i.Title })
                    .FirstOrDefault();

                var jobTitle = db.Positions
                    .Where(j => j.Id == programPlan.JobTitleId)
                    .Select(j => new { j.Id, j.Title })
                    .FirstOrDefault();


                var response = new
                {
                    programPlan.Id,
                    programPlan.Name,
                    programPlan.Intro,
                    Industry = industry,
                    JobTitle = jobTitle,
                    programPlan.Address,
                    programPlan.ContactName,
                    programPlan.ContactPhone,
                    programPlan.MinPeople,
                    programPlan.MaxPeople,
                    programPlan.PublishStartDate,
                    programPlan.PublishDurationDays,
                    programPlan.PublishEndDate,
                    programPlan.ProgramStartDate,
                    programPlan.ProgramEndDate,
                    programPlan.ProgramDurationDays,
                    programPlan.Status,
                    Steps = steps
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/ProgramPlans/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutProgramPlans(int id, ProgramPlan programPlans)
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
        public IHttpActionResult PostProgramPlans(ProgramPlan programPlans)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ProgramPlan.Add(programPlans);
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
            {   // 1. 驗證最大.最少人數與階段
                if (dto.MaxPeople < dto.MinPeople)
                {
                    return BadRequest("最大人數不得小於最少人數");
                }

                //if (dto.Steps.Count > 5)
                //{
                //    return BadRequest("階段數量不能超過 5 個");
                //}

                // 2. 取得登入企業 ID
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                {
                    return Unauthorized();
                }
                int companyId = (int)userIdObj;

                // 3. 檢查方案是否過期並更新狀態
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

                // 4. 查詢仍然有效的方案
                var planUsage = db.PlanUsage
                    .FirstOrDefault(p => p.CompanyId == companyId && p.Status == "active");
                if (planUsage == null)
                {
                    return BadRequest("尚未購買方案或方案已過期，無法新增體驗計畫");
                }

                // 5. 檢查剩餘人數
                if (planUsage.RemainingPeople < dto.MaxPeople)
                {
                    return BadRequest("體驗剩餘人數已達上限");
                }

                // 6. 計算日期
                // 計算刊登結束日期
                var publishEndDate = dto.PublishStartDate.AddDays(dto.PublishDurationDays);
                // 計算體驗期間
                var programDurationDays = (dto.ProgramEndDate - dto.ProgramStartDate).Days+1;

                // 7. 建立 ProgramPlan
                var programPlan = new ProgramPlan
                {
                    CompanyId = companyId,
                    Name = dto.Name,
                    Intro = dto.Intro,
                    IndustryId = dto.IndustryId,
                    JobTitleId = dto.JobTitleId,
                    Address = dto.Address,
                    ContactName = dto.ContactName,
                    ContactPhone = dto.ContactPhone,
                    MinPeople = dto.MinPeople,
                    MaxPeople = dto.MaxPeople,
                    PublishStartDate = dto.PublishStartDate, //體驗刊登開始日期
                    PublishEndDate = dto.PublishStartDate.AddDays(dto.PublishDurationDays-1), //體驗刊登結束日期
                    PublishDurationDays = dto.PublishDurationDays,  //體驗刊登期間  
                    ProgramStartDate = dto.ProgramStartDate,  //體驗計畫執行開始日期
                    ProgramEndDate = dto.ProgramEndDate,  //體驗計畫執行結束日期
                    ProgramDurationDays = programDurationDays, // 體驗計畫執行期間
                    Status = "Under review", // 暫扣人數
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                db.ProgramPlan.Add(programPlan); // 先存 ProgramPlan 取得 Id
                db.SaveChanges(); 

                // 7. 新增階段
                foreach (var stepDto in dto.Steps)
                {
                    var step = new ProgramStep
                    {
                        Name = stepDto.Name,
                        Description = stepDto.Description,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        ProgramPlanId = programPlan.Id // FK
                    };
                    db.ProgramStep.Add(step);
                }

                // 8.扣掉剩餘人數，用 MaxPeople 扣額度，假設最多會來多少人
                planUsage.RemainingPeople -= dto.MaxPeople;

                db.SaveChanges();

                // 9. 建立 DTO 回傳前端
                var responseDto = new ProgramPlanDto
                {
                    Name = programPlan.Name,
                    Intro = programPlan.Intro,
                    IndustryId = programPlan.IndustryId,
                    JobTitleId = programPlan.JobTitleId,
                    Address = programPlan.Address,
                    ContactName = programPlan.ContactName,
                    ContactPhone = programPlan.ContactPhone,
                    MinPeople= programPlan.MinPeople,
                    MaxPeople= programPlan.MaxPeople,
                    PublishStartDate = programPlan.PublishStartDate,  //體驗計畫刊登開始日期
                    PublishDurationDays = programPlan.PublishDurationDays,  //體驗刊登期間
                    PublishEndDate = programPlan.PublishEndDate,  //體驗計畫刊登結束日期
                    ProgramStartDate = programPlan.ProgramStartDate,  //體驗計畫執行開始日期
                    ProgramDurationDays = programPlan.ProgramDurationDays,
                    ProgramEndDate = programPlan.ProgramEndDate,  //體驗計畫執行結束日期
                    Steps = dto.Steps
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
        [ResponseType(typeof(ProgramPlan))]
        public IHttpActionResult DeleteProgramPlans(int id)
        {
            ProgramPlan programPlans = db.ProgramPlan.Find(id);
            if (programPlans == null)
            {
                return NotFound();
            }

            db.ProgramPlan.Remove(programPlans);
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
            return db.ProgramPlan.Count(e => e.Id == id) > 0;
        }
    }
}