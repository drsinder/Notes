using Notes.Protos;

namespace Notes.Client.Pages
{
    public partial class About
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        private AboutModel model { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Gets or sets up time.
        /// </summary>
        /// <value>Up time.</value>
        private TimeSpan upTime { get; set; }

        /// <summary>
        /// The text
        /// </summary>
        private string text = string.Empty;
        /// <summary>
        /// Get some simple stuff from server
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnInitializedAsync()
        {
            text = (await Client.GetTextFileAsync(new AString()
            { Val = "about.html" })).Val;
            try
            {
                model = await Client.GetAboutAsync(new NoRequest());
            }
            finally
            {
            }
        }

    }
}