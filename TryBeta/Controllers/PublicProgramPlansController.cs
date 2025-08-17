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
using static TryBeta.Models.ProgramPlanDto;

namespace TryBeta.Controllers
{
    [RoutePrefix("api/v1/programs")]
    public class PublicProgramPlansController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();

        // GET: api/PublicProgramPlans
        public IQueryable<ProgramPlan> GetProgramPlan()
        {
            return db.ProgramPlan;
        }

        // GET: api/v1/users/programs 體驗計畫篩選器，包含全部體驗計畫
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetPublicProgramsFilter(
            string search = null,
            int? industry_id = null,
            int? job_title_id = null,
            string city_id = null,
            string sort = "publish_start_desc",
            int page = 1,
            int limit = 21)
        {
            try
            {
                var now = DateTime.Now;

                // 基本查詢：只取已發布且時間有效的計畫
                var query = db.ProgramPlan
                    .Include(p => p.Industry)
                    .Include(p => p.JobTitle)
                    .Include(p => p.Status)
                    .Include(p => p.Steps)
                    .AsQueryable();

                // 關鍵字搜尋
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p =>
                        p.Name.Contains(search) ||
                        p.Intro.Contains(search) ||
                        p.Steps.Any(s => s.Name.Contains(search) || s.Description.Contains(search))
                    );
                }

                // 產業篩選
                if (industry_id.HasValue)
                {
                    query = query.Where(p => p.IndustryId == industry_id.Value);
                }

                // 職務篩選
                if (job_title_id.HasValue)
                {
                    query = query.Where(p => p.JobTitleId == job_title_id.Value);
                }

                // 地區篩選：根據 city_id 查 City 表名稱，再比對 Address
                if (!string.IsNullOrEmpty(city_id))
                {
                    var cityName = db.City
                                     .Where(c => c.Id.ToString() == city_id)
                                     .Select(c => c.Name)
                                     .FirstOrDefault();

                    if (!string.IsNullOrEmpty(cityName))
                    {
                        query = query.Where(p => p.Address.Contains(cityName));
                    }
                }

                // 排序
                switch (sort)
                {
                    case "publish_start_asc":        // 刊登開始日期舊到新
                        query = query.OrderBy(p => p.PublishStartDate);
                        break;
                    case "publish_start_desc":       // 刊登開始日期新到舊
                        query = query.OrderByDescending(p => p.PublishStartDate);
                        break;
                    case "publish_end_asc":          // 刊登截止日期舊到新
                        query = query.OrderBy(p => p.PublishEndDate);
                        break;
                    case "publish_end_desc":         // 刊登截止日期新到舊
                        query = query.OrderByDescending(p => p.PublishEndDate);
                        break;
                    case "program_start_asc":        // 體驗開始日期舊到新
                        query = query.OrderBy(p => p.ProgramStartDate);
                        break;
                    case "program_start_desc":       // 體驗開始日期新到舊
                        query = query.OrderByDescending(p => p.ProgramStartDate);
                        break;
                    default:                         // 預設刊登日期新到舊
                        query = query.OrderByDescending(p => p.PublishStartDate);
                        break;
                }

