namespace wt4u.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EndDateNullableInAllocationLeading : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProjectAllocations", "StartDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.ProjectLeadings", "EndDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProjectLeadings", "EndDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.ProjectAllocations", "StartDate", c => c.DateTime());
        }
    }
}
