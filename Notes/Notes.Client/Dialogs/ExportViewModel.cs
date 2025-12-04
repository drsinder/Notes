using Notes.Client.Menus;
using Notes.Protos;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Represents the configuration and options for exporting notes from a note file, including format, scope,
    /// destination, and related metadata.      
    /// </summary>
    /// <remarks>Use this view model to specify export parameters such as the source note file, archive
    /// selection, output format (HTML or plain), whether the export should be collapsible, direct output or via a
    /// dialog, the specific note ordinal to export, a set of marked notes to limit the export scope, an optional email
    /// address for sending exported notes, and an associated menu for export actions. All properties should be set
    /// prior to initiating the export operation.</remarks>
    public class ExportViewModel
    {
        /// <summary>
        /// Notefile we are exporting from
        /// </summary>
        /// <value>The note file.</value>
        public GNotefile NoteFile { get; set; }

        /// <summary>
        /// Possible non 0 archive
        /// </summary>
        /// <value>The archive number.</value>
        public int ArchiveNumber { get; set; }

        /// <summary>
        /// Is it to be in html format?
        /// </summary>
        /// <value><c>true</c> if this instance is HTML; otherwise, <c>false</c>.</value>
        public bool isHtml { get; set; }

        /// <summary>
        /// Collapsible or "flat"
        /// </summary>
        /// <value><c>true</c> if this instance is collapsible; otherwise, <c>false</c>.</value>
        public bool isCollapsible { get; set; }

        /// <summary>
        /// Direct output or destination collected via a dailog?
        /// </summary>
        /// <value><c>true</c> if this instance is direct output; otherwise, <c>false</c>.</value>
        public bool isDirectOutput { get; set; }

        //public bool isOnPage { get; set; }

        /// <summary>
        /// NoteOrdinal to export - 0 for all notes
        /// </summary>
        /// <value>The note ordinal.</value>
        public int NoteOrdinal { get; set; }

        /// <summary>
        /// "Marks" to limit scope of notes exportes the a specific set
        /// selected by user "Marked"
        /// </summary>
        /// <value>The marks.</value>
        public List<Mark> Marks { get; set; }

        /// <summary>
        /// Email address if being mailed to someone
        /// </summary>
        /// <value>The email.</value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets my menu.
        /// </summary>
        /// <value>My menu.</value>
        public ListMenu myMenu { get; set; }
    }

    /// <summary>
    /// Represents a marker or reference to a specific note, response, or header within an archive or note file for a
    /// user.
    /// </summary>
    /// <remarks>The Mark class encapsulates identifiers and ordinals that uniquely locate a note, response,
    /// or header in a structured archive system. It is typically used to track or reference user-specific notes and
    /// their associated metadata. The ResponseOrdinal property distinguishes between the whole note string, the base
    /// note, and individual responses: a value of -1 refers to the entire note string, 0 refers to the base note only,
    /// and values greater than 0 refer to specific responses.</remarks>
    public class Mark
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the note file identifier.
        /// </summary>
        /// <value>The note file identifier.</value>
        public int NoteFileId { get; set; }

        /// <summary>
        /// Gets or sets the archive identifier.
        /// </summary>
        /// <value>The archive identifier.</value>
        public int ArchiveId { get; set; }

        /// <summary>
        /// Gets or sets the mark ordinal.
        /// </summary>
        /// <value>The mark ordinal.</value>
        public int MarkOrdinal { get; set; }

        /// <summary>
        /// Gets or sets the note ordinal.
        /// </summary>
        /// <value>The note ordinal.</value>
        public int NoteOrdinal { get; set; }

        /// <summary>
        /// Gets or sets the note header identifier.
        /// </summary>
        /// <value>The note header identifier.</value>
        public long NoteHeaderId { get; set; }

        /// <summary>
        /// Gets or sets the response ordinal.
        /// </summary>
        /// <value>The response ordinal.</value>
        public int ResponseOrdinal { get; set; }  // -1 == whole string, 0 base note only, > 0 Response
    }


}
