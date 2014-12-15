namespace wt4u.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class endtimesnullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Breaks", "endTime", c => c.DateTime());
            AlterColumn("dbo.Projects", "StartDate", c => c.DateTime());
            AlterColumn("dbo.Projects", "EndDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Projects", "EndDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Projects", "StartDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Breaks", "endTime", c => c.DateTime(nullable: false));
        }
    }
}
