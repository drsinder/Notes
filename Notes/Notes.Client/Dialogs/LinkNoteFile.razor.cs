using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Notes.Protos;
using Syncfusion.Blazor.Grids;

namespace Notes.Client.Dialogs
{
    public partial class LinkNoteFile
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
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject] NotesServer.NotesServerClient Client { get; set; }

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

        [Parameter]
        public HomePageModel Model { get; set; }

        protected List<GLinkedFile> myLinks { get; set; }

        private SfGrid<GLinkedFile> MyGrid;


        protected override async Task OnParametersSetAsync()
        {
            myLinks = [.. Model.LinkedFiles.Where(x => x.HomeFileId == FileId)];
        }

        /// <summary>
        /// We are done
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CancelAsync();
        }

        private async Task TestLink(GLinkedFile link)
        {
            LinkTestMessage linkTestMessage = new LinkTestMessage
            {
                RemoteBaseUri = link.RemoteBaseUri,
                RemoteFileName = link.RemoteFileName,
                Secret = link.Secret
            };
            RequestStatus status = await Client.LocalTestLinkConnectionAsync(linkTestMessage);
            var parameters = new ModalParameters();
            parameters.Add("MessageInput", status.Message);
            _ = Modal.Show<Dialogs.MessageBox>("Status", parameters);
        }

    }
}