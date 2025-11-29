using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Notes.Protos;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Class HelpDialog.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class HelpDialog
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// The text
        /// </summary>
        private string text = string.Empty;
        /// <summary>
        /// Get some simple stuff from server
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
       

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CancelAsync();
        }
    }
}