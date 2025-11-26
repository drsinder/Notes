using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Diagnostics.Latency;
using Notes.Client;
using Notes.Protos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
// using Google.Protobuf.WellKnownTypes;

namespace Notes.Entities
{
    /// <summary>
    /// This class defines a table in the database.
    /// NoteHeader objects are the high level descriptors for a note.
    /// They contain all the information about a note EXCEPT the
    /// body, which is contained in related class NoteContent.
    /// The client index gets the complete set for a given notfile.
    /// This enables quick display, manipulation, and searching of
    /// the index.  Each object is related to one file object.
    /// Fields:
    /// Id          - The 64 bit unique Identifier for the note.
    /// NoteFileId  - The file which the note is a part of.
    /// ArchiveId   - 0 for the main file. Positive for archived notes.
    /// An Archive is a kind of subfile.
    /// BaseNoteId  - 0 for base notes. For responses the Id of its parent.
    /// NoteOrdinal - The number that appears in the index to Id a Base note.
    /// ResponseOrdinal - The number of a response. 0 for a base note.
    /// NoteSubject - You guessed it!
    /// LastEdited  - When the note was last edited
    /// ThreadLastEdited - When any note in a string was edited
    /// CreateDate  - DateTime when note was created
    /// ResponseCount - Only non-zero for a base note
    /// AuthorID    - UserId of the author
    /// AuthorName  - friendly name of author
    /// LinkGuid    - used to tie link notes together
    /// RefId       - Id of Note user was viewing when they responded
    /// IsDeleted   - true if the note has been marked as deleted
    /// Version     - Version Id for edited notes. Current is 0. Older are +
    /// DirectorMessage - Text message that may head a note.
    /// A single NoteContent is associated with each NoteHeader.
    /// </summary>
    [DataContract]
    public class NoteHeader
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// Uniquely identifies the note
        /// </summary>
        /// <value>The identifier.</value>
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember(Order = 1)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the note file identifier.
        /// The fileid the note belongs to
        /// </summary>
        /// <value>The note file identifier.</value>
        [Required]
        [DataMember(Order = 2)]
        public int NoteFileId { get; set; }

        /// <summary>
        /// Gets or sets the archive identifier.
        /// </summary>
        /// <value>The archive identifier.</value>
        [Required]
        [DataMember(Order = 3)]
        public int ArchiveId { get; set; }

        /// <summary>
        /// Gets or sets the base note identifier.
        /// </summary>
        /// <value>The base note identifier.</value>
        [DataMember(Order = 4)]
        public long BaseNoteId { get; set; }

        /// <summary>
        /// Gets or sets the note ordinal.
        /// the ordinal on a Base note and all its responses
        /// </summary>
        /// <value>The note ordinal.</value>
        [Required]
        [Display(Name = "Note #")]
        [DataMember(Order = 5)]
        public int NoteOrdinal { get; set; }

        /// <summary>
        /// Gets or sets the response ordinal.
        /// The ordinal of the response where 0 is a Base Note
        /// </summary>
        /// <value>The response ordinal.</value>
        [Required]
        [Display(Name = "Response #")]
        [DataMember(Order = 6)]
        public int ResponseOrdinal { get; set; }

        /// <summary>
        /// Gets or sets the note subject.
        /// Subject/Title of a note
        /// </summary>
        /// <value>The note subject.</value>
        [Required]
        [StringLength(200)]
        [Display(Name = "Subject")]
        [DataMember(Order = 7)]
        public string? NoteSubject { get; set; }

        /// <summary>
        /// Gets or sets the last edited.
        /// When the note was created or last edited
        /// </summary>
        /// <value>The last edited.</value>
        [Required]
        [Display(Name = "Last Edited")]
        [DataMember(Order = 8)]
        public DateTime LastEdited { get; set; }

