using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Notes.Protos;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Class Forward.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
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