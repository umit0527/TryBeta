using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
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
    public class CompanyPlanOrdersController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();

        // GET: api/CompanyPlanOrders
        public IQueryable<CompanyPlanOrder> GetCompanyPlanOrders()
        {
            return db.CompanyPlanOrders;
        }

        // GET: api/CompanyPlanOrders/5
        [HttpGet, Route("{companyId:int}/orders/{orderId:int}")]
        [ResponseType(typeof(CompanyPlanOrder))]
        [JwtAuthFilter]
        public IHttpActionResult GetCompanyPlanOrder(int orderId, int companyId)
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
                                  .Include(c => c.User)
                                  .FirstOrDefault(c => c.UserId == userId);

            if (companyEntity == null)
            {
                return NotFound();
            }

            var order = db.CompanyPlanOrders
                      .Include(o => o.Plan)
                      .Include(o => o.CompanyInfoes) // 公司資訊
                      .FirstOrDefault(o => o.CompanyId == companyEntity.Id && o.Id == orderId);

            if (order == null)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    Message = "查無此訂單。"
                });
            }

            var dto = new CompanyPlanOrderDto
            {
                Id = order.Id,
                OrderNum= order.OrderNum,
                PlanName = order.Plan.Name,
                Price= order.Plan.Price,
                DurationDays=order.Plan.DurationDays,
                Maxparticipants=order.Plan.MaxParticipants,
                LastCardNum=order.LastCardNum,
                StartDate = order.StartDate,
                EndDate = order.EndDate,
                PaymentMethod=order.PaymentMethod,
                PaymentStatus=order.PaymentStatus,
                Company = new CompanyInfoDto
                {
                    Id = order.CompanyInfoes.Id,
                    Name = order.CompanyInfoes.Name
                }
            };

            return Ok(dto);
        }


        // PUT: api/CompanyPlanOrders/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutCompanyPlanOrder(int id, CompanyPlanOrder companyPlanOrder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != companyPlanOrder.Id)
            {
                return BadRequest();
            }

            db.Entry(companyPlanOrder).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyPlanOrderExists(id))
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

        // POST: api/CompanyPlanOrders
        [ResponseType(typeof(CompanyPlanOrder))]
        public IHttpActionResult PostCompanyPlanOrder(CompanyPlanOrder companyPlanOrder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.CompanyPlanOrders.Add(companyPlanOrder);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = companyPlanOrder.Id }, companyPlanOrder);
        }

        // DELETE: api/CompanyPlanOrders/5
        [ResponseType(typeof(CompanyPlanOrder))]
        public IHttpActionResult DeleteCompanyPlanOrder(int id)
        {
            CompanyPlanOrder companyPlanOrder = db.CompanyPlanOrders.Find(id);
            if (companyPlanOrder == null)
            {
                return NotFound();
            }

            db.CompanyPlanOrders.Remove(companyPlanOrder);
            db.SaveChanges();

            return Ok(companyPlanOrder);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CompanyPlanOrderExists(int id)
        {
            return db.CompanyPlanOrders.Count(e => e.Id == id) > 0;
        }
    }
}