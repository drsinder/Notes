using Microsoft.AspNetCore.Components;
using Notes.Protos;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Represents a UI component that displays and manages access permissions using a checkbox interface.
    /// </summary>
    /// <remarks>Use this component to allow users to view and modify specific access rights for an item. The
    /// component updates the underlying access model and communicates changes to the server when the checkbox state is
    /// toggled. This class is typically used within a Blazor application to bind access control data and handle
    /// permission updates interactively.</remarks>
    public partial class AccessCheckBox
    {
        /// <summary>
        /// The item and its full token
        /// </summary>
        /// <value>The model.</value>
        [Parameter]
        public AccessItem Model { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject] NotesServer.NotesServerClient Client { get; set; }
    
        /// <summary>
        /// Initializes a new instance of the <see cref="AccessCheckBox"/> class.
        /// </summary>
        public AccessCheckBox() { }

        /// <summary>
        /// Invert checked state and update
        /// </summary>
        protected async Task OnClick()
        {
            Model.isChecked = !Model.isChecked;
            switch (Model.which)
            {
                case AccessX.ReadAccess:
                    {
                        Model.Item.ReadAccess = Model.isChecked;
                        break;
                    }

                case AccessX.Respond:
                    {
                        Model.Item.Respond = Model.isChecked;
                        break;
                    }

                case AccessX.Write:
                    {
                        Model.Item.Write = Model.isChecked;
                        break;
                    }

                case AccessX.DeleteEdit:
                    {
                        Model.Item.DeleteEdit = Model.isChecked;
                        break;
                    }

                case AccessX.SetTag:
                    {
                        Model.Item.SetTag = Model.isChecked;
                        break;
                    }

                case AccessX.ViewAccess:
                    {
                        Model.Item.ViewAccess = Model.isChecked;
                        break;
                    }

                case AccessX.EditAccess:
                    {
                        Model.Item.EditAccess = Model.isChecked;
                        break;
                    }

                default:
                    break;
            }

            _ = await Client.UpdateAccessItemAsync(Model.Item, myState.AuthHeader);
        }
    }
}