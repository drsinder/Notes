using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Notes.Protos;
using System.ComponentModel.DataAnnotations;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Represents a Blazor component for deleting a note file, providing functionality to confirm or cancel the
    /// deletion within a modal dialog.
    /// </summary>
    /// <remarks>This component is typically used within a modal dialog to prompt users for confirmation
    /// before deleting a note file. It interacts with the modal infrastructure and requires parameters specifying the
    /// file to be deleted. The component handles user actions to either submit the deletion request or cancel the
    /// operation.</remarks>
    public partial class DeleteNoteFile
    {
        /// <summary>
        /// The dummy file
        /// </summary>
        public CreateFileModel dummyFile = new();
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the file identifier.
        /// </summary>
        /// <value>The file identifier.</value>
        [Parameter]
        public int FileId { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        [Parameter]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file title.
        /// </summary>
        /// <value>The file title.</value>
        [Parameter]
        public string FileTitle { get; set; }

        /// <summary>
        /// Handles the valid submit.
        /// </summary>
        private async Task HandleValidSubmit()
        {
            await Client.DeleteNoteFileAsync(new GNotefile()
            { Id = FileId }, myState.AuthHeader);
            await ModalInstance.CloseAsync(ModalResult.Ok($"Delete was submitted successfully."));
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CancelAsync();
        }

        /// <summary>
        /// Class CreateFileModel.
        /// </summary>
        public class CreateFileModel
        {
            /// <summary>
            /// Gets or sets the name of the note file.
            /// </summary>
            /// <value>The name of the note file.</value>
            [Required]
            public string NoteFileName { get; set; }

            /// <summary>
            /// Gets or sets the note file title.
            /// </summary>
            /// <value>The note file title.</value>
            [Required]
            public string NoteFileTitle { get; set; }
        }

    }
}