namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addProgramPlanFeild : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProgramPlans", "AddressMap", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProgramPlans", "AddressMap");
        }
    }
}
