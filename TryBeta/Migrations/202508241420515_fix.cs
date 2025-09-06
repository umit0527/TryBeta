namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class fix : DbMigration
    {
        public override void Up()
        {// 建立外鍵
            //AddForeignKey("dbo.ProgramSubmits", "StatusId", "dbo.ProgramSubmitStatus", "Id", cascadeDelete: false);
            //CreateIndex("dbo.ProgramSubmits", "StatusId");
        }

        public override void Down()
        {
            //DropIndex("dbo.ProgramSubmits", new[] { "StatusId" });
            //DropForeignKey("dbo.ProgramSubmits", "StatusId", "dbo.ProgramSubmitStatus");
        }
    }
}
