
// ***********************************************************************
// <copyright file="SCheckBox.razor.cs" company="Notes.Client">
//     Copyright (c) 2026 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.AspNetCore.Components;
using Notes.Protos;
using Notes.Client.Pages;

namespace Notes.Client.Comp
{
    /// <summary>
    /// Represents a checkbox component that tracks its checked state and file association, and interacts with a tracker
    /// and model for sequencer operations.
    /// </summary>
    /// <remarks>SCheckBox is designed for use in UI scenarios where a checkbox needs to be bound to a model
    /// and perform create or delete operations based on its checked state. The component updates its model and
    /// communicates with external services when the checked state changes. It is intended to be used within a Blazor or
    /// similar component framework, where parameters are supplied by the parent and actions are triggered by user
    /// interaction.</remarks>
    public partial class SCheckBox
    {
        /// <summary>
        /// Gets or sets the tracker.
        /// </summary>
        /// <value>The tracker.</value>
        [Parameter]
        public required Tracker Tracker { get; set; }

        /// <summary>
        /// Gets or sets the file identifier.
        /// </summary>
        /// <value>The file identifier.</value>
        [Parameter]
#pragma warning disable IDE1006 // Naming Styles
        public int fileId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        /// <value><c>true</c> if this instance is checked; otherwise, <c>false</c>.</value>
        [Parameter]
        public bool isChecked { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        public SCheckModel Model { get; set; }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            Model = new SCheckModel{IsChecked = isChecked, FileId = fileId};
        }

        /// <summary>
        /// Toggles the checked state and asynchronously creates or deletes the sequencer item based on the new state.
        /// </summary>
        /// <remarks>If the checked state becomes enabled, a new sequencer item is created; otherwise, the
        /// existing item is deleted. After the operation, the tracker is shuffled to reflect the updated
        /// state.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task OnClick()
        {
            isChecked = !isChecked;
            if (isChecked) // create item
            {
                await Client.CreateSequencerAsync(Model, myState.AuthHeader);
            }
            else // delete it
            {
                await Client.DeleteSequencerAsync(Model, myState.AuthHeader);
            }

            await Tracker.Shuffle();
        }
    }
}