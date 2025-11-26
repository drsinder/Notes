using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.RichTextEditor;
using System.Timers;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// This takes the provided PrintStuff string and puts it in a
    /// Syncfusion editor component for the user to see.  It then allows
    /// the user to press a button to print.  TO do the printing
    /// the built in print function of the Sysncfusion editor is
    /// used so printing does not have to be separately implemented.
    /// </summary>
    public partial class PrintDlg : ComponentBase
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the print stuff.
        /// </summary>
        /// <value>The print stuff.</value>
        [Parameter]
        public string PrintStuff { get; set; }

#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// Gets or sets a value indicating whether [readonly print].
        /// </summary>
        /// <value><c>true</c> if [readonly print]; otherwise, <c>false</c>.</value>
        private bool readonlyPrint { get; set; }

        /// <summary>
        /// The rte object
        /// </summary>
        private SfRichTextEditor RteObj { get; set; }
        /// <summary>
        /// Gets or sets the timer2.
        /// </summary>
        /// <value>The timer2.</value>
        private System.Timers.Timer timer2 { get; set; }
        /// <summary>
        /// Ons the print.
        /// </summary>
        private void onPrint(MouseEventArgs args)
        {
            RteObj.PrintAsync();
        }

        /// <summary>
        /// Method invoked after each time the component has been rendered.
        /// </summary>
        /// <param name="firstRender">Set to <c>true</c> if this is the first time <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> has been invoked
        /// on this component instance; otherwise <c>false</c>.</param>
        /// <remarks>The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> and <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)" /> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        /// once.</remarks>
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                timer2 = new System.Timers.Timer(500);
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
                timer2.Elapsed += TimerTick2;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
                timer2.Enabled = true;
            }
        }

        /// <summary>
        /// Timers the tick2.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        protected void TimerTick2(Object source, ElapsedEventArgs e)
        {
            timer2.Enabled = false;
            timer2.Stop();
            readonlyPrint = false;
            this.RteObj.ExecuteCommandAsync(CommandName.InsertHTML, PrintStuff);
            readonlyPrint = true;
            StateHasChanged();
        }

        /// <summary>
        /// Closes the print.
        /// </summary>
        private void ClosePrint()
        {
            ModalInstance.CancelAsync();
        }
    }
}