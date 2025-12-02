using Grpc.Net.Client;
using Microsoft.AspNetCore.Components;
using Notes.Protos;
using Syncfusion.Blazor.DropDowns;
using System.Timers;

namespace Notes.Client.Pages
{
    public partial class HomeIndex
    {
//        [Inject]
//        GrpcChannel Channel { get; set; }

        [Inject] 
        NotesServer.NotesServerClient NotesClient { get; set; } = null!;

        protected ServerTime? serverTime { get; set; }

        protected HomePageModel? hpData { get; set; }

        protected HomePageModel? hpModel { get; set; }

        /// <summary>
        /// The dummy file
        /// </summary>
        private GNotefile dummyFile = new GNotefile { Id = 0, NoteFileName = " ", NoteFileTitle = " " };

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
        private System.Timers.Timer? timer2 { get; set; }

        /// <summary>
        /// The ticks
        /// </summary>
        private int ticks = 0;

        /// <summary>
        /// Timers the tick2.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        protected void TimerTick2(Object source, ElapsedEventArgs e)
        {
            if (++ticks == 10)
                timer2?.Interval = 5000;
            else if (++ticks == 60)
                timer2?.Interval = 15000;

            //Globals.LoginDisplay?.Reload();
            //Globals.NavMenu?.Reload().GetAwaiter();
//            StateHasChanged();
        }


        /// <summary>
        /// Method invoked after each time the component has been rendered.
        /// </summary>
        /// <param name="firstRender">Set to <c>true</c> if this is the first time <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> has been invoked
        /// on this component instance; otherwise <c>false</c>.</param>
        /// <remarks>The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> and <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)" /> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        /// once.</remarks>
//        protected override void OnAfterRender(bool firstRender)
//        {
//            if (false && firstRender)
//            {
//                timer2 = new System.Timers.Timer(1000);
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
                // timer2.Elapsed += TimerTick2;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
//                timer2.Enabled = true;

  //              myState.OnChange += OnParametersSet; // get notified of login status changes
  //         }
  //      }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
  //      protected override void OnParametersSet()
  //      {
  //          OnParametersSetAsync().GetAwaiter();    // notified of login status change
  //          StateHasChanged();
  //       }



        protected override async Task OnParametersSetAsync()
       //protected override async Task OnAfterRenderAsync(bool firstRender)
        {
//            bool needStateChange = false;
//            if (serverTime is null)
//                needStateChange = true;

            fileList = new List<GNotefile>();
            nameList = new GNotefileList();
            histfileList = new GNotefileList();
            impfileList = new GNotefileList();

         //   NotesClient = Globals.GetNotesClient(Navigation);

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
                    GNotefile work = new GNotefile { Id = fileList1.List[i].Id, NoteFileName = fileList1.List[i].NoteFileName, NoteFileTitle = fileList1.List[i].NoteFileTitle };

                    // handle special important and history files
                    string fname = work.NoteFileName;
                    if (fname == "Opbnotes" || fname == "Gnotes" || fname.StartsWith("sysnotes") || fname == "Cannounce")
                        histfileList.List.Add(work);

                    if (fname == "announce" || fname == "pbnotes" || fname == "noteshelp")
                        impfileList.List.Add(work);
                }

            }
         //   if (firstRender || needStateChange)
          //      StateHasChanged();
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