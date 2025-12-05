using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Notes.Client.Pages;
using Notes.Client.Dialogs;
using Notes.Client.Menus;
using Notes.Protos;
using Syncfusion.Blazor.Inputs;
using System.Text;

/// <summary>
/// The Notes.Client.Panels namespace contains components for displaying and managing notes within the client application.
/// </summary>
namespace Notes.Client.Panels
{
    /// <summary>
    /// Represents a panel for displaying and interacting with notes, including support for responses, navigation, and
    /// various display modes within a note management application.
    /// </summary>
    /// <remarks>The NotePanel component provides functionality for viewing note content, managing responses,
    /// navigating between notes, and handling user interactions such as printing and editing. It supports multiple
    /// display styles, history/version tracking, and integration with services such as modal dialogs, navigation,
    /// JavaScript interop, and session storage. The panel can operate in different modes, such as root note view, mini
    /// view, and alternate styling, and exposes parameters to control its behavior and appearance. NotePanel is
    /// designed for use within Blazor applications and relies on dependency injection for service access.
    /// It can invoke itself recursivly</remarks>
    public partial class NotePanel
    {
        /// <summary>
        /// For Dialogs
        /// </summary>
        /// <value>The modal.</value>
        [CascadingParameter] public IModalService Modal { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the note we are displaying.
        /// </summary>
        [Parameter] public long NoteId { get; set; }

        /// <summary>
        /// Whether or not child windows should be shown.  These might include
        /// Responses, versions, references.
        /// </summary>
        /// <value><c>true</c> if [show child]; otherwise, <c>false</c>.</value>
        [Parameter] public bool ShowChild { get; set; }

        /// <summary>
        /// Is this at the "root" of something
        /// </summary>
        /// <value><c>true</c> if this instance is root note; otherwise, <c>false</c>.</value>
        [Parameter] public bool IsRootNote { get; set; }

        /// <summary>
        /// Should optional buttons be shown
        /// </summary>
        /// <value><c>true</c> if [show buttons]; otherwise, <c>false</c>.</value>
        [Parameter] public bool ShowButtons { get; set; } = true;

        /// <summary>
        /// Is this panel to be shown in the alternate style?
        /// </summary>
        /// <value><c>true</c> if [alt style]; otherwise, <c>false</c>.</value>
        [Parameter] public bool AltStyle { get; set; }

        /// <summary>
        /// Should certain functions be suppressed at head and tail of panel
        /// </summary>
        /// <value><c>true</c> if this instance is mini; otherwise, <c>false</c>.</value>
        [Parameter] public bool IsMini { get; set; }

        /// <summary>
        /// Are we showing history versions
        /// </summary>
        /// <value>The vers.</value>
        [Parameter] public int Vers { get; set; } = 0;

        /// <summary>
        /// Who is my container
        /// </summary>
        [Parameter] public NoteIndex MyNoteIndex { get; set; }

        /// <summary>
        /// List of responses
        /// </summary>
        /// <value>The resp headers.</value>
#pragma warning disable IDE1006 // Naming Styles
        protected List<GNoteHeader> respHeaders { get; set; }

        /// <summary>
        /// Header style string
        /// </summary>
        /// <value>The header style.</value>
        protected string HeaderStyle { get; set; }

        /// <summary>
        /// Body style string
        /// </summary>
        /// <value>The body style.</value>
        protected string BodyStyle { get; set; }

        /// <summary>
        /// Are responses shown
        /// </summary>
        /// <value><c>true</c> if [resp shown]; otherwise, <c>false</c>.</value>
        protected bool RespShown { get; set; }

        /// <summary>
        /// Public value of RespShown
        /// </summary>
        /// <value><c>true</c> if [show resp]; otherwise, <c>false</c>.</value>
        public bool ShowResp { get { return ResetShown; } }

        /// <summary>
        /// Gets or sets a value indicating whether [reset shown].
        /// </summary>
        /// <value><c>true</c> if [reset shown]; otherwise, <c>false</c>.</value>
        protected bool ResetShown { get; set; } = false;

        /// <summary>
        /// Is the order of responses flipped?
        /// </summary>
        /// <value><c>true</c> if [resp flipped]; otherwise, <c>false</c>.</value>
        protected bool RespFlipped { get; set; }

        /// <summary>
        /// Should the typing box "eat" the next enter key?
        /// </summary>
        /// <value><c>true</c> if [eat enter]; otherwise, <c>false</c>.</value>
        protected bool EatEnter { get; set; }

        /// <summary>
        /// Are we showing version history?
        /// </summary>
        /// <value><c>true</c> if [show vers]; otherwise, <c>false</c>.</value>
        protected bool ShowVers { get; set; } = false;

        /// <summary>
        /// Are we sequencing?
        /// </summary>
        /// <value><c>true</c> if this instance is seq; otherwise, <c>false</c>.</value>
        protected bool IsSeq { get; set; }

        /// <summary>
        /// Gets or sets the display model associated with the current instance.
        /// </summary>
        protected DisplayModel model { get; set; }

        /// <summary>
        /// Reference to our menu so we can talk to it
        /// </summary>
        /// <value>My menu.</value>
        public NoteMenu MyMenu { get; set; }

        /// <summary>
        /// Reference to our fancy html editor
        /// </summary>
        /// <value>The sf text box.</value>
        SfTextBox sfTextBox { get; set; }

        /// <summary>
        /// Accumulator for the typin nav box
        /// </summary>
        /// <value>The nav string.</value>
        public string NavString { get; set; }
        //public string NavCurrentVal { get; set; }

        /// <summary>
        /// Gets or sets the resp x.
        /// </summary>
        /// <value>The resp x.</value>
        public string respX { get; set; }
        /// <summary>
        /// Gets or sets the resp y.
        /// </summary>
        /// <value>The resp y.</value>
        public string respY { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject] NotesServer.NotesServerClient Client { get; set; }
        /// <summary>
        /// Gets or sets the navigation.
        /// </summary>
        /// <value>The navigation.</value>
        [Inject] NavigationManager Navigation { get; set; }
        /// <summary>
        /// Gets or sets the js.
        /// </summary>
        /// <value>The js.</value>
        [Inject] IJSRuntime JS { get; set; }    // enables calling javascript
        /// <summary>
        /// Gets or sets the session storage.
        /// </summary>
        /// <value>The session storage.</value>
        [Inject] Blazored.SessionStorage.ISessionStorageService sessionStorage { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        /// <summary>
        /// Initialize defaults for a "root" note - not showing children
        /// </summary>
        public NotePanel()
        {
            ShowChild = false;
            IsRootNote = true;
        }

        /// <summary>
        /// Get our data and set IsSeq flag from state
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            await GetData();

            RespShown = (IsRootNote && MyNoteIndex.Model.UserData.Pref3 && model.Header.ResponseOrdinal == 0 && model.Header.ResponseCount > 0 && !AltStyle && !model.Header.IsDeleted);
            if (RespShown)
            {
                // Get response headers from the index
                respHeaders = MyNoteIndex.GetResponseHeaders(model.Header.Id);
            }

            RespFlipped = (MyNoteIndex is not null) && MyNoteIndex.Model.UserData.Pref4;
            if (IsRootNote && RespFlipped && RespShown)
                respHeaders = respHeaders.OrderByDescending(x => x.ResponseOrdinal).ToList();
            else if (IsRootNote && RespShown)
                respHeaders = respHeaders.OrderBy(x => x.ResponseOrdinal).ToList();

            IsSeq = await sessionStorage.GetItemAsync<bool>("IsSeq");
        }

        /// <summary>
        /// Get data for note
        /// </summary>
        protected async Task GetData()
        {
            //RespShown = false;

            //Set style for note and header
            HeaderStyle = "noteheader";
            BodyStyle = "notebody";

            if (AltStyle)
            {
                HeaderStyle += "-alt";
                BodyStyle += "-alt";
            }

            // Get data from the server - just the content -
            // we already have the header in the container (index)
            model = await Client.GetNoteContentAsync(new DisplayModelRequest() { Vers = Vers, NoteId = NoteId }, myState.AuthHeader);

            model = model is not null ? model : new();

            // set text to be displayed re responses
            respX = respY = "";
            if (model.Header.ResponseCount > 0)
            {
                respX = " - " + model.Header.ResponseCount + " Responses ";
            }
            else if (model.Header.ResponseOrdinal > 0)
            {
                respX = " Response " + model.Header.ResponseOrdinal;
                respY = "." + model.Header.ResponseOrdinal;
            }

            if (IsRootNote)
            {
                if (RespShown)
                {
                    //RespShown = false;
                    ResetShown = true;
                    respHeaders = MyNoteIndex.GetResponseHeaders(model.Header.Id);
                    if (IsRootNote && RespFlipped && RespShown)
                        respHeaders = respHeaders.OrderByDescending(x => x.ResponseOrdinal).ToList();
                    else if (IsRootNote && RespShown)
                        respHeaders = respHeaders.OrderBy(x => x.ResponseOrdinal).ToList();

                    //StateHasChanged();
                }
            }

        }

        //private void OnClickResp(MouseEventArgs args)
        //{
        //    long bnId = model.header.Id;           // if base note
        //    if (model.header.ResponseOrdinal > 0)   // if response
        //    {
        //        bnId = model.header.BaseNoteId;
        //    }

        //    Navigation.NavigateTo("newnote/" + model.noteFile.Id + "/" + bnId + "/" + model.header.Id);
        //}

        /// <summary>
        /// Handle change of responses shown switch
        /// </summary>
        /// <param name="args">The arguments.</param>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task ShowRespChange(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (RespShown)
            {
                // Get response headers from the index
                respHeaders = MyNoteIndex.GetResponseHeaders(model.Header.Id);
                if (IsRootNote && RespFlipped && RespShown)
                    respHeaders = respHeaders.OrderByDescending(x => x.ResponseOrdinal).ToList();
                else if (IsRootNote && RespShown)
                    respHeaders = respHeaders.OrderBy(x => x.ResponseOrdinal).ToList();

            }
        }

