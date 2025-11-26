using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Notes.Protos;
using System.ComponentModel.DataAnnotations;

namespace Notes.Client.Dialogs
{
    public partial class CreateNoteFile
    {
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
        /// The dummy file
        /// </summary>
        public CreateFileModel dummyFile = new CreateFileModel();
        /// <summary>
        /// Handles the valid submit.
        /// </summary>
        private async Task HandleValidSubmit()
        {
            await Client.CreateNoteFileAsync(new GNotefile()
            { NoteFileName = dummyFile.NoteFileName, NoteFileTitle = dummyFile.NoteFileTitle }, myState.AuthHeader);
            await ModalInstance.CloseAsync();
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