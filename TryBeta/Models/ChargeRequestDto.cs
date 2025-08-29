using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ChargeRequestDto
    {
        [JsonProperty("plan_id")]
        public int PlanId { get; set; }       // 方案 ID

        [JsonProperty("company_id")]
        public int CompanyId { get; set; }       // 方案 ID

        [JsonProperty("payment_method")]
        public string PaymentMethod { get; set; }  //付款方式

        [JsonProperty("email")]
        public string Email { get; set; }     // 使用者 Email

        [JsonProperty("return_url")] 
        public string ReturnUrl { get; set; } // 藍新付款完成後導回前端的網址

        [JsonProperty("notify_url")] 
        public string NotifyUrl { get; set; } // 藍新付款通知後端的網址

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;    // 建立時間
    }

    public class PaymentCallbackDto
    {
        // 藍新回傳的交易資料，加密過 (AES)
        [JsonProperty("TradeInfo")]
        public string TradeInfo { get; set; }

        // 用 SHA256 驗證的雜湊值
        [JsonProperty("TradeSha")]
        public string TradeSha { get; set; }

        // 藍新可能會回傳交易狀態
        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("plan_id")]
        public int PlanId { get; set; }       // 方案 ID
    }

    public class PaymentTradeInfoDto
    {
        [JsonProperty("MerchantID")]
        public string MerchantId { get; set; }

        [JsonProperty("Amt")]
        public decimal Amount { get; set; }

        [JsonProperty("TradeNo")]
        public string TradeNo { get; set; }

        [JsonProperty("MerchantOrderNo")]
        public string MerchantOrderNo { get; set; }

        [JsonProperty("PaymentType")]
        public string PaymentType { get; set; }

        [JsonProperty("RespondCode")]
        public string RespondCode { get; set; }

        [JsonProperty("AuthBank")]
        public string AuthBank { get; set; }

        [JsonProperty("Card4No")]
        public string Card4No { get; set; }

        [JsonProperty("PayTime")]
        public string PayTime { get; set; }
    }
}