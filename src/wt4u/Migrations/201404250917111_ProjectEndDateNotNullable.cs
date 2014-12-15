namespace wt4u.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectEndDateNotNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Projects", "EndDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Projects", "EndDate", c => c.DateTime());
        }
    }
}
