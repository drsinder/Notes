/*--------------------------------------------------------------------------
    **
    **  Copyright © 2026, Dale Sinder
    **
    **  Name: LocalService.cs
    **
    **  This program is free software: you can redistribute it and/or modify
    **  it under the terms of the GNU General Public License version 3 as
    **  published by the Free Software Foundation.
    **
    **  This program is distributed in the hope that it will be useful,
    **  but WITHOUT ANY WARRANTY; without even the implied warranty of
    **  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    **  GNU General Public License version 3 for more details.
    **
    **  You should have received a copy of the GNU General Public License
    **  version 3 along with this program in file "license-gpl-3.0.txt".
    **  If not, see <http://www.gnu.org/licenses/gpl-3.0.txt>.
    **
    **--------------------------------------------------------------------------*/

using Microsoft.EntityFrameworkCore;
using Notes.Protos;
using Notes.Data;
using Notes.Entities;
using Notes.Manager;
using System.Text;

namespace Notes.Services
{
    /// <summary>
    /// Provides functionality to generate formatted note content for email forwarding within the Notes 2026 system.
    /// </summary>
    /// <remarks>This static class contains methods for creating email-ready representations of notes,
    /// including metadata such as author, subject, and file information. The generated content is intended for use in
    /// email bodies and includes relevant details to facilitate sharing and review. All members are thread-safe due to
    /// the static nature of the class.</remarks>
    public static class LocalService
    {
        /// <summary>
        /// Makes the note for email.
        /// </summary>
        /// <param name="fv">The fv.</param>
        /// <param name="NoteFile">The note file.</param>
        /// <param name="db">The database.</param>
        /// <param name="email">The email.</param>
        /// <param name="name">The name.</param>
        /// <returns>System.String.</returns>
        public static async Task<string> MakeNoteForEmail(ForwardViewModel fv, GNotefile NoteFile, NotesDbContext db, string email, string name)
        {
            NoteHeader nc = await NoteDataManager.GetNoteByIdWithFile(db, fv.NoteID);

            if (!fv.Hasstring || !fv.Wholestring)
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                return "Forwarded by Notes 2026 - User: " + email + " / " + name
                    + "<p>File: " + NoteFile.NoteFileName + " - File Title: " + NoteFile.NoteFileTitle + "</p><hr/>"
                    + "<p>Author: " + nc.AuthorName + "  - Director Message: " + nc.DirectorMessage + "</p><p>"
                    + "<p>Subject: " + nc.NoteSubject + "</p>"
                    + nc.LastEdited.ToShortDateString() + " " + nc.LastEdited.ToShortTimeString() + " UTC" + "</p>"
                    + nc.NoteContent.NoteBody;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                //+ "<hr/>" + "<a href=\"" + Globals.ProductionUrl + "/notedisplay/" + fv.NoteID + "\" >Link to note</a>";   // TODO
            }
            else
            {
                List<NoteHeader> bnhl = await db.NoteHeader
                    .Where(p => p.NoteFileId == nc.NoteFileId && p.NoteOrdinal == nc.NoteOrdinal && p.ResponseOrdinal == 0)
                    .ToListAsync();
                NoteHeader bnh = bnhl[0];
                fv.NoteSubject = bnh.NoteSubject;
                List<NoteHeader> notes = await db.NoteHeader.Include("NoteContent")
                    .Where(p => p.NoteFileId == nc.NoteFileId && p.NoteOrdinal == nc.NoteOrdinal)
                    .ToListAsync();

                StringBuilder sb = new();
                sb.Append("Forwarded by Notes 2026 - User: " + email + " / " + name
                    + "<p>\nFile: " + NoteFile.NoteFileName + " - File Title: " + NoteFile.NoteFileTitle + "</p>"
                    + "<hr/>");

                for (int i = 0; i < notes.Count; i++)
                {
                    if (i == 0)
                    {
                        sb.Append("<p>Base Note - " + (notes.Count - 1) + " Response(s)</p>");
                    }
                    else
                    {
                        sb.Append("<hr/><p>Response - " + notes[i].ResponseOrdinal + " of " + (notes.Count - 1) + "</p>");
                    }
                    sb.Append("<p>Author: " + notes[i].AuthorName + "  - Director Message: " + notes[i].DirectorMessage + "</p>");
                    sb.Append("<p>Subject: " + notes[i].NoteSubject + "</p>");
                    sb.Append("<p>" + notes[i].LastEdited.ToShortDateString() + " " + notes[i].LastEdited.ToShortTimeString() + " UTC" + " </p>");
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    sb.Append(notes[i].NoteContent.NoteBody);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    //sb.Append("<hr/>");
                    //sb.Append("<a href=\"");
                    //sb.Append(Globals.ProductionUrl + "/notedisplay/" + notes[i].Id + "\" >Link to note</a>");  // TODO
                }

                return sb.ToString();
            }
        }
    }
}
