using Notes.Protos;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Enum AccessX
    /// </summary>
    public enum AccessX
    {
        /// <summary>
        /// The read access
        /// </summary>
        ReadAccess,
        /// <summary>
        /// The respond
        /// </summary>
        Respond,
        /// <summary>
        /// The write
        /// </summary>
        Write,
        /// <summary>
        /// The set tag
        /// </summary>
        SetTag,
        /// <summary>
        /// The delete edit
        /// </summary>
        DeleteEdit,
        /// <summary>
        /// The view access
        /// </summary>
        ViewAccess,
        /// <summary>
        /// The edit access
        /// </summary>
        EditAccess
    }

    /// <summary>
    /// Used for editing an access token segment (one flag)
    /// </summary>
    public class AccessItem
    {
        /// <summary>
        /// The whole token
        /// </summary>
        /// <value>The item.</value>
        public GNoteAccess Item { get; set; }

        /// <summary>
        /// Indicates which segment we are dealing with
        /// </summary>
        /// <value>The which.</value>
        public AccessX which { get; set; }

        /// <summary>
        /// Is it currently checked?
        /// </summary>
        /// <value><c>true</c> if this instance is checked; otherwise, <c>false</c>.</value>
        public bool isChecked { get; set; }

        /// <summary>
        /// Can current user change it?
        /// </summary>
        /// <value><c>true</c> if this instance can edit; otherwise, <c>false</c>.</value>
        public bool canEdit { get; set; }
    }

}
