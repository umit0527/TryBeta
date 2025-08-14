using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using TryBeta.Models;
using static TryBeta.Models.CompanInfoDto;

namespace TryBeta.Controllers
{
    [RoutePrefix("api/v1/company")]
    public class CompanyController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();

        // GET: api/Company 
        [HttpGet]
        [Route("")]
        public IQueryable<CompanyInfoes> GetCompanyinfos()
        {
            return db.Companyinfoes;
        }

        // GET: api/Company/ 取得登入企業的基本資料
        [HttpGet]
        [Route("{userid:int}")]
        [JwtAuthFilter] 
        [ResponseType(typeof(CompanyRegisterDto))]
        public IHttpActionResult GetMyCompanyInfo()
        {
            // 1. 從 JwtAuthFilter 裡取得 UserId
            if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
            {
                return Unauthorized(); // Token 無效或缺少
            }
            int userId = (int)userIdObj;

            // 2. 從資料庫抓取登入者所屬的公司資料
            //    Include 關聯表：CompanyContacts、CompanyImages、User
            var companyEntity = db.Companyinfoes
                                  .Include(c => c.CompanyContacts)
                                  .Include(c => c.CompanyImages)
                                  .Include(c => c.User)
                                  .FirstOrDefault(c => c.UserId == userId);

            if (companyEntity == null)
            {
                return NotFound(); 
            }

            // 3. Entity 轉 DTO（這裡簡單手動轉）
            var dto = new CompanyRegisterDto
            {
                Name = companyEntity.Name,
                IndustryId = companyEntity.IndustryId,
                TaxIdNum = companyEntity.TaxIdNum,
                Address = companyEntity.Address,
                Website = companyEntity.Website,
                Intro = companyEntity.Intro,
                ScaleId = companyEntity.ScaleId,
                Account = companyEntity.User.Account,  // 回傳帳號（密碼不回傳）
                Email = companyEntity.User.Email,
                // 轉聯絡人
                CompanyContact = companyEntity.CompanyContacts.Select(contact => new CompanyContactDto
                {
                    Name = contact.Name,
                    JobTitle = contact.JobTitle,
                    Email = contact.Email,
                    Phone = contact.Phone
                }).ToList(),
                // 轉圖片
                CompanyImg = companyEntity.CompanyImages.Select(img => new CompanyImgDto
                {
                    Type = img.Type,
                    ImgPath = img.ImgPath
                }).ToList()
            };

            // 4. 回傳 DTO
            return Ok(dto);
        }

        // PUT: api/CompanyRegister/5
        [HttpPut]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutCompanyInfo(int id, CompanyInfoes companyInfo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != companyInfo.Id)
            {
                return BadRequest();
            }

            db.Entry(companyInfo).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyInfoExists(id))
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

        // POST: api/Company 註冊 
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(CompanyInfoes))]
        public IHttpActionResult PostCompanyInfo(CompanyRegisterDto dto)
        {
            var allErrors = new List<string>();

            // 先跑 ModelState 驗證
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage)
                                  .ToList();
                allErrors.AddRange(modelErrors);
            }

            // 檢查帳號、Email 在 Users 表中是否已存在，409
            bool accountExists = db.Users.Any(u => u.Account == dto.Account);
            if (accountExists)
            {
                allErrors.Add("該帳號已被使用");
            }

            bool emailExists = db.Users.Any(u => u.Email == dto.Email);
            if (emailExists)
            {
                allErrors.Add("該 Email 已被使用");
            }

            // 企業名稱和統編在 CompanyInfo 表中檢查
            bool nameExists = db.Companyinfoes.Any(c => c.Name == dto.Name);
            if (nameExists)
            {
                allErrors.Add("該企業名稱已被使用");
            }

            bool taxExists = db.Companyinfoes.Any(c => c.TaxIdNum == dto.TaxIdNum);
            if (taxExists)
            {
                allErrors.Add("該統編已被使用");
            }

            // 如果有任何錯誤就統一回傳
            if (allErrors.Any())
            {
                var content = new
                {
                    status = 400,
                    message = "註冊資料有誤",
                    errors = allErrors
                };
                return Content(HttpStatusCode.BadRequest, content);
            }

            var hashedPassword = PasswordHasher.HashPassword(dto.Password); // 將密碼(明碼)加鹽雜湊
            
            // 若帳號和email是獨立 User 表的資料，需要先建立 User
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 建立 User
                    var user = new Users
                    {
                        Account = dto.Account,
                        Email = dto.Email,
                        PasswordHash = hashedPassword,
                        Role = "Company",
                        Status = 1, // 預設啟用
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    db.Users.Add(user);
                    db.SaveChanges();

                    // 建立企業基本資料
                    var company = new CompanyInfoes
                    {
                        Name = dto.Name,
                        IndustryId = dto.IndustryId,
                        TaxIdNum = dto.TaxIdNum,
                        Address = dto.Address,
                        Website = dto.Website,
                        Intro = dto.Intro,
                        ScaleId = dto.ScaleId,
                        UserId = user.Id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    };
                    db.Companyinfoes.Add(company);
                    db.SaveChanges();

                    // 建立聯絡人資料
                    if (dto.CompanyContact != null && dto.CompanyContact.Count > 0)
                    {
                        foreach (var contactDto in dto.CompanyContact)
                        {
                            var contact = new CompanyContacts
                            {
                                CompanyId = company.Id,
                                Name = contactDto.Name,
                                JobTitle = contactDto.JobTitle,
                                Email = contactDto.Email,
                                Phone = contactDto.Phone,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };
                            db.CompanyContact.Add(contact);
                        }
                        db.SaveChanges();
                    }

                    //建立圖片
                    if (dto.CompanyImg != null && dto.CompanyImg.Any())
                    {
                        //檢查是否超過6張
                        var environmentCount = dto.CompanyImg.Count(i => i.Type == "environment");
                        if (environmentCount > 6)
                        {
                            return BadRequest("環境照片最多只能上傳 6 張");
                        }

                        //加入圖片
                        foreach (var imgDto in dto.CompanyImg)
                        {
                            var image = new CompanyImages
                            {
                                CompanyId = company.Id,
                                Type = imgDto.Type,
                                ImgPath = imgDto.ImgPath,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };
                            db.CompanyImages.Add(image);
                        }
                        db.SaveChanges();
                    }
                    transaction.Commit();

                    return Content(HttpStatusCode.Created, new
                    {
                        status=201,
                        message = "註冊成功",
                        Name = dto.Name,
                        IndustryId = dto.IndustryId,
                        TaxIdNum = dto.TaxIdNum,
                        Address = dto.Address,
                        Website = dto.Website,
                        Intro = dto.Intro,
                        ScaleId = dto.ScaleId,
                        UserId = user.Id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    });  //201
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return InternalServerError(ex);
                }
            }
        }

        // DELETE: api/CompanyRegister/5
        [HttpDelete]
        [Route("{id:int}")]
        [ResponseType(typeof(CompanyInfoes))]
        public IHttpActionResult DeleteCompanyInfo(int id)
        {
            CompanyInfoes companyInfo = db.Companyinfoes.Find(id);
            if (companyInfo == null)
            {
                return NotFound();
            }

            db.Companyinfoes.Remove(companyInfo);
            db.SaveChanges();

            return Ok(companyInfo);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        private bool CompanyInfoExists(int id)
        {
            return db.Companyinfoes.Count(e => e.Id == id) > 0;
        }

        private IHttpActionResult Conflict(string message)
        {
            var content = new { status = 409, message = message };
            return Content(HttpStatusCode.Conflict, content);
        }


    }
}