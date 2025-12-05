// ***********************************************************************
// <copyright file="TrackerMover.razor.cs" company="Notes.Client">
//     Copyright (c) 2026 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.AspNetCore.Components;
using Notes.Client.Pages;
using Notes.Client.Comp;
using Notes.Protos;
using Syncfusion.Blazor.SplitButtons;

namespace Notes.Client.Comp
{
    /// <summary>
    /// Represents a component that manages the movement and ordering of tracker items within a collection.
    /// </summary>
    /// <remarks>TrackerMover provides functionality to reorder tracker items, such as moving them up, down,
    /// to the top, or to the bottom of the list. It maintains references to the current tracker, its container, and
    /// neighboring items to facilitate these operations. This class is typically used in UI scenarios where users can
    /// interactively change the order of items. The component relies on its parameters being set by the parent and
    /// updates the underlying data source when items are moved.</remarks>
    public partial class TrackerMover
    {
        /// <summary>
        /// Who are we
        /// </summary>
        /// <value>The current tracker.</value>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Parameter] public GSequencer CurrentTracker { get; set; }

        [Parameter] public CookieStateAgent myState { get; set; }
        /// <summary>
        /// List of trackers
        /// </summary>
        /// <value>The trackers.</value>
        [Parameter] public List<GSequencer> Trackers { get; set; }

        /// <summary>
        /// Our container/caller
        /// </summary>
        /// <value>The tracker.</value>
        [Parameter] public Tracker Tracker { get; set; }

        /// <summary>
        /// List of items before me
        /// </summary>
        /// <value>The befores.</value>
        List<GSequencer> befores { get; set; }

        /// <summary>
        /// List of items after me
        /// </summary>
        /// <value>The afters.</value>
        List<GSequencer> afters { get; set; }

        /// <summary>
        /// Item just before me
        /// </summary>
        /// <value>The before.</value>
        GSequencer before { get; set; }

        /// <summary>
        /// Item just after me
        /// </summary>
        /// <value>The after.</value>
        GSequencer after { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            // find before and after items
            if (CurrentTracker is not null)
            {
                befores = Trackers.Where(p => p.Ordinal < CurrentTracker.Ordinal).OrderByDescending(p => p.Ordinal).ToList();
                if (befores is not null && befores.Count > 0)
                    before = befores.First();

                afters = Trackers.Where(p => p.Ordinal > CurrentTracker.Ordinal).OrderBy(p => p.Ordinal).ToList();
                if (afters is not null && afters.Count > 0)
                    after = afters.First();
            }
        }

        /// <summary>
        /// Move an item as wished
        /// </summary>
        /// <param name="args">The <see cref="MenuEventArgs"/> instance containing the event data.</param>
        private async Task ItemSelected(MenuEventArgs args)
        {

            switch (args.Item.Text)
            {
                case "Up":
                    if (before is null)
                        return;

                    await Swap(before, CurrentTracker);

                    break;

                case "Down":
                    if (after is null)
                        return;
                    await Swap(after, CurrentTracker);

                    break;

                case "Top":
                    if (before is null)
                        return;
                    await Swap(befores[befores.Count - 1], CurrentTracker);
                    break;

                case "Bottom":
                    if (after is null)
                        return;
                    await Swap(afters[afters.Count - 1], CurrentTracker);

                    break;

                default:
                    break;
            }

            await Tracker.Shuffle();

        }

        /// <summary>
        /// Swap the postion of two trackers
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        private async Task Swap(GSequencer a, GSequencer b)
        {
            int aord = a.Ordinal;
            int bord = b.Ordinal;

            a.Ordinal = bord;
            b.Ordinal = aord;

            await Client.UpdateSequencerOrdinalAsync(a, myState.AuthHeader);
            await Client.UpdateSequencerOrdinalAsync(b, myState.AuthHeader);
        }
    }
}
