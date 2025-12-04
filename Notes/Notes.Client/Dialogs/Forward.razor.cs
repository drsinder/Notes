using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Notes.Protos;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Represents a component that manages forwarding operations, including displaying the forward view and handling
    /// user actions to forward content.
    /// </summary>
    /// <remarks>This class is typically used within a Blazor modal dialog to facilitate forwarding
    /// functionality. It interacts with a modal instance and a forward view model to process user input and perform the
    /// forward operation. The class is designed to be used as part of a UI workflow where users can review and submit
    /// forwarding details.</remarks>
    public partial class Forward
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the forward view.
        /// </summary>
        /// <value>The forward view.</value>
        [Parameter]
        public ForwardViewModel ForwardView { get; set; }

        /// <summary>
        /// Forwardits this instance.
        /// </summary>
        private async Task Forwardit()
        {
            if (ForwardView.ToEmail is null || ForwardView.ToEmail.Length < 8 || !ForwardView.ToEmail.Contains("@") || !ForwardView.ToEmail.Contains("."))
                return;
            await Client.DoForwardAsync(ForwardView, myState.AuthHeader);
            await ModalInstance.CancelAsync();
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