                // 分頁
                var total = query.Count();
                var items = query
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToList()
                    .Select(p =>
                    {
                        // 已申請人數
                        var appliedCount = db.ProgramSubmits.Count(a => a.ProgramPlanId == p.Id);

                        // 判斷顯示 DaysLeft 或 IsOngoing
                        int? daysLeft = null;
                        bool? isOngoing = null;

                        if (now < p.ProgramStartDate)
                        {
                            daysLeft = (p.PublishEndDate - now).Days;
                        }
                        else if (now >= p.ProgramStartDate && now <= p.ProgramEndDate)
                        {
                            isOngoing = true;
                        }
                        else
                        {
                            isOngoing = false;
                        }

                        return new
                        {
                            // 計畫基本資料
                            p.Id,
                            p.Name,
                            p.Intro,
                            p.Address,
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
                            }),
                            AppliedCount = appliedCount,
                            DaysLeft = daysLeft,
                            IsOngoing = isOngoing
                        };
                    })
                    .ToList();

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


        // GET: api/PublicProgramPlans/5 取得單一體驗計畫
        [HttpGet]
        [Route("{programId:int}")]
        public IHttpActionResult GetPublicPrograms(int programId)
        {
            try
            {
                var now = DateTime.Now;

                // 取得單一計畫
                var plan = db.ProgramPlan
                    .Include(p => p.Status)
                    .FirstOrDefault(p => p.Id == programId);

                if (plan == null)
                    return NotFound();

                // 取得企業名稱
                var companyName = db.Companyinfoes
                    .Where(c => c.Id == plan.CompanyId)
                    .Select(c => c.Name)
                    .FirstOrDefault();

                // 取得企業 Logo / Cover
                var companyImages = db.CompanyImages
                    .Where(ci => ci.CompanyId == plan.CompanyId)
                    .Select(ci => new
                    {
                        ci.Type, 
                        ci.ImgPath
                    })
                    .ToList();
                // 轉成 DTO 欄位
                var logo = companyImages.FirstOrDefault(i => i.Type == "logo")?.ImgPath;
                var cover = companyImages.FirstOrDefault(i => i.Type == "cover")?.ImgPath;

                // 取得已通過人數
                var appliedCount = db.ProgramSubmits
                    .Where(a => a.ProgramPlanId == plan.Id && a.StatusId == 2)
                    .Sum(a => (int?)a.ParticipantsCount) ?? 0;

                // 取得產業
                var industry = db.Industries
                    .Where(i => i.Id == plan.IndustryId)
                    .Select(i => new SimpleEntityDto
                    {
                        Id = i.Id,
                        Title = i.Title
                    })
                    .FirstOrDefault();

                // 取得職務
                var jobTitle = db.Positions
                    .Where(j => j.Id == plan.JobTitleId)
                    .Select(j => new SimpleEntityDto
                    {
                        Id = j.Id,
                        Title = j.Title
                    })
                    .FirstOrDefault();

                // 取得狀態
                var status = db.ProgramPlanStatuses
                    .Where(s => s.Id == plan.StatusId)
                    .Select(s => new SimpleEntityDto
                    {
                        Id = s.Id,
                        Title = s.Title
                    })
                    .FirstOrDefault();

                // 取得 Steps
                var steps = db.ProgramStep
    .Where(s => s.ProgramPlanId == plan.Id)
    .OrderBy(s => s.Id)
    .Select(s => new ProgramStepDto
    {
        Name = s.Name,
        Description = s.Description
    })
    .ToList();

                // 取得 Images
                var images = db.ProgramPlanImages
    .Where(i => i.ProgramPlanId == plan.Id)
    .OrderBy(i => i.Id)
    .Select(i => i.ImgPath)
    .ToList();


                // 建立 DTO
                var dto = new ProgramPlanDto
                {
                    CompanyName=companyName,
                    CompanyLogo = logo,
                    CompanyCover = cover,
                    Name = plan.Name,
                    Intro = plan.Intro,
                    Industry = industry,
                    JobTitle = jobTitle,
                    Status = status,
                    Address = plan.Address,
                    AddressMap = plan.AddressMap,
                    ContactName = plan.ContactName,
                    ContactPhone = plan.ContactPhone,
                    ContactEmail = plan.ContactEmail,
                    ProgramStartDate = plan.ProgramStartDate,
                    ProgramEndDate = plan.ProgramEndDate,
                    ProgramDurationDays = (plan.ProgramEndDate - plan.ProgramStartDate).Days,
                    AppliedCount = appliedCount,
                    Steps = steps,
                    Images = images
                };

                // 判斷 DaysLeft / IsOngoing
                if (now < plan.ProgramStartDate)
                {
                    dto.DaysLeft = Math.Max(0, (plan.ProgramStartDate - now).Days);
                    dto.IsOngoing = null;
                }
                else if (now >= plan.ProgramStartDate && now <= plan.ProgramEndDate)
                {
                    dto.IsOngoing = true;
                    dto.DaysLeft = null;
                }
                else
                {
                    dto.IsOngoing = false;
                    dto.DaysLeft = null;
                }

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

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
        [HttpPost]
        [Route("{programId}/applications")]
        [JwtAuthFilter]
        [ResponseType(typeof(ProgramPlan))]
        public IHttpActionResult PostProgramPlan(int programId, [FromBody] ProgramSubmitDto dto)
        {
            try
            {
                // 1. 驗證登入
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                {
                    return Unauthorized();
                }
                int userId = (int)userIdObj;

                // 2. 驗證條款
                if (!dto.AgreeTerms)
                {
                    return BadRequest("請同意條款與隱私權政策");
                }

                // 3. 驗證 ProgramPlan 存在
                var programPlan = db.ProgramPlan.FirstOrDefault(p => p.Id == programId);
                if (programPlan == null)
                {
                    return NotFound();
                }

                // 4. 驗證 ParticipantInfoes
                var participant = db.ParticipantInfoes.FirstOrDefault(p => p.UserId == userId);
                if (participant == null)
                {
                    return BadRequest("找不到對應的體驗者資料，請先建立個人資料");
                }

                // 5. 驗證 Resume
                // 驗證 Resume
                if (dto.ResumeType.ToLower() == "simple resume")
                {
                    var simpleResume = db.SimpleResume.FirstOrDefault(r => r.Id == dto.ResumeId && r.UserId == userId);
                    if (simpleResume == null)
                        return BadRequest("找不到該簡單履歷");
                }
                else if (dto.ResumeType.ToLower() == "existing resume")
                {
                    var existingResume = db.ExistingResume.FirstOrDefault(r => r.Id == dto.ResumeId && r.UserId == userId);
                    if (existingResume == null)
                        return BadRequest("找不到該上傳履歷");
                }
                else
                {
                    return BadRequest("履歷類型錯誤");
                }

                // 6. 建立 ProgramSubmit
                var submit = new ProgramSubmit
                {
                    ProgramPlanId = programId,
                    ParticipantId = participant.Id,  
                    ParticipantsCount = dto.ParticipantsCount,
                    Note = dto.Note,
                    MotivationContent = dto.MotivationContent,
                    ResumeType = dto.ResumeType,
                    SubmitAt = DateTime.Now,
                    StatusId = 1, // 待審核
                    AgreeTerms = dto.AgreeTerms
                };

                if (dto.ResumeType == "simple")
                    submit.SimpleResumeId = dto.ResumeId;
                else if (dto.ResumeType == "existing")
                    submit.ExistingResumeId = dto.ResumeId;

                db.ProgramSubmits.Add(submit);
                db.SaveChanges();

                return Ok(new
                {
                    success = true,
                    //application_id = submit.Id,
                    message = "申請已送出，請等待企業審核"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
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