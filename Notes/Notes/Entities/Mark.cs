using Notes.Protos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notes.Entities
{
    /// <summary>
    /// Class Mark.
    /// </summary>
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


