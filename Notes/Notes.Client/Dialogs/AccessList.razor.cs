using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Notes.Protos;
using Syncfusion.Blazor.Grids;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Access editor for a files access tokens
    /// </summary>
    public partial class AccessList
    {
        /// <summary>
        /// Gets or sets the modal.
        /// </summary>
        /// <value>The modal.</value>
        [CascadingParameter] public IModalService Modal { get; set; }
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter] public BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// File Id we are working on
        /// </summary>
        /// <value>The file identifier.</value>
        [Parameter] public int fileId { get; set; }

        /// <summary>
        /// Grid of tokens
        /// </summary>
        private SfGrid<GNoteAccess> MyGrid;

        /// <summary>
        /// List of tokens
        /// </summary>
        /// <value>My list.</value>
        private List<GNoteAccess> myList { get; set; }

        /// <summary>
        /// Temp list of tokens
        /// </summary>
        /// <value>The user list.</value>
        private List<GAppUser> userList { get; set; }

        /// <summary>
        /// My access
        /// </summary>
        /// <value>My access.</value>
        private GNoteAccess myAccess { get; set; }
        /// <summary>
        /// Gets or sets the arc identifier.
        /// </summary>
        /// <value>The arc identifier.</value>
        private int arcId { get; set; }

        /// <summary>
        /// message to display
        /// </summary>
        /// <value>The message.</value>
        private string message { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject] NotesServer.NotesServerClient Client { get; set; }
        /// <summary>
        /// Gets or sets the session storage.
        /// </summary>
        /// <value>The session storage.</value>
        [Inject] Blazored.SessionStorage.ISessionStorageService sessionStorage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessList"/> class.
        /// </summary>
        public AccessList()
        {
        }

        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected async override Task OnParametersSetAsync()
        {
            arcId = await sessionStorage.GetItemAsync<int>("ArcId");

            AccessAndUserList myLists = await Client.GetAccessAndUserListAsync(new AccessAndUserListRequest() 
            { FileId = fileId, ArcId = arcId, UserId = myState.UserInfo?.Subject }, myState.AuthHeader);

            myList = myLists.AccessList.List.ToList();
            userList = myLists.AppUsers.List.ToList();
            myAccess = myLists.UserAccess;
        }

        /// <summary>
        /// We are done
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CancelAsync();
        }

        /// <summary>
        /// Add a new token for another user
        /// </summary>
        protected async void CreateNew()
        {
            var parameters = new ModalParameters();
            parameters.Add("userList", userList);
            parameters.Add("NoteFileId", fileId);

            var xx = Modal.Show<AddAccessDlg>("", parameters);
            await xx.Result;

            StateHasChanged();
            await MyGrid.Refresh();
        }

        /// <summary>
        /// Item deleted - refresh list
        /// </summary>
        /// <param name="newMessage">The new message.</param>
        protected async Task ClickHandler(string newMessage)
        {
            arcId = await sessionStorage.GetItemAsync<int>("ArcId");

            GNoteAccessList myLists = await Client.GetAccessListAsync(new AccessAndUserListRequest() { FileId = fileId, ArcId = arcId, UserId = myState.UserInfo?.Subject }, myState.AuthHeader);
            myList = myLists.List.ToList();
            StateHasChanged();
            await MyGrid.Refresh();
        }

    }

}