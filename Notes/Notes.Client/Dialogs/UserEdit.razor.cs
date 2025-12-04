using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Notes.Protos;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Represents a Blazor component for editing a user's roles and details within a modal dialog.
    /// </summary>
    /// <remarks>This component is typically used within a modal context to display and update user
    /// information. It interacts with a backend service to retrieve and update user roles. The component requires a
    /// valid user identifier and a modal instance to function correctly.</remarks>
    public partial class UserEdit
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter] public BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        [Parameter] public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        protected EditUserViewModel Model { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject] NotesServer.NotesServerClient Client { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEdit"/> class.
        /// </summary>
        public UserEdit()
        {
        }

        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            Model = await Client.GetUserRolesAsync(new AppUserRequest() { Subject = UserId }, myState.AuthHeader);
        }

        /// <summary>
        /// Submits this instance.
        /// </summary>
        private async Task Submit()
        {
            await Client.UpdateUserRolesAsync(Model, myState.AuthHeader);
            await ModalInstance.CancelAsync();
        }


        /// <summary>
        /// Dones this instance.
        /// </summary>
        private async Task Done()
        {
            await ModalInstance.CancelAsync();
        }


    }
}