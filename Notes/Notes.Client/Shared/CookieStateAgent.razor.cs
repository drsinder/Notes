using Grpc.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Notes.Protos;
using System.Text.Json;

namespace Notes.Client.Shared
{
    /// <summary>
    /// Provides state management and authentication logic for user login using cookies in a Blazor component. Enables
    /// reading, writing, and tracking login information, authentication status, and user roles.
    /// </summary>
    /// <remarks>CookieStateAgent interacts with browser cookies via JavaScript interop to persist and
    /// retrieve login information. It exposes properties to determine authentication and user roles, and provides an
    /// event to notify subscribers when the login state changes. This class is intended for use within Blazor
    /// applications that require client-side authentication state tracking.</remarks>
    public partial class CookieStateAgent : ComponentBase
    {
        /// <summary>
        /// Dealing with login related info
        /// </summary>
        private LoginReply? savedLogin;
        /// <summary>
        /// The module for calling javascript
        /// </summary>
        private IJSObjectReference? module; // for calling javascript

        [CascadingParameter]
        private Task<AuthenticationState>? authenticationStateTask { get; set; }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        async ValueTask IAsyncDisposable.DisposeAsync()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        {
            if (module is not null)
            {
                await module.DisposeAsync();
                module = null;
            }
        }

        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>


        protected override async Task OnAfterRenderAsync(bool firstRender) 
        {
            if (firstRender)
            {

                if (module is null)
                    module = await JS.InvokeAsync<IJSObjectReference>("import", "./cookies.js");

                if (!IsAuthenticated)
                {
                    try
                    {
                        await GetLoginReplyAsync();
                    }
                    catch (Exception)
                    {
                    }
                }
            }

        }

        /// <summary>
        /// Try to get login cookie
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task GetLoginReplyAsync()
        {
            try
            {
                string? cookie;

                {
                    if (module is null)
                        module = await JS.InvokeAsync<IJSObjectReference>("import", "./cookies.js");

                    cookie = await ReadCookie(Globals.CookieName);
                }

                if (!string.IsNullOrEmpty(cookie))
                {
                    // found a cookie!
                    savedLogin = JsonSerializer.Deserialize<LoginReply>(cookie);

                    // Login at server
                    // await Notes2006Client.CreateClient().ReLoginAsync(new NoRequest(), AuthHeader);
                    /*
                    if (Globals.NavMenu != null)
                    {
                        await Globals.NavMenu.Reload();
                    }

                    if (Globals.LoginDisplay != null)
                    {
                        Globals.LoginDisplay.Reload();
                    }
                    */

                    //                    NotifyStateChanged(); // notify subscribers

                    //                    Pinger.Interval = 500;
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Read a cookie
        /// </summary>
        /// <param name="cookieName">cookie name</param>
        /// <returns>needs to be deserialized)</returns>
        public async Task<string?> ReadCookie(string cookieName)
        {
            if (module is not null)
            {
                try
                {
                    return Globals.Base64Decode(await module.InvokeAsync<string>("ReadCookie", cookieName));
                }
                catch (Exception)
                {
                }
            }

            return null;
        }

        /// <summary>
        /// Write a Cookie
        /// </summary>
        /// <param name="cookieName">Name of the cookie</param>
        /// <param name="newCookie">Serialized cookie</param>
        /// <param name="hours">expiry</param>
        public async Task WriteCookie(string cookieName, string newCookie, int hours)
        {
            if (module is not null)
            {
                try
                {
                    string stuff = Globals.Base64Encode(newCookie);
                    _ = await module.InvokeAsync<string>("CreateCookie", cookieName, Globals.Base64Encode(newCookie), hours);
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Gets or sets the login reply.  Setting also notifies subsrcibers
        /// </summary>
        /// <value>The LoginReply - the current state of login.</value>
        public LoginReply? LoginReply
        {
            get
            {
                return savedLogin;
            }

            set
            {
                savedLogin = value;

                NotifyStateChanged(); // notify subscribers
            }
        }

        /// <summary>
        /// Occurs when Login state changes.
        /// </summary>
        public event System.Action? OnChange;
        /// <summary>
        /// Notifies subscribers of login state change.
        /// </summary>
        private void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }

        /// <summary>
        /// Check if user is authenticated - Login reply is not null and status == 200
        /// </summary>
        /// <value><c>true</c> if this instance is authenticated; otherwise, <c>false</c>.</value>
        public bool IsAuthenticated
        {
            get
            {
                return authenticationStateTask?.GetAwaiter().GetResult().User.Identity?.IsAuthenticated == true &&
                    (LoginReply is not null) && LoginReply.Status == 200;
            }
        }

        /// <summary>
        /// Is user in Admin role
        /// </summary>
        /// <value><c>true</c> if this instance is admin; otherwise, <c>false</c>.</value>
        public bool IsAdmin
        {
            get
            {
                if (LoginReply is null || LoginReply.Status != 200)
                    return false;
                return UserInfo is not null && UserInfo.IsAdmin;
            }
        }

        /// <summary>
        /// Is user in User role
        /// </summary>
        /// <value><c>true</c> if this instance is user; otherwise, <c>false</c>.</value>
        public bool IsUser
        {
            get
            {
                if (LoginReply is null || LoginReply.Status != 200)
                    return false;
                return UserInfo is not null && UserInfo.IsUser;
            }
        }

        /// <summary>
        /// Get a Metadata/header for authetication to server in gRPC calls
        /// </summary>
        /// <value>The authentication header.</value>
        public Metadata AuthHeader
        {
            get
            {
                Metadata? headers = new();
                if (LoginReply is not null && LoginReply.Status == 200)
                    headers.Add("Authorization", $"Bearer {LoginReply.Jwt}");
                return headers;
            }
        }

        /// <summary>
        /// Get the decoded user info
        /// </summary>
        /// <value>The user information.</value>
        public UserInfo? UserInfo
        {
            get
            {
                if (LoginReply is not null && LoginReply.Status == 200)
                {
                    return LoginReply.Info;
                }

                return null;
            }
        }


    }
}
