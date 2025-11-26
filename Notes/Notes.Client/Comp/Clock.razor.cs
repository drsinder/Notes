// ***********************************************************************
// Assembly         : Notes2022.Client
// Author           : Dale Sinder
// Created          : 05-08-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-08-2022
// ***********************************************************************
// <copyright file="Clock.razor.cs" company="Notes2022.Client">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.LinearGauge;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.SplitButtons;
using Syncfusion.Blazor.Calendars;
using System.Timers;

namespace Notes.Client.Comp
{
    /// <summary>
    /// Class Clock.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class Clock
    {
        /// <summary>
        /// Gets or sets the interval.
        /// </summary>
        /// <value>The interval.</value>
        [Parameter]
        public int Interval { get; set; } = 1000;
#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// Gets or sets the timer2.
        /// </summary>
        /// <value>The timer2.</value>
        private System.Timers.Timer timer2 { get; set; }

        /// <summary>
        /// Gets or sets the mytime.
        /// </summary>
        /// <value>The mytime.</value>
        private DateTime mytime { get; set; } = DateTime.Now;
#pragma warning restore IDE1006 // Naming Styles

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
                mytime = DateTime.Now;
                timer2 = new System.Timers.Timer(Interval);
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
            mytime = DateTime.Now;
            StateHasChanged();
        }
    }
}