using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Notes.Client;
using Notes.Data;
using Notes.Entities;

namespace Notes.Manager
{
    /// <summary>
    /// Class NoteDataManager.  
    /// </summary>
    public static class NoteDataManager
    {
        /// <summary>
        /// Create a NoteFile
        /// </summary>
        /// <param name="db">NotesDbContext</param>
        /// <param name="userId">UserID of creator</param>
        /// <param name="name">NoteFile name</param>
        /// <param name="title">NoteFile title</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static async Task<bool> CreateNoteFile(NotesDbContext db,
            string userId, string name, string title)
        {
            IQueryable<NoteFile>? query = db.NoteFile.Where(p => p.NoteFileName == name);
            if (!query.Any())
            {
                NoteFile noteFile = new()
                {
                    NoteFileName = name,
                    NoteFileTitle = title,
                    Id = 0,
                    OwnerId = userId,
                    LastEdited = DateTime.UtcNow
                };
                db.NoteFile.Add(noteFile);
                await db.SaveChangesAsync();

                NoteFile? nf = await db.NoteFile
                    .Where(p => p.NoteFileName == noteFile.NoteFileName)
                    .FirstOrDefaultAsync();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                _ = await AccessManager.CreateBaseEntries(db, userId, nf.Id);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                NoteAccess access;
                int padid;
                switch (name)   // these files get special treatment access wise.
                {
                    case "announce":
                        padid = nf.Id;
                        access = await AccessManager.GetOneAccess(db, Globals.AccessOtherId, padid, 0);
                        access.ReadAccess = true;
                        db.Entry(access).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                        break;

                    case "pbnotes":
                        padid = nf.Id;
                        access = await AccessManager.GetOneAccess(db, Globals.AccessOtherId, padid, 0);
                        access.ReadAccess = true;
                        access.Respond = true;
                        access.Write = true;
                        db.Entry(access).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                        break;

                    case "noteshelp":
                        padid = nf.Id;
                        access = await AccessManager.GetOneAccess(db, Globals.AccessOtherId, padid, 0);
                        access.ReadAccess = true;
                        access.Respond = true;
                        access.Write = true;
                        db.Entry(access).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                        break;

                    case "pad":
                        padid = nf.Id;
                        access = await AccessManager.GetOneAccess(db, Globals.AccessOtherId, padid, 0);
                        access.ReadAccess = true;
                        access.Respond = true;
                        access.Write = true;
                        db.Entry(access).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                        break;

                    default:
                        break;
                }
                return true;
            }
            return false;
        }

        ///// <summary>
        ///// Delete a NoteFile
        ///// </summary>
        ///// <param name="db">NotesDbContext</param>
        ///// <param name="id">NoteFileID</param>
        ///// <returns></returns>
        //public static async Task<bool> DeleteNoteFile(NotesDbContext db, int id)
        //{
        //    // Things to delete:
        //    // 1)  X Entries in NoteContent
        //    // 2)  X Entries in BaseNoteHeader
        //    // 3)  X Entries in Sequencer
        //    // 4)  X Entries in NoteAccesses
        //    // 5)  X Entries in Marks
        //    // 6)  X Entries in SearchView
        //    // 7)  1 Entry in NoteFile

        //    // The above (1 - 6) now done by Cascade Delete of NoteFile

        //    //List<NoteContent> nc = await _db.NoteContent
        //    //    .Where(p => p.NoteFileID == id)
        //    //    .ToListAsync();
        //    //List<BaseNoteHeader> bnh = await GetBaseNoteHeadersForFile(_db, id);
        //    //List<Sequencer> seq = await _db.Sequencer
        //    //.Where(p => p.NoteFileID == id)
        //    //.ToListAsync();
        //    //List<NoteAccess> na = await AccessManager.GetAccessListForFile(_db, id);
        //    //List<Mark> marks = await _db.Mark
        //    //    .Where(p => p.NoteFileID == id)
        //    //    .ToListAsync();
        //    //List<SearchView> sv = await _db.SearchView
        //    //    .Where(p => p.NoteFileID == id)
        //    //    .ToListAsync();

        //    //_db.NoteContent.RemoveRange(nc);
        //    //_db.BaseNoteHeader.RemoveRange(bnh);
        //    //_db.Sequencer.RemoveRange(seq);
        //    //_db.NoteAccess.RemoveRange(na);
        //    //_db.Mark.RemoveRange(marks);
        //    //_db.SearchView.RemoveRange(sv);

        //    NoteFile noteFile = await db.NoteFile
        //       .Where(p => p.Id == id)
        //       .FirstAsync();

        //    for (int arcId = 0; arcId <= noteFile.NumberArchives; arcId++)
        //    {
        //        List<NoteAccess> na = await AccessManager.GetAccessListForFile(db, id, arcId);
        //        db.NoteAccess.RemoveRange(na);
        //    }

