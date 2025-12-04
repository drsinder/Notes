using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Represents a modal dialog component that displays a yes/no confirmation message.
    /// </summary>
    /// <remarks>Use this component to prompt users for confirmation before proceeding with an action. The
    /// dialog displays a custom message and provides options to confirm or cancel. Integrates with Blazored Modal for
    /// modal management.</remarks>
    public partial class YesNo
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

        /// <summary>
        /// Oks this instance.
        /// </summary>
        private void Ok()
        {
            ModalInstance.CloseAsync(ModalResult.Ok("Yes"));
        }
    }

}