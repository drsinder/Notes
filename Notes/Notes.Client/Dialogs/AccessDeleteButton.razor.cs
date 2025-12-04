using Microsoft.AspNetCore.Components;
using Notes.Protos;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Represents a button component that allows deletion of note access within the application.   
    /// </summary>
    /// <remarks>This component is typically used in user interfaces where access permissions to notes can be
    /// managed. It interacts with the notes server to perform deletion operations and notifies consumers via the
    /// OnClick callback when a delete action occurs.</remarks>
    public partial class AccessDeleteButton
    {
        /// <summary>
        /// Gets or sets the note access.
        /// </summary>
        /// <value>The note access.</value>
        [Parameter]
        public GNoteAccess noteAccess { get; set; }

        /// <summary>
        /// Gets or sets the on click.
        /// </summary>
        /// <value>The on click.</value>
        [Parameter]
        public EventCallback<string> OnClick { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject] NotesServer.NotesServerClient Client { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="AccessDeleteButton"/> class.
        /// </summary>
        public AccessDeleteButton() { }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        protected async Task Delete()
        {
            await Client.DeleteAccessItemAsync(noteAccess, myState.AuthHeader);
            await OnClick.InvokeAsync("Delete");
        }
    }
}