        //    List<Subscription> subs = await db.Subscription
        //        .Where(p => p.NoteFileId == id)
        //        .ToListAsync();
        //    db.Subscription.RemoveRange(subs);

        //    db.NoteFile.Remove(noteFile);

        //    await db.SaveChangesAsync();

        //    return true;
        //}

        ///// <summary>
        ///// Archive a notefile - Bump the files NumberArchives -
        ///// Set all current ArchiveId (=0) to NumberArchives for
        ///// NoteHeader, NoteAccess, Tags
        ///// </summary>
        ///// <param name="_db"></param>
        ///// <param name="noteFile"></param>
        //public static void ArchiveNoteFile(NotesDbContext _db, NoteFile noteFile)
        //{
        //    noteFile.NumberArchives++;
        //    _db.Update(noteFile);

        //    List<NoteHeader> nhl = _db.NoteHeader.Where(p => p.NoteFileId == noteFile.Id && p.ArchiveId == 0).ToList();

        //    foreach (NoteHeader nh in nhl)
        //    {
        //        nh.ArchiveId = noteFile.NumberArchives;
        //        _db.Update(nh);
        //    }

        //    List<NoteAccess> nal = _db.NoteAccess.Where(p => p.NoteFileId == noteFile.Id && p.ArchiveId == 0).ToList();
        //    foreach (NoteAccess na in nal)
        //    {
        //        na.ArchiveId = noteFile.NumberArchives;
        //    }
        //    _db.NoteAccess.AddRange(nal);

        //    List<Tags> ntl = _db.Tags.Where(p => p.NoteFileId == noteFile.Id && p.ArchiveId == 0).ToList();
        //    foreach (Tags nt in ntl)
        //    {
        //        nt.ArchiveId = noteFile.NumberArchives;
        //        _db.Update(nt);
        //    }

        //    _db.SaveChanges();
        //}

        /// <summary>
        /// Create a new note
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="nh">NoteHeader for new note</param>
        /// <param name="body">Note content</param>
        /// <param name="tags">the tags</param>
        /// <param name="dMessage">Director message (hold over from when it was not in header...)</param>
        /// <param name="send">Should we send emails? / Is this an Imported note?</param>
        /// <param name="linked">Are we processing linked file?</param>
        /// <param name="editing">Are we editing?</param>
        /// <returns>NoteHeader.</returns>
        public static async Task<NoteHeader> CreateNote(NotesDbContext db, NoteHeader nh, string body, string tags, string dMessage, bool send, bool linked, bool editing = false)
        {
            long editingId = nh.Id;

            nh.Id = 0;
            if (nh.ResponseOrdinal == 0 && !editing)  // base note
            {
                // get the next available note Ordinal
                nh.NoteOrdinal = await NextBaseNoteOrdinal(db, nh.NoteFileId, nh.ArchiveId);
            }

            if (!linked && !editing)    // create a GUID for use over link
            {
                nh.LinkGuid = Guid.NewGuid().ToString();
            }

            if (!send) // indicates an import operation / adjust time to UCT / assume original was CST = UCT-06, so add 6 hours
            {
                int offset = 6;
                if (nh.LastEdited.IsDaylightSavingTime())
                    offset--;       // five if DST

                // throw in an added random time in ms
                Random rand = new();
                int ms = rand.Next(999);

                nh.LastEdited = nh.LastEdited.AddHours(offset).AddMilliseconds(ms);
                nh.CreateDate = nh.LastEdited;
                nh.ThreadLastEdited = nh.CreateDate;
            }

            NoteFile? nf = await db.NoteFile
                .Where(p => p.Id == nh.NoteFileId)
                .FirstOrDefaultAsync();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            nf.LastEdited = nh.CreateDate;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            db.Entry(nf).State = EntityState.Modified;
            db.NoteHeader.Add(nh);
            await db.SaveChangesAsync();

            NoteHeader newHeader = nh;

            if (newHeader.ResponseOrdinal == 0) // base note
            {
                newHeader.BaseNoteId = newHeader.Id;
                db.Entry(newHeader).State = EntityState.Modified;

                if (editing)
                {
                    // update BaseNoteId for all responses
                    List<NoteHeader> rhl = db.NoteHeader.Where(p => p.BaseNoteId == editingId && p.ResponseOrdinal > 0).ToList();
                    foreach (NoteHeader ln in rhl)
                    {
                        ln.BaseNoteId = newHeader.Id;
                        db.Entry(ln).State = EntityState.Modified;
                    }
                }

                await db.SaveChangesAsync();
            }
            else    // response
            {
                NoteHeader? baseNote = await db.NoteHeader
                    .Where(p => p.NoteFileId == newHeader.NoteFileId && p.ArchiveId == newHeader.ArchiveId && p.NoteOrdinal == newHeader.NoteOrdinal && p.ResponseOrdinal == 0)
                    .FirstOrDefaultAsync();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                newHeader.BaseNoteId = baseNote.Id;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                db.Entry(newHeader).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }

            if (editing)
            {
                // update RefId
                List<NoteHeader> rhl = db.NoteHeader.Where(p => p.RefId == editingId).ToList();
                foreach (NoteHeader ln in rhl)
                {
                    ln.RefId = newHeader.Id;
                    db.Entry(ln).State = EntityState.Modified;
                }
                await db.SaveChangesAsync();
            }

            NoteContent newContent = new()
            {
                NoteHeaderId = newHeader.Id,
                NoteBody = body
            };
            db.NoteContent.Add(newContent);
            await db.SaveChangesAsync();

            // deal with tags

            if (tags is not null && tags.Length > 1)
            {
                var theTags = Tags.StringToList(tags, newHeader.Id, newHeader.NoteFileId, newHeader.ArchiveId);

                if (theTags.Count > 0)
                {
                    await db.Tags.AddRangeAsync(theTags);
                    await db.SaveChangesAsync();
                }
            }

            // Check for linked notefile(s)

            List<LinkedFile> links = await db.LinkedFile.Where(p => p.HomeFileId == newHeader.NoteFileId && p.SendTo).ToListAsync();

            if (linked || links is null || links.Count < 1)
            {
            }
            else
            {
                foreach (var link in links) // que up the linked notes
                {
                    if (link.SendTo)
                    {
                        LinkQueue q = new()
                        {
                            Activity = newHeader.ResponseOrdinal == 0 ? LinkAction.CreateBase : LinkAction.CreateResponse,
                            LinkGuid = newHeader.LinkGuid,
                            LinkedFileId = newHeader.NoteFileId,
                            BaseUri = link.RemoteBaseUri,
                            Secret = link.Secret
                        };

                        db.LinkQueue.Add(q);
                        await db.SaveChangesAsync();
                    }
                }
            }

            return newHeader;
        }

