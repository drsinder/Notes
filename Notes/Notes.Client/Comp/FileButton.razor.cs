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
    /// Class FileButton.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class FileButton
    {
        /// <summary>
        /// Gets or sets the note file.
        /// </summary>
        /// <value>The note file.</value>
        [Parameter] public GNotefile NoteFile { get; set; }

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
        /// Called when [click].
        /// </summary>
        protected void OnClick()
        {
            Navigation.NavigateTo("noteindex/" + NoteFile.Id);
        }

    }
}
