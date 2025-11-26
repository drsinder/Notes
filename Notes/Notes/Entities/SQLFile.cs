using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notes.Entities
{
    /// <summary>
    /// This class defines a table in the database.
    /// Not currently in use.
    /// </summary>
    public class SQLFile
    {
        /// <summary>
        /// Gets or sets the file identifier.
        /// </summary>
        /// <value>The file identifier.</value>
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FileId { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        [Required]
        [StringLength(300)]
        public string? FileName { get; set; }

        /// <summary>
        /// Gets or sets the type of the content.
        /// </summary>
        /// <value>The type of the content.</value>
        [Required]
        [StringLength(100)]
        public string? ContentType { get; set; }

        /// <summary>
        /// Gets or sets the contributor.
        /// </summary>
        /// <value>The contributor.</value>
        [Required]
        [StringLength(300)]
        public string? Contributor { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>The content.</value>
        public SQLFileContent? Content { get; set; }


        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        /// <value>The comments.</value>
        [StringLength(1000)]
        public string? Comments { get; set; }

    }

    /// <summary>
    /// This class defines a table in the database.
    /// Not currently in use.
    /// </summary>
    public class SQLFileContent
    {

        /// <summary>
        /// Gets or sets the SQL file identifier.
        /// </summary>
        /// <value>The SQL file identifier.</value>
        [Key]
        public long SQLFileId { get; set; }

        /// <summary>
        /// Gets or sets the SQL file.
        /// </summary>
        /// <value>The SQL file.</value>
        public SQLFile? SQLFile { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>The content.</value>
        [Required]
        public byte[]? Content { get; set; }
    }
}

