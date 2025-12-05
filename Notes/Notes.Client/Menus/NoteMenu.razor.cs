using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Notes.Client.Comp;
using Notes.Client.Dialogs;
using Notes.Client.Pages;
using Notes.Protos;
using Syncfusion.Blazor.Navigations;
using System.Timers;

namespace Notes.Client.Menus
{
    /// <summary>
    /// Represents the menu component for note-related actions, providing user interface options for navigating,
    /// editing, exporting, and managing notes within the application.
    /// </summary>
    /// <remarks>The NoteMenu component dynamically constructs its menu items based on the user's access
    /// rights and the current note context. It supports actions such as listing notes, navigating between notes,
    /// creating responses, editing, deleting, exporting, and forwarding notes. The component integrates with modal
    /// dialogs for user confirmations and additional actions. It is typically used within a container that provides the
    /// necessary model and navigation context.</remarks>
    public partial class NoteMenu
    {
        /// <summary>
        /// For dialogs
        /// </summary>
        /// <value>The modal.</value>
        [CascadingParameter] public IModalService Modal { get; set; }

        /// <summary>
        /// Model data reference from container
        /// </summary>
        /// <value>The model.</value>
        [Parameter] public DisplayModel Model { get; set; }

        /// <summary>
        /// Reference to our caller/container so we can call back into it.
        /// </summary>
        /// <value>The index of my note.</value>
        [Parameter] public NoteIndex MyNoteIndex { get; set; }

        /// <summary>
        /// Menu structure
        /// </summary>
        /// <value>The menu items.</value>
#pragma warning disable IDE1006 // Naming Styles
        private static List<MenuItem> menuItems { get; set; }

