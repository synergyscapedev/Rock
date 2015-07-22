// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class FollowingEventsSuggestions : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.FollowingEvent",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(),
                        EntityTypeId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        SendOnWeekends = c.Boolean(nullable: false),
                        LastCheckDateTime = c.DateTime(),
                        IsNoticeRequired = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.FollowingSuggestion",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(),
                        EntityTypeId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);

//            RockMigrationHelper.UpdateEntityType( "Rock.Follow.PersonBirthdayEvent", "532A7405-A3FB-4147-BE67-3B75A230AADE", false, true );
//            Sql( @"
//    DECLARE @EntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '532A7405-A3FB-4147-BE67-3B75A230AADE' )
//    INSERT INTO [FollowingEvent] ( [Name], [Description], [EntityTypeId], [IsActive], [SendOnWeekends], [IsNoticeRequired], [Guid] )
//    VALUES 
//	    ( 'Person Birthday (5 day notice)', 'Five day notice of an upcoming birthday.', @EntityTypeId, 1, 0, 0, 'E1C2F8BD-E875-4C7B-91A1-EDB98AB01BDC' ),
//	    ( 'Person Birthday (day of notice)', 'Notice of a birthday today.', @EntityTypeId, 1, 0, 0, 'F3A577DB-8F4A-4245-BD00-0B2B8F789131' )
//" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.FollowingSuggestion", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FollowingSuggestion", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.FollowingSuggestion", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FollowingEvent", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FollowingEvent", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.FollowingEvent", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.FollowingSuggestion", new[] { "ForeignId" });
            DropIndex("dbo.FollowingSuggestion", new[] { "Guid" });
            DropIndex("dbo.FollowingSuggestion", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.FollowingSuggestion", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.FollowingSuggestion", new[] { "EntityTypeId" });
            DropIndex("dbo.FollowingEvent", new[] { "ForeignId" });
            DropIndex("dbo.FollowingEvent", new[] { "Guid" });
            DropIndex("dbo.FollowingEvent", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.FollowingEvent", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.FollowingEvent", new[] { "EntityTypeId" });
            DropTable("dbo.FollowingSuggestion");
            DropTable("dbo.FollowingEvent");
        }
    }
}
