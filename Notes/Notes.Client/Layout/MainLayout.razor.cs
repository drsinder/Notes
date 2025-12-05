using System.Timers;

/// <summary>
/// The Notes.Client.Layout namespace contains layout components for the Notes client application.
/// </summary>
namespace Notes.Client.Layout
{
    public partial class MainLayout
    {
        private System.Timers.Timer? timer2 { get; set; }

        private bool menuVisible { get; set; } = false;

        protected override void OnAfterRender(bool firstRender)
        {
            if (timer2 is null)
            {
                timer2 = new System.Timers.Timer(120); // wait a bit before showing the menu
                timer2.Elapsed += TimerTick2;
                timer2.Enabled = true;

            }
            
        }
        protected void TimerTick2(object source, ElapsedEventArgs e)
        {
            menuVisible = true;
            StateHasChanged();
        }


    }
}