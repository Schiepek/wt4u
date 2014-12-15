namespace wt4u.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectAllocationDate : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProjectAllocations", "StartDate", c => c.DateTime());
            AlterColumn("dbo.ProjectAllocations", "EndDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProjectAllocations", "EndDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.ProjectAllocations", "StartDate", c => c.DateTime(nullable: false));
        }
    }
}