        /// <summary>
        /// Change the order of shown responses
        /// </summary>
        /// <param name="args">The arguments.</param>
        private void FlipRespChange(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args)
        {
            if (RespFlipped)
                respHeaders = respHeaders.OrderByDescending(x => x.ResponseOrdinal).ToList();
            else
                respHeaders = respHeaders.OrderBy(x => x.ResponseOrdinal).ToList();
        }

        /// <summary>
        /// Return to index mode...
        /// </summary>
        /// <param name="args">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        private void OnDone(MouseEventArgs args)
        {
            MyNoteIndex.Listing();
        }

        /// <summary>
        /// Print the note
        /// </summary>
        /// <param name="args">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        private async void OnPrint(MouseEventArgs args)
        {
            await PrintString(false);
        }

        /// <summary>
        /// Print the whole string
        /// </summary>
        /// <param name="args">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        private async void OnPrintString(MouseEventArgs args)
        {
            await PrintString(true);
        }

        /// <summary>
        /// Print a whole string if wholeString is true
        /// </summary>
        /// <param name="wholeString">if set to <c>true</c> [whole string].</param>
        protected async Task PrintString(bool wholeString)
        {
            NoteDisplayIndexModel Model = MyNoteIndex.GetModel();
            string respX = string.Empty;

            // keep track of base note
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            GNoteHeader baseHeader = Model.AllNotes.List.SingleOrDefault(p => p.Id == model.Header.Id);

            GNoteHeader currentHeader = baseHeader;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            StringBuilder sb = new();

            sb.Append("<h4 class=\"text-center\">" + Model.NoteFile.NoteFileTitle + "</h4>");

        reloop: // come back here to do another note
            respX = "";
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            if (currentHeader.ResponseCount > 0)
                respX = " - " + currentHeader.ResponseCount + " Responses ";
            else if (currentHeader.ResponseOrdinal > 0)
                respX = " Response " + currentHeader.ResponseOrdinal;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            sb.Append("<div class=\"noteheader\"><p> <span class=\"keep-right\">Note: ");
            sb.Append(currentHeader.NoteOrdinal + " " + respX);
            sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;</span></p><h4>Subject: ");
            sb.Append(currentHeader.NoteSubject);
            sb.Append("<br />Author: ");
            sb.Append(currentHeader.AuthorName + "    ");
            sb.Append(Globals.LocalTimeBlazor(currentHeader.LastEdited.ToDateTime()).ToLongDateString() + " " + Globals.LocalTimeBlazor(currentHeader.LastEdited.ToDateTime()).ToShortTimeString());

            GNoteContent currentContent = await Client.GetExport2Async(new NoteId() { Id = currentHeader.Id }, myState.AuthHeader);

            if (!string.IsNullOrEmpty(currentHeader.DirectorMessage))
            {
                sb.Append("<br /><span>Director Message: ");
                sb.Append(currentHeader.DirectorMessage);
                sb.Append("</span>");
            }
            //if (tags is not null && tags.Count > 0)
            //{
            //    sb.Append(" <br /><span>Tags: ");
            //    foreach (Tags tag in tags)
            //    {
            //        sb.Append(tag.Tag + " ");
            //    }
            //    sb.Append("</span>");
            //}
            sb.Append("</h4></div><div class=\"notebody\" >");
            sb.Append(currentContent.NoteBody);
            sb.Append("</div>");

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            if (wholeString && currentHeader.ResponseOrdinal < baseHeader.ResponseCount) // more responses in string
            {
                currentHeader = Model.AllNotes.List.Single(p => p.NoteOrdinal == currentHeader.NoteOrdinal && p.ResponseOrdinal == currentHeader.ResponseOrdinal + 1);

                goto reloop;        // print another note
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            currentHeader = baseHeader; // set back to base note
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            string stuff = sb.ToString();           // turn accumulated output into a string

            var parameters = new ModalParameters();
            parameters.Add("PrintStuff", stuff);    // pass string to print dialog
            Modal.Show<PrintDlg>("", parameters);   // invloke print dialog with our output
        }

        /// <summary>
        /// collect input and clear EatEnter
        /// </summary> 
        /// <param name="args">The <see cref="InputEventArgs" /> instance containing the event data.</param>
        private async void NavInputHandler(InputEventArgs args)
        {
            NavString = args.Value;
            await Task.CompletedTask;
            EatEnter = false;
        }

        /// <summary>
        /// Clear the NavString
        /// </summary>
        private async Task ClearNav()
        {
            NavString = string.Empty;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Protential to do something when a key up event happens
        /// Handle single key press commands right away.  Otherwise
        /// let input accumulate.
        /// </summary>
        /// <param name="args">The <see cref="KeyboardEventArgs" /> instance containing the event data.</param>
        private async Task KeyUpHandler(KeyboardEventArgs args)
        {
            switch (NavString)
            {
                case "I":
                case "L":
                    await ClearNav();
                    await MyMenu.ExecMenu("ListNotes");
                    return;

                case "N":
                    await ClearNav();
                    await MyMenu.ExecMenu("NewResponse");
                    return;

                case "Z":
                    await ClearNav();
                    Modal.Show<HelpDialog2>();
                    EatEnter = true;
                    return;

                case "E":
                    await ClearNav();
                    await MyMenu.ExecMenu("Edit");
                    return;

                case "B":
                    await ClearNav();
                    await MyMenu.ExecMenu("PreviousBase");
                    return;

                case "b":
                    await ClearNav();
                    await MyMenu.ExecMenu("PreviousNote");
                    return;

                case "D":
                    await ClearNav();
                    await MyMenu.ExecMenu("Delete");
                    EatEnter = true;
                    return;

                case "F":
                    await ClearNav();
                    await MyMenu.ExecMenu("Forward");
                    return;

                case "C":
                    await ClearNav();
                    await MyMenu.ExecMenu("Copy");
                    return;

                case "m":
                    await ClearNav();
                    await MyMenu.ExecMenu("mail");
                    return;

                case "H":
                    await ClearNav();
                    await MyMenu.ExecMenu("Html");
                    return;

                case "h":
                    await ClearNav();
                    await MyMenu.ExecMenu("html");
                    return;

                case " ":
                    await ClearNav();

                    if (args.ShiftKey)
                    {
                        await NextSearch();
                    }
                    else
                    {
                        await SeqNext();
                    }
                    return;

                default:
                    break;
            }

            if (args.Key == "Enter" && EatEnter) // Ignore this next Enter key!
            {
                EatEnter = false;
                return;
            }

            if (args.Key == "Enter")    // handle enter key
            {
                // lone enter shifted enter key -> Next Base Note
                if (args.ShiftKey && string.IsNullOrEmpty(NavString))
                {
                    await MyMenu.ExecMenu("NextBase");
                    await ClearNav();
                    return;
                }
                // - plus shift enter -> previous base note
                else if (args.ShiftKey && NavString == "-")
                {
                    await MyMenu.ExecMenu("PreviousBase");
                    return;
                }
                // - plus enter -> previous note
                else if (NavString == "-")
                {
                    await MyMenu.ExecMenu("PreviousNote");
                    return;
                }
                // lone enter enter key -> Next Note
                else if (string.IsNullOrEmpty(NavString))
                {
                    await MyMenu.ExecMenu("NextNote");
                    await ClearNav();
                    return;
                }

                // now handle more complex entries
                //
                // cases:
                // #
                // #.#
                // .#
                // +#
                // -#
                // +.#
                // -.#
                // +#.#
                // -#.#
                //
                bool IsPlus = false;
                bool IsMinus = false;
                bool IsRespOnly = false;

                // Ignore ; and spaces
                string stuff = NavString.Replace(";", "").Replace(" ", "");

                // check for modifier prefixes + and -
                if (stuff.StartsWith("+"))
                    IsPlus = true;
                if (stuff.StartsWith("-"))
                    IsMinus = true;

                // now strip the modifiers that we have recorded them
                stuff = stuff.Replace("+", "").Replace("-", "");

                // . implies we are working on response navigation
                if (stuff.StartsWith('.'))
                {
                    await ClearNav();
                    IsRespOnly = true;
                    stuff = stuff.Replace(".", ""); // strip the .
                }

                // parse string for # or #.#
                string[] parts = stuff.Split('.');
                if (parts.Length > 2)
                {
                    ShowMessage("Too many '.'s : " + parts.Length);
                }
                int noteNum;
                if (parts.Length == 1)  // dealing with single number
                {
                    if (!int.TryParse(parts[0], out noteNum))   // try to get value
                    {
                        await ClearNav();
                        EatEnter = true;
                        ShowMessage("Could not parse : " + parts[0]);
                    }
                    else
                    {
                        if (!IsRespOnly) // dealing with notes not responses
                        {
                            if (IsPlus) // add or subtract based on prefix if any
                                noteNum = model.Header.NoteOrdinal + noteNum;
                            else if (IsMinus)
                                noteNum = model.Header.NoteOrdinal - noteNum;
                        }
                        else // dealing with responses
                        {
                            if (IsPlus)  // add or subtract based on prefix if any
                                noteNum = model.Header.ResponseOrdinal + noteNum;
                            else if (IsMinus)
                                noteNum = model.Header.ResponseOrdinal - noteNum;

                            // look for target response
                            long headerId2 = MyNoteIndex.GetNoteHeaderId(model.Header.NoteOrdinal, noteNum);
                            if (headerId2 != 0) // found it!  Get the data and re-render page
                            {
                                NoteId = headerId2;
                                await GetData();
                                StateHasChanged();
                            }
                            else
                                ShowMessage("Could not find note : " + NavString);
                            return;
                        }
                        // look for note
                        long headerId = MyNoteIndex.GetNoteHeaderId(noteNum, 0);
                        if (headerId != 0) // found it!  Get the data and re-render page
                        {
                            NoteId = headerId;
                            await GetData();
                            StateHasChanged();
                        }
                        else
                            ShowMessage("Could not find note : " + NavString);
                        await ClearNav();
                        return;
                    }
                }
                else if (parts.Length == 2)     // #.# entered - direct nav
                {
                    if (!int.TryParse(parts[0], out noteNum))
                    {
                        ShowMessage("Could not parse : " + parts[0]);
                        EatEnter = true;
                    }
                    if (!int.TryParse(parts[1], out int noteRespOrd))
                    {
                        ShowMessage("Could not parse : " + parts[1]);
                        EatEnter = true;
                    }
                    if (noteNum != 0 && noteRespOrd != 0)
                    {
                        {
                            if (IsPlus)     // relative nav forward
                                noteNum = model.Header.NoteOrdinal + noteNum;
                            else if (IsMinus) // relative nav backward
                                noteNum = model.Header.NoteOrdinal - noteNum;
                            // attempt to get note
                            long headerId2 = MyNoteIndex.GetNoteHeaderId(noteNum, 0);
                            if (headerId2 != 0) // found it!  Get the data and re-render page
                            {
                                NoteId = headerId2;
                                await GetData();
                                StateHasChanged();
                            }
                            else
                                ShowMessage("Could not find note : " + NavString);
                        }
                        long headerId = MyNoteIndex.GetNoteHeaderId(noteNum, noteRespOrd);
                        if (headerId != 0)
                        {
                            NoteId = headerId;
                            await GetData();
                            StateHasChanged();
                        }
                        else
                            ShowMessage("Could not find note : " + NavString);
                    }
                    await ClearNav();
                }
            }
        }

        /// <summary>
        /// GET next matching item from search
        /// </summary>
        protected async Task NextSearch()
        {
            bool InSearch = await sessionStorage.GetItemAsync<bool>("InSearch");
            if (!InSearch)
                return;

            int SearchIndex = await sessionStorage.GetItemAsync<int>("SearchIndex");
            List<GNoteHeader> SearchList = await sessionStorage.GetItemAsync<List<GNoteHeader>>("SearchList");

            if ((++SearchIndex + 1) < SearchList.Count)
            {
                long mode = SearchList[SearchIndex].Id;
                await sessionStorage.SetItemAsync<int>("SearchIndex", SearchIndex);

                NoteId = mode;
                await GetData();
                StateHasChanged();
            }
            else
            {
                await sessionStorage.SetItemAsync<bool>("InSearch", false);
                await sessionStorage.RemoveItemAsync("SearchIndex");
                await sessionStorage.RemoveItemAsync("SearchList");

                ShowMessage("Search Completed!");
            }
        }

        /// <summary>
        /// Find next recent note
        /// </summary>
        protected async Task SeqNext()
        {
            if (!IsSeq)
                return;

            int currentIndex = await sessionStorage.GetItemAsync<int>("SeqHeaderIndex");
            List<GNoteHeader> headerList = await sessionStorage.GetItemAsync<List<GNoteHeader>>("SeqHeaders");
            if (headerList.Count > ++currentIndex) // proceed to next note in file
            {
                await sessionStorage.SetItemAsync("SeqHeaderIndex", currentIndex);

                GNoteHeader currHeader = headerList[currentIndex];

                await sessionStorage.SetItemAsync("CurrentSeqHeader", currHeader);

                NoteId = currHeader.Id;
                await GetData();
                StateHasChanged();
            }
            else
            {
                // update seq entry for user
                GSequencer seq = await sessionStorage.GetItemAsync<GSequencer>("SeqItem");
                seq.Active = false;
                await Client.UpdateSequencerAsync(seq, myState.AuthHeader);

                // goto next file
                List<GSequencer> sequencers = await sessionStorage.GetItemAsync<List<GSequencer>>("SeqList");
                int seqIndex = await sessionStorage.GetItemAsync<int>("SeqIndex");
                if (sequencers.Count <= ++seqIndex)
                {
                    await sessionStorage.SetItemAsync("IsSeq", false);
                    await sessionStorage.RemoveItemAsync("SeqList");
                    await sessionStorage.RemoveItemAsync("SeqItem");
                    await sessionStorage.RemoveItemAsync("SeqIndex");

                    await sessionStorage.RemoveItemAsync("SeqHeaders");
                    await sessionStorage.RemoveItemAsync("SeqHeaderIndex");
                    await sessionStorage.RemoveItemAsync("CurrentSeqHeader");

                    ShowMessage("You have seen all the new notes!");

                    return;  // end it all
                }

                GSequencer currSeq = sequencers[seqIndex];

                await sessionStorage.SetItemAsync("SeqItem", currSeq);
                await sessionStorage.SetItemAsync("SeqIndex", seqIndex);

                Navigation.NavigateTo("noteindex/" + -currSeq.NoteFileId);
            }
        }

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void ShowMessage(string message)
        {
            var parameters = new ModalParameters();
            parameters.Add("MessageInput", message);
            Modal.Show<MessageBox>("", parameters);
        }

        /// <summary>
        /// The module
        /// </summary>
        private IJSObjectReference? module;

        /// <summary>
        /// On after render as an asynchronous operation.
        /// </summary>
        /// <param name="firstRender">Set to <c>true</c> if this is the first time <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> has been invoked
        /// on this component instance; otherwise <c>false</c>.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <remarks>The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> and <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)" /> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        /// once.</remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)    // load the prism script
            {
                module = await JS.InvokeAsync<IJSObjectReference>("import",
                    "./prism.min.js");
            }
            else
            {
                if (ResetShown)
                {
                    ResetShown = false;
                    //RespShown = true;
                    //StateHasChanged();
                }
                // have to wait a bit before putting focus in textbox
                if (sfTextBox is not null)
                {
                    //await Task.Delay(300);
                    await sfTextBox.FocusAsync();
                }
                if (module is not null) // highight inserted code with prism script
                {
                    await module.InvokeVoidAsync("doPrism", "x");
                }
            }
        }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        async ValueTask IAsyncDisposable.DisposeAsync()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        {
            if (module is not null)
            {
                await module.DisposeAsync();
            }
        }
    }
}