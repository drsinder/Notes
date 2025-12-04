
using System.Timers;

namespace Notes.Client.Menus
{
    public partial class LoginDisplay
    {
        private System.Timers.Timer timer2;

        /// <summary>
        /// Begins the sign out.
        /// </summary>
        private void BeginSignOut()
        {
            Navigation.NavigateTo("Account/Logout", true);
        }

        /// <summary>
        /// Gotoes the profile.
        /// </summary>
        private void GotoProfile()
        {
            Navigation.NavigateTo("Account/Manage", true);
        }

        /// <summary>
        /// Gotoes the register.
        /// </summary>
        private void GotoRegister()
        {
            Navigation.NavigateTo("Account/Register", true);
        }

        /// <summary>
        /// Gotoes the login.
        /// </summary>
        private void GotoLogin()
        {
            Navigation.NavigateTo("Account/Login", true);
        }

        /// <summary>
        /// Gotoes the home.
        /// </summary>
        private void GotoHome()
        {
            Navigation.NavigateTo("", false);
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            Globals.LoginDisplay = this;
        }

        /// <summary>
        /// Delays the rendering of the component to allow other components to load first.
        /// </summary>
        /// <param name="firstRender"></param>
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                timer2 = new System.Timers.Timer(100);
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
                timer2.Elapsed += TimerTick2;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
                timer2.Enabled = true;

                myState.OnChange += StateHasChanged;
            }
        }

        protected void TimerTick2(object source, ElapsedEventArgs e)
        { 
            Reload();
            timer2.Enabled = false;
            timer2.Stop();
        }

        /// <summary>
        /// Reloads this instance.
        /// </summary>
        public void Reload()
        {
            StateHasChanged();
        }
    }
}