        /// <summary>
        /// Create a new Response - See CreateNote for params
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="nh">The nh.</param>
        /// <param name="body">The body.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="dMessage">The d message.</param>
        /// <param name="send">if set to <c>true</c> [send].</param>
        /// <param name="linked">if set to <c>true</c> [linked].</param>
        /// <param name="editing">if set to <c>true</c> [editing].</param>
        /// <returns>NoteHeader.</returns>
        public static async Task<NoteHeader> CreateResponse(NotesDbContext db, NoteHeader nh, string body, string tags, string dMessage, bool send, bool linked, bool editing = false)
        {
            // do setup and call CreateNote
            NoteHeader mine = await GetBaseNoteHeader(db, nh.BaseNoteId);
            db.Entry(mine).State = EntityState.Unchanged;
            await db.SaveChangesAsync();

            mine.ThreadLastEdited = DateTime.UtcNow;
            mine.ResponseCount++;

            db.Entry(mine).State = EntityState.Modified;
            await db.SaveChangesAsync();

            nh.ResponseOrdinal = mine.ResponseCount;
            nh.NoteOrdinal = mine.NoteOrdinal;
            return await CreateNote(db, nh, body, tags, dMessage, send, linked, editing);
        }

        /// <summary>
        /// Delete a Note
        /// </summary>
        /// <param name="_db">The database.</param>
        /// <param name="nh">The nh.</param>
        public static async Task DeleteNote(NotesDbContext _db, NoteHeader nh)
        {
            nh.IsDeleted = true;
            _db.Entry(nh).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            if (nh.ResponseOrdinal == 0 && nh.ResponseCount > 0)
            {
                // delete all responses
                for (int i = 1; i <= nh.ResponseCount; i++)
                {
                    NoteHeader rh = _db.NoteHeader.Single(p => p.ResponseOrdinal == i && p.Version == 0);
                    rh.IsDeleted = true;
                    _db.Entry(rh).State = EntityState.Modified;
                }
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Edit a note.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="nh">The nh.</param>
        /// <param name="nc">The nc.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>NoteHeader.</returns>
        public static async Task<NoteHeader> EditNote(NotesDbContext db, UserManager<ApplicationUser> userManager, NoteHeader nh, NoteContent nc, string tags)
        {
            // this is for making the current version 0 a higher version and creating a new version 0
            // begin by getting the old header and setting version to 1 more than the highest existing version

            // clone nh
            NoteHeader dh = nh.Clone();

            int nvers = await db.NoteHeader.CountAsync(p => p.NoteFileId == dh.NoteFileId && p.NoteOrdinal == dh.NoteOrdinal
                && p.ResponseOrdinal == dh.ResponseOrdinal && p.ArchiveId == dh.ArchiveId);

            NoteHeader oh = await db.NoteHeader.SingleAsync(p => p.Id == dh.Id);     //.Where(p => p.Id == nh.Id);
            oh.Version = nvers;
            db.Entry(oh).State = EntityState.Modified;

            await db.SaveChangesAsync();

            dh.LastEdited = DateTime.UtcNow;

            // then create new note

#pragma warning disable CS8604 // Possible null reference argument.
            return await CreateNote(db, dh, nc.NoteBody, tags, nh.DirectorMessage, true, false, true);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        /// <summary>
        /// Copy user prefs from ApplicationUser to UserData entity
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="fname">The fname.</param>
        /// <returns>UserData.</returns>
        public static async Task<NoteFile> GetFileByName(NotesDbContext db, string fname)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await db.NoteFile
                .Where(p => p.NoteFileName == fname)
                .FirstOrDefaultAsync();
#pragma warning restore CS8603 // Possible null reference return.
        }

        /// <summary>
        /// Get next available BaseNoteOrdinal
        /// </summary>
        /// <param name="db">NotesDbContext</param>
        /// <param name="noteFileId">NoteFileID</param>
        /// <param name="arcId">The arc identifier.</param>
        /// <returns>System.Int32.</returns>
        public static async Task<int> NextBaseNoteOrdinal(NotesDbContext db, int noteFileId, int arcId)
        {
            IOrderedQueryable<NoteHeader> bnhq = GetBaseNoteHeaderByIdRev(db, noteFileId, arcId);

            if (bnhq is null || !bnhq.Any())
                return 1;

            NoteHeader bnh = await bnhq.FirstAsync();
            return bnh.NoteOrdinal + 1;
        }

        /// <summary>
        /// Get BaseNoteHeaders in reverse order - we only plan to look at the
        /// first one/one with highest NoteOrdinal
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="noteFileId">The note file identifier.</param>
        /// <param name="arcId">The arc identifier.</param>
        /// <returns>IOrderedQueryable&lt;NoteHeader&gt;.</returns>
        private static IOrderedQueryable<NoteHeader> GetBaseNoteHeaderByIdRev(NotesDbContext db, int noteFileId, int arcId)
        {
            return db.NoteHeader
                            .Where(p => p.NoteFileId == noteFileId && p.ArchiveId == arcId && p.ResponseOrdinal == 0)
                            .OrderByDescending(p => p.NoteOrdinal);
        }

        /// <summary>
        /// Get notefile entity from its Id
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>NoteFile.</returns>
        public static async Task<NoteFile> GetFileById(NotesDbContext db, int id)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await db.NoteFile
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
#pragma warning restore CS8603 // Possible null reference return.
        }

        /// <summary>
        /// No Longer includes NoteFile but does include NoteContent
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="noteid">The noteid.</param>
        /// <returns>NoteHeader.</returns>
        public static async Task<NoteHeader> GetNoteByIdWithFile(NotesDbContext db, long noteid)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await db.NoteHeader
                .Include("NoteContent")
                //.Include("NoteFile")
                .Include("Tags")
                .Where(p => p.Id == noteid)
                .OrderBy((x => x.NoteOrdinal))
                .FirstOrDefaultAsync();
#pragma warning restore CS8603 // Possible null reference return.
        }

