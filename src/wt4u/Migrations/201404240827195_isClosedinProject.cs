namespace wt4u.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class isClosedinProject : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Projects", "isClosed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Projects", "isClosed");
        }
    }
}
