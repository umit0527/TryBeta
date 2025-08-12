namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using TryBeta.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<TryBeta.Models.TryBetaDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(TryBeta.Models.TryBetaDbContext context)
        {
            context.CompanyScales.AddOrUpdate(
            cs => cs.EmployeeNum,
     new CompanyScales { EmployeeNum = "1-10人" },
     new CompanyScales { EmployeeNum = "11-30人" },
     new CompanyScales { EmployeeNum = "31-50人" },
     new CompanyScales { EmployeeNum = "50-100人" },
     new CompanyScales { EmployeeNum = "101-200人" },
     new CompanyScales { EmployeeNum = "201-500人" }
 );
        }
    }
}
