/*--------------------------------------------------------------------------
    **
    **  Copyright © 2026, Dale Sinder
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
using Notes.Client;
using Notes.Data;
using Notes.Entities;

namespace Notes.Manager
{
    /// <summary>
    /// Provides static methods for managing and querying access control entries for note files, including creation of
    /// base access entries, retrieval of user-specific access, and validation of link-based access.
    /// </summary>
    /// <remarks>The AccessManager class is intended to centralize access control logic for note files within
    /// the application. All methods are static and thread-safe, as they operate on provided database contexts. This
    /// class should be used to create, retrieve, and verify access permissions for users and linked files. It is not
    /// intended to be instantiated.</remarks>
    public static class AccessManager
    {
        /// <summary>
        /// Create an Access Token
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="noteFileId">The note file identifier.</param>
        /// <param name="read">if set to <c>true</c> [read].</param>
        /// <param name="respond">if set to <c>true</c> [respond].</param>
        /// <param name="write">if set to <c>true</c> [write].</param>
        /// <param name="setTag">if set to <c>true</c> [set tag].</param>
        /// <param name="deleteEdit">if set to <c>true</c> [delete edit].</param>
        /// <param name="director">if set to <c>true</c> [director].</param>
        /// <param name="editAccess">if set to <c>true</c> [edit access].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static async Task<bool> Create(NotesDbContext db, string userId, int noteFileId, bool read, bool respond,
            bool write, bool setTag, bool deleteEdit, bool director, bool editAccess)
        {
            NoteAccess na = new()
            {
                UserID = userId,
                NoteFileId = noteFileId,

                ReadAccess = read,
                Respond = respond,
                Write = write,
                SetTag = setTag,
                DeleteEdit = deleteEdit,
                ViewAccess = director,
                EditAccess = editAccess
            };
            db.NoteAccess.Add(na);
            return (await db.SaveChangesAsync()) == 1;
        }

        /// <summary>
        /// Create standard starting entires for a access controls for a new file.
        /// "Other" -- no access
        /// creating user (Admin) -- Full Access
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static async Task<bool> CreateBaseEntries(NotesDbContext db, string userId, int fileId)
        {
            bool flag1 = await Create(db, Globals.AccessOtherId, fileId, false, false, false, false, false, false, false);
            if (!flag1)
                return false;

            flag1 = await Create(db, userId, fileId, true, true, true, true, true, true, true);
            if (!flag1)
                return false;

            return true;
        }

        /// <summary>
        /// All access checks call this.
        /// </summary>
        /// <param name="db">NotesDbContext</param>
        /// <param name="userId">ID of logged in user</param>
        /// <param name="fileId">NoteFileID</param>
        /// <param name="arcId">The arc identifier.</param>
        /// <returns>NoteAcess Object</returns>
        public static async Task<NoteAccess> GetAccess(NotesDbContext db, string userId, int fileId, int arcId)
        {
            // First we check for this user specifically
            NoteAccess? na = await db.NoteAccess
                .Where(p => p.UserID == userId && p.NoteFileId == fileId && p.ArchiveId == arcId).FirstOrDefaultAsync();

            // If specific user not in list use "Other"
            na ??= await db.NoteAccess
                    .Where(p => p.UserID == Globals.AccessOtherId && p.NoteFileId == fileId && p.ArchiveId == arcId).FirstOrDefaultAsync();

            return na is null ? new() : na;
        }

        /// <summary>
        /// Get Users Specific Access Entry
        /// </summary>
        /// <param name="db">NotesDbContext</param>
        /// <param name="userId">ID of logged in user</param>
        /// <param name="fileId">NoteFileID</param>
        /// <param name="arcId">The arc identifier.</param>
        /// <returns>NoteAcess Object</returns>
        public static async Task<NoteAccess> GetOneAccess(NotesDbContext db, string userId, int fileId, int arcId)
        {
            NoteAccess? na = await db.NoteAccess
                .Where(p => p.UserID == userId && p.NoteFileId == fileId && p.ArchiveId == arcId).FirstOrDefaultAsync();
            return na is null ? new() : na;
        }

        /// <summary>
        /// Gets the access list for file.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="arcId">The arc identifier.</param>
        /// <returns>List&lt;NoteAccess&gt;.</returns>
        public static async Task<List<NoteAccess>> GetAccessListForFile(NotesDbContext db, int fileId, int arcId)
        {
            return await db.NoteAccess
                .Where(p => p.NoteFileId == fileId && p.ArchiveId == arcId)
                .ToListAsync();
        }

        /// <summary>
        /// Tests the link access.
        /// </summary>
        /// <param name="NotesDbContext">The notes database context.</param>
        /// <param name="noteFile">The note file.</param>
        /// <param name="secret">The secret.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static async Task<bool> TestLinkAccess(NotesDbContext NotesDbContext,
            NoteFile noteFile, string secret)
        {
            try
            {
                List<LinkedFile> linkedFiles;

                if (string.IsNullOrEmpty(secret))
                {
                    linkedFiles = await NotesDbContext.LinkedFile
                        .Where(p => p.RemoteFileName == noteFile.NoteFileName && p.AcceptFrom)
                        .ToListAsync();
                }
                else
                {
                    linkedFiles = await NotesDbContext.LinkedFile
                        .Where(p => p.RemoteFileName == noteFile.NoteFileName && p.AcceptFrom && p.Secret == secret)
                        .ToListAsync();
                }
                if (linkedFiles is null || linkedFiles.Count < 1)
                    return false;

                return true;
            }
            catch { return false; }
        }
    }

}
