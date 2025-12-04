using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.RichTextEditor;
using System.Text;
using System.Web;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Provides functionality for formatting code snippets and managing code format options within a modal dialog
    /// component.
    /// </summary>
    /// <remarks>Use this class to select code formats, edit code content, and interact with modal dialogs for
    /// code insertion. The available code formats include common programming and markup languages such as C#, C++,
    /// JavaScript, HTML, and others. This class is typically used in Blazor applications to facilitate rich text
    /// editing and code formatting scenarios.</remarks>
    public partial class CodeFormat
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter] BlazoredModalInstance ModalInstance { get; set; }
#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// Gets or sets the stuff.
        /// </summary>
        /// <value>The stuff.</value>
        [Parameter] public string stuff { get; set; }
        /// <summary>
        /// Gets or sets the edit object.
        /// </summary>
        /// <value>The edit object.</value>
        [Parameter] public SfRichTextEditor EditObj { get; set; }

        /// <summary>
        /// Gets or sets the text object.
        /// </summary>
        /// <value>The text object.</value>
        protected SfTextBox TextObj { get; set; }
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        protected string message { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        /// <summary>
        /// Gets or sets a value indicating whether this instance is editing.
        /// </summary>
        /// <value><c>true</c> if this instance is editing; otherwise, <c>false</c>.</value>
        protected bool IsEditing { get; set; } = false;
        /// <summary>
        /// The drop value
        /// </summary>
        public string DropVal;

        /// <summary>
        /// Class CFormat.
        /// </summary>
        public class CFormat
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; set; }
            /// <summary>
            /// Gets or sets the code.
            /// </summary>
            /// <value>The code.</value>
            public string Code { get; set; }
        }

        /// <summary>
        /// The c formats
        /// </summary>
        public List<CFormat> CFormats = new()
        {
            new CFormat() { Name = "None",  Code = "none" },
            new CFormat() { Name = "C#",    Code = "csharp" },
            new CFormat() { Name = "C++",   Code = "cpp" },
            new CFormat() { Name = "C",     Code = "c" },
            new CFormat() { Name = "Razor", Code = "razor" },
            new CFormat() { Name = "Css",   Code = "css" },
            new CFormat() { Name = "Java",  Code = "java" },
            new CFormat() { Name = "JavaScript", Code = "js" },
            new CFormat() { Name = "Json",  Code = "json" },
            new CFormat() { Name = "Html",  Code = "html" },
            new CFormat() { Name = "Perl",  Code = "perl" },
            new CFormat() { Name = "Php",   Code = "php" },
            new CFormat() { Name = "SQL",   Code = "sql" },
            new CFormat() { Name = "PowerShell", Code = "powershell" }
        };

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            message = stuff;
            IsEditing = !string.IsNullOrEmpty(stuff);   // not yet permitted at higher levels
        }

        /// <summary>
        /// Oks this instance.
        /// </summary>
        private async Task Ok()
        {
            string code;

            if (DropVal is not null && !string.IsNullOrEmpty(DropVal))
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                code = CFormats.Find(p => p.Name == DropVal).Code;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            else
                code = "none";

            switch (code)
            {
                case "none":
                    message = HttpUtility.HtmlEncode(TextObj.Value);
                    break;

                default:
                    message = HttpUtility.HtmlEncode(TextObj.Value);
                    message = MakeCode(message, code);
                    break;
            }
            //await EditObj.ExecuteCommandAsync(CommandName.InsertHTML, message);
            await ModalInstance.CloseAsync(ModalResult.Ok(message));
        }

        /// <summary>
        /// Makes the code.
        /// </summary>
        /// <param name="stuff2">The stuff2.</param>
        /// <param name="codeType">Type of the code.</param>
        /// <returns>System.String.</returns>
        /// <font color="red">Badly formed XML comment.</font>
        private static string MakeCode(string stuff2, string codeType)
        {
            StringBuilder sb = new();

            sb.Append("<pre><code class=\"language-");
            sb.Append(codeType);
            sb.Append("\">");
            sb.Append(stuff2);
            sb.Append("</code></pre>");

            return sb.ToString();
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CancelAsync();
        }
    }

}