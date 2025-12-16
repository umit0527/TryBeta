using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.UI;
using TryBeta.Models;
using static TryBeta.Models.ParticipantDetailDto;

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

        // GET: api/v1/company/{companyid}/programs/{programId} 取得單一體驗計畫詳情
        [HttpGet]
        [Route("programs/{programplanId:int}")]
        [JwtAuthFilter]
        public IHttpActionResult GetProgramPlan(int companyid, int programplanId)
        {
            try
            {
                // 1. 從 JWT 取得登入使用者 UserId
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                {
                    return Unauthorized();
                }

                if (!int.TryParse(userIdObj?.ToString(), out var userId))
                {
                    return Unauthorized();
                }

                // 2. 從 UserId 找到對應的公司
                var company = db.Companyinfoes.FirstOrDefault(c => c.UserId == userId);
                if (company == null)
                {
                    return Unauthorized();
                }

                // 3. 檢查 route companyid 是否與登入公司一致
                if (company.Id != companyid)
                {
                    return Unauthorized();
                }

                // 4. 查詢指定 ProgramPlan
                var programPlan = db.ProgramPlan
                    .Where(p => p.Id == programplanId && p.CompanyId == company.Id)
                    .FirstOrDefault();

                if (programPlan == null)
                {
                    return NotFound();
                }

                // 5. 取得階段資料
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

                // 6. 取得產業名稱與職務名稱
                var industry = db.Industries
                    .Where(i => i.Id == programPlan.IndustryId)
                    .Select(i => new { i.Id, i.Title })
                    .FirstOrDefault();

                var jobTitle = db.Positions
                    .Where(j => j.Id == programPlan.JobTitleId)
                    .Select(j => new { j.Id, j.Title })
                    .FirstOrDefault();

                // 7. 取得狀態名稱
                var status = db.ProgramPlanStatuses
                    .Where(s => s.Id == programPlan.StatusId)
                    .Select(s => new { s.Id, s.Title })
                    .FirstOrDefault();

                // 8. 取得圖片資料
                var images = db.ProgramPlanImages
                    .Where(img => img.ProgramPlanId == programPlan.Id)
                    .Select(img => new { img.Id, img.ImgPath })
                    .ToList();

                // 9. 取得申請統計資訊 (統一跟申請列表邏輯一致)
                var totalApplicants = db.ProgramSubmits.Count(s => s.ProgramPlanId == programPlan.Id);
                var pendingCount = db.ProgramSubmits.Count(s => s.ProgramPlanId == programPlan.Id && s.StatusId == 1);
                var reviewedCount = totalApplicants - pendingCount;

                // 10. 瀏覽統計
                var now = DateTime.Now;
                var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
                var startOfDay = now.Date;

                var totalViews = db.ProgramViews.Count(v => v.ProgramPlanId == programPlan.Id);
                var weeklyViews = db.ProgramViews.Count(v => v.ProgramPlanId == programPlan.Id && v.ViewedAt >= startOfWeek);
                var dailyViews = db.ProgramViews.Count(v => v.ProgramPlanId == programPlan.Id && v.ViewedAt >= startOfDay);

                // 11. 組合回傳
                var response = new
                {
                    ProgramPlanSerialNum = programPlan.SerialNum,
                    Statistics = new
                    {
                        TotalApplicants = totalApplicants,
                        ReviewedCount = reviewedCount,
                        PendingCount = pendingCount
                    },
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
                    Steps = steps,
                    Images = images,
                    Views = new
                    {
                        TotalViews = totalViews,
                        WeeklyViews = weeklyViews,
                        DailyViews = dailyViews
                    }
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
        // GET: api/v1/company/{companyid}/programs 企業的體驗計畫篩選器，包含全部體驗計畫
        [HttpGet]
        [Route("programs")]
        [JwtAuthFilter]
        public IHttpActionResult GetCompanyPrograms(
        int companyid,
            string search = null,
        int? industry_id = null,
        int? job_title_id = null,
        int? status_id = null,
        string sort = "publish_start_desc",
        int page = 1,
        int limit = 21)
        {
            try
            {
                // 驗證登入企業ID
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                    return Unauthorized();
                int loggedInUserId = (int)userIdObj;

                // 透過 UserId 找出對應的 Company
                var company = db.Companyinfoes.FirstOrDefault(c => c.UserId == loggedInUserId);
                if (company == null || company.Id != companyid)
                    return Unauthorized();

                // 基本查詢
                var query = db.ProgramPlan
                    .Include(p => p.Industry)
                    .Include(p => p.JobTitle)
                    .Include(p => p.Status)
                    .Include(p => p.Steps)
                    .Include(p => p.ProgramPlanImages)
                    .Where(p => p.CompanyId == companyid)
                    .AsQueryable();

                // 關鍵字搜尋
                if (!string.IsNullOrEmpty(search))
                    query = query.Where(p => p.Name.Contains(search));

                // 產業篩選
                if (industry_id.HasValue)
                    query = query.Where(p => p.IndustryId == industry_id.Value);

                // 職務篩選
                if (job_title_id.HasValue)
                    query = query.Where(p => p.JobTitleId == job_title_id.Value);

                // 狀態篩選
                if (status_id.HasValue)
                {
                    var now = DateTime.Now;
                    switch (status_id.Value)
                    {
                        case 1: query = query.Where(p => p.StatusId == 1); break;
                        case 2: query = query.Where(p => p.StatusId == 2); break;
                        case 3: query = query.Where(p => p.StatusId == 3); break;
                        case 4: query = query.Where(p => p.StatusId == 4); break;
                        case 5: query = query.Where(p => p.StatusId == 5); break;
                        case 6: query = query.Where(p => p.PublishStartDate > now); break;
                        case 7:
                            query = query.Where(p =>
                            (p.StatusId == 2 || p.StatusId == 4) &&
                            p.PublishStartDate <= now &&
                            (p.PublishEndDate == null || p.PublishEndDate >= now)
                        ); break;
                        case 15: query = query.Where(p => p.StatusId == 2 || p.StatusId == 4); break;
                        case 16: query = query.Where(p => p.StatusId == 3 || p.StatusId == 5); break;
                    }
                }

                // 排序
                switch (sort)
                {
                    case "publish_start_asc": query = query.OrderBy(p => p.PublishStartDate); break;
                    case "publish_end_asc": query = query.OrderBy(p => p.PublishEndDate); break;
                    case "publish_end_desc": query = query.OrderByDescending(p => p.PublishEndDate); break;
                    case "program_start_asc": query = query.OrderBy(p => p.ProgramStartDate); break;
                    case "program_start_desc": query = query.OrderByDescending(p => p.ProgramStartDate); break;
                    case "program_end_asc": query = query.OrderBy(p => p.ProgramEndDate); break;
                    case "program_end_desc": query = query.OrderByDescending(p => p.ProgramEndDate); break;
                    default: query = query.OrderByDescending(p => p.PublishStartDate); break;
                }

                var request = HttpContext.Current.Request;
                var baseUrl = request.Url.GetLeftPart(UriPartial.Authority);

                // 將圖片路徑轉成完整 URL
                Func<string, string> normalizePath = (path) =>
                {
                    if (string.IsNullOrEmpty(path)) return null;
                    path = path.Replace("~/", "").Replace("\\", "/").TrimStart('/');
                    return $"{baseUrl}/api/v1/programs/image/{path}";
                };

                // 分頁
                var total = query.Count();
                var items = query
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToList()
                    .Select(p =>
                    {
                        // 企業 Logo / Cover
                        var companyImages = db.CompanyImages
                            .Where(ci => ci.CompanyId == p.CompanyId)
                            .ToList();
                        var logo = normalizePath(companyImages.FirstOrDefault(ci => ci.Type == "logo")?.ImgPath);
                        var cover = normalizePath(companyImages.FirstOrDefault(ci => ci.Type == "cover")?.ImgPath);

                        // 取得多張 Images
                        var images = p.ProgramPlanImages
                            .OrderBy(i => i.Id)
                            .Select(i => normalizePath(i.ImgPath))
                            .ToList();

                        return new
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
                            p.Address,
                            CompanyLogo = logo,
                            CompanyCover = cover,
                            CoverImage = images.FirstOrDefault(),
                            Images = images,
                            Steps = p.Steps.Select(s => new
                            {
                                s.Id,
                                s.Name,
                                s.Description,
                                s.CreatedAt,
                                s.UpdatedAt
                            }),
                            AppliedCount = db.ProgramSubmits
                        .Count(a => a.ProgramPlanId == p.Id)
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

        // GET: api/v1/company/{companyid}/programs/{programId}/applications  查看單一體驗的申請者列表
        [HttpGet]
        [Route("~/api/v1/company/{companyId:int}/programs/{programId:int}/applications")]
        [JwtAuthFilter]
        public IHttpActionResult GetProgramApplications(
            int companyId,
            int programId,
            string pending_sort = "submit_desc",
            string reviewed_sort = "submit_desc",
            int? reviewed_filter = null)
        {
            try
            {
                // 1. 從 JWT 取得登入使用者 UserId
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                    return Unauthorized();

                if (!int.TryParse(userIdObj?.ToString(), out var userId))
                    return Unauthorized();

                // 2. 從 UserId 找到對應公司
                var company = db.Companyinfoes.FirstOrDefault(c => c.UserId == userId);
                if (company == null)
                    return Unauthorized();

                // 3. 檢查 route companyId 是否與登入公司一致
                if (company.Id != companyId)
                    return Unauthorized();

                // 4. 查詢指定 ProgramPlan
                var programPlan = db.ProgramPlan.FirstOrDefault(p => p.Id == programId && p.CompanyId == company.Id);
                if (programPlan == null)
                    return NotFound();

                // 5. 取得所有申請，包含 Participant 與 Status
                var applications = db.ProgramSubmits
                    .Include(a => a.Participant)
                    .Include(a => a.Status)
                    .Where(a => a.ProgramPlanId == programPlan.Id)
                    .ToList();

                // 6. baseUrl 用來拼完整 URL
                string baseUrl = $"{Request.RequestUri.Scheme}://{Request.RequestUri.Host}";

                // 7. Helper: 取得 Headshot URL
                Func<string, string> GetHeadshotUrl = (headshot) =>
                {
                    if (!string.IsNullOrEmpty(headshot))
                    {
                        string filePath = headshot.Replace("~/", "").TrimStart('/');
                        if (!filePath.Contains("Participant"))
                        {
                            filePath = $"Images/Participant/{System.IO.Path.GetFileName(filePath)}";
                        }
                        return $"{baseUrl}/api/v1/programs/image/{Uri.EscapeDataString(filePath)}";
                    }
                    else
                    {
                        return $"{baseUrl}/api/v1/programs/image/Images/Participant/default.png";
                    }
                };

                // --- Pending ---
                var pendingQuery = applications.Where(a => a.StatusId == 1);
                pendingQuery = pending_sort == "submit_asc" ? pendingQuery.OrderBy(a => a.SubmitAt) : pendingQuery.OrderByDescending(a => a.SubmitAt);

                var pending = pendingQuery.Select(a => new
                {
                    participant_id = a.ParticipantId,
                    applicant_name = a.Participant.Name,
                    identity = a.Participant.IdentityId,
                    submit_date = a.SubmitAt,
                    review_status = a.Status.Title,
                    headshot = GetHeadshotUrl(a.Participant.Headshot)
                }).ToList();

                // --- Reviewed ---
                var reviewedQuery = applications.Where(a => a.StatusId != 1);
                if (reviewed_filter.HasValue)
                {
                    switch (reviewed_filter.Value)
                    {
                        case 1: reviewedQuery = reviewedQuery.Where(a => a.StatusId == 2); break; // 已通過
                        case 2: reviewedQuery = reviewedQuery.Where(a => a.StatusId == 3); break; // 已拒絕
                    }
                }

                reviewedQuery = reviewed_sort == "submit_asc" ? reviewedQuery.OrderBy(a => a.SubmitAt) : reviewedQuery.OrderByDescending(a => a.SubmitAt);

                var reviewed = reviewedQuery.Select(a => new
                {
                    participant_id = a.ParticipantId,
                    applicant_name = a.Participant.Name,
                    identity = a.Participant.IdentityId,
                    submit_date = a.SubmitAt,
                    review_status = a.Status.Title,
                    review_date = a.SubmitAt,
                    headshot = GetHeadshotUrl(a.Participant.Headshot)
                }).ToList();

                // 8. 統計資訊
                var response = new
                {
                    Statistics = new
                    {
                        TotalApplicants = applications.Count,
                        ReviewedCount = reviewed.Count,
                        PendingCount = pending.Count
                    },
                    ProgramPlanSerialNum = programPlan.SerialNum,
                    PendingApplications = pending,
                    ReviewedApplications = reviewed
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/v1/company/{companyid}/programs/{programId}/applications/{participantId:int}  查看單一體驗的單一申請者詳情
        [HttpGet]
        [Route("programs/{programId:int}/applications/{participantId:int}")]
        [JwtAuthFilter]
        public IHttpActionResult GetApplicantDetail(int companyId, int programId, int participantId)
        {
            try
            {
                // 1. 從 JWT 取得登入使用者 UserId
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                    return Unauthorized();

                if (!int.TryParse(userIdObj?.ToString(), out var userId))
                    return Unauthorized();

                // 2. 從 UserId 找到對應公司
                var company = db.Companyinfoes.FirstOrDefault(c => c.UserId == userId);
                if (company == null)
                    return Unauthorized();

                // 3. 驗證 route 的 companyId 是否與 JWT 公司一致
                if (company.Id != companyId)
                    return Unauthorized();

                // 4. 查詢指定 ProgramPlan 並確認公司
                var program = db.ProgramPlan.FirstOrDefault(p => p.Id == programId && p.CompanyId == company.Id);
                if (program == null)
                    return NotFound();

                // 5. 查詢申請 + 參加者資料
                var application = db.ProgramSubmits
                    .Include(a => a.Participant)
                    .Include(a => a.Participant.Education)
                    .Include(a => a.Participant.City)
                    .Include(a => a.Participant.District)
                    .Include(a => a.Participant.Identity)
                    .Include(a => a.Participant.User)
                    .Include(a => a.ProgramPlan)
                    .Include(a => a.Status)
                    .FirstOrDefault(a => a.ProgramPlanId == programId && a.ParticipantId == participantId);

                if (application == null)
                    return NotFound();

                var participant = application.Participant;

                // 6. 取得評價
                var reviews = db.ParticipantEvaluations
                    .Where(r => r.ParticipantId == participantId)
                    .ToList();

                // 7. 取得使用者啟用的簡單履歷
                var simpleResume = db.SimpleResume
                    .Include(r => r.Skills)
                    .Include(r => r.PortfolioFiles)
                    .FirstOrDefault(r => r.UserId == participant.UserId && r.IsActive);

                // 8. baseUrl 用來拼完整 URL 
                string baseUrl = $"{Request.RequestUri.Scheme}://{Request.RequestUri.Host}:{Request.RequestUri.Port}";

                // 9. 將 Headshot 轉成完整 URL
                string headshotUrl = null;
                if (!string.IsNullOrEmpty(participant.Headshot))
                {
                    // 移除 ~/
                    string filePath = participant.Headshot.Replace("~/", "").TrimStart('/');

                    // 如果存的不是正確資料夾，補上 Participant
                    if (!filePath.Contains("Participant"))
                    {
                        filePath = $"Images/Participant/{System.IO.Path.GetFileName(filePath)}";
                    }

                    // 拼完整 URL
                    headshotUrl = $"{baseUrl}/api/v1/programs/image/{filePath}";
                }

                // 8. 組成 DTO
                var dto = new ParticipantDetailDto
                {
                    ReviewStatusId = application.StatusId,
                    ReviewStatusName = application.Status?.Title ?? "",
                    ParticipantSerialNum = application.ParticipantSerialNum ?? application.Id.ToString(),
                    Name = participant.Name,
                    Phone = participant.Phone,
                    Age = DateTime.Now.Year - participant.Birthday.Year -
                          (DateTime.Now.DayOfYear < participant.Birthday.DayOfYear ? 1 : 0),
                    Gender = participant.Gender,
                    IdentityId = participant.IdentityId,
                    IdentityName = participant.Identity?.Title ?? "",
                    Address = (participant.City?.Name ?? "") + (participant.District?.Name ?? "") + participant.Street,
                    Email = participant.User?.Email ?? "",
                    Headshot = headshotUrl ?? "",
                    SchoolName = participant.Education?.SchoolName ?? "",
                    Major = participant.Education?.Major ?? "",
                    StatusId = participant.Education?.StatusId ?? 0,
                    ReviewCount = reviews.Count,
                    AverageScore = reviews.Count > 0 ? Math.Round(reviews.Average(r => r.Score), 2) : 0,

                    ProgramPlan = new ParticipantDetailDto.ProgramInfoDto
                    {
                        Name = program.Name,
                        SerialNum = program.SerialNum,
                        ProgramStartDate = program.ProgramStartDate,
                        ProgramEndDate = program.ProgramEndDate,
                        DurationDays = program.ProgramDurationDays,
                        Address = program.Address
                    },
                    MotivationContent = application.MotivationContent,
                    Skills = simpleResume?.Skills.Select(s => s.SkillName).ToList() ?? new List<string>(),
                    PortfolioFiles = simpleResume?.PortfolioFiles.Select(f => new ParticipantDetailDto.PortfolioFileDto
                    {
                        Id = f.Id,
                        Title = f.Title,
                        PortfolioPath = f.PortfolioPath,
                        FileSize = f.FileSize
                    }).ToList() ?? new List<ParticipantDetailDto.PortfolioFileDto>(),
                };

                // 9. 過去參加體驗計畫
                var pastPrograms = db.ProgramSubmits
    .Include(s => s.ProgramPlan)
    .Include(s => s.Status)
    .Where(s => s.ParticipantId == participantId)
    .Where(s => s.StatusId == 2 || s.StatusId == 4 || s.StatusId == 17)
    .Select(s => new ParticipantDetailDto.PastProgramDto
    {
        ProgramName = s.ProgramPlan.Name,
        ProgramStartDate = s.ProgramPlan.ProgramStartDate,
        ProgramEndDate = s.ProgramPlan.ProgramEndDate,
        ParticipationStatus = s.StatusId == 2 ? "已參加" : (s.StatusId == 4 ? "已取消" : "待審核"),
        CancelReason = s.StatusId == 4 ? s.CancelReason : null,
        ReviewScore = (s.StatusId == 2 || s.StatusId == 17)
                        ? db.ParticipantEvaluations
                            .Where(r => r.ParticipantId == participantId && r.ProgramPlanId == s.ProgramPlanId)
                            .Select(r => (double?)r.Score)
                            .FirstOrDefault()
                        : null
    })
    .OrderByDescending(s => s.ProgramEndDate)
    .ToList();

                dto.PastPrograms = pastPrograms;

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/v1/company/{companyId}/evaluations  企業取得評價列表資訊
        [HttpGet]
        [Route("~/api/v1/company/{companyId}/evaluations")]
        [JwtAuthFilter]
        public IHttpActionResult GetCompanyEvaluations(
            int companyId,
            string search = null,
            int? score = null,                // 分數 1-5
            DateTime? start_date = null,       // 評價起始日
            DateTime? end_date = null,         // 評價結束日
            string sort = "date_desc",        // 排序方式
            int page = 1,
            int limit = 20)
        {
            var query = db.ParticipantEvaluations
        .Where(e => e.Program.CompanyId == companyId)
        .Where(e => e.StatusId == 2 || e.StatusId == 4 || e.StatusId == 15);

            // 搜尋
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e =>
                    e.Participant.Name.Contains(search) ||
                    e.Comment.Contains(search) ||
                    e.Program.Name.Contains(search));
            }

            // 篩選分數
            if (score.HasValue)
            {
                query = query.Where(e => e.Score == score.Value);
            }

            // 篩選日期
            if (start_date.HasValue)
                query = query.Where(e => e.CreatedAt >= start_date.Value);
            if (end_date.HasValue)
            {
                var endDateInclusive = end_date.Value.Date.AddDays(1);
                query = query.Where(e => e.CreatedAt < endDateInclusive);
            }

            // 排序
            switch (sort.ToLower())
            {
                case "date_asc":
                    query = query.OrderBy(e => e.CreatedAt);
                    break;
                case "date_desc":
                default:
                    query = query.OrderByDescending(e => e.CreatedAt);
                    break;
            }

            var totalCount = query.Count();

            // baseUrl
            var request = HttpContext.Current.Request;
            var baseUrl = request.Url.GetLeftPart(UriPartial.Authority);

            // normalizePath for headshot
            Func<string, string> normalizePath = (path) =>
            {
                if (string.IsNullOrEmpty(path)) return null;

                path = path.Replace("~/", "").Replace("\\", "/").TrimStart('/');

                if (!path.Contains("Participant"))
                {
                    path = $"Images/Participant/{System.IO.Path.GetFileName(path)}";
                }

                return $"{baseUrl}/api/v1/programs/image/{path}";
            };

            // 投影
            var tempList = query
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(e => new
                {
                    e.Id,
                    e.Participant.Name,
                    e.Participant.Identity,
                    e.Participant.Birthday,
                    e.Participant.Headshot,
                    ProgramName = e.Program.Name,
                    ProgramPlanId = e.Program.Id,
                    e.Score,
                    e.Comment,
                    e.CreatedAt
                })
                .ToList();

            var evaluations = tempList.Select(e => new
            {
                e.Id,
                ParticipantName = e.Name,
                e.Identity,
                ParticipantAge = DateTime.Today.Year - e.Birthday.Year -
                                 (DateTime.Today.DayOfYear < e.Birthday.DayOfYear ? 1 : 0),
                Headshot = normalizePath(e.Headshot) ?? "",
                e.ProgramPlanId,
                ProgramPlanName = e.ProgramName,
                e.Score,
                e.Comment,
                EvaluationDate = e.CreatedAt
            }).ToList();

            var result = new
            {
                TotalCount = totalCount,
                Page = page,
                Limit = limit,
                Data = evaluations
            };

            return Ok(result);
        }

        // GET: api/v1/industries 產業
        [HttpGet]
        [Route("~/api/v1/industries")]
        [ResponseType(typeof(IEnumerable<Industry>))]
        public IHttpActionResult GetIndustries()
        {
            var industries = db.Industries
                               .OrderBy(i => i.Id)
                               .Select(i => new
                               {
                                   id = i.Id,
                                   title = i.Title
                               })
                               .ToList();

            return Ok(industries);
        }

        // GET: api/v1/positions
        [HttpGet]
        [Route("~/api/v1/positions")]
        [ResponseType(typeof(IEnumerable<Position>))]
        public IHttpActionResult GetPositions()
        {
            var positions = db.Positions
                              .OrderBy(p => p.Id)
                              .Select(p => new
                              {
                                  id = p.Id,
                                  title = p.Title
                              })
                              .ToList();

            return Ok(positions);
        }

        // POST: api/ProgramPlans
        //public IHttpActionResult PostProgramPlans(ProgramPlan programPlans)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    db.ProgramPlan.Add(programPlans);
        //    db.SaveChanges();

        //    return CreatedAtRoute("DefaultApi", new { id = programPlans.Id }, programPlans);
        //}

        // POST: api/v1/program 新增體驗計畫
        [HttpPost]
        [Route("programs")]
        [JwtAuthFilter]
        public IHttpActionResult CreateProgramPlan(int companyId, [FromBody] ProgramPlanDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("請提供 ProgramPlan 資料");
                // 1. 驗證最大.最少人數
                if (dto.MaxPeople < dto.MinPeople)
                    return BadRequest("最大人數不得小於最少人數");

                // 驗證登入企業 ID
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                    return Unauthorized();
                int loggedInUserId = (int)userIdObj;

                // 找出 User 對應的公司
                var company = db.Companyinfoes.FirstOrDefault(c => c.UserId == loggedInUserId);
                if (company == null)
                    return Unauthorized();

                // 檢查 URL 帶的 companyid 是否跟登入的公司一致
                if (company.Id != companyId)
                    return Unauthorized();

                // 2. 檢查方案是否過期或額滿
                var planUsage = db.PlanUsage
                    .Include("Plan")
                    .Include("PlanUsageStatus")
                    .Where(p => p.CompanyId == companyId)
                    .OrderByDescending(p => p.StartDate)
                    .FirstOrDefault();

                if (planUsage == null)
                    return BadRequest("尚未購買方案或方案已過期");

                bool changed = false;
                if (planUsage.EndDate.HasValue && planUsage.EndDate.Value < DateTime.Now)
                {
                    planUsage.StatusId = 2; // expired
                    changed = true;
                }
                else if (planUsage.RemainingPeople <= 0)
                {
                    planUsage.StatusId = 4; // full
                    changed = true;
                }

                if (changed) db.SaveChanges();

                if (planUsage.StatusId != 1)
                    return BadRequest("方案不可用（已過期或已額滿）");

                if (planUsage.RemainingPeople < dto.MaxPeople)
                    return BadRequest("體驗剩餘人數不足");

                // 3. 生成序號
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                int todayCount = db.ProgramPlan.Count(p => p.CreatedAt >= today && p.CreatedAt < tomorrow) + 1;
                string serialNumber = $"PRJ-{DateTime.Now:yyyyMMdd}-{todayCount:D3}";

                if (string.IsNullOrWhiteSpace(dto.Address))
                    return BadRequest("地址不能為空");

                // 4. 安全生成 Google Map Embed URL
                string googleMapEmbedUrl;
                try
                {
                    googleMapEmbedUrl = GoogleMapsHelper.GenerateEmbedUrl(dto.Address);
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }


                // 4. 建立 ProgramPlan
                var programPlan = new ProgramPlan
                {
                    CompanyId = companyId,
                    SerialNum = serialNumber,
                    Name = dto.Name,
                    Intro = dto.Intro,
                    IndustryId = dto.IndustryId,
                    JobTitleId = dto.JobTitleId,
                    Address = dto.Address,
                    AddressMap= googleMapEmbedUrl,
                    ContactName = dto.ContactName,
                    ContactPhone = dto.ContactPhone,
                    ContactEmail = dto.ContactEmail,
                    MinPeople = dto.MinPeople,
                    MaxPeople = dto.MaxPeople,
                    PublishStartDate = dto.PublishStartDate,
                    PublishEndDate = dto.PublishStartDate.AddDays(dto.PublishDurationDays - 1),
                    PublishDurationDays = dto.PublishDurationDays,
                    ProgramStartDate = dto.ProgramStartDate,
                    ProgramEndDate = dto.ProgramEndDate,
                    ProgramDurationDays = (dto.ProgramEndDate - dto.ProgramStartDate).Days + 1,
                    StatusId = 2,  // 預設通過
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                db.ProgramPlan.Add(programPlan);

                // 5. 建立階段
                foreach (var stepDto in dto.Steps)
                {
                    var step = new ProgramStep
                    {
                        Name = stepDto.Name,
                        Description = stepDto.Description,
                        ProgramPlan = programPlan,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    db.ProgramStep.Add(step);
                }

                // 6. 扣掉剩餘人數
                planUsage.RemainingPeople -= dto.MaxPeople;
                if (planUsage.RemainingPeople <= 0) planUsage.StatusId = 4; // full

                db.SaveChanges();

                // 7. 取得公司資訊
                var companyInfo = db.Companyinfoes
                    .Include("CompanyImages")
                    .FirstOrDefault(c => c.Id == companyId);

                var companyName = companyInfo?.Name;
                var companyLogo = companyInfo?.CompanyImages.FirstOrDefault(img => img.Type == "logo")?.ImgPath;
                var companyCover = companyInfo?.CompanyImages.FirstOrDefault(img => img.Type == "cover")?.ImgPath;

                // 8. 取得產業、職務、狀態名稱
                var industry = db.Industries.FirstOrDefault(i => i.Id == programPlan.IndustryId)?.Title;
                var jobTitle = db.Positions.FirstOrDefault(p => p.Id == programPlan.JobTitleId)?.Title;
                var status = db.ProgramPlanStatuses.FirstOrDefault(s => s.Id == programPlan.StatusId);

                // 9. 計算申請剩餘天數
                var daysLeft = (programPlan.PublishEndDate - DateTime.Today).Days;
                if (daysLeft < 0) daysLeft = 0;

                // 10. 回傳 DTO
                var responseDto = new ProgramPlanDto
                {
                    Id = programPlan.Id,
                    SerialNum = programPlan.SerialNum,
                    CompanyName = companyName,
                    CompanyLogo = companyLogo,
                    CompanyCover = companyCover,
                    Name = programPlan.Name,
                    Intro = programPlan.Intro,
                    IndustryId = programPlan.IndustryId,
                    JobTitleId = programPlan.JobTitleId,
                    Address = programPlan.Address,
                    AddressMap = programPlan.AddressMap,
                    ContactName = programPlan.ContactName,
                    ContactPhone = programPlan.ContactPhone,
                    ContactEmail = programPlan.ContactEmail,
                    MinPeople = programPlan.MinPeople,
                    MaxPeople = programPlan.MaxPeople,
                    PublishStartDate = programPlan.PublishStartDate,
                    PublishEndDate = programPlan.PublishEndDate,
                    PublishDurationDays = programPlan.PublishDurationDays,
                    ProgramStartDate = programPlan.ProgramStartDate,
                    ProgramEndDate = programPlan.ProgramEndDate,
                    ProgramDurationDays = programPlan.ProgramDurationDays,
                    StatusId = programPlan.StatusId,
                    Status = status != null ? new ProgramPlanDto.SimpleEntityDto { Id = status.Id, Title = status.Title } : null,
                    Industry = new ProgramPlanDto.SimpleEntityDto { Id = programPlan.IndustryId, Title = industry },
                    JobTitle = new ProgramPlanDto.SimpleEntityDto { Id = programPlan.JobTitleId, Title = jobTitle },
                    Steps = dto.Steps,
                    Images = dto.Images,
                    DaysLeft = daysLeft
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

        // PUT: api/ProgramPlans/5 審核體驗者通過或拒絕
        [HttpPut]
        [Route("programs/{programId:int}/applications/{participantId:int}/review")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> ReviewParticipant(int companyId, int programId, int participantId, [FromBody] ProgramSubmitReviewDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.StatusId != (int)ReviewStatus.Approved && dto.StatusId != (int)ReviewStatus.Rejected)
                return BadRequest("只能設定為核准申請或婉拒申請");

            // 驗證登入企業 ID
            if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                return Unauthorized();

            int loggedInUserId = (int)userIdObj;

            // 找出 User 對應的公司
            var company = db.Companyinfoes.FirstOrDefault(c => c.UserId == loggedInUserId);
            if (company == null)
                return Unauthorized();

            if (company.Id != companyId)
                return Unauthorized();

            var program = db.ProgramPlan.FirstOrDefault(p => p.Id == programId && p.CompanyId == companyId);
            if (program == null)
                return Content(System.Net.HttpStatusCode.Forbidden, new { message = "非本公司不得審核該體驗計畫" });

            // 最新申請紀錄
            var application = db.ProgramSubmits
                .Include(a => a.Participant.User)
                .Where(a => a.ProgramPlanId == programId && a.ParticipantId == participantId)
                .OrderByDescending(a => a.SubmitAt)
                .FirstOrDefault();

            if (application == null)
                return NotFound();

            // 查詢目前已核准人數
            var approvedCount = db.ProgramSubmits
                .Count(a => a.ProgramPlanId == programId && a.StatusId == (int)ReviewStatus.Approved);

            int newStatusId;
            if (dto.StatusId == (int)ReviewStatus.Approved)
            {
                if (approvedCount >= program.MaxPeople)
                    return BadRequest($"此體驗計畫已達人數上限 ({program.MaxPeople}人)，無法再核准新的申請。");

                newStatusId = (int)ReviewStatus.Approved;

                // 扣掉方案額度
                var planUsage = db.PlanUsage
                                .Where(pu => pu.CompanyId == companyId && pu.StatusId == 1 && pu.RemainingPeople > 0)
                                .OrderBy(pu => pu.CreatedAt)
                                .FirstOrDefault();
                if (planUsage == null || planUsage.RemainingPeople <= 0)
                    return BadRequest("體驗人數剩餘額度不足，無法核准申請");

                planUsage.RemainingPeople--;
                planUsage.UpdatedAt = DateTime.Now;

                // 更新熱門分數（已核准人數 +1）
                program.Score = program.ViewsCount * 1
                              + program.FavoritesCount * 3
                              + (approvedCount + 1) * 5;
            }
            else
            {
                newStatusId = (int)ReviewStatus.Rejected;
            }

            // 更新申請狀態
            application.StatusId = newStatusId;
            application.ReviewedAt = DateTime.Now;

            var review = new ProgramSubmitReview
            {
                ProgramSubmitId = application.Id,
                StatusId = newStatusId,
                Comment = dto.Comment,
                ReviewedAt = DateTime.Now,
                ReviewerId = companyId
            };
            db.ProgramSubmitReviews.Add(review);

            string evaluationMessage;

            try
            {
                await db.SaveChangesAsync();

                // 建立空評價
                var existingEvaluation = await db.ParticipantEvaluations
                    .FirstOrDefaultAsync(e => e.ParticipantId == application.ParticipantId && e.ProgramPlanId == programId);

                if (existingEvaluation != null)
                {
                    evaluationMessage = $"空的評價已存在，不再建立";
                }
                else
                {
                    var evaluation = new ParticipantEvaluation
                    {
                        ParticipantId = application.ParticipantId,
                        ProgramPlanId = programId,
                        SerialNum = application.ParticipantSerialNum,
                        Score = 0,
                        Comment = null,
                        StatusId = 17,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    db.ParticipantEvaluations.Add(evaluation);
                    await db.SaveChangesAsync();

                    evaluationMessage = $"空的評價已建立";
                }

                // 發送 Email
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var participantEmail = application.Participant.User?.Email;
                        if (!string.IsNullOrEmpty(participantEmail))
                        {
                            await EmailService.SendReviewResultAsync(
                                participantId, 
                                programId,
                                ((ReviewStatus)application.StatusId).ToString(),
                                review.Comment,
                                participantEmail,
                                program.Name);

                            if (dto.StatusId == (int)ReviewStatus.Approved)
                            {
                                await Task.Delay(30000);
                                await EmailService.SendEvaluationAvailableEmail(
                                    db,
                                    application.Participant.UserId,
                                    application.ParticipantId,
                                    application.ProgramPlanId,
                                    application.ParticipantSerialNum,
                                    participantEmail,
                                    program.Name);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Email 發送失敗: {ex.Message}");
                    }
                });
            }
            catch (DbEntityValidationException ex)
            {
                var errors = ex.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors)
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
                return BadRequest("資料驗證失敗: " + string.Join("; ", errors));
            }

            return Ok(new
            {
                message = "審核完成",
                status = application.StatusId,
                status_title = ((ReviewStatus)application.StatusId).ToString(),
                comment = review.Comment,
                evaluation_status = evaluationMessage
            });
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