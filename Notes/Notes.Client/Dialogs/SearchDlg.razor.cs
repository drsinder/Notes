using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using static Notes.Client.Pages.NoteIndex;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Represents a modal dialog component used for configuring and initiating a search operation within the
    /// application.
    /// </summary>
    /// <remarks>This component is typically displayed as a modal dialog and allows users to specify search
    /// criteria such as type, text, and options. It interacts with the parent context via BlazoredModal to return the
    /// configured search parameters or to cancel the operation. The dialog supports various search options, including
    /// author, title, content, tags, and time-based filters. Usage of this component requires integration with
    /// BlazoredModal and appropriate parameter binding from the parent component.</remarks>
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

        private bool casesensitive { get; set; } = false;

        private bool wholeword { get; set; } = false;

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
                case 3: target.Option = Client.Pages.NoteIndex.SearchOption.Content; 
                    target.CaseSensitive = casesensitive;
                    target.WholeWords = wholeword;
                    break;
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