using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Notes.Entities;

/// <summary>
/// The Notes.Data namespace contains the Entity Framework Core database context for the notes application,
/// providing access to note-related entities and ASP.NET Core Identity features.
/// </summary>
namespace Notes.Data;
/// <summary>
/// Represents the Entity Framework Core database context for the notes application, providing access to note-related
/// entities and ASP.NET Core Identity features.
/// </summary>
/// <remarks>This context integrates with ASP.NET Core Identity by inheriting from
/// IdentityDbContext<ApplicationUser>, enabling user authentication and authorization features alongside application
/// data. Use this context to query and save instances of note files, note content, tags, audits, and related entities.
/// The context should be registered with the dependency injection container and disposed of appropriately.</remarks>
/// <param name="options">The options to be used by the DbContext. Typically includes configuration such as the database provider and
/// connection string. Cannot be null.</param>
public class NotesDbContext(DbContextOptions<NotesDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    /// <summary>
    /// Gets or sets the note file.
    /// </summary>
    /// <value>The note file.</value>
    public DbSet<NoteFile> NoteFile { get; set; }
    /// <summary>
    /// Gets or sets the note access.
    /// </summary>
    /// <value>The note access.</value>
    public DbSet<NoteAccess> NoteAccess { get; set; }
    /// <summary>
    /// Gets or sets the note header.
    /// </summary>
    /// <value>The note header.</value>
    public DbSet<NoteHeader> NoteHeader { get; set; }
    /// <summary>
    /// Gets or sets the content of the note.
    /// </summary>
    /// <value>The content of the note.</value>
    public DbSet<NoteContent> NoteContent { get; set; }
    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    /// <value>The tags.</value>
    public DbSet<Tags> Tags { get; set; }

    /// <summary>
    /// Gets or sets the audit.
    /// </summary>
    /// <value>The audit.</value>
    public DbSet<Audit> Audit { get; set; }
    /// <summary>
    /// Gets or sets the home page message.
    /// </summary>
    /// <value>The home page message.</value>
    public DbSet<HomePageMessage> HomePageMessage { get; set; }
    /// <summary>
    /// Gets or sets the mark.
    /// </summary>
    /// <value>The mark.</value>
    public DbSet<Mark> Mark { get; set; }
    /// <summary>
    /// Gets or sets the search.
    /// </summary>
    /// <value>The search.</value>
    public DbSet<Search> Search { get; set; }
    /// <summary>
    /// Gets or sets the sequencer.
    /// </summary>
    /// <value>The sequencer.</value>
    public DbSet<Sequencer> Sequencer { get; set; }
    /// <summary>
    /// Gets or sets the subscription.
    /// </summary>
    /// <value>The subscription.</value>
    public DbSet<Subscription> Subscription { get; set; }

    /// <summary>
    /// Gets or sets the SQL file.
    /// </summary>
    /// <value>The SQL file.</value>
    public DbSet<SQLFile> SQLFile { get; set; }
    /// <summary>
    /// Gets or sets the content of the SQL file.
    /// </summary>
    /// <value>The content of the SQL file.</value>
    public DbSet<SQLFileContent> SQLFileContent { get; set; }

    //public void AddJsonFile(string v, bool optional)
    //{
    //    throw new NotImplementedException();
    //}

    /// <summary>
    /// Gets or sets the linked file.
    /// </summary>
    /// <value>The linked file.</value>
    public DbSet<LinkedFile> LinkedFile { get; set; }
    /// <summary>
    /// Gets or sets the link queue.
    /// </summary>
    /// <value>The link queue.</value>
    public DbSet<LinkQueue> LinkQueue { get; set; }
    /// <summary>
    /// Gets or sets the link log.
    /// </summary>
    /// <value>The link log.</value>
    public DbSet<LinkLog> LinkLog { get; set; }


    /// <summary>
    /// Called when [model creating].
    /// </summary>
    /// <param name="builder">The builder.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);


        builder.Entity<NoteAccess>()
            .HasKey(new string[] { "UserID", "NoteFileId", "ArchiveId" });

        builder.Entity<NoteHeader>()
            .HasOne(p => p.NoteContent);
        //.WithOne(i => i.NoteHeader)
        //.HasForeignKey<NoteContent>(b => b.NoteHeaderId);

        builder.Entity<NoteHeader>()
            .HasIndex(new string[] { "NoteFileId" });

        builder.Entity<NoteHeader>()
            .HasIndex(new string[] { "NoteFileId", "ArchiveId" });

        builder.Entity<NoteHeader>()
            .HasIndex(new string[] { "LinkGuid" });

        builder.Entity<Tags>()
            .HasKey(new string[] { "Tag", "NoteHeaderId" });

        builder.Entity<Tags>()
            .HasIndex(new string[] { "NoteFileId" });

        builder.Entity<Tags>()
            .HasIndex(new string[] { "NoteFileId", "ArchiveId" });

        builder.Entity<Sequencer>()
            .HasKey(new string[] { "UserId", "NoteFileId" });

        builder.Entity<Search>()
            .HasKey(new string[] { "UserId" });

        builder.Entity<Mark>()
            .HasKey(new string[] { "UserId", "NoteFileId", /*"ArchiveId",*/ "MarkOrdinal" });

        builder.Entity<Mark>()
            .HasIndex(new string[] { "UserId", "NoteFileId" });

        //builder.Entity<Mark>()
        //    .HasIndex(new string[] { "UserId", "NoteFileId", "NoteOrdinal" });

        builder.Entity<SQLFile>()
            .HasOne(p => p.Content)
            .WithOne(i => i.SQLFile)
            .HasForeignKey<SQLFileContent>(b => b.SQLFileId);


        builder.Entity<NoteContent>()
            .Property(p => p.NoteBody)
            .UseCollation("SQL_Latin1_General_CP1_CS_AS"); // Case Sensitive collation for NoteBody for case sensitive searches


        //builder.Entity<IdentityRole>().HasData(
        //    new IdentityRole { Name = "User", NormalizedName = "USER", Id = "7550e722-3cc2-4731-b4fc-e9ba0e6d20f3" },
        //    new IdentityRole { Name = "Admin", NormalizedName = "ADMIN", Id = "c0fd3f06-97b7-4c72-afa6-96e250749dc7" },
        //    new IdentityRole { Name = "Guest", NormalizedName = "GUEST", Id = "f12ec6c2-a191-4076-ba71-a9ad3fbc9216" }
        //    );

    }
}


