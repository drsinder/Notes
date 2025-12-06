/*--------------------------------------------------------------------------
    **
    **  Copyright © 2026, Dale Sinder
    **
    **  Name: Preferences.razor
    **
    **  Description:
    **      Set user preferences
    **
    **  This program is free software: you can redistribute it and/or modify
    **  it under the terms of the GNU General Public License version 3 as
    **  published by the Free Software Foundation.
    **
    **  This program is distributed in the hope that it will be useful,
    **  but WITHOUT ANY WARRANTY; without even the implied warranty of
    **  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    **  GNU General Public License version 3 for more details.
    **
    **  You should have received a copy of the GNU General Public License
    **  version 3 along with this program in file "license-gpl-3.0.txt".
    **  If not, see <http://www.gnu.org/licenses/gpl-3.0.txt>.
    **
    **--------------------------------------------------------------------------*/

using Notes.Protos;

namespace Notes.Client.Pages
{
    /// <summary>
    /// Represents a container for managing user preferences and related data within the application.
    /// </summary>
    /// <remarks>The Preferences class encapsulates user-specific settings, such as page size and available
    /// size options, and provides methods for initializing, updating, and canceling preference changes. It is typically
    /// used to persist and retrieve user customization data, ensuring a personalized experience across
    /// sessions.</remarks>
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
            MySizes = new List<LocalModel2> { new("0", "All"), new("5"), new("10"), new("12"), new("15"), new("18"), new("20") };
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