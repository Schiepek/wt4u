namespace wt4u.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectAllocation_NewField_IsCurrentProject : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProjectAllocations", "IsCurrentProject", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProjectAllocations", "IsCurrentProject");
        }
    }
}
