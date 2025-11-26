using Blazored.Modal;
using Microsoft.AspNetCore.Components;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Class MessageBox.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class MessageBox
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the message input.
        /// </summary>
        /// <value>The message input.</value>
        [Parameter]
        public string MessageInput { get; set; }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CancelAsync();
        }
    }

}