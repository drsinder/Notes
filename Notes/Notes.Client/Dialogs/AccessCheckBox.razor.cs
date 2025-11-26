using Microsoft.AspNetCore.Components;
using Notes.Protos;

namespace Notes.Client.Dialogs
{
    public partial class AccessCheckBox
    {
        /// <summary>
        /// The item and its full toekn
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