using Grpc.Net.Client;
using Microsoft.AspNetCore.Components;
using Notes.Protos;
using Syncfusion.Blazor.DropDowns;
using System.Timers;

namespace Notes.Client.Pages
{
    public partial class HomeIndex
    {
        /// <summary>
        /// Gets or sets the gRPC client used to communicate with the Notes server.
        /// </summary>
        [Inject] 
        NotesServer.NotesServerClient NotesClient { get; set; } = null!;

        /// <summary>
        /// Gets or sets the identifier of the notes file to be entered.
        /// </summary>
        [Parameter] public int EnterNotesfileId { get; set; } = 0;

        /// <summary>
        /// Gets or sets the file name to be used when directly entering file.
        /// </summary>
        [Parameter] public string EnterNotesfileName { get; set; } = "";

        /// <summary>
        /// Gets or sets the ordinal position used to determine note file entry behavior.
        /// </summary>
        [Parameter] public long Ordinal { get; set; } = 0;

        /// <summary>
        /// Gets or sets the current server time information, if available.
        /// </summary>
        protected ServerTime? serverTime { get; set; }

        /// <summary>
        /// Gets or sets the data model for the home page.
        /// </summary>
        protected HomePageModel? hpData { get; set; }

        /// <summary>
        /// Gets or sets the model representing the data and state of the home page.
        /// </summary>
        protected HomePageModel? hpModel { get; set; }

        /// <summary>
        /// Represents a placeholder instance of a <see cref="GNotefile"/> used for default or dummy operations within
        /// the containing class.
        /// </summary>
        /// <remarks>This field is initialized with default values and may be used to avoid null
        /// references or to represent an uninitialized state. It is not intended for use as a valid note file in
        /// application logic.</remarks>
        private GNotefile dummyFile = new GNotefile { Id = 0, NoteFileName = " ", NoteFileTitle = " " };

        /// <summary>
        /// Represents the target note file used for navigation operations.
        /// </summary>
        private GNotefile GoToFile = new GNotefile { Id = 0, NoteFileName = " ", NoteFileTitle = " " };

        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        /// <value>The item.</value>
        private GNotefile? item { get; set; }


        /// <summary>
        /// Gets or sets the file list.
        /// </summary>
        /// <value>The file list.</value>
        private List<GNotefile>? fileList { get; set; }

        /// <summary>
        /// List of files ordered by title
        /// </summary>
        /// <value>The name list.</value>
        private GNotefileList? nameList { get; set; }

        /// <summary>
        /// Important file list
        /// </summary>
        /// <value>The impfile list.</value>
        private GNotefileList? impfileList { get; set; }

        /// <summary>
        /// History file list
        /// </summary>
        /// <value>The histfile list.</value>
        private GNotefileList? histfileList { get; set; }

        /// <summary>
        /// Asynchronously updates the component's state when its parameters are set. This method initializes file
        /// lists, retrieves the current server time, and resets session storage values based on the user's
        /// authentication status.
        /// </summary>
        /// <remarks>This method is called by the Blazor framework after component parameters have been
        /// assigned. It performs state initialization and session storage updates, ensuring that the component reflects
        /// the latest parameter and authentication information. If the user is authenticated, additional data is loaded
        /// and session storage is reset to default values.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {

            fileList = new List<GNotefile>();
            nameList = new GNotefileList();
            histfileList = new GNotefileList();
            impfileList = new GNotefileList();

            serverTime = await NotesClient.GetServerTimeAsync(new NoRequest(), myState.AuthHeader);

            if (myState.IsAuthenticated)
            {
                // Set and reset local state vars
                await sessionStorage.SetItemAsync<int>("ArcId", 0);
                await sessionStorage.SetItemAsync<int>("IndexPage", 1);

                await sessionStorage.SetItemAsync<bool>("IsSeq", false);
                await sessionStorage.RemoveItemAsync("SeqList");
                await sessionStorage.RemoveItemAsync("SeqItem");
                await sessionStorage.RemoveItemAsync("SeqIndex");

                await sessionStorage.RemoveItemAsync("SeqHeaders");
                await sessionStorage.RemoveItemAsync("SeqHeaderIndex");
                await sessionStorage.RemoveItemAsync("CurrentSeqHeader");

                await sessionStorage.SetItemAsync<bool>("InSearch", false);
                await sessionStorage.RemoveItemAsync("SearchIndex");
                await sessionStorage.RemoveItemAsync("SearchList");

                hpData = hpModel = await NotesClient.GetHomePageModelAsync(new NoRequest(), myState.AuthHeader);

                GNotefileList fileList1 = hpModel.NoteFiles;
                GNotefileList nameList1 = hpModel.NoteFiles;
                fileList = fileList1.List.ToList().OrderBy(p => p.NoteFileName).ToList();
                nameList = nameList1;

                impfileList.List.Clear();
                histfileList.List.Clear();

                for (int i = 0; i < fileList1.List.Count; i++)
                {
                    GNotefile work = new GNotefile 
                        { Id = fileList1.List[i].Id, NoteFileName = fileList1.List[i].NoteFileName, 
                        NoteFileTitle = fileList1.List[i].NoteFileTitle };

                    if (EnterNotesfileId == work.Id)
                        GoToFile = work;
                    else if (EnterNotesfileName == work.NoteFileName)
                        GoToFile = work;

                    // handle special important and history files
                    string fname = work.NoteFileName;
                    if (fname == "Opbnotes" || fname == "Gnotes" || fname.StartsWith("sysnotes") || fname == "Cannounce")
                        histfileList.List.Add(work);

                    if (fname == "announce" || fname == "pbnotes" || fname == "noteshelp")
                        impfileList.List.Add(work);
                }
            }
        }

        /// <summary>
        /// Handle typed in file name
        /// </summary>
        /// <param name="value">The value.</param>
        protected void TextHasChanged(string value)
        {
            value = value.Trim().Replace("'\n", "").Replace("'\r", ""); //.Replace(" ", "");

            try
            {
                for (int i = 0; i < fileList?.Count; i++)
                {
                    item = fileList[i];
                    if (value == item.NoteFileName)
                    {
                        Navigation.NavigateTo("noteindex/" + item.Id); // goto the file
                        return;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Values the change handler.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private void ValueChangeHandler(ChangeEventArgs<int, GNotefile> args)
        {
            Navigation.NavigateTo("noteindex/" + args.Value); // goto the file

        }

    }
}