/*--------------------------------------------------------------------------
    **
    ** Copyright © 2026, Dale Sinder
    **
    ** Name: Tracker.razor.cs
    **
    ** Description:
    **      Sequencer / Tracker editor
    **
    ** This program is free software: you can redistribute it and/or modify
    ** it under the terms of the GNU General Public License version 3 as
    ** published by the Free Software Foundation.   
    **
    ** This program is distributed in the hope that it will be useful,
    ** but WITHOUT ANY WARRANTY; without even the implied warranty of
    ** MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
    ** GNU General Public License version 3 for more details.
    **
    **  You should have received a copy of the GNU General Public License
    **  version 3 along with this program in file "license-gpl-3.0.txt".
    **  If not, see <http://www.gnu.org/licenses/gpl-3.0.txt>.
    **
    **--------------------------------------------------------------------------*/



using Notes.Protos;

namespace Notes.Client.Pages
{
    /// <summary>
    /// Represents a component that manages and organizes collections of note files and sequencers, providing
    /// functionality to retrieve, order, and shuffle these items asynchronously.
    /// </summary>
    /// <remarks>The Tracker class is designed to interact with external services to fetch and update lists of
    /// note files and sequencers. It provides asynchronous operations to initialize and reorder its internal
    /// collections based on external data. This class is intended to be used as part of a larger application where
    /// dynamic organization and display of note files and sequencers are required.</remarks>
    public partial class Tracker
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>
        /// Gets or sets the stuff.
        /// </summary>
        /// <value>The stuff.</value>
        private List<GNotefile> stuff { get; set; }

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>The files.</value>
        private List<GNotefile> files { get; set; }

        /// <summary>
        /// Gets or sets the trackers.
        /// </summary>
        /// <value>The trackers.</value>
        private List<GSequencer> trackers { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnAfterRenderAsync(bool first)
        {
            if (!first)
                return;
            trackers = (await Client.GetSequencerAsync(new NoRequest(), myState.AuthHeader)).List.ToList();
            HomePageModel model = await Client.GetHomePageModelAsync(new NoRequest(), myState.AuthHeader);

            stuff = model.NoteFiles.List.OrderBy(p => p.NoteFileName).ToList();
            await Shuffle();
            //StateHasChanged();
        }

        /// <summary>
        /// Shuffles this instance.
        /// </summary>
        public async Task Shuffle()
        {
            files = new List<GNotefile>();

            trackers = (await Client.GetSequencerAsync(new NoRequest(), myState.AuthHeader)).List.ToList();

            if (trackers is not null)
            {
                trackers = trackers.OrderBy(p => p.Ordinal).ToList();
                foreach (var tracker in trackers)
                {
                    files.Add(stuff.Find(p => p.Id == tracker.NoteFileId));
                }
            }
            foreach (var s in stuff)
            {
                if (files.Find(p => p.Id == s.Id) is null)
                    files.Add(s);
            }
            StateHasChanged();
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            NavMan.NavigateTo("");
        }
    }
}