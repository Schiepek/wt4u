namespace wt4u.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Start_End_Break : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Breaks", "Start", c => c.DateTime(nullable: false));
            AddColumn("dbo.Breaks", "End", c => c.DateTime());
            DropColumn("dbo.Breaks", "StartTime");
            DropColumn("dbo.Breaks", "endTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Breaks", "endTime", c => c.DateTime());
            AddColumn("dbo.Breaks", "StartTime", c => c.DateTime(nullable: false));
            DropColumn("dbo.Breaks", "End");
            DropColumn("dbo.Breaks", "Start");
        }
    }
}
