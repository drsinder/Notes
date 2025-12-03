// ***********************************************************************
// Assembly         : Notes2022.Client
// Author           : Dale Sinder
// Created          : 05-08-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-09-2022
// ***********************************************************************
// <copyright file="SCheckBox.razor.cs" company="Notes2022.Client">
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
using Notes.Client;
using Notes.Client.Shared;
using Notes.Protos;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.LinearGauge;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.SplitButtons;
using Syncfusion.Blazor.Calendars;
using System.Text;
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