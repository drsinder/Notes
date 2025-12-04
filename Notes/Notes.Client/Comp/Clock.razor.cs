// ***********************************************************************
// <copyright file="Clock.razor.cs" company="Notes.Client">
//     Copyright (c) 2026 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.AspNetCore.Components;
using System.Timers;

namespace Notes.Client.Comp
{
    /// <summary>
    /// Represents a component that displays and updates the current time at a specified interval.
    /// </summary>
    /// <remarks>Use the <see cref="Interval"/> property to configure how frequently the time is updated. The
    /// component initializes its timer and starts updating the displayed time after the first render. This class is
    /// intended for use in Blazor applications where periodic UI updates are required.</remarks>
    public partial class Clock
    {
        /// <summary>
        /// Gets or sets the interval, in milliseconds, between consecutive operations.
        /// </summary>
        /// <remarks>The interval determines how frequently the associated action is performed. Setting a
        /// lower value increases the operation rate, which may impact performance depending on the workload.</remarks>
        [Parameter]
        public int Interval { get; set; } = 1000;
#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// Gets or sets the timer used for scheduled operations.
        /// </summary>
        private System.Timers.Timer timer2 { get; set; }

        /// <summary>
        /// Gets or sets the date and time value associated with this instance.
        /// </summary>
        private DateTime mytime { get; set; } = DateTime.Now;
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Handles post-render logic for the component, initializing resources when rendering for the first time.
        /// </summary>
        /// <remarks>Override this method to perform actions that should occur after the component has
        /// rendered. Initialization code should be placed within the <paramref name="firstRender"/> check to ensure it
        /// runs only once.</remarks>
        /// <param name="firstRender">Indicates whether this is the first time the component has been rendered. If <see langword="true"/>,
        /// initialization logic is performed.</param>
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
        /// Handles the timer's elapsed event by updating the current time and triggering a UI refresh.
        /// </summary>
        /// <remarks>This method is intended to be used as an event handler for timer events. It updates
        /// the time and refreshes the component's state to reflect the change.</remarks>
        /// <param name="source">The source of the timer event, typically the timer object that raised the event.</param>
        /// <param name="e">An <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data for the elapsed timer
        /// event.</param>
        protected void TimerTick2(Object source, ElapsedEventArgs e)
        {
            mytime = DateTime.Now;
            StateHasChanged();
        }
    }
}