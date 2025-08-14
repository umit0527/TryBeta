using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Description;
using TryBeta.Models;
using static TryBeta.Models.CompanInfoDto;

namespace TryBeta.Controllers
{
    [RoutePrefix("api/v1/users")]
    public class ParticipantController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();

        // GET: api/Users
        public IQueryable<Users> GetUsers()
        {
            return db.Users;
        }

        // GET: api/Users/5 取得基本資料
        [HttpGet]
        [Route("{id:int}")]
        [JwtAuthFilter]
        [ResponseType(typeof(ParticipantRegisterDto))]
        public IHttpActionResult GetParticipant(int id)
        {
            // 先從 JwtAuthFilter 裡取得 UserId
            if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
            {
                return Unauthorized();
            }
            int userId = (int)userIdObj;

            // 權限判斷：確認該 userId 是否屬於 id 這個體驗者
            bool hasAccess = db.ParticipantInfoes.Any(c => c.Id == id && c.UserId == userId);
            if (!hasAccess)
            {
                var resp = Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = "權限不足" });
                return ResponseMessage(resp);
            }

            // 1. 從資料庫抓
            var participantEntity = db.ParticipantInfoes.Include(p => p.Identity)
                                                        .Include(p => p.User)
                                                        .FirstOrDefault(c => c.Id == id);
            if (participantEntity == null)
            {
                return NotFound();
            }

            // 2. Entity轉DTO（這裡簡單手動轉）
            var dto = new ParticipantDto
            {
                Name = participantEntity.Name,
                Phone = participantEntity.Phone,
                Birthday = participantEntity.Birthday,
                Headshot = participantEntity.Headshot,
                CityId = participantEntity.CityId,
                DistrictId = participantEntity.DistrictId,
                Street = participantEntity.Street,
                IdentityId = participantEntity.IdentityId,
                IdentityElse = participantEntity.IdentityElse,
                IdentityName = participantEntity.Identity?.Title, // Identity 對應欄位
                Gender = participantEntity.Gender
            };

            // 3. 回傳DTO
            return Ok(dto);
        }

        // PUT: api/Users/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUsers(int id, Users users)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != users.Id)
            {
                return BadRequest();
            }

            db.Entry(users).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersExists(id))
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

        // POST: api/Users 註冊
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(Users))]
        public IHttpActionResult PostUsers(ParticipantRegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage)
                                  .ToList();
                var content = new { status = 400, message = "資料驗證失敗", errors = errors };
                return Content(HttpStatusCode.BadRequest, content);
            }

            // 檢查帳號或 Email 是否已存在
            if (db.Users.Any(u => u.Account == dto.Account))
            {
                return BadRequest("該帳號已被使用");
            }

            if (db.Users.Any(u => u.Email == dto.Email))
            {
                return BadRequest("該 Email 已被使用");
            }

            // 加密密碼
            var hashedPassword = PasswordHasher.HashPassword(dto.Password); // 將密碼(明碼)加鹽雜湊

            // 若帳號和email是獨立 User 表的資料，需要先建立 User
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 建立 Users 物件
                    var user = new Users
                    {
                        Role = "Participant", // 體驗/參與者，Participant較常見
                        Account = dto.Account,
                        Email = dto.Email,
                        PasswordHash = hashedPassword,
                        Status = 1, // 預設啟用
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    db.Users.Add(user);
                    db.SaveChanges();

                    transaction.Commit();

                    return Content(HttpStatusCode.Created, new
                    {
                        status = 201,
                        message= "註冊成功",
                        id= user.Id,
                        Role = "Participant", // 體驗/參與者，Participant較常見
                        Account = dto.Account,
                        Email = dto.Email,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }

                catch (Exception ex)
                {
                    transaction.Rollback();
                    return InternalServerError(ex);
                }
            }
        }

        // DELETE: api/Users/5
        [ResponseType(typeof(Users))]
        public IHttpActionResult DeleteUsers(int id)
        {
            Users users = db.Users.Find(id);
            if (users == null)
            {
                return NotFound();
            }

            db.Users.Remove(users);
            db.SaveChanges();

            return Ok(users);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UsersExists(int id)
        {
            return db.Users.Count(e => e.Id == id) > 0;
        }
    }
}