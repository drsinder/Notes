using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Notes.Protos;
using System.Text;

namespace Notes.Client.Dialogs
{
    /// <summary>
    /// Class ExportUtil1.
    /// Implements the <see cref="ComponentBase" />
    /// Implements the <see cref="System.IAsyncDisposable" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    /// <seealso cref="System.IAsyncDisposable" />
    public partial class ExportUtil1
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter] public BlazoredModalInstance ModalInstance { get; set; }
#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        [Parameter] public ExportViewModel model { get; set; }
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        [Parameter] public string FileName { get; set; }

        /// <summary>
        /// The module
        /// </summary>
        private IJSObjectReference? module;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ExportUtil1"/> is marked.
        /// </summary>
        /// <value><c>true</c> if marked; otherwise, <c>false</c>.</value>
        private bool marked { get; set; }
        /// <summary>
        /// The message
        /// </summary>
        private string message = "Getting ready...";
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// On after render as an asynchronous operation.
        /// </summary>
        /// <param name="firstRender">Set to <c>true</c> if this is the first time <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> has been invoked
        /// on this component instance; otherwise <c>false</c>.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <remarks>The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> and <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)" /> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        /// once.</remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                module = await JS.InvokeAsync<IJSObjectReference>("import",
                    "./scripts.js");
            }
        }

        //public async ValueTask<string?> Prompt(string message) =>
        //    module is not null ?
        //        await module.InvokeAsync<string>("showPrompt", message) : null;

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        async ValueTask IAsyncDisposable.DisposeAsync()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        {
            if (module is not null)
            {
                await module.DisposeAsync();
            }
        }
        /// <summary>
        /// On initialized as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected async override Task OnInitializedAsync()
        {
            if (model.Marks is not null && model.Marks.Count > 0)
                marked = true;
            else
                marked = false;

            if (model.isDirectOutput)
            {
                message = "Exporting " + FileName + " ... ";
            }
            else
            {
                message = "Emailing ... ";
            }

            MemoryStream ms = await DoExportFast();

            if (model.isDirectOutput)
            {
                await SaveAs(FileName, ms.GetBuffer());
            }
            else
            {
                byte[] bytes = ms.GetBuffer();

                System.Text.Encoding enc = System.Text.Encoding.ASCII;
                string email = enc.GetString(bytes);

                GEmail stuff = new()
                {
                    Address = model.Email,
                    Subject = "Notes 2022 - " + model.NoteFile.NoteFileTitle,
                    Body = email
                };

                await Client.SendEmailAuthAsync(stuff, myState.AuthHeader);
            }

            ms.Dispose();

            await ModalInstance.CancelAsync();
        }

        //        /// <summary>
        //        /// Does the export.
        //        /// </summary>
        //        /// <returns>MemoryStream.</returns>
        //        private async Task<MemoryStream> DoExport()
        //        {
        //            bool isHtml = model.isHtml;
        //            bool isCollapsible = model.isCollapsible;

        //            if (model.myMenu is not null)
        //            {
        //                model.myMenu.IsPrinting = true;
        //                model.myMenu.Replot();
        //            }

        //            GNotefile nf = model.NoteFile;
        //            int nfid = nf.Id;

        //            MemoryStream ms = new();
        //            StreamWriter sw = new(ms);
        //            StringBuilder sb = new();

        //            List<GTags> tags = new(); //await Http.GetFromJsonAsync<List<Tags>>("api/Tags/" + nfid);

        //            if (isHtml)
        //            {
        //                // Start the document
        //                sb.AppendLine("<!DOCTYPE html>");
        //                sb.AppendLine("<html>");
        //                sb.AppendLine("<meta charset=\"utf-8\" />");
        //                sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        //                sb.AppendLine("<title>" + nf.NoteFileTitle + "</title>");

        //                sb.AppendLine("<link rel = \"stylesheet\" href = \"https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css\">");
        //                sb.AppendLine("<link href=\"https://www.drsinder.com/NotesStuff/Notes2022/NotesExport.css\" rel=\"stylesheet\" />");
        //                sb.AppendLine("<link href=\"https://www.drsinder.com/NotesStuff/Notes2022/prism.css\" rel=\"stylesheet\" />");


        //                sb.AppendLine("<script src = \"https://ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js\" ></script >");
        //                sb.AppendLine("<script src = \"https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js\" ></script >");
        //                sb.AppendLine("<script src = \"https://www.drsinder.com/NotesStuff/Notes2022/prism.min.js\" ></script >");

        //                sb.AppendLine("</head>");
        //                sb.AppendLine("<body><div>");
        //                await sw.WriteAsync(sb.ToString());

        //                // ready to start  writing content of file
        //                sb = new StringBuilder();
        //            }
        //            if (isHtml)
        //                sb.Append("<h2>");

        //            // File Header
        //            sb.Append("2022 NoteFile " + nf.NoteFileName + " - " + nf.NoteFileTitle);
        //            sb.Append(" - Created " + DateTime.Now.ToUniversalTime().ToLongDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
        //            if (isHtml)
        //            {
        //                sb.Append("</h2>");
        //                //sb.Append("<h4>");
        //                //sb.Append("<a href=\"");
        //                //sb.Append(/*ProdUri + */"/NoteDisplay/Create/" + nf.Id +
        //                //          "\" target=\"_blank\">New Base Note</a>");
        //                //sb.Append("</h4>");
        //            }

        //            await sw.WriteLineAsync(sb.ToString());
        //            await sw.WriteLineAsync();

        //            // get ordered list of basenoteheaders to start process
        //            GNoteHeaderList? bnhl = null;
        //            //string req;
        //            int NoteOrd = 0;
        //            if (model.NoteOrdinal == 0)
        //            {
        //                //req = "" + nfid + "." + model.ArchiveNumber + ".0.0";
        //            }
        //            else
        //            {

        //                //req = "" + nfid + "." + model.ArchiveNumber + "." + model.NoteOrdinal + ".0";
        //                NoteOrd = model.NoteOrdinal;
        //            }
        //            bnhl = await Client.GetExportAsync(new ExportRequest() { FileId = nfid, ArcId = model.ArchiveNumber, NoteOrdinal = NoteOrd, ResponseOrdinal = 0 }, myState.AuthHeader);

        //            // loop over each base note in order
        //            foreach (GNoteHeader bnh in bnhl.List)
        //            {
        //                if (bnh.IsDeleted || bnh.Version > 0)
        //                    continue;

        //                if (marked && !model.Marks.Where(p => p.NoteOrdinal == bnh.NoteOrdinal).Any())
        //                    continue;

        //                // get content for base note
        //                GNoteContent? nc = null;
        //                //req = bnh.Id.ToString();
        //                nc = await Client.GetExport2Async(new NoteId() { Id = bnh.Id }, myState.AuthHeader);
        //                // format it and write it
        //                await WriteNote(sw, bnh, bnh, nc, isHtml, false, tags);

        //                await sw.WriteLineAsync();

        //                // extra stuff for collapsable responses

        //                if (isCollapsible && isHtml && bnh.ResponseCount > 0)
        //                {
        //                    await sw.WriteLineAsync("<div class=\"container\"><div class=\"panel-group\">" +
        //                        "<div class=\"panel panel-default\"><div class=\"panel-heading\"><div class=\"panel-title\"><a data-toggle=\"collapse\" href=\"#collapse" +
        //                        bnh.NoteOrdinal + "\"><h5>Toggle " + bnh.ResponseCount + " Response" + (bnh.ResponseCount > 1 ? "s" : "") + "</h5></a></div></div><div id = \"collapse" + bnh.NoteOrdinal +
        //                        "\" class=\"panel-collapse collapse\"><div class=\"panel-body\">");
        //                }

        //                for (int rnum = 1; rnum <= bnh.ResponseCount; rnum++)
        //                {
        //#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        //                    GNoteHeader nh = null;
        //                    GNoteContent ncr = null;
        //#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        //                    //req = "" + nfid + "." + model.ArchiveNumber + "." + bnh.NoteOrdinal + "." + rnum;
        //                    GNoteHeaderList zz1 = await Client.GetExportAsync(new ExportRequest()
        //                    { FileId = nfid, ArcId = model.ArchiveNumber, NoteOrdinal = bnh.NoteOrdinal, ResponseOrdinal = rnum }, myState.AuthHeader);


        //                    nh = zz1.List[0];

        //                    if (!nh.IsDeleted && nh.Version == 0)
        //                    {

        //                        //req = nh.Id.ToString();

        //                        ncr = await Client.GetExport2Async(new NoteId() { Id = nh.Id }, myState.AuthHeader);

        //                        await WriteNote(sw, bnh, nh, ncr, isHtml, true, tags);
        //                    }
        //                }

        //                // extra stuff to terminate collapsable responses
        //                if (isCollapsible && isHtml && bnh.ResponseCount > 0)
        //                {
        //                    await sw.WriteLineAsync("</div></div></div></div></div> ");
        //                }

        //                if (model.myMenu is not null)
        //                    model.myMenu.myGauge.SetPointerValue(0, 0, bnh.NoteOrdinal);
        //            }

        //            if (isHtml)  // end the html
        //            {
        //                //sb.AppendLine("<script src = \"https://www.drsinder.com/Notes2021/js/prism.min.js\" ></script >");

        //                await sw.WriteLineAsync("</div></body></html>");
        //            }

        //            // make sure all output is written to stream and rewind it
        //            await sw.FlushAsync();
        //            ms.Seek(0, SeekOrigin.Begin);
        //            // send stream to caller
        //            if (model.myMenu is not null)
        //            {
        //                model.myMenu.IsPrinting = false;
        //                model.myMenu.Replot();
        //            }

        //            return ms;
        //        }

        /// <summary>
        /// Does the export.
        /// </summary>
        /// <returns>MemoryStream.</returns>
        private async Task<MemoryStream> DoExportFast()
        {
            bool isHtml = model.isHtml;
            bool isCollapsible = model.isCollapsible;

            if (model.myMenu is not null)
            {
                model.myMenu.IsPrinting = true;
                model.myMenu.Replot();
            }

            GNotefile nf = model.NoteFile;
            int nfid = nf.Id;

            MemoryStream ms = new();
            StreamWriter sw = new(ms);
            StringBuilder sb = new();

            List<GTags> tags = new(); //await Http.GetFromJsonAsync<List<Tags>>("api/Tags/" + nfid);

            if (isHtml)
            {
                // Start the document
                sb.AppendLine("<!DOCTYPE html>");
                sb.AppendLine("<html>");
                sb.AppendLine("<meta charset=\"utf-8\" />");
                sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
                sb.AppendLine("<title>" + nf.NoteFileTitle + "</title>");

                sb.AppendLine("<link rel = \"stylesheet\" href = \"https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css\">");
                sb.AppendLine("<link href=\"https://www.drsinder.com/NotesStuff/Notes2022/NotesExport.css\" rel=\"stylesheet\" />");
                sb.AppendLine("<link href=\"https://www.drsinder.com/NotesStuff/Notes2022/prism.css\" rel=\"stylesheet\" />");


                sb.AppendLine("<script src = \"https://ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js\" ></script >");
                sb.AppendLine("<script src = \"https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js\" ></script >");
                sb.AppendLine("<script src = \"https://www.drsinder.com/NotesStuff/Notes2022/prism.min.js\" ></script >");

                sb.AppendLine("</head>");
                sb.AppendLine("<body><div>");
                await sw.WriteAsync(sb.ToString());

                // ready to start  writing content of file
                sb = new StringBuilder();
            }
            if (isHtml)
                sb.Append("<h2>");

            // File Header
            sb.Append("2022 NoteFile " + nf.NoteFileName + " - " + nf.NoteFileTitle);
            sb.Append(" - Created " + DateTime.Now.ToUniversalTime().ToLongDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
            if (isHtml)
            {
                sb.Append("</h2>");
                //sb.Append("<h4>");
                //sb.Append("<a href=\"");
                //sb.Append(/*ProdUri + */"/NoteDisplay/Create/" + nf.Id +
                //          "\" target=\"_blank\">New Base Note</a>");
                //sb.Append("</h4>");
            }

            await sw.WriteLineAsync(sb.ToString());
            await sw.WriteLineAsync();

            // get ordered list of basenoteheaders to start process
            //GNoteHeaderList? bnhl = null;
            //string req;
            int NoteOrd = 0;
            if (model.NoteOrdinal == 0)
            {
                //req = "" + nfid + "." + model.ArchiveNumber + ".0.0";
            }
            else
            {

                //req = "" + nfid + "." + model.ArchiveNumber + "." + model.NoteOrdinal + ".0";
                NoteOrd = model.NoteOrdinal;
            }

            JsonExport stuff;
            stuff = await Client.GetExportJsonAsync(new ExportRequest() { FileId = nfid, ArcId = model.ArchiveNumber }, myState.AuthHeader);

            List<GNoteHeader> allHeaders = stuff.NoteHeaders.List.ToList();


            List<GNoteHeader> baseNotes;
            if (NoteOrd == 0)
                baseNotes = allHeaders.Where(p => p.ResponseOrdinal == 0).ToList();
            else
                baseNotes = allHeaders.Where(p => p.NoteOrdinal == NoteOrd && p.ResponseOrdinal == 0).ToList();

            //bnhl = await Client.GetExportAsync(new ExportRequest() { FileId = nfid, ArcId = model.ArchiveNumber, NoteOrdinal = NoteOrd, ResponseOrdinal = 0 }, myState.AuthHeader);

            // loop over each base note in order
            foreach (GNoteHeader bnh in baseNotes)
            {
                if (bnh.IsDeleted || bnh.Version > 0)
                    continue;

                if (marked && !model.Marks.Where(p => p.NoteOrdinal == bnh.NoteOrdinal).Any())
                    continue;

                // get content for base note
                GNoteContent? nc = null;
                //req = bnh.Id.ToString();
                nc = bnh.Content;
                // format it and write it
                await WriteNote(sw, bnh, bnh, nc, isHtml, false, tags);

                await sw.WriteLineAsync();

                // extra stuff for collapsable responses

                if (isCollapsible && isHtml && bnh.ResponseCount > 0)
                {
                    await sw.WriteLineAsync("<div class=\"container\"><div class=\"panel-group\">" +
                        "<div class=\"panel panel-default\"><div class=\"panel-heading\"><div class=\"panel-title\"><a data-toggle=\"collapse\" href=\"#collapse" +
                        bnh.NoteOrdinal + "\"><h5>Toggle " + bnh.ResponseCount + " Response" + (bnh.ResponseCount > 1 ? "s" : "") + "</h5></a></div></div><div id = \"collapse" + bnh.NoteOrdinal +
                        "\" class=\"panel-collapse collapse\"><div class=\"panel-body\">");
                }

                for (int rnum = 1; rnum <= bnh.ResponseCount; rnum++)
                {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    GNoteHeader nh = null;
                    GNoteContent ncr = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                    //req = "" + nfid + "." + model.ArchiveNumber + "." + bnh.NoteOrdinal + "." + rnum;
                    //GNoteHeaderList zz1 = await Client.GetExportAsync(new ExportRequest()
                    //{ FileId = nfid, ArcId = model.ArchiveNumber, NoteOrdinal = bnh.NoteOrdinal, ResponseOrdinal = rnum }, myState.AuthHeader);

                    List<GNoteHeader> zz1 = allHeaders.Where(p => p.NoteOrdinal == bnh.NoteOrdinal && p.ResponseOrdinal != 0).ToList();


                    nh = zz1[rnum - 1];

                    if (!nh.IsDeleted && nh.Version == 0)
                    {

                        //req = nh.Id.ToString();

                        ncr = nh.Content;

                        await WriteNote(sw, bnh, nh, ncr, isHtml, true, tags);
                    }
                }

                // extra stuff to terminate collapsable responses
                if (isCollapsible && isHtml && bnh.ResponseCount > 0)
                {
                    await sw.WriteLineAsync("</div></div></div></div></div> ");
                }

                if (model.myMenu is not null)
                {
                    model.myMenu.myGauge.SetPointerValue(0, 0, bnh.NoteOrdinal);
                    if (bnh.NoteOrdinal % 10 == 0)
                    {
                        await Client.NoOpAsync(new NoRequest()); // needed to let Menu progress update!
                    }
                }
            }

            if (isHtml)  // end the html
            {
                //sb.AppendLine("<script src = \"https://www.drsinder.com/Notes2021/js/prism.min.js\" ></script >");

                await sw.WriteLineAsync("</div></body></html>");
            }

            // make sure all output is written to stream and rewind it
            await sw.FlushAsync();
            ms.Seek(0, SeekOrigin.Begin);
            // send stream to caller
            if (model.myMenu is not null)
            {
                model.myMenu.IsPrinting = false;
                model.myMenu.Replot();
            }

            return ms;
        }



        /// <summary>
        /// Writes the note.
        /// </summary>
        /// <param name="sw">The sw.</param>
        /// <param name="bnh">The BNH.</param>
        /// <param name="nh">The nh.</param>
        /// <param name="nc">The nc.</param>
        /// <param name="isHtml">if set to <c>true</c> [is HTML].</param>
        /// <param name="isResponse">if set to <c>true</c> [is response].</param>
        /// <param name="tags">The tags.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static async Task<bool> WriteNote(StreamWriter sw, GNoteHeader bnh, GNoteHeader nh, GNoteContent nc, bool isHtml, bool isResponse, List<GTags> tags)
        {
            //StringBuilder sb;
            if (isHtml)
            {
                string extra = "";
                if (nc.NoteBody.StartsWith("<DIV STYLE="))
                    extra = "-client";

                if (!isResponse)
                    await sw.WriteLineAsync("<div class=\"base-note" + extra + "\">");
                else
                    await sw.WriteLineAsync("<div class=\"response-note" + extra + "\">");

                await sw.WriteLineAsync("<h3>");
            }

            if (!isResponse)
            {
                // write base note header
                await sw.WriteLineAsync("Note: " + nh.NoteOrdinal + " - Subject: "
                + nh.NoteSubject + (isHtml ? "<br />" : " - ") + "Author: " + nh.AuthorName + " - "
                + nh.LastEdited.ToDateTime().ToShortDateString() + " " + nh.LastEdited.ToDateTime().ToShortTimeString() + " - "
                + bnh.ResponseCount + " Response" + (bnh.ResponseCount > 1 || bnh.ResponseCount == 0 ? "s" : ""));
            }
            else
            {
                // write response note header
                await sw.WriteLineAsync("Note: " + nh.NoteOrdinal + " - Subject: "
                + nh.NoteSubject + (isHtml ? "<br />" : " - ") + "Author: " + nh.AuthorName + " - "
                + nh.LastEdited.ToDateTime().ToShortDateString() + " " + nh.LastEdited.ToDateTime().ToShortTimeString() + " - "
                + "Response " + nh.ResponseOrdinal + " of " + bnh.ResponseCount);
                await sw.WriteLineAsync((isHtml ? "<br />" : string.Empty) + "Base Note Subject: " + bnh.NoteSubject);
            }

            if (isHtml)
            {
                await sw.WriteLineAsync("</h3>");
            }
            // Do tags

            List<GTags> tl = tags.Where(p => p.NoteHeaderId == nc.NoteHeaderId).ToList();

            if (!isHtml || (tl is not null && tl.Count > 0))
            {
                await sw.WriteAsync((isHtml ? "<h5>" : "") + "Tags - ");
                foreach (GTags item in tl)
                {
                    await sw.WriteAsync(item.Tag + " ");
                }
                await sw.WriteLineAsync((isHtml ? "</h5>" : ""));
            }

            if (!isHtml || !string.IsNullOrEmpty(nh.DirectorMessage))
            {
                await sw.WriteLineAsync((isHtml ? "<h5>" : "") + "Director Message - " + nh.DirectorMessage + (isHtml ? "</h5>" : ""));
            }
            await sw.WriteLineAsync();
            //if (isHtml)
            //{
            //    sb = new StringBuilder();
            //    sb.Append("<h4>");
            //    sb.Append("<a href=\"");
            //    sb.Append(ProdUri + "/NoteDisplay/CreateResponse/" + nc.BaseNoteId +
            //              "\" target=\"_blank\">New Response</a>");
            //    sb.Append("</h4>");
            //    await sw.WriteLineAsync(sb.ToString());
            //}

            if (isHtml)
                await sw.WriteLineAsync(nc.NoteBody.Replace("<br />", "<br />\r\n") + "</div>");
            else
                await sw.WriteLineAsync(nc.NoteBody.Replace("<br />", "\r\n"));
            await sw.WriteLineAsync();

            return true;
        }


        /// <summary>
        /// Saves as.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="data">The data.</param>
        public async Task SaveAs(string filename, byte[] data)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await module.InvokeVoidAsync("saveAsFile", filename, Convert.ToBase64String(data));
#pragma warning restore CS8604 // Possible null reference argument.
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