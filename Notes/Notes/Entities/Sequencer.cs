using Google.Protobuf.WellKnownTypes;
using Notes.Protos;
using Notes.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Notes.Entities
{
    /// <summary>
    /// This class defines a table in the database.
    /// Object of this class may be associated with a user
    /// and file to be used to find notes written since the
    /// "Recent" function was last invoked.
    /// </summary>
    [DataContract]
    public class Sequencer
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// ID of the user who owns the item
        /// </summary>
        /// <value>The user identifier.</value>
        [Required]
        [Column(Order = 0)]
        [StringLength(450)]
        [DataMember(Order = 1)]
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the note file identifier.
        /// ID of target notfile
        /// </summary>
        /// <value>The note file identifier.</value>
        [Required]
        [Column(Order = 1)]
        [DataMember(Order = 2)]
        public int NoteFileId { get; set; }

        /// <summary>
        /// Gets or sets the ordinal.
        /// </summary>
        /// <value>The ordinal.</value>
        [Required]
        [Display(Name = "Position in List")]
        [DataMember(Order = 3)]
        public int Ordinal { get; set; }

        /// <summary>
        /// Gets or sets the last time.
        /// Time we last completed a run with this
        /// </summary>
        /// <value>The last time.</value>
        [Display(Name = "Last Time")]
        [DataMember(Order = 4)]
        public DateTime LastTime { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// Time a run in this file started - will get copied to LastTime when complete
        /// </summary>
        /// <value>The start time.</value>
        [DataMember(Order = 5)]
        public DateTime StartTime { get; set; }

        // Is this item active now?  Are we sequencing this file
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Sequencer"/> is active.
        /// </summary>
        /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
        [DataMember(Order = 6)]
        public bool Active { get; set; }

        /// <summary>
        /// Gets the sequencer.
        /// Conversions between Db Entity space and gRPC space.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>Sequencer.</returns>
        public static Sequencer GetSequencer(GSequencer other)
        {
            Sequencer s = new Sequencer();
            s.UserId = other.UserId;
            s.NoteFileId = other.NoteFileId;
            s.Ordinal = other.Ordinal;
            s.LastTime = other.LastTime.ToDateTime();
            s.StartTime = other.StartTime.ToDateTime();
            s.Active = other.Active;
            return s;
        }

        /// <summary>
        /// Gets the g sequencer.
        /// Conversions between Db Entity space and gRPC space.
        /// </summary>
        /// <returns>GSequencer.</returns>
        public GSequencer GetGSequencer()
        {
            GSequencer s = new GSequencer();
            s.UserId = this.UserId;
            s.NoteFileId = this.NoteFileId;
            s.Ordinal = this.Ordinal;
            s.LastTime = Timestamp.FromDateTime(Globals.UTimeBlazor(this.LastTime).ToUniversalTime());
            s.StartTime = Timestamp.FromDateTime(Globals.UTimeBlazor(this.StartTime).ToUniversalTime());
            s.Active = this.Active;
            return s;
        }

        /// <summary>
        /// Gets the sequencer list.
        /// Conversions between Db Entity space and gRPC space.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>List&lt;Sequencer&gt;.</returns>
        public static List<Sequencer> GetSequencerList(GSequencerList other)
        {
            List<Sequencer> list = new List<Sequencer>();
            foreach (GSequencer t in other.List)
            {
                list.Add(GetSequencer(t));
            }
            return list;
        }

        /// <summary>
        /// Gets the g sequencer list.
        /// Conversions between Db Entity space and gRPC space.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>GSequencerList.</returns>
        public static GSequencerList GetGSequencerList(List<Sequencer> other)
        {
            GSequencerList list = new GSequencerList();
            foreach (Sequencer t in other)
            {
                list.List.Add(t.GetGSequencer());
            }
            return list;
        }

    }
}

