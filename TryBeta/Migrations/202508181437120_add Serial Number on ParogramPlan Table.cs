namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addSerialNumberonParogramPlanTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProgramPlans", "SerialNumber", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProgramPlans", "SerialNumber");
        }
    }
}
