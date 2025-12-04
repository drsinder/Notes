
// ***********************************************************************
// <copyright file="FileButton.razor.cs" company="Notes.Client">
//     Copyright (c) 2026 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.AspNetCore.Components;
using Notes.Protos;

namespace Notes.Client.Comp
{
    /// <summary>
    /// Represents a button component that navigates to a note index page based on the specified note file and ordinal
    /// value.
    /// </summary>
    /// <remarks>Use this component to provide navigation functionality to a note index page within a Blazor
    /// application. The navigation target is determined by the associated note file and, optionally, an ordinal value.
    /// The component can be triggered programmatically by setting the Triggered property to <see
    /// langword="true"/>.</remarks>
    public partial class FileButton
    {
        /// <summary>
        /// Gets or sets the note file.
        /// </summary>
        /// <value>The note file.</value>
        [Parameter] public GNotefile NoteFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the component has been triggered.
        /// </summary>
        [Parameter] public bool Triggered { get; set; } = false;
        
        [Parameter] public long Ordinal { get; set; } = 0;

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
            if (Ordinal == 0)
            {
                Navigation.NavigateTo("noteindex/" + NoteFile.Id);
            }
            else
            {
                Navigation.NavigateTo("noteindex/" + NoteFile.Id + "/" + Ordinal);
            }
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
