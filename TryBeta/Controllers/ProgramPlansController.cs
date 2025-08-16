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
        //[HttpGet]
        //[Route("programs")]
        //[JwtAuthFilter] // 必須登入
        //public IHttpActionResult GetProgramPlans(int page = 1, int pageSize = 21)
        //{
        //    try
        //    {
        //        // 1. 取得登入企業 ID
        //        if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
        //        {
        //            return Unauthorized();
        //        }
        //        int companyId = (int)userIdObj;


        //        // 2.保護 page/pageSize
        //        if (page <= 0) page = 1;
        //        if (pageSize <= 0 || pageSize > 100) pageSize = 21;

        //        // 3. 計算總筆數
        //        var totalCount = db.ProgramPlan.Count(p => p.CompanyId == companyId);

        //        // 4.分頁查詢
        //        var programs = db.ProgramPlan
        //            .Where(p => p.CompanyId == companyId)
        //            .OrderByDescending(p => p.CreatedAt)
        //            .Skip((page - 1) * pageSize)   // 跳過前面頁數的資料
        //            .Take(pageSize)                // 只取一頁大小
        //            .Select(p => new
        //            {
        //                p.Id,
        //                p.StatusId,
        //                p.Name,
        //                p.Intro,
                        
                        
        //                p.PublishStartDate,
        //                p.PublishDurationDays,
        //                p.ProgramStartDate,
        //                p.ProgramEndDate,
        //                p.CreatedAt,
        //                p.UpdatedAt,
        //            // 成團判斷，假設你有一個方法計算當前報名人數
        //        IsConfirmed = db.ProgramRegistrations
        //                        .Count(r => r.ProgramId == p.Id) >= p.MinPeople
        //            })
        //    .ToList();

        //        // 回傳分頁資訊
        //        return Ok(new
        //        {
        //            total = totalCount,     // 總筆數
        //            page,
        //            pageSize,
        //            data = programs
        //        });               
        //    }
        //    catch (Exception ex)
        //    {
        //        return InternalServerError(ex);
        //    }
        //}

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

                // 取得狀態名稱
                var status = db.ProgramPlanStatuses
                    .Where(s => s.Id == programPlan.StatusId)
                    .Select(s => new { s.Id, s.Title })
                    .FirstOrDefault();

                var response = new
                {
                    programPlan.Id,
                    programPlan.Name,
                    programPlan.Intro,
                    Industry = industry,
                    JobTitle = jobTitle,
                    Status = status,
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
                    Steps = steps
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// 企業的體驗計畫清單 (支援搜尋、篩選、排序、分頁)
        /// </summary>
        // GET: api/v1/company/{companyid}/programs 企業的體驗計畫篩選器
        [HttpGet]
        [Route("programs")]
        [JwtAuthFilter]
        public IHttpActionResult GetCompanyPrograms(
        string search = null,
        int? industry = null,
        int? jobtitle = null,
        int? status = null,   // 1=全部 2=已通過 3=已發布 4=待發布 5=已拒絕 6=審核中
        string sort = "newest",
        int page = 1,
        int limit = 21)
        {
            try
            {
                // 驗證登入企業ID
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                    return Unauthorized();
                int companyId = (int)userIdObj;

                // 基本查詢
                var query = db.ProgramPlan
                    .Include(p => p.Industry)
                    .Include(p => p.JobTitle)
                    .Include(p => p.Status)
                    .Include(p => p.Steps)
                    .Where(p => p.CompanyId == companyId)
                    .AsQueryable();

                // 關鍵字搜尋
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.Name.Contains(search) || 
                                        p.Intro.Contains(search) || 
                                        p.Steps.Any(s => s.Name.Contains(search) ||
                                        s.Description.Contains(search)));
                }

                // 產業篩選
                if (industry.HasValue)
                {
                    query = query.Where(p => p.IndustryId == industry.Value);
                }

                // 職務篩選
                if (jobtitle.HasValue)
                {
                    query = query.Where(p => p.JobTitleId == jobtitle.Value);
                }

                if (status.HasValue)
                {
                    var s = (ProgramPlanStatusEnum)status.Value;
                    switch (s)
                    {
                        case ProgramPlanStatusEnum.SystemPass:
                        case ProgramPlanStatusEnum.ManualPass:
                            query = query.Where(p => p.StatusId == (int)ProgramPlanStatusEnum.SystemPass
                                                   || p.StatusId == (int)ProgramPlanStatusEnum.ManualPass);
                            break;

                        case ProgramPlanStatusEnum.Published:
                            query = query.Where(p => p.StatusId == (int)ProgramPlanStatusEnum.Published
                                                   && p.PublishStartDate <= DateTime.Now
                                                   && p.PublishEndDate >= DateTime.Now);
                            break;

                        case ProgramPlanStatusEnum.Pending:
                            query = query.Where(p => p.StatusId == (int)ProgramPlanStatusEnum.Pending
                                                   && p.PublishStartDate > DateTime.Now);
                            break;

                        case ProgramPlanStatusEnum.SystemRejected:
                        case ProgramPlanStatusEnum.ManualRejected:
                            query = query.Where(p => p.StatusId == (int)ProgramPlanStatusEnum.SystemRejected
                                                   || p.StatusId == (int)ProgramPlanStatusEnum.ManualRejected);
                            break;

                        case ProgramPlanStatusEnum.UnderReview:
                            query = query.Where(p => p.StatusId == (int)ProgramPlanStatusEnum.UnderReview);
                            break;
                    }
                }

                // 排序
                switch (sort)
                {
                    case "oldest":
                        query = query.OrderBy(p => p.Id);
                        break;
                    case "newest":
                    default:
                        query = query.OrderByDescending(p => p.Id);
                        break;
                }

                // 分頁
                var total = query.Count();
                var items = query
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Intro,
                        Industry = new { p.Industry.Id, p.Industry.Title },
                        JobTitle = new { p.JobTitle.Id, p.JobTitle.Title },
                        p.PublishStartDate,
                        p.PublishEndDate,
                        p.ProgramStartDate,
                        p.ProgramEndDate,
                        Steps = p.Steps.Select(s => new
                        {
                            s.Id,
                            s.Name,
                            s.Description,
                            s.CreatedAt,
                            s.UpdatedAt
                        })
                    })
                    .ToList();

                // 回傳訊息
                string message = total == 0 ? "查無符合條件的體驗計畫" : null;

                return Ok(new
                {
                    total,
                    page,
                    limit,
                    items,
                    message
                });
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
                var planUsage = db.PlanUsage
            .Include("Plan")
            .Include("PlanUsageStatus")
            .Where(p => p.CompanyId == companyId)
            .OrderByDescending(p => p.StartDate)
            .FirstOrDefault();

                if (planUsage == null)
                    return BadRequest("尚未購買方案或方案已過期");
                // 4. 驗證方案狀態（過期 / 額滿）
                bool changed = false;

                if (planUsage.EndDate.HasValue && planUsage.EndDate.Value.Date < DateTime.Now.Date)
                {
                    planUsage.StatusId = 2; // expired
                    changed = true;
                }
                else if (planUsage.RemainingPeople <= 0)
                {
                    planUsage.StatusId = 4; // full
                    changed = true;
                }

                if (changed)
                    db.SaveChanges();

                if (planUsage.StatusId != 1) // 1 = active
                    return BadRequest("方案不可用（已過期或已額滿）");

                // 5. 驗證剩餘人數
                if (planUsage.RemainingPeople < dto.MaxPeople)
                    return BadRequest("體驗剩餘人數不足");

                // 6. 建立 ProgramPlan
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
                    PublishStartDate = dto.PublishStartDate,
                    PublishEndDate = dto.PublishStartDate.AddDays(dto.PublishDurationDays - 1),
                    PublishDurationDays = dto.PublishDurationDays,
                    ProgramStartDate = dto.ProgramStartDate,
                    ProgramEndDate = dto.ProgramEndDate,
                    ProgramDurationDays = (dto.ProgramEndDate - dto.ProgramStartDate).Days + 1,
                    StatusId = 1, // 暫扣人數
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                db.ProgramPlan.Add(programPlan);
                db.SaveChanges();

                // 7. 建立階段
                foreach (var stepDto in dto.Steps)
                {
                    var step = new ProgramStep
                    {
                        Name = stepDto.Name,
                        Description = stepDto.Description,
                        ProgramPlanId = programPlan.Id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    db.ProgramStep.Add(step);
                }

                // 8. 扣掉剩餘人數
                planUsage.RemainingPeople -= dto.MaxPeople;
                if (planUsage.RemainingPeople <= 0)
                {
                    planUsage.StatusId = 4; // full
                }
                db.SaveChanges();

                // 9. 取得狀態名稱
                var statusTitle = db.ProgramPlanStatuses
                    .Where(s => s.Id == programPlan.StatusId)
                    .Select(s => s.Title)
                    .FirstOrDefault();

                // 10. 回傳 DTO
                var responseDto = new ProgramPlanDto
                {
                    StatusId = programPlan.StatusId,
                    StatusTitle = statusTitle,
                    Name = programPlan.Name,
                    Intro = programPlan.Intro,
                    IndustryId = programPlan.IndustryId,
                    JobTitleId = programPlan.JobTitleId,
                    Address = programPlan.Address,
                    ContactName = programPlan.ContactName,
                    ContactPhone = programPlan.ContactPhone,
                    MinPeople = programPlan.MinPeople,
                    MaxPeople = programPlan.MaxPeople,
                    PublishStartDate = programPlan.PublishStartDate,
                    PublishDurationDays = programPlan.PublishDurationDays,
                    PublishEndDate = programPlan.PublishEndDate,
                    ProgramStartDate = programPlan.ProgramStartDate,
                    ProgramDurationDays = programPlan.ProgramDurationDays,
                    ProgramEndDate = programPlan.ProgramEndDate,
                    Steps = dto.Steps
                };

                return Ok(responseDto);
            }
            catch (DbEntityValidationException ex)
            {
                var allErrors = ex.EntityValidationErrors
                    .SelectMany(eve => eve.ValidationErrors)
                    .Select(ve => new { Property = ve.PropertyName, Error = ve.ErrorMessage })
                    .ToList();

                return Content(HttpStatusCode.BadRequest, new { Message = "欄位驗證失敗", Errors = allErrors });
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