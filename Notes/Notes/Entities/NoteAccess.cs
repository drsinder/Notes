using Notes.Protos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Notes.Entities
{
    /// <summary>
    /// This class defines a table in the database.
    /// Objects of this class are Access Tokens for a file.
    /// There are a minimum of two for each file:
    /// 1 for the file Owner.
    /// 1 for the unknown "Other" user - if an entry is not
    /// found for a user, this is the fallback.
    /// There COULD be one for each user.  But the Other entry is
    /// usually used for public file and so not too many other entries
    /// are needed.
    /// The fields should be self evident.
    /// </summary>
    [DataContract]
    public class NoteAccess
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        [Required]
        [Column(Order = 0)]
        [StringLength(450)]
        [DataMember(Order = 1)]
        public string? UserID { get; set; }

        /// <summary>
        /// Gets or sets the note file identifier.
        /// </summary>
        /// <value>The note file identifier.</value>
        [Required]
        [Column(Order = 1)]
        [DataMember(Order = 2)]
        public int NoteFileId { get; set; }

        /// <summary>
        /// Gets or sets the archive identifier.
        /// </summary>
        /// <value>The archive identifier.</value>
        [Required]
        [Column(Order = 2)]
        [DataMember(Order = 3)]
        public int ArchiveId { get; set; }

        // Control options

        /// <summary>
        /// Gets or sets a value indicating whether [read access].
        /// </summary>
        /// <value><c>true</c> if [read access]; otherwise, <c>false</c>.</value>
        [Required]
        [Display(Name = "Read")]
        [DataMember(Order = 4)]
        public bool ReadAccess { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="NoteAccess"/> is respond.
        /// </summary>
        /// <value><c>true</c> if respond; otherwise, <c>false</c>.</value>
        [Required]
        [Display(Name = "Respond")]
        [DataMember(Order = 5)]
        public bool Respond { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="NoteAccess"/> is write.
        /// </summary>
        /// <value><c>true</c> if write; otherwise, <c>false</c>.</value>
        [Required]
        [Display(Name = "Write")]
        [DataMember(Order = 6)]
        public bool Write { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [set tag].
        /// </summary>
        /// <value><c>true</c> if [set tag]; otherwise, <c>false</c>.</value>
        [Required]
        [Display(Name = "Set Tag")]
        [DataMember(Order = 7)]
        public bool SetTag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [delete edit].
        /// </summary>
        /// <value><c>true</c> if [delete edit]; otherwise, <c>false</c>.</value>
        [Required]
        [Display(Name = "Delete/Edit")]
        [DataMember(Order = 8)]
        public bool DeleteEdit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [view access].
        /// </summary>
        /// <value><c>true</c> if [view access]; otherwise, <c>false</c>.</value>
        [Required]
        [Display(Name = "View Access")]
        [DataMember(Order = 9)]
        public bool ViewAccess { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [edit access].
        /// </summary>
        /// <value><c>true</c> if [edit access]; otherwise, <c>false</c>.</value>
        [Required]
        [Display(Name = "Edit Access")]
        [DataMember(Order = 10)]
        public bool EditAccess { get; set; }


        /// <summary>
        /// Gets the note access.
        /// Conversions between Db Entity space and gRPC space.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>NoteAccess.</returns>
        public static NoteAccess GetNoteAccess(GNoteAccess other)
        {
            NoteAccess a = new NoteAccess();
            a.UserID = other.UserID;
            a.NoteFileId = other.NoteFileId;
            a.ArchiveId = other.ArchiveId;
            a.ReadAccess = other.ReadAccess;
            a.Respond = other.Respond;
            a.Write = other.Write;
            a.SetTag = other.SetTag;
            a.DeleteEdit = other.DeleteEdit;
            a.ViewAccess = other.ViewAccess;
            a.EditAccess = other.EditAccess;
            return a;
        }

        /// <summary>
        /// Gets the g note access.
        /// Conversions between Db Entity space and gRPC space.
        /// </summary>
        /// <returns>GNoteAccess.</returns>
        public GNoteAccess GetGNoteAccess()
        {
            GNoteAccess a = new GNoteAccess();
            a.UserID = this.UserID;
            a.NoteFileId = this.NoteFileId;
            a.ArchiveId = this.ArchiveId;
            a.ReadAccess = this.ReadAccess;
            a.Respond = this.Respond;
            a.Write = this.Write;
            a.SetTag = this.SetTag;
            a.DeleteEdit = this.DeleteEdit;
            a.ViewAccess = this.ViewAccess;
            a.EditAccess = this.EditAccess;
            return a;
        }

        /// <summary>
        /// Gets the note accesses.
        /// Conversions between Db Entity space and gRPC space.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>List&lt;NoteAccess&gt;.</returns>
        public static List<NoteAccess> GetNoteAccesses(GNoteAccessList other)
        {
            List<NoteAccess> list = new List<NoteAccess>();
            foreach (GNoteAccess a in other.List)
            {
                list.Add(GetNoteAccess(a));
            }
            return list;
        }

        /// <summary>
        /// Gets the g note access list.
        /// Conversions between Db Entity space and gRPC space.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>GNoteAccessList.</returns>
        public static GNoteAccessList GetGNoteAccessList(List<NoteAccess> other)
        {
            GNoteAccessList list = new GNoteAccessList();
            foreach (NoteAccess a in other)
            {
                list.List.Add(a.GetGNoteAccess());
            }
            return list;
        }

    }
}