        /// <summary>
        /// Gets or sets the thread last edited.
        /// When the thread was last edited
        /// </summary>
        /// <value>The thread last edited.</value>
        [Required]
        [Display(Name = "Thread Last Edited")]
        [DataMember(Order = 9)]
        public DateTime ThreadLastEdited { get; set; }

        /// <summary>
        /// Gets or sets the create date.
        /// </summary>
        /// <value>The create date.</value>
        [Required]
        [Display(Name = "Created")]
        [DataMember(Order = 10)]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Gets or sets the response count.
        /// Meaningful only if ResponseOrdinal = 0
        /// </summary>
        /// <value>The response count.</value>
        [Required]
        [DataMember(Order = 11)]
        public int ResponseCount { get; set; }

        /// <summary>
        /// Gets or sets the author identifier.
        /// </summary>
        /// <value>The author identifier.</value>
        [StringLength(450)]
        [DataMember(Order = 12)]
        public string? AuthorID { get; set; }

        /// <summary>
        /// Gets or sets the name of the author.
        /// </summary>
        /// <value>The name of the author.</value>
        [Required]
        [StringLength(50)]
        [DataMember(Order = 13)]
        public string? AuthorName { get; set; }

        /// <summary>
        /// Gets or sets the link unique identifier.
        /// </summary>
        /// <value>The link unique identifier.</value>
        [StringLength(100)]
        [DataMember(Order = 14)]
        public string? LinkGuid { get; set; }

        /// <summary>
        /// Gets or sets the reference identifier.
        /// </summary>
        /// <value>The reference identifier.</value>
        [DataMember(Order = 15)]
        public long RefId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is deleted.
        /// </summary>
        /// <value><c>true</c> if this instance is deleted; otherwise, <c>false</c>.</value>
        [DataMember(Order = 16)]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        [DataMember(Order = 17)]
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the director message.
        /// </summary>
        /// <value>The director message.</value>
        [StringLength(200)]
        [Display(Name = "Director Message")]
        [DataMember(Order = 18)]
        public string? DirectorMessage { get; set; }

