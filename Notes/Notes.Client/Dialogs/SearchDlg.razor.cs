using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using static Notes.Client.Pages.NoteIndex;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Class SearchDlg.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class SearchDlg
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter] BlazoredModalInstance ModalInstance { get; set; }

        //[Parameter] public TZone zone { get; set; }
        /// <summary>
        /// Gets or sets the searchtype.
        /// </summary>
        /// <value>The searchtype.</value>
        [Parameter] public string searchtype { get; set; }

        //string Message { get; set; }

        /// <summary>
        /// Gets or sets the option.
        /// </summary>
        /// <value>The option.</value>
        private int option { get; set; }
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        private string text { get; set; }
        /// <summary>
        /// Gets or sets the time.
        /// </summary>
        /// <value>The time.</value>
        private DateTime theTime { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            option = 0;
            theTime = DateTime.UtcNow;
        }
        /// <summary>
        /// Searchfors this instance.
        /// </summary>
        private void Searchfor()
        {
            Search target = new Search();
            switch (option)
            {
                case 1: target.Option = Client.Pages.NoteIndex.SearchOption.Author; break;
                case 2: target.Option = Client.Pages.NoteIndex.SearchOption.Title; break;
                case 3: target.Option = Client.Pages.NoteIndex.SearchOption.Content; break;
                case 4: target.Option = Client.Pages.NoteIndex.SearchOption.DirMess; break;
                case 5: target.Option = Client.Pages.NoteIndex.SearchOption.Tag; break;
                case 6: target.Option = Client.Pages.NoteIndex.SearchOption.TimeIsBefore; break;
                case 7: target.Option = Client.Pages.NoteIndex.SearchOption.TimeIsAfter; break;
                default: return;
            }

            if (text is null)
                text = String.Empty;
            target.Text = text;

            //theTime = zone.Universal(theTime);
            target.Time = theTime;

            ModalInstance.CloseAsync(ModalResult.Ok<Search>(target));
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CloseAsync(ModalResult.Cancel());
        }

    }

}