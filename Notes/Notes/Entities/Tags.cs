using Notes.Protos;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Notes.Entities
{
    /// <summary>
    /// This class defines a table in the database.
    /// Zero or more of these objects may be associated with each note.
    /// Defines a simple tag or set of tags for a note.
    /// </summary>
    [DataContract]
    public class Tags
    {
        /// <summary>
        /// Gets or sets the note file identifier.
        /// The fileid the tag belongs to
        /// </summary>
        /// <value>The note file identifier.</value>
        [Required]
        [DataMember(Order = 1)]
        public int NoteFileId { get; set; }

        /// <summary>
        /// Gets or sets the archive identifier.
        /// </summary>
        /// <value>The archive identifier.</value>
        [Required]
        [DataMember(Order = 2)]
        public int ArchiveId { get; set; }
        /// <summary>
        /// Gets or sets the note header identifier.
        /// </summary>
        /// <value>The note header identifier.</value>
        [Required]
        [DataMember(Order = 3)]
        public long NoteHeaderId { get; set; }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>The tag.</value>
        [Required]
        [StringLength(30)]
        [DataMember(Order = 4)]
        public string? Tag { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string? ToString()
        {
            return Tag;
        }

        /// <summary>
        /// Lists to string.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>System.String.</returns>
        public static string ListToString(List<Tags> list)
        {
            string s = string.Empty;
            if (list is null || list.Count < 1)
                return s;

            foreach (Tags tag in list)
            {
                s += tag.Tag + " ";
            }

            return s.TrimEnd(' ');
        }

        /// <summary>
        /// Strings to list.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns>List&lt;Tags&gt;.</returns>
        public static List<Tags> StringToList(string s)
        {
            List<Tags> list = new();

            if (string.IsNullOrEmpty(s) || s.Length < 1)
                return list;

            string[] tags = s.Split(',', ';', ' ');

            if (tags is null || tags.Length < 1)
                return list;

            foreach (string t in tags)
            {
                string r = t.Trim().ToLower();
                list.Add(new Tags() { Tag = r });
            }

            return list;
        }

        /// <summary>
        /// Strings to list.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="hId">The h identifier.</param>
        /// <param name="fId">The f identifier.</param>
        /// <param name="arcId">The arc identifier.</param>
        /// <returns>List&lt;Tags&gt;.</returns>
        public static List<Tags> StringToList(string s, long hId, int fId, int arcId)
        {
            List<Tags> list = new();

            if (string.IsNullOrEmpty(s) || s.Length < 1)
                return list;

            string[] tags = s.Split(',', ';', ' ');

            if (tags is null || tags.Length < 1)
                return list;

            foreach (string t in tags)
            {
                string r = t.Trim().ToLower();
                list.Add(new Tags() { Tag = r, NoteHeaderId = hId, NoteFileId = fId, ArchiveId = arcId });
            }

            return list;
        }

        /// <summary>
        /// Clones for link.
        /// </summary>
        /// <param name="inp">The inp.</param>
        /// <returns>List&lt;Tags&gt;.</returns>
        public static List<Tags> CloneForLink(List<Tags> inp)
        {
            if (inp is null)
#pragma warning disable CS8603 // Possible null reference return.
                return null;
#pragma warning restore CS8603 // Possible null reference return.

            List<Tags> outp = new();

            if (inp.Count == 0)
                return outp;

            foreach (Tags t in inp)
            {
                outp.Add(new Tags { Tag = t.Tag });
            }

            return outp;
        }

        //
        // Conversions between Db Entity space and gRPC space.
        //
        /// <summary>
        /// Gets the tags.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>Tags.</returns>
        public static Tags GetTags(GTags other)
        {
            Tags t = new()
            {
                NoteFileId = other.NoteFileId,
                ArchiveId = other.ArchiveId,
                NoteHeaderId = other.NoteHeaderId,
                Tag = other.Tag
            };

            return t;
        }

        /// <summary>
        /// Gets the g tags.
        /// </summary>
        /// <returns>GTags.</returns>
        public GTags GetGTags()
        {
            GTags t = new()
            {
                NoteFileId = this.NoteFileId,
                NoteHeaderId = this.NoteHeaderId,
                ArchiveId = this.ArchiveId,
                Tag = this.Tag
            };
            return t;
        }

        /// <summary>
        /// Gets the tags list.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>List&lt;Tags&gt;.</returns>
        public static List<Tags> GetTagsList(GTagsList other)
        {
            List<Tags> list = new();
            foreach (GTags t in other.List)
            {
                list.Add(GetTags(t));
            }
            return list;
        }

        /// <summary>
        /// Gets the g tags list.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>GTagsList.</returns>
        public static GTagsList GetGTagsList(List<Tags> other)
        {
            GTagsList list = new();
            foreach (Tags t in other)
            {
                list.List.Add(t.GetGTags());
            }
            return list;
        }
    }
}

