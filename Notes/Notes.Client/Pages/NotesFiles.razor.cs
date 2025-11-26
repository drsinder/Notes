using Notes.Protos;
using Syncfusion.Blazor.Grids;

namespace Notes.Client.Pages
{
    /// <summary>
    /// Display list of notefiles
    /// </summary>
    public partial class NotesFiles
    {
        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>The files.</value>
        private GNotefileList Files { get; set; }

        /// <summary>
        /// Gets or sets the user data.
        /// </summary>
        /// <value>The user data.</value>
        private GAppUser UserData { get; set; }

        /// <summary>
        /// Set up and get data from server
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            await sessionStorage.SetItemAsync("ArcId", 0);
            await sessionStorage.SetItemAsync("IndexPage", 1);

            // grab data from server
            HomePageModel model = await Client.GetHomePageModelAsync(new NoRequest(), myState.AuthHeader);

            Files = model.NoteFiles;
            UserData = model.UserData;
            if (UserData.Ipref2 == 0)
                UserData.Ipref2 = 10;
        }

        /// <summary>
        /// Displays it.
        /// </summary>
        /// <param name="args">The arguments.</param>
        protected void DisplayIt(RowSelectEventArgs<GNotefile> args)
        {
            Navigation.NavigateTo("noteindex/" + args.Data.Id);
        }
    }

}