        /// <summary>
        /// Get a NoteHeader given its Id
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>NoteHeader.</returns>
        public static async Task<NoteHeader> GetBaseNoteHeader(NotesDbContext db, long id)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            NoteHeader nh = await db.NoteHeader
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            return await db.NoteHeader
                .Where(p => p.Id == nh.BaseNoteId)
                .FirstOrDefaultAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8603 // Possible null reference return.
        }

        /// <summary>
        /// Get the BaseNoteHeader for a Note
        /// </summary>
        /// <param name="db">NotesDbContext</param>
        /// <param name="fileId">fileid</param>
        /// <param name="arcId">The arc identifier.</param>
        /// <param name="noteOrd">noteordinal</param>
        /// <returns>List&lt;NoteHeader&gt;.</returns>
        public static async Task<NoteHeader> GetBaseNoteHeader(NotesDbContext db, int fileId, int arcId, int noteOrd)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await db.NoteHeader
                                .Where(p => p.NoteFileId == fileId && p.ArchiveId == arcId && p.NoteOrdinal == noteOrd && p.ResponseOrdinal == 0)
                                .FirstOrDefaultAsync();
#pragma warning restore CS8603 // Possible null reference return.
        }

        /// <summary>
        /// Gets a base note header given its headerid
        /// </summary>
        /// <param name="db"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<NoteHeader> GetBaseNoteHeaderById(NotesDbContext db, long id)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await db.NoteHeader
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
