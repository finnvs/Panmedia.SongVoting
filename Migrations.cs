using System;
using System.Data;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Panmedia.SongVoting.Models;
using System.Runtime.InteropServices;

namespace Panmedia.SongVoting
{
    [ComVisible(false)]
    public class Migrations : DataMigrationImpl
    {

        public int Create()
        {
            // Creating table PollChoiceRecord
            SchemaBuilder.CreateTable("PollChoiceRecord", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("Answer", DbType.String)
                .Column("PollPartRecord_Id", DbType.Int32)                
            );

            // Creating table PollResultRecord
            SchemaBuilder.CreateTable("PollResultRecord", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("Count", DbType.Int32)
                .Column("PollChoiceRecord_Id", DbType.Int32)
            );

            // Creating table PollVoteRecord
            SchemaBuilder.CreateTable("PollVoteRecord", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("Username", DbType.String)
                .Column("Fingerprint", DbType.String)
                .Column("VoteDateUtc", DbType.DateTime)
                .Column("PollPartRecord_Id", DbType.Int32)
                .Column("PollChoiceRecord_Id", DbType.Int32)
            );

            // Creating table PollPartRecord
            SchemaBuilder.CreateTable("PollPartRecord", table => table
                .ContentPartRecord()
                .Column("Question", DbType.String)
                .Column<DateTime>("OpenDateUtc")
                .Column<DateTime>("CloseDateUtc")
                .Column("MaxVotes", DbType.Int32)
                .Column("Shown", DbType.Boolean)
                .Column("MusicPoll", DbType.Boolean)
            );

            ContentDefinitionManager.AlterPartDefinition(
                typeof(PollPart).Name, cfg => cfg.Attachable());

            return 1;
        }

        public int UpdateFrom1()
        {
            ContentDefinitionManager.AlterTypeDefinition(
                "Poll", cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("PollPart")
                    .WithPart("IdentityPart")
                    .WithPart("ContainablePart")
                    .Creatable());
            return 2;
        }

        public int UpdateFrom2()
        {
            ContentDefinitionManager.AlterTypeDefinition(
                "Poll", cfg => cfg
                    .WithPart("TitlePart"));
            return 3;
        }

        public int UpdateFrom3()
        {
            // Creating table SongPartRecord
            SchemaBuilder.CreateTable("SongPartRecord", table => table
                .ContentPartRecord()
                .Column("SongTitle", DbType.String)
                .Column("SongAuthor", DbType.String)
                .Column("SongUrl", DbType.String)
                .Column("SongThumbnailUrl", DbType.String)
                .Column("SongDescription", DbType.String)
                .Column("SongCategory", DbType.String)
                .Column("SongEditorUserName", DbType.String)
                .Column("SongEditorUserItemId", DbType.Int32)
                .Column<DateTime>("CreatedDateUtc")
                .Column<DateTime>("SubmittedDateUtc")
                .Column("Votes", DbType.Int32)
                .Column("Shown", DbType.Boolean)
                .Column("PollChoiceRecord_Id", DbType.Int32)
            );

            ContentDefinitionManager.AlterPartDefinition(
                typeof(SongPart).Name, cfg => cfg.Attachable());

            // Enum field for song category DK / UK           
            string songCategoryOptions = string.Join(System.Environment.NewLine,
                new[] { "Original", "Cowrite", "IntOriginal", "IntCowrite" });

            ContentDefinitionManager.AlterPartDefinition(typeof(SongPart).Name, builder => builder
                .WithField("SongCategory", cfg => cfg
                .OfType("EnumerationField")
                .WithDisplayName("SongCategory")
                .WithSetting("EnumerationFieldSettings.Required", "True")
                .WithSetting("EnumerationFieldSettings.ListMode", "Radiobutton")
                .WithSetting("EnumerationFieldSettings.Options", songCategoryOptions))
            );
                       
            return 4;
        }

        public int UpdateFrom4()
        {
            ContentDefinitionManager.AlterTypeDefinition(
                "Song", cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("SongPart")
                    .WithPart("IdentityPart")
                    .WithPart("ContainablePart")
                    .WithPart("TitlePart"));

            return 5;
        }

        // set SongUrl column to long string - default SQL CE behavior is to truncate strings,
        // because column string type is set to NVARCHAR(255) if no max string length is given
        public int UpdateFrom5()
        {
            SchemaBuilder.AlterTable("SongPartRecord",
                table =>
                {
                    table.AlterColumn("SongUrl", x => x.WithType(DbType.String).Unlimited());                    
                }
            );
            return 6;
        }

        public int UpdateFrom6()
        {
            SchemaBuilder.AlterTable("PollPartRecord",
                table =>
                {
                    table.AddColumn("ShowVotingUI", DbType.Boolean);
                }
            );
            return 7;
        }

        public int UpdateFrom7()
        {
            SchemaBuilder.AlterTable("PollPartRecord",
                table =>
                {
                    table.AddColumn("MusicPollInTestMode", DbType.Boolean);
                }
            );
            return 8;
        }       

    }
}
