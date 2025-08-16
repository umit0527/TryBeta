using Jose;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class CompanyPlanOrder
    {
        [JsonProperty("id")]
        public int Id { get; set; }                  // 訂單 ID

        [JsonProperty("order_num")]
        [Required]
        public string OrderNum { get; set; }         // 訂單編號

        [ForeignKey("CompanyInfoes")]
        [JsonProperty("company_id")]
        [Required(ErrorMessage = "缺少企業 ID")]
        public int CompanyId { get; set; }           // 企業 ID (FK)

        [ForeignKey("Plan")]
        [JsonProperty("plan_id")]
        [Required(ErrorMessage = "缺少方案 ID")]

        public int PlanId { get; set; }              // 方案 ID (FK)

        [Column(TypeName = "decimal")]
        [JsonProperty("price")]
        [Range(0, 999999, ErrorMessage = "價格必須為正數")]
        public decimal Price { get; set; }           // 購買價格（當下成交價）

        [JsonProperty("purchase_date")]
        [Required]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;   // 購買日期
        
        [JsonProperty("start_date")]
        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;      // 方案生效日期

        [JsonProperty("end_date")]
        public DateTime? EndDate { get; set; }        // 方案到期日期

        [JsonProperty("payment_status")]
        [MaxLength(50)]
        [Required]
        public string PaymentStatus { get; set; }    // 付款狀態（Pending, Paid, Failed）
        
        [JsonProperty("payment_method")]
        [MaxLength(50)]
        [Required]
        public string PaymentMethod { get; set; }    // 付款方式（CreditCard, BankTransfer...）

        [JsonProperty("last_card_num")]
        [MaxLength(4)]
        [Required]
        public string LastCardNum { get; set; }    // 卡片末四碼

        [JsonProperty("created_at")]
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;      // 建立時間

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;      // 更新時間

        // 關聯屬性 
        public virtual CompanyInfoes CompanyInfoes { get; set; } // 企業資訊
        public virtual Plan Plan { get; set; }       // 方案資訊
    }
}