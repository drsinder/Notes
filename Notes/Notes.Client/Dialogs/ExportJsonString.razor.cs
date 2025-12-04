using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Notes.Protos;
using System.Text;
using Google.Protobuf;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Provides functionality to export note header data as a JSON file within a Blazor modal dialog component.
    /// </summary>
    /// <remarks>This component integrates with Blazored Modal and JavaScript interop to enable exporting note
    /// data to a downloadable JSON file. It supports asynchronous lifecycle methods for initialization and resource
    /// management. The export process serializes note data using protobuf JSON formatting to ensure compatibility with
    /// the expected data structure.</remarks>
    public partial class ExportJsonString
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter] public BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the model.  Represents what we want to export
        /// </summary>
        /// <value>The model.</value>
        [Parameter] public NoteHeadersRequest model { get; set; }

        /// <summary>
        /// Used to write file
        /// </summary>
        private IJSObjectReference? module;

        /// <summary>
        /// On initialized as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnInitializedAsync()
        {
            MemoryStream ms2 = await DoExport();

            await SaveAs(model.FileName, ms2.GetBuffer());
            ms2.Dispose();

            await ModalInstance.CancelAsync();
        }

        /// <summary>
        /// Does the export.
        /// </summary>
        /// <returns>MemoryStream.</returns>
        private async Task<MemoryStream> DoExport()
        {
            StringContent stringContent;
            JsonExport wrapper = new JsonExport();

            wrapper.NoteFile = await Client.GetNoteFileAsync(new NoteFileRequest() { NoteFileId = model.NoteFileId }, myState.AuthHeader);
            wrapper.NoteHeaders = await Client.GetNoteHeadersAsync(model, myState.AuthHeader);

            // Use protobuf JsonFormatter for correct protobuf serialization
            var json = JsonFormatter.Default.Format(wrapper);
            stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            Stream ms0 = await stringContent.ReadAsStreamAsync();
            MemoryStream ms = new MemoryStream();
            await ms0.CopyToAsync(ms);
            ms0.Dispose();
            ms.Close();
            return ms;
        }

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
            if (firstRender)
            {
                module = await JS.InvokeAsync<IJSObjectReference>("import",
                    "./scripts.js");
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (module is not null)
            {
                await module.DisposeAsync();
            }
        }

        /// <summary>
        /// Saves as.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="data">The data.</param>
        public async Task SaveAs(string filename, byte[] data)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await module.InvokeVoidAsync("saveAsFile", filename, Convert.ToBase64String(data));
#pragma warning restore CS8604 // Possible null reference argument.
        }
    }

}