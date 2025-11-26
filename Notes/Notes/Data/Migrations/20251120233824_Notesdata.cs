using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notes.Migrations
{
    /// <inheritdoc />
    public partial class Notesdata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ipref0",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ipref1",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ipref2",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ipref3",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ipref4",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ipref5",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ipref6",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ipref7",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ipref8",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ipref9",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MyGuid",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Pref0",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pref1",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pref2",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pref3",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pref4",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pref5",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pref6",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pref7",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pref8",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pref9",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TimeZoneID",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Audit",
                columns: table => new
                {
                    AuditID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    EventTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Event = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit", x => x.AuditID);
                });

            migrationBuilder.CreateTable(
                name: "HomePageMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Posted = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomePageMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LinkedFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomeFileId = table.Column<int>(type: "int", nullable: false),
                    HomeFileName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RemoteFileName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RemoteBaseUri = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AcceptFrom = table.Column<bool>(type: "bit", nullable: false),
                    SendTo = table.Column<bool>(type: "bit", nullable: false),
                    Secret = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkedFile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LinkLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EventTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Event = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LinkQueue",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LinkedFileId = table.Column<int>(type: "int", nullable: false),
                    LinkGuid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Activity = table.Column<int>(type: "int", nullable: false),
                    BaseUri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Enqueued = table.Column<bool>(type: "bit", nullable: false),
                    Secret = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkQueue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NoteAccess",
                columns: table => new
                {
                    UserID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    NoteFileId = table.Column<int>(type: "int", nullable: false),
                    ArchiveId = table.Column<int>(type: "int", nullable: false),
                    ReadAccess = table.Column<bool>(type: "bit", nullable: false),
                    Respond = table.Column<bool>(type: "bit", nullable: false),
                    Write = table.Column<bool>(type: "bit", nullable: false),
                    SetTag = table.Column<bool>(type: "bit", nullable: false),
                    DeleteEdit = table.Column<bool>(type: "bit", nullable: false),
                    ViewAccess = table.Column<bool>(type: "bit", nullable: false),
                    EditAccess = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteAccess", x => new { x.UserID, x.NoteFileId, x.ArchiveId });
                });

            migrationBuilder.CreateTable(
                name: "NoteFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumberArchives = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    NoteFileName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NoteFileTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LastEdited = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteFile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NoteHeader",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoteFileId = table.Column<int>(type: "int", nullable: false),
                    ArchiveId = table.Column<int>(type: "int", nullable: false),
                    BaseNoteId = table.Column<long>(type: "bigint", nullable: false),
                    NoteOrdinal = table.Column<int>(type: "int", nullable: false),
                    ResponseOrdinal = table.Column<int>(type: "int", nullable: false),
                    NoteSubject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LastEdited = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThreadLastEdited = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResponseCount = table.Column<int>(type: "int", nullable: false),
                    AuthorID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AuthorName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LinkGuid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RefId = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    DirectorMessage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteHeader", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Search",
                columns: table => new
                {
                    NoteFileId = table.Column<int>(type: "int", nullable: false),
                    ArchiveId = table.Column<int>(type: "int", nullable: false),
                    BaseOrdinal = table.Column<int>(type: "int", nullable: false),
                    ResponseOrdinal = table.Column<int>(type: "int", nullable: false),
                    NoteID = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Option = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Search", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Sequencer",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    NoteFileId = table.Column<int>(type: "int", nullable: false),
                    Ordinal = table.Column<int>(type: "int", nullable: false),
                    LastTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sequencer", x => new { x.UserId, x.NoteFileId });
                });

            migrationBuilder.CreateTable(
                name: "SQLFile",
                columns: table => new
                {
                    FileId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Contributor = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SQLFile", x => x.FileId);
                });

            migrationBuilder.CreateTable(
                name: "Mark",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    NoteFileId = table.Column<int>(type: "int", nullable: false),
                    ArchiveId = table.Column<int>(type: "int", nullable: false),
                    MarkOrdinal = table.Column<int>(type: "int", nullable: false),
                    NoteOrdinal = table.Column<int>(type: "int", nullable: false),
                    NoteHeaderId = table.Column<long>(type: "bigint", nullable: false),
                    ResponseOrdinal = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mark", x => new { x.UserId, x.NoteFileId, x.MarkOrdinal });
                    table.ForeignKey(
                        name: "FK_Mark_NoteFile_NoteFileId",
                        column: x => x.NoteFileId,
                        principalTable: "NoteFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscription",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoteFileId = table.Column<int>(type: "int", nullable: false),
                    SubscriberId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscription_NoteFile_NoteFileId",
                        column: x => x.NoteFileId,
                        principalTable: "NoteFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NoteContent",
                columns: table => new
                {
                    NoteHeaderId = table.Column<long>(type: "bigint", nullable: false),
                    NoteBody = table.Column<string>(type: "nvarchar(max)", maxLength: 100000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteContent", x => x.NoteHeaderId);
                    table.ForeignKey(
                        name: "FK_NoteContent_NoteHeader_NoteHeaderId",
                        column: x => x.NoteHeaderId,
                        principalTable: "NoteHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    NoteHeaderId = table.Column<long>(type: "bigint", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NoteFileId = table.Column<int>(type: "int", nullable: false),
                    ArchiveId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => new { x.Tag, x.NoteHeaderId });
                    table.ForeignKey(
                        name: "FK_Tags_NoteHeader_NoteHeaderId",
                        column: x => x.NoteHeaderId,
                        principalTable: "NoteHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SQLFileContent",
                columns: table => new
                {
                    SQLFileId = table.Column<long>(type: "bigint", nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SQLFileContent", x => x.SQLFileId);
                    table.ForeignKey(
                        name: "FK_SQLFileContent_SQLFile_SQLFileId",
                        column: x => x.SQLFileId,
                        principalTable: "SQLFile",
                        principalColumn: "FileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mark_NoteFileId",
                table: "Mark",
                column: "NoteFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Mark_UserId_NoteFileId",
                table: "Mark",
                columns: new[] { "UserId", "NoteFileId" });

            migrationBuilder.CreateIndex(
                name: "IX_NoteHeader_LinkGuid",
                table: "NoteHeader",
                column: "LinkGuid");

            migrationBuilder.CreateIndex(
                name: "IX_NoteHeader_NoteFileId",
                table: "NoteHeader",
                column: "NoteFileId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteHeader_NoteFileId_ArchiveId",
                table: "NoteHeader",
                columns: new[] { "NoteFileId", "ArchiveId" });

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_NoteFileId",
                table: "Subscription",
                column: "NoteFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_NoteFileId",
                table: "Tags",
                column: "NoteFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_NoteFileId_ArchiveId",
                table: "Tags",
                columns: new[] { "NoteFileId", "ArchiveId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_NoteHeaderId",
                table: "Tags",
                column: "NoteHeaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Audit");

            migrationBuilder.DropTable(
                name: "HomePageMessage");

            migrationBuilder.DropTable(
                name: "LinkedFile");

            migrationBuilder.DropTable(
                name: "LinkLog");

            migrationBuilder.DropTable(
                name: "LinkQueue");

            migrationBuilder.DropTable(
                name: "Mark");

            migrationBuilder.DropTable(
                name: "NoteAccess");

            migrationBuilder.DropTable(
                name: "NoteContent");

            migrationBuilder.DropTable(
                name: "Search");

            migrationBuilder.DropTable(
                name: "Sequencer");

            migrationBuilder.DropTable(
                name: "SQLFileContent");

            migrationBuilder.DropTable(
                name: "Subscription");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "SQLFile");

            migrationBuilder.DropTable(
                name: "NoteFile");

            migrationBuilder.DropTable(
                name: "NoteHeader");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Ipref0",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Ipref1",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Ipref2",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Ipref3",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Ipref4",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Ipref5",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Ipref6",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Ipref7",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Ipref8",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Ipref9",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MyGuid",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pref0",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pref1",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pref2",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pref3",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pref4",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pref5",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pref6",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pref7",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pref8",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pref9",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TimeZoneID",
                table: "AspNetUsers");
        }
    }
}