        /// <summary>
        /// Gets or sets the content of the note.
        /// </summary>
        /// <value>The content of the note.</value>
        public NoteContent? NoteContent { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        /// <value>The tags.</value>
        public List<Tags>? Tags { get; set; }


        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>NoteHeader.</returns>
        public NoteHeader Clone()
        {
            NoteHeader nh = new NoteHeader()
            {
                Id = Id,
                NoteFileId = NoteFileId,
                ArchiveId = ArchiveId,
                BaseNoteId = BaseNoteId,
                NoteOrdinal = NoteOrdinal,
                NoteSubject = NoteSubject,
                DirectorMessage = DirectorMessage,
                LastEdited = LastEdited,
                ThreadLastEdited = ThreadLastEdited,
                CreateDate = CreateDate,
                ResponseCount = ResponseCount,
                AuthorID = AuthorID,
                AuthorName = AuthorName,
                LinkGuid = LinkGuid,
                RefId = RefId,
                IsDeleted = IsDeleted,
                Version = Version,
                ResponseOrdinal = ResponseOrdinal
            };

            return nh;
        }


        /// <summary>
        /// Clones for link.
        /// </summary>
        /// <returns>NoteHeader.</returns>
        public NoteHeader CloneForLink()
        {
            NoteHeader nh = new NoteHeader()
            {
                Id = Id,
                NoteSubject = NoteSubject,
                DirectorMessage = DirectorMessage,
                LastEdited = LastEdited,
                ThreadLastEdited = ThreadLastEdited,
                CreateDate = CreateDate,
                AuthorID = AuthorID,
                AuthorName = AuthorName,
                LinkGuid = LinkGuid
            };

            return nh;
        }

        /// <summary>
        /// Clones for link r.
        /// </summary>
        /// <returns>NoteHeader.</returns>
        public NoteHeader CloneForLinkR()
        {
            NoteHeader nh = new NoteHeader()
            {
                Id = Id,
                NoteSubject = NoteSubject,
                DirectorMessage = DirectorMessage,
                LastEdited = LastEdited,
                ThreadLastEdited = ThreadLastEdited,
                CreateDate = CreateDate,
                AuthorID = AuthorID,
                AuthorName = AuthorName,
                LinkGuid = LinkGuid,
                ResponseOrdinal = ResponseOrdinal
            };

            return nh;
        }

        //
        // Conversions between Db Entity space and gRPC space.
        //
        /// <summary>
        /// Gets the note header.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>NoteHeader.</returns>
        public static NoteHeader GetNoteHeader(GNoteHeader other)
        {
            NoteHeader h = new NoteHeader();
            h.Id = other.Id;
            h.NoteFileId = other.NoteFileId;
            h.ArchiveId = other.ArchiveId;
            h.BaseNoteId = other.BaseNoteId;
            h.NoteOrdinal = other.NoteOrdinal;
            h.ResponseOrdinal = other.ResponseOrdinal;
            h.NoteSubject = other.NoteSubject;
            h.LastEdited = other.LastEdited.ToDateTime();
            h.ThreadLastEdited = other.ThreadLastEdited.ToDateTime();
            h.CreateDate = other.CreateDate.ToDateTime();
            h.ResponseCount = other.ResponseCount;
            h.AuthorID = other.AuthorID;
            h.AuthorName = other.AuthorName;
            h.LinkGuid = other.LinkGuid;
            h.RefId = other.RefId;
            h.IsDeleted = other.IsDeleted;
            h.Version = other.Version;
            if (other.DirectorMessage != null)
                h.DirectorMessage = other.DirectorMessage;
            return h;
        }

        /// <summary>
        /// Gets the g note header.
        /// </summary>
        /// <returns>GNoteHeader.</returns>
        public GNoteHeader GetGNoteHeader()
        {
            GNoteHeader h = new GNoteHeader();
            NoteHeader other = this;
            h.Id = other.Id;
            h.NoteFileId = other.NoteFileId;
            h.ArchiveId = other.ArchiveId;
            h.BaseNoteId = other.BaseNoteId;
            h.NoteOrdinal = other.NoteOrdinal;
            h.ResponseOrdinal = other.ResponseOrdinal;
            h.NoteSubject = other.NoteSubject;
            h.LastEdited = Timestamp.FromDateTime(Globals.UTimeBlazor(other.LastEdited).ToUniversalTime());
            h.ThreadLastEdited = Timestamp.FromDateTime(Globals.UTimeBlazor(other.ThreadLastEdited).ToUniversalTime());
            h.CreateDate = Timestamp.FromDateTime(Globals.UTimeBlazor(other.CreateDate).ToUniversalTime());
            h.ResponseCount = other.ResponseCount;
            h.AuthorID = other.AuthorID;
            h.AuthorName = other.AuthorName;
            h.LinkGuid = other.LinkGuid;
            h.RefId = other.RefId;
            h.IsDeleted = other.IsDeleted;
            h.Version = other.Version;
            if (other.DirectorMessage != null)
                h.DirectorMessage = other.DirectorMessage;
            return h;
        }

        /// <summary>
        /// Gets the note headers.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>List&lt;NoteHeader&gt;.</returns>
        public static List<NoteHeader> GetNoteHeaders(GNoteHeaderList other)
        {
            List<NoteHeader> list = new List<NoteHeader>();
            foreach (GNoteHeader notefile in other.List)
            {
                list.Add(GetNoteHeader(notefile));
            }
            return list;
        }

        /// <summary>
        /// Gets the g note header list.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>GNoteHeaderList.</returns>
        public static GNoteHeaderList GetGNoteHeaderList(List<NoteHeader> other)
        {
            GNoteHeaderList list = new GNoteHeaderList();
            foreach (NoteHeader h in other)
            {
                list.List.Add(h.GetGNoteHeader());
            }
            return list;
        }



        //public static explicit operator NoteHeader(Task<NoteHeader> v)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
