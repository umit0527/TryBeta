using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Globalization;


namespace TryBeta.Models
{
    public class TryBetaDbContext : DbContext
    {
        public TryBetaDbContext() : base("TryBeta") // 對應 web.config 的 connection string 名稱
        {
        }

        public DbSet<CompanyInfoes> Companyinfoes { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<CompanyScales> CompanyScales { get; set; }
        public DbSet<CompanyContacts> CompanyContact { get; set; }
        public DbSet<CompanyImages> CompanyImages { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // 設定 email 欄位唯一
            modelBuilder.Entity<Users>()
                .Property(u => u.Email)
                .HasMaxLength(100)
                .HasColumnAnnotation(
                    "Index",
                    new IndexAnnotation(
                        new IndexAttribute("IX_Unique_Email") { IsUnique = true }));

            base.OnModelCreating(modelBuilder);
        }


    }
}