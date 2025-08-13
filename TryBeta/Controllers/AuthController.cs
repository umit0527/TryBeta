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
    [RoutePrefix("api/v1")]
    public class AuthController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();

        // GET: api/Auth
        public IQueryable<Users> GetUsers()
        {
            return db.Users;
        }

        // GET: api/Auth/5
        [ResponseType(typeof(Users))]
        public IHttpActionResult GetUsers(int id)
        {
            Users users = db.Users.Find(id);
            if (users == null)
            {
                return NotFound();
            }

            return Ok(users);
        }

        // PUT: api/Auth/5
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

        // POST: api/Auth
        [HttpPost]
        [Route("company/login")]
        [ResponseType(typeof(Users))]
        public IHttpActionResult PostUsers(CompanyLoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("登入資料有誤");
            }
                
            // 找出對應的帳號/email
            var user = db.Users.FirstOrDefault(u =>(u.Account == dto.Identifier || u.Email == dto.Identifier) && u.Role == "Company");

            if (user == null)
            {
                return Content(HttpStatusCode.Unauthorized, new
                {
                    status = 401,
                    message = "帳號或密碼錯誤"
                });
            }

            // 驗證密碼
            bool isPasswordValid = PasswordHasher.VerifyPassword(user.PasswordHash, dto.Password);

            if (!isPasswordValid)
            {
                return Content(HttpStatusCode.Unauthorized, new
                {
                    status = 401,
                    message = "帳號或密碼錯誤"
                });
            }

            // 撈取公司資料
            var company = db.Companyinfoes.FirstOrDefault(c => c.UserId == user.Id);

            // 產生 JWT Token
            var jwtUtil = new JwtAuthUtil();
            string token = jwtUtil.GenerateToken(user.Id, user.Account, company?.Name ?? "");

            return Ok(new
            {
                status = 200,
                message = "登入成功",
                token = token,
                user = new
                {
                    user.Id,
                    user.Account,
                    user.Email,
                    user.Role,
                },
                company = new
                {
                    company?.Id,
                    company?.Name,
                    company?.TaxIdNum,
                    company?.IndustryId,
                    company?.Address,
                    company?.Website,
                    company?.Intro,
                    company?.ScaleId
                }
            });
        }

        // DELETE: api/Auth/5
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