        /// <summary>
        /// Top level menu instance
        /// </summary>
        /// <value>The top menu.</value>
        protected SfMenu<MenuItem> topMenu { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Gets or sets a value indicating whether [hamburger mode].
        /// </summary>
        /// <value><c>true</c> if [hamburger mode]; otherwise, <c>false</c>.</value>
        private bool HamburgerMode { get; set; } = false;

        /// <summary>
        /// Gets or sets the navigation.
        /// </summary>
        /// <value>The navigation.</value>
        [Inject] NavigationManager Navigation { get; set; }

        [Inject] CookieStateAgent myState { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteMenu"/> class.
        /// </summary>
        public NoteMenu()
        {
        }

        /// <summary>
        /// Construct our menu based on user access token
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> representing any asynchronous operation.</returns>
        protected override Task OnParametersSetAsync()
        {
            menuItems = new List<MenuItem>();

#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
            MenuItem item = new() { Id = "ListNotes", Text = "Listing" };
            menuItems.Add(item);

            item = new() { Id = "NextBase", Text = "Next Base" };
            menuItems.Add(item);

            item = new() { Id = "PreviousBase", Text = "Previous Base" };
            menuItems.Add(item);

            item = new() { Id = "NextNote", Text = "Next" };
            menuItems.Add(item);

            item = new() { Id = "PreviousNote", Text = "Previous" };
            menuItems.Add(item);

            if (Model.Access.ReadAccess)
            {
                MenuItem item2 = new()
                {
                    Id = "OutPutMenu",
                    Text = "Output",
                    Items = new List<MenuItem>
                {
                    new() { Id = "Forward", Text = "Forward" },
                    new() { Id = "Copy", Text = "Copy" },
                    new() { Id = "mail", Text = "mail" },
                    //item2.Items.Add(new MenuItem() { Id = "Mark", Text = "Mark for output" });
                    new() { Id = "Html", Text = "Html (expandable)" },
                    new() { Id = "html", Text = "html (flat)" },
                    new() { Id = "Json", Text = "Json" }
                }
                };
                menuItems.Add(item2);

                if (Model.Access.Respond)
                {
                    item = new() { Id = "NewResponse", Text = "New Response" };
                    menuItems.Add(item);
                }

                if (Model.CanEdit)
                {
                    item = new() { Id = "Edit", Text = "Edit" };
                    menuItems.Add(item);

                    if (Model.Access.UserID == Model.Header.AuthorID || Model.IsAdmin)
                    {
                        item = new() { Id = "Delete", Text = "Delete" };
                        menuItems.Add(item);
                    }
                }

                //menuItems.Add(new() { Id = "SearchFromNote", Text = "Search" });
                menuItems.Add(new() { Id = "NoteHelp", Text = "Z for HELP" });
            }

            return Task.CompletedTask;
        }
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.
        /// <summary>
        /// Menu item invoked
        /// </summary>
        /// <param name="e">The e.</param>
        public async Task OnSelect(MenuEventArgs<MenuItem> e)
        {
            await ExecMenu(e.Item.Id);
        }

        /// <summary>
        /// This can be called not only from above but also by the container
        /// that shares the same model
        /// </summary>
        /// <param name="id">The identifier.</param>
        public async Task ExecMenu(string id)
        {
            long myId;
            switch (id)
            {
                case "ListNotes":
                    MyNoteIndex.Listing();
                    break;

                case "NewResponse":
                    long bnId = Model.Header.Id;           // if base note
                    if (Model.Header.ResponseOrdinal > 0)   // if response
                    {
                        bnId = Model.Header.BaseNoteId;
                    }
                    Navigation.NavigateTo("newnote/" + Model.NoteFile.Id + "/" + bnId + "/" + Model.Header.Id);
                    break;

                case "Edit":
                    if (Model.CanEdit)
                        Navigation.NavigateTo("editnote/" + Model.Header.Id);
                    break;

                case "NextBase":
                    myId = MyNoteIndex.GetNextBaseNote(Model.Header);
                    if (myId > 0)
                    {
                        MyNoteIndex.GotoNote(myId);
                    }
                    break;

                case "PreviousBase":
                    myId = MyNoteIndex.GetPreviousBaseNote(Model.Header);

                    if (myId > 0)
                    {
                        MyNoteIndex.GotoNote(myId);
                    }
                    break;

                case "NextNote":
                    myId = MyNoteIndex.GetNextNote(Model.Header);
                    if (myId > 0)
                    {
                        MyNoteIndex.GotoNote(myId);
                    }
                    break;

                case "PreviousNote":
                    myId = MyNoteIndex.GetPreviousNote(Model.Header);

                    if (myId > 0)
                    {
                        MyNoteIndex.GotoNote(myId);
                    }
                    break;

                case "NoteHelp":
                    Modal.Show<HelpDialog2>();
                    break;

                case "Delete":
                    if (Model.IsAdmin || Model.CanEdit)
                    {
                        if (!await YesNo("Are you sure you want to delete this note?"))
                            return;
                        await Client.DeleteNoteAsync(new NoteId() { Id = Model.Header.Id }, myState.AuthHeader);
                        Navigation.NavigateTo("notedisplay/" + Model.Header.Id);   //, true);
                    }
                    else
                    {
                        ShowMessage("You may not delete this note.");
                    }

                    break;

                case "Forward":
                    Forward();
                    break;

                case "Copy":
                    var parameters = new ModalParameters();
                    parameters.Add("Note", Model.Header);
                    Modal.Show<Copy>("", parameters);
                    break;

                case "mail":
                    await DoEmail();
                    break;

                case "Html":
                    DoExport(true, true);
                    break;

                case "html":
                    DoExport(true, false);
                    break;

                case "Json":
                    DoJson();
                    break;
            }
        }

        protected void DoJson()
        {
            NoteHeadersRequest request = new()
            {
                NoteFileId = Model.NoteFile.Id,
                ArcId = Model.Header.ArchiveId,
                NoteOrdinal = Model.Header.NoteOrdinal,
                ResponseOrdinal = -1,
                ContentAndTags = true,
                FileName = Model.NoteFile.NoteFileName + ".note-" + Model.Header.NoteOrdinal + ".json",
                NestResponses = true
            };

            if (Model.Header.ResponseOrdinal > 0)
            {
                request.ResponseOrdinal = Model.Header.ResponseOrdinal;
                request.FileName = Model.NoteFile.NoteFileName + ".note-" + Model.Header.NoteOrdinal + "-response-" + Model.Header.ResponseOrdinal + ".json";
            }
            else if (!MyNoteIndex.MyNotePanel.ShowResp)
            {
                request.ResponseOrdinal = 0;
            }


            var parameters = new ModalParameters();
            parameters.Add("model", request);
            Modal.Show<ExportJsonString>("", parameters);

        }

        /// <summary>
        /// Forwards a note
        /// </summary>
        protected void Forward()
        {
            var parameters = new ModalParameters();
            ForwardViewModel fv = new()
            {
                NoteID = Model.Header.Id,
                FileID = Model.Header.NoteFileId,
                ArcID = Model.Header.ArchiveId,
                NoteOrdinal = Model.Header.NoteOrdinal,
                NoteSubject = Model.Header.NoteSubject,
                NoteFile = Model.NoteFile
            };

            if (Model.Header.ResponseCount > 0 || Model.Header.BaseNoteId > 0)
                fv.Hasstring = true;

            parameters.Add("ForwardView", fv);

            Modal.Show<Forward>("", parameters);
        }

        /// <summary>
        /// Exports a note
        /// </summary>
        /// <param name="isHtml">if set to <c>true</c> [is HTML].</param>
        /// <param name="isCollapsible">if set to <c>true</c> [is collapsible].</param>
        /// <param name="isEmail">if set to <c>true</c> [is email].</param>
        /// <param name="emailaddr">The emailaddr.</param>
        private void DoExport(bool isHtml, bool isCollapsible, bool isEmail = false, string emailaddr = "")
        {
            var parameters = new ModalParameters();

            
                        ExportViewModel vm = new()
                        {
                            ArchiveNumber = Model.Header.ArchiveId,
                            isCollapsible = isCollapsible,
                            isDirectOutput = !isEmail,
                            isHtml = isHtml,
                            NoteFile = Model.NoteFile,
                            NoteOrdinal = Model.Header.NoteOrdinal,
                            Email = emailaddr
                        };
                        //vm.myMenu = this;

                        parameters.Add("Model", vm);
                        parameters.Add("FileName", Model.NoteFile.NoteFileName + (isHtml ? ".html" : ".txt"));

                        Modal.Show<ExportUtil1>("", parameters);
            
        }

        /// <summary>
        /// Emails a note
        /// </summary>
        private async Task DoEmail()
        {
            
            string emailaddr;
            var parameters = new ModalParameters();
            var formModal = Modal.Show<Dialogs.Email>("", parameters);
            var result = await formModal.Result;
            if (result.Cancelled)
                return;
            emailaddr = (string)result.Data;
            if (string.IsNullOrEmpty(emailaddr))
                return;

            DoExport(true, true, true, emailaddr);
            
        }

        /// <summary>
        /// Confirmation dialog
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private async Task<bool> YesNo(string message)
        {
            var parameters = new ModalParameters();
            parameters.Add("MessageInput", message);
             
                        var formModal = Modal.Show<YesNo>("", parameters);
                        var result = await formModal.Result;
                        return !result.Cancelled;
            
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

    }
}