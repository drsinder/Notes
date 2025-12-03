// ***********************************************************************
// Assembly         : Notes2022.Client
// Author           : Dale Sinder
// Created          : 05-06-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-06-2022
// ***********************************************************************
// <copyright file="FileButton.razor.cs" company="Notes2022.Client">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.AspNetCore.Components;
using Notes.Protos;


namespace Notes.Client.Comp
{
    /// <summary>
    /// Represents a UI component that displays a button for navigating to a specific note file.
    /// </summary>
    /// <remarks>Use this component to provide users with a button that, when clicked, navigates to the
    /// index page of the associated note file. The navigation behavior depends on the configured <see
    /// cref="NavigationManager"/> and the <see cref="GNotefile"/> instance provided.</remarks>
    public partial class FileButton
    {
        /// <summary>
        /// Gets or sets the note file.
        /// </summary>
        /// <value>The note file.</value>
        [Parameter] public GNotefile NoteFile { get; set; }

        [Parameter] public bool Triggered { get; set; } = false;

        /// <summary>
        /// Gets or sets the navigation.
        /// </summary>
        /// <value>The navigation.</value>
        [Inject] NavigationManager Navigation { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="FileButton"/> class.
        /// </summary>
        public FileButton()
        {
        }

        /// <summary>
        /// Navigates to the note index page for the current note file.
        /// </summary>
        /// <remarks>This method constructs a navigation URL using the identifier of the current note file
        /// and initiates navigation to that page. The navigation occurs immediately when the method is
        /// called.</remarks>
        protected void OnClick()
        {
            Navigation.NavigateTo("noteindex/" + NoteFile.Id);
        }

        protected override void OnParametersSet()
        {
            if (Triggered)
            {
                OnClick();
            }
        }

    }
}
