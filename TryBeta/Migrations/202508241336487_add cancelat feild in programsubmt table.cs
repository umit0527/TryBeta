namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addcancelatfeildinprogramsubmttable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProgramSubmits", "CancelAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProgramSubmits", "CancelAt");
        }
    }
}
