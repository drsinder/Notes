using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Class Email.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
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