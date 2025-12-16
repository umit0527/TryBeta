namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addFeildScoreinProramPlanTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProgramPlans", "Score", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProgramPlans", "Score");
        }
    }
}
