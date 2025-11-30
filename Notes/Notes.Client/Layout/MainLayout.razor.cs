using System.Timers;

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
                timer2 = new System.Timers.Timer(120);
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
                timer2.Elapsed += TimerTick2;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
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