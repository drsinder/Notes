using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Notes.Entities
{
    /// <summary>
    /// This class defines a table in the database.
    /// Used to associate a user and a file for the purpose of
    /// forwarding an email when new notes are written.
    /// </summary>
    [DataContract]
    public class Subscription
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember(Order = 1)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the note file identifier.
        /// </summary>
        /// <value>The note file identifier.</value>
        [Required]
        [DataMember(Order = 2)]
        public int NoteFileId { get; set; }

        /// <summary>
        /// Gets or sets the subscriber identifier.
        /// </summary>
        /// <value>The subscriber identifier.</value>
        [Required]
        [StringLength(450)]
        [DataMember(Order = 3)]
        public string? SubscriberId { get; set; }

        /// <summary>
        /// Gets or sets the note file.
        /// </summary>
        /// <value>The note file.</value>
        [ForeignKey("NoteFileId")]
        [DataMember(Order = 4)]
        public NoteFile? NoteFile { get; set; }
    }
}
