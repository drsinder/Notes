using Notes.Protos;

namespace Notes.Client.Pages
{
    /// <summary>
    /// Class Preferences.
    /// Implements the <see cref="Microsoft.AspNetCore.Components.ComponentBase" />
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Components.ComponentBase" />
    public partial class Preferences
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>
        /// Gets or sets the user data.
        /// </summary>
        /// <value>The user data.</value>
        private GAppUser UserData { get; set; }

        /// <summary>
        /// Gets or sets the current text.
        /// </summary>
        /// <value>The current text.</value>
        private string currentText { get; set; }

        /// <summary>
        /// Gets or sets my sizes.
        /// </summary>
        /// <value>My sizes.</value>
        private List<LocalModel2> MySizes { get; set; }

        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>The size of the page.</value>
        private string pageSize { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// On initialized as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnInitializedAsync()
        {
            UserData = await Client.GetUserDataAsync(new NoRequest(), myState.AuthHeader);
            pageSize = UserData.Ipref2.ToString();
            MySizes = new List<LocalModel2> { new LocalModel2("0", "All"), new LocalModel2("5"), new LocalModel2("10"), new LocalModel2("12"), new LocalModel2("20") };
            currentText = " ";
        }

        /// <summary>
        /// Called when [submit].
        /// </summary>
        private async Task OnSubmit()
        {
            UserData.Ipref2 = int.Parse(pageSize);
            await Client.UpdateUserDataAsync(UserData, myState.AuthHeader);
            Navigation.NavigateTo("");
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            Navigation.NavigateTo("");
        }

        /// <summary>
        /// Class LocalModel2.
        /// </summary>
        public class LocalModel2
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LocalModel2"/> class.
            /// </summary>
            /// <param name="psize">The psize.</param>
            public LocalModel2(string psize)
            {
                Psize = psize;
                Name = psize;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="LocalModel2"/> class.
            /// </summary>
            /// <param name="psize">The psize.</param>
            /// <param name="name">The name.</param>
            public LocalModel2(string psize, string name)
            {
                Psize = psize;
                Name = name;
            }

            /// <summary>
            /// Gets or sets the psize.
            /// </summary>
            /// <value>The psize.</value>
            public string Psize { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; set; }
        }
    }
}