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
    [RoutePrefix("api/v1/admin")]
    public class AdminController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();

        // GET: api/Admin
        //public IQueryable<ParticipantEvaluation> GetParticipantEvaluations()
        //{
        //    return db.ParticipantEvaluations;
        //}

        // GET: api/v1/admin/evaluations 取得所有體驗者評價
        [HttpGet]
        [Route("evaluations")]
        [JwtAuthFilter]
        public IHttpActionResult GetEvaluations(
            int? score = null,
            int? status_id = null,
            string search = null,
            string sort = "date_desc",
            int page = 1,
            int pageSize = 20)
        {
            var query = db.ParticipantEvaluations
                .Include("Participant")
                .Include("Program")
                .AsQueryable();

            // 篩選分數
            if (score.HasValue)
                query = query.Where(e => e.Score == score.Value);

            // 篩選審核狀態
            if (status_id.HasValue)
            {
                switch (status_id.Value)
                {
                    case 15: // 已通過
                        query = query.Where(e => e.StatusId == 2 || e.StatusId == 4);
                        break;
                    case 16: // 已拒絕
                        query = query.Where(e => e.StatusId == 3 || e.StatusId == 5);
                        break;
                    default:
                        query = query.Where(e => e.StatusId == status_id.Value);
                        break;
                }
            }

            // 搜尋體驗計畫名稱、體驗者名稱或評價內容
            if (!string.IsNullOrEmpty(search))
                query = query.Where(e => e.Program.Name.Contains(search) || e.Participant.Name.Contains(search)
                || e.Comment.Contains(search));

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

            // 分頁
            var total = query.Count();
            var evaluations = query.Select(e => new
            {
                id = e.Id,
                programName = e.Program.Name,
                programId = e.Program.Id,
                participant = new
                {
                    id = e.Participant.Id,
                    name = e.Participant.Name
                },
                score = e.Score,
                // 取得體驗評價的審核狀態
                status = db.ProgramPlanStatuses
                   .Where(s => s.Id == e.StatusId)
                   .Select(s => s.Title)
                   .FirstOrDefault(),
                evaluationDate = e.CreatedAt
            }).ToList();

            return Ok(new
            {
                total,
                page,
                pageSize,
                evaluations
            });
        }

        // GET: api/v1/admin/evaluation/{evaluationId}
        // 管理員取得單一體驗者評價資訊
        [HttpGet]
        [Route("evaluation/{evaluationId:int}")]
        [JwtAuthFilter] // 確保只有管理員能呼叫
        public IHttpActionResult GetSingleEvaluation(int evaluationId)
        {
            // 撈取評價，包含 Participant 與 Program 導覽屬性
            var evaluation = db.ParticipantEvaluations
                .Include("Participant")
                .Include("Program")
                .FirstOrDefault(e => e.Id == evaluationId);

            if (evaluation == null)
                return NotFound();

            // 撈取體驗者資訊（年齡計算）
            var participant = evaluation.Participant;
            int age = DateTime.Now.Year - participant.Birthday.Year;
            if (participant.Birthday > DateTime.Now.AddYears(-age)) age--;

            // 撈取 ProgramSubmit 的提交日期與更新日期
            var programSubmit = db.ProgramSubmits
                .FirstOrDefault(ps => ps.ParticipantId == participant.Id && ps.ProgramPlanId == evaluation.ProgramPlanId);

            //撈取目前的審核狀態
            var currentReviewStatus = db.EvaluationReviews
                .Where(r => r.EvaluationId == evaluation.Id)
                .OrderByDescending(r => r.ReviewedAt)
                .Select(r => new
                {
                    id = r.Id,
                    status = r.Status.Title
                })
                .FirstOrDefault();

            // 撈取該評價的審核歷史
            var reviews = db.EvaluationReviews
                .Where(r => r.EvaluationId == evaluation.Id)
                .OrderByDescending(r => r.ReviewedAt)
                .Select(r => new
                {
                    id = r.Id,
                    reviewedAt = r.ReviewedAt,
                    reviewer = new
                    {
                        id = r.ReviewerId,
                        name = r.Reviewer.AdminInfo != null ? r.Reviewer.AdminInfo.Name : r.Reviewer.Account
                    },
                    reviewType = r.ReviewTypeId,
                    comment = r.Comment,
                    status = r.Status.Title
                })
                .ToList();

            // 取得 ProgramPlanDto
            var programPlanDto = new ProgramPlanDto
            {
                Name = evaluation.Program?.Name,
                SerialNum = evaluation.Program?.SerialNum,
                Industry = new ProgramPlanDto.SimpleEntityDto
                {
                    Id = evaluation.Program?.IndustryId ?? 0,
                    Title = evaluation.Program?.Industry?.Title
                },
                JobTitle = new ProgramPlanDto.SimpleEntityDto
                {
                    Id = evaluation.Program?.JobTitleId ?? 0,
                    Title = evaluation.Program?.JobTitle?.Title
                },
                Address = evaluation.Program?.Address,
                ProgramStartDate = evaluation.Program?.ProgramStartDate ?? DateTime.MinValue,
                ProgramEndDate = evaluation.Program?.ProgramEndDate ?? DateTime.MinValue,
                ProgramDurationDays = evaluation.Program?.ProgramDurationDays ?? 0
            };

            return Ok(new
            {
                currentStatus = currentReviewStatus?.status,
                id = evaluation.Id,
                submitDate = programSubmit?.SubmitAt,
                updateDate = programSubmit?.ReviewedAt,
                program = new
                {
                    // 區塊三：體驗資訊
                    name = programPlanDto.Name,
                    serialNum = programPlanDto.SerialNum,
                    industry = programPlanDto.Industry?.Title,
                    jobTitle = programPlanDto.JobTitle?.Title,
                    address = programPlanDto.Address,
                    startDate = programPlanDto.ProgramStartDate,
                    endDate = programPlanDto.ProgramEndDate,
                    durationDays = programPlanDto.ProgramDurationDays
                },
                participant = new
                {
                    id = participant.Id,
                    name = participant.Name,
                    identity = participant.Identity?.Title,
                    age = age
                },
                score = evaluation.Score,
                comment = evaluation.Comment,
                reviews = reviews
            });
        }

        // PUT: api/Admin/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutParticipantEvaluation(int id, ParticipantEvaluation participantEvaluation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != participantEvaluation.Id)
            {
                return BadRequest();
            }

            db.Entry(participantEvaluation).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParticipantEvaluationExists(id))
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

        // POST: api/Admin
        [ResponseType(typeof(ParticipantEvaluation))]
        public IHttpActionResult PostParticipantEvaluation(ParticipantEvaluation participantEvaluation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ParticipantEvaluations.Add(participantEvaluation);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = participantEvaluation.Id }, participantEvaluation);
        }

        // DELETE: api/Admin/5
        [ResponseType(typeof(ParticipantEvaluation))]
        public IHttpActionResult DeleteParticipantEvaluation(int id)
        {
            ParticipantEvaluation participantEvaluation = db.ParticipantEvaluations.Find(id);
            if (participantEvaluation == null)
            {
                return NotFound();
            }

            db.ParticipantEvaluations.Remove(participantEvaluation);
            db.SaveChanges();

            return Ok(participantEvaluation);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ParticipantEvaluationExists(int id)
        {
            return db.ParticipantEvaluations.Count(e => e.Id == id) > 0;
        }
    }
}