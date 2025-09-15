namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixProgramSubmit2 : DbMigration
    {
        public override void Up()
        {
            // 建立 StatusId 的索引
            CreateIndex("dbo.ProgramSubmits", "StatusId");
            // 建立 StatusId 的外鍵
            AddForeignKey("dbo.ProgramSubmits", "StatusId", "dbo.ProgramSubmitStatus", "Id");
        }

        public override void Down()
        {
            // 刪除 StatusId 的外鍵
            DropForeignKey("dbo.ProgramSubmits", "StatusId", "dbo.ProgramSubmitStatus");
            // 刪除 StatusId 的索引
            DropIndex("dbo.ProgramSubmits", new[] { "StatusId" });
        }
    }
}
