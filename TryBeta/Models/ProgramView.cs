using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class ProgramView
    {
        public int Id { get; set; }
        public int ProgramPlanId { get; set; }
        public DateTime ViewedAt { get; set; }
        public string ViewerIp { get; set; }       // 可選：紀錄 IP
        public int? ViewerUserId { get; set; }     // 可選：登入使用者 ID

        public virtual ProgramPlan ProgramPlan { get; set; }
    }
}