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
            if (fv == null) throw new ArgumentNullException(nameof(fv));
            if (NoteFile == null) throw new ArgumentNullException(nameof(NoteFile));
            if (db == null) throw new ArgumentNullException(nameof(db));

            NoteHeader nc = await NoteDataManager.GetNoteByIdWithFile(db, fv.NoteID);

            if (nc == null)
            {
                return "Forwarded by Notes 2026 - User: " + email + " / " + name
                    + "<p>Note not found (Id: " + fv.NoteID + ")</p>";
            }

            if (!fv.Hasstring || !fv.Wholestring)
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var sbSingle = new StringBuilder(256);
                sbSingle.Append("Forwarded by Notes 2026 - User: " + email + " / " + name);
                sbSingle.Append("<p>File: " + (NoteFile.NoteFileName ?? string.Empty) + " - File Title: " + (NoteFile.NoteFileTitle ?? string.Empty) + "</p><hr/>");
                sbSingle.Append("<p>Author: " + (nc.AuthorName ?? string.Empty) + "  - Director Message: " + (nc.DirectorMessage ?? string.Empty) + "</p><p>");
                sbSingle.Append("<p>Subject: " + (nc.NoteSubject ?? string.Empty) + "</p>");
                sbSingle.Append(nc.LastEdited.ToShortDateString() + " " + nc.LastEdited.ToShortTimeString() + " UTC" + "</p>");
                sbSingle.Append(nc.NoteContent?.NoteBody ?? string.Empty);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                //+ "<hr/>" + "<a href=\"" + Globals.ProductionUrl + "/notedisplay/" + fv.NoteID + "\" >Link to note</a>";   // TODO
                return sbSingle.ToString();
            }
            else
            {
                List<NoteHeader> notes = await db.NoteHeader.Include(p => p.NoteContent)
                    .Where(p => p.NoteFileId == nc.NoteFileId && p.NoteOrdinal == nc.NoteOrdinal)
                    .OrderBy(p => p.ResponseOrdinal)
                    .ToListAsync();

                if (notes == null || notes.Count == 0)
                {
                    return "Forwarded by Notes 2026 - User: " + email + " / " + name
                        + "<p>No notes found for File: " + (NoteFile.NoteFileName ?? string.Empty) + "</p>";
                }

                fv.NoteSubject = notes[0].NoteSubject ?? fv.NoteSubject;

                StringBuilder sb = new(256 + notes.Count * 256);
                sb.Append("Forwarded by Notes 2026 - User: " + email + " / " + name
                    + "<p>\nFile: " + (NoteFile.NoteFileName ?? string.Empty) + " - File Title: " + (NoteFile.NoteFileTitle ?? string.Empty) + "</p>"
                    + "<hr/>");

                for (int i = 0; i < notes.Count; i++)
                {
                    var note = notes[i];
                    if (i == 0)
                    {
                        sb.Append("<p>Base Note - " + (notes.Count - 1) + " Response(s)</p>");
                    }
                    else
                    {
                        sb.Append("<hr/><p>Response - " + note.ResponseOrdinal + " of " + (notes.Count - 1) + "</p>");
                    }
                    sb.Append("<p>Author: " + (note.AuthorName ?? string.Empty) + "  - Director Message: " + (note.DirectorMessage ?? string.Empty) + "</p>");
                    sb.Append("<p>Subject: " + (note.NoteSubject ?? string.Empty) + "</p>");
                    sb.Append("<p>" + note.LastEdited.ToShortDateString() + " " + note.LastEdited.ToShortTimeString() + " UTC" + " </p>");
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    sb.Append(note.NoteContent?.NoteBody ?? string.Empty);
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
