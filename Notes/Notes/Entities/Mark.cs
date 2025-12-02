using Notes.Protos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notes.Entities
{
    /// <summary>
    /// Represents a mark associated with a user and a specific note within an notefile.
    /// </summary>
    /// <remarks>The Mark class is typically used to identify notes or responses
    /// within note files, supporting features such as Output.  
    /// Each Mark instance is uniquely identified by a combination of user, note file, archive, and ordinal values. This
    /// class is commonly used in scenarios where user interactions with notes need to be recorded or
    /// Output.</remarks>
    public class Mark
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        [Required]
        [Column(Order = 0)]
        [StringLength(450)]
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the note file identifier.
        /// </summary>
        /// <value>The note file identifier.</value>
        [Required]
        [Column(Order = 1)]
        public int NoteFileId { get; set; }

        /// <summary>
        /// Gets or sets the archive identifier.
        /// </summary>
        /// <value>The archive identifier.</value>
        [Required]
        [Column(Order = 2)]
        public int ArchiveId { get; set; }

        /// <summary>
        /// Gets or sets the mark ordinal.
        /// </summary>
        /// <value>The mark ordinal.</value>
        [Required]
        [Column(Order = 3)]
        public int MarkOrdinal { get; set; }

        /// <summary>
        /// Gets or sets the note ordinal.
        /// </summary>
        /// <value>The note ordinal.</value>
        [Required]
        public int NoteOrdinal { get; set; }

        /// <summary>
        /// Gets or sets the note header identifier.
        /// </summary>
        /// <value>The note header identifier.</value>
        [Required]
        public long NoteHeaderId { get; set; }

        /// <summary>
        /// Gets or sets the response ordinal.
        /// </summary>
        /// <value>The response ordinal.</value>
        [Required]
        public int ResponseOrdinal { get; set; }  // -1 == whole string, 0 base note only, > 0 Response

        /// <summary>
        /// Gets or sets the note file.
        /// </summary>
        /// <value>The note file.</value>
        [ForeignKey("NoteFileId")]
        public NoteFile? NoteFile { get; set; }
    }
}


