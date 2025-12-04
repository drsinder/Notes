using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Represents an email input component that interacts with a modal dialog.
    /// </summary>
    /// <remarks>This class is typically used within a Blazored modal to capture and return an email address
    /// entered by the user. It provides mechanisms to confirm or cancel the input, communicating the result back to the
    /// modal infrastructure. The class relies on a cascading parameter for modal interaction and is intended for use in
    /// Blazor applications.</remarks>
    public partial class Email
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        public BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the emailaddr.
        /// </summary>
        /// <value>The emailaddr.</value>
        public string emailaddr { get; set; }

        /// <summary>
        /// Oks this instance.
        /// </summary>
        private void Ok()
        {
            ModalInstance.CloseAsync(ModalResult.Ok(emailaddr));
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CancelAsync();
        }
    }

}