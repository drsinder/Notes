using Microsoft.AspNetCore.Components;
using Notes.Protos;

namespace Notes.Client.Pages
{
    /// <summary>
    /// Setup for calling note editor panel to edit an existing note
    /// </summary>
    public partial class EditNote
    {
        /// <summary>
        /// Gets or sets the note identifier.
        /// </summary>
        /// <value>The note identifier.</value>
        [Parameter] public long NoteId { get; set; }   //  what we are editing

        /// <summary>
        /// our data for the note in edit model
        /// </summary>
        /// <value>The model.</value>
        protected TextViewModel Model { get; set; } = new TextViewModel();

        /// <summary>
        /// A note display model
        /// </summary>
        /// <value>The stuff.</value>
        protected DisplayModel stuff { get; set; }

        /// <summary>
        /// The go
        /// </summary>
        protected bool go = false;

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject] NotesServer.NotesServerClient Client { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="EditNote"/> class.
        /// </summary>
        public EditNote()
        {
        }

        // get all the data
        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            stuff = await Client.GetNoteContentAsync(new DisplayModelRequest() { NoteId = NoteId, Vers = 0 }, myState.AuthHeader);

            Model.NoteFileID = stuff.NoteFile.Id;
            Model.NoteID = NoteId;
            Model.BaseNoteHeaderID = stuff.Header.BaseNoteId;
            Model.RefId = stuff.Header.RefId;
            Model.MyNote = stuff.Content.NoteBody;
            Model.MySubject = stuff.Header.NoteSubject;
            Model.DirectorMessage = stuff.Header.DirectorMessage;

            string tags = "";
            foreach (var tag in stuff.Tags.List)
            {
                tags += tag + " ";
            }
            Model.TagLine = tags;
            go = true;
        }
    }

}