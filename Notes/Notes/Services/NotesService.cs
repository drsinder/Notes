using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Notes.Client;
using Notes.Client.Shared;
using Notes.Data;
using Notes.Entities;
using Notes.Manager;
using Notes.Protos;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime;
using System.Security.Claims;
using System.Text;


namespace Notes.Services
{
    public class NotesService(ILogger<NotesService> logger,
        NotesDbContext _db,
        IConfiguration _configuration,
        RoleManager<IdentityRole> _roleManager,
        //SignInManager<ApplicationUser> signInManager,
        IEmailSender _emailSender,
        UserManager<ApplicationUser> _userManager
    ) : NotesServer.NotesServerBase
    {

        /// <summary>
        /// Retrieves the current server time in Coordinated Universal Time (UTC) along with the local time zone offset.
        /// </summary>
        /// <remarks>The returned time reflects the server's system clock at the moment the request is
        /// processed. The time zone offset is based on the server's local settings and may vary depending on daylight
        /// saving time or system configuration.</remarks>
        /// <param name="request">A request object that does not contain any parameters. This value is required but its contents are ignored.</param>
        /// <param name="context">The context for the server call, providing information about the RPC environment and metadata.</param>
        /// <returns>A <see cref="ServerTime"/> object containing the current UTC time and the local time zone offset in hours
        /// and minutes.</returns>
        public override async Task<ServerTime> GetServerTime(NoRequest request, ServerCallContext context)
        {
            logger.LogInformation("Received GetServerTime request for ID: {Id}", 1);
            DateTime now = DateTime.UtcNow;
            var response = new ServerTime()
            {
                UtcTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(now),
                OffsetHours = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalHours,
                OffsetMinutes = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Minutes % 60
            };
            return (response);
        }

        /// <summary>
        /// Handles a no-operation request and returns a default response.
        /// </summary>
        /// <param name="request">The request message for the no-operation call. This parameter must not be null.</param>
        /// <param name="context">The context for the server-side call, providing information about the RPC environment.</param>
        /// <returns>A default <see cref="NoRequest"/> response representing the result of the no-operation call.</returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<NoRequest> NoOp(NoRequest request, ServerCallContext context)

        {
            return new();
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        /// <summary>
        /// Retrieves the access permissions for a specified note file based on the current user's context.
        /// </summary>
        /// <param name="request">The request containing the identifiers of the note file and archive for which access permissions are being
        /// queried.</param>
        /// <param name="context">The server call context that provides information about the current gRPC call, including user authentication
        /// and metadata.</param>
        /// <returns>A <see cref="GNoteAccess"/> object representing the user's access permissions for the specified note file.</returns>
        public override async Task<GNoteAccess> GetAccess(NoteFileRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, request.NoteFileId, request.ArcId);
            return na.GetGNoteAccess();

        }

        /// <summary>
        /// Generates a JSON Web Token (JWT) containing the specified claims and valid for the given duration.
        /// </summary>
        /// <remarks>The token is signed using the HMAC SHA-256 algorithm and the secret key configured in
        /// the application's settings. Ensure that the claims provided are appropriate for the intended authentication
        /// or authorization scenario.</remarks>
        /// <param name="authClaims">The collection of claims to include in the generated JWT. Each claim represents user or application-specific
        /// information to be embedded in the token.</param>
        /// <param name="hours">The number of hours for which the generated token will remain valid. Must be a positive integer.</param>
        /// <returns>A JwtSecurityToken instance representing the generated JWT with the specified claims and expiration.</returns>
        private JwtSecurityToken GetToken(List<Claim> authClaims, int hours)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTAuth:SecretKey"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWTAuth:ValidIssuerURL"],
                audience: _configuration["JWTAuth:ValidAudienceURL"],
                expires: DateTime.Now.AddHours(hours),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        /// <summary>
        /// Retrieves a list of all application users accessible to administrators.
        /// </summary>
        /// <remarks>This method can only be called by users with the 'Admin' role. The returned user list
        /// reflects the current state of the application's user store.</remarks>
        /// <param name="request">A request object containing no parameters. This value is ignored.</param>
        /// <param name="context">The server call context for the current gRPC request. Provides access to request metadata and cancellation
        /// tokens.</param>
        /// <returns>A <see cref="GAppUserList"/> containing information about all users in the application. The list will be
        /// empty if no users exist.</returns>
        [Authorize(Roles = "Admin")]
        public override async Task<GAppUserList> GetUserList(NoRequest request, ServerCallContext context)
        {
            List<ApplicationUser> list = await _userManager.Users.ToListAsync();
            return ApplicationUser.GetGAppUserList(list);
        }

        /// <summary>
        /// Retrieves the roles assigned to a specified user and returns a view model containing user information and
        /// role membership status.
        /// </summary>
        /// <remarks>This method requires the caller to have the 'Admin' role. The returned view model
        /// includes all roles in the system, with membership status for the specified user. The method is asynchronous
        /// and should be awaited.</remarks>
        /// <param name="request">An object containing the user's identifier and related request data. The user's identifier is used to locate
        /// the user whose roles are to be retrieved.</param>
        /// <param name="context">The server call context for the current gRPC request. Provides information about the call, such as
        /// cancellation and authentication details.</param>
        /// <returns>An EditUserViewModel containing the user's data and a list of all available roles, each indicating whether
        /// the user is a member of that role.</returns>
        [Authorize(Roles = "Admin")]
        public override async Task<EditUserViewModel> GetUserRoles(AppUserRequest request, ServerCallContext context)
        {
            EditUserViewModel model = new()
            {
                RolesList = new CheckedUserList()
            };
            string Id = request.Subject;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            ApplicationUser user = await _userManager.FindByIdAsync(Id);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            model.UserData = user?.GetGAppUser();

            var allRoles = _roleManager.Roles.ToList();

            //var myRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in allRoles)
            {
                CheckedUser cu = new()
                {
                    TheRole = new()
                };
                cu.TheRole.RoleId = role.Id;
                cu.TheRole.RoleName = role.Name;
                cu.IsMember = await _userManager.IsInRoleAsync(user, role.Name);
                model.RolesList.List.Add(cu);
            }

            return model;
        }

        /// <summary>
        /// Updates the roles assigned to a user based on the specified role selections.
        /// </summary>
        /// <remarks>This method requires the caller to have the 'Admin' role. Roles are updated to match
        /// the selections provided in <paramref name="model"/>; roles not selected are removed, and selected roles are
        /// added. The operation is performed asynchronously.</remarks>
        /// <param name="model">An object containing the user's data and a list of roles to be added or removed. The roles list indicates
        /// which roles the user should be a member of after the update.</param>
        /// <param name="context">The server call context for the current gRPC request. Provides information about the request and its
        /// execution environment.</param>
        /// <returns>A <see cref="NoRequest"/> object indicating that the operation has completed. No additional data is
        /// returned.</returns>
        [Authorize(Roles = "Admin")]
        public override async Task<NoRequest> UpdateUserRoles(EditUserViewModel model, ServerCallContext context)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            ApplicationUser user = await _userManager.FindByIdAsync(model.UserData.Id);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            var myRoles = await _userManager.GetRolesAsync(user);
            foreach (CheckedUser item in model.RolesList.List)
            {
                if (item.IsMember && !myRoles.Contains(item.TheRole.RoleName)) // need to add role
                {
                    await _userManager.AddToRoleAsync(user, item.TheRole.RoleName);
                }
                else if (!item.IsMember && myRoles.Contains(item.TheRole.RoleName)) // need to remove role
                {
                    await _userManager.RemoveFromRoleAsync(user, item.TheRole.RoleName);
                }
            }

            return new NoRequest();
        }

        /// <summary>
        /// Retrieves the current authenticated application user associated with the specified gRPC call context.
        /// </summary>
        /// <remarks>This method extracts the user identity from the HTTP context within the gRPC call and
        /// attempts to locate the corresponding application user. If the user is not authenticated or cannot be found,
        /// the result will be <see langword="null"/>.</remarks>
        /// <param name="context">The server call context containing information about the current gRPC request, including authentication
        /// details.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the authenticated <see
        /// cref="ApplicationUser"/> if found; otherwise, <see langword="null"/>.</returns>
        private async Task<ApplicationUser> GetAppUser(ServerCallContext context)
        {
            var user = context.GetHttpContext().User;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            ApplicationUser? appUser = await _userManager.FindByIdAsync(user.FindFirst(ClaimTypes.NameIdentifier).Value);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            return appUser;
        }

        /// <summary>
        /// Creates a new note file using the specified request data and returns the created note file.
        /// </summary>
        /// <remarks>This method requires the caller to have the 'Admin' role. The returned note file
        /// reflects the data provided in the request. The operation is asynchronous and may involve database
        /// access.</remarks>
        /// <param name="request">An object containing the details of the note file to create, including its name and title.</param>
        /// <param name="context">The server call context associated with the current gRPC request. Used to identify the calling user and
        /// manage request metadata.</param>
        /// <returns>A <see cref="GNotefile"/> representing the newly created note file, or <see langword="null"/> if the
        /// creation fails.</returns>
        [Authorize(Roles = "Admin")]
        public override async Task<GNotefile?> CreateNoteFile(GNotefile request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            await NoteDataManager.CreateNoteFile(_db, appUser.Id, request.NoteFileName, request.NoteFileTitle);

            List<NoteFile> x = _db.NoteFile.OrderBy(x => x.Id).ToList();
            NoteFile newfile = x[^1];
            return newfile.GetGNotefile();
        }

        /// <summary>
        /// Retrieves the data model for the home page in response to a client request.
        /// </summary>
        /// <remarks>This method requires the caller to be authorized. The returned model may vary based
        /// on the user's authentication and authorization status.</remarks>
        /// <param name="request">An object representing the request parameters for the home page. This value must not be null.</param>
        /// <param name="context">The server call context for the current gRPC request. Provides information about the call, such as user
        /// identity and cancellation tokens.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="HomePageModel"/>
        /// with the data for the home page.</returns>
        [Authorize]
        public override async Task<HomePageModel> GetHomePageModel(NoRequest request, ServerCallContext context)
        {
            return await GetBaseHomePageModelAsync(request, context);
        }

        /// <summary>
        /// Retrieves the administrative home page model, including user and note access data, for authorized admin
        /// users.
        /// </summary>
        /// <remarks>This method is restricted to users with the 'Admin' role. The returned model includes
        /// a list of all application users and their note access details, intended for administrative
        /// overview.</remarks>
        /// <param name="request">A request object containing parameters for the page retrieval. This parameter is required but does not carry
        /// any data.</param>
        /// <param name="context">The server call context for the current gRPC request, providing information about the call and its
        /// environment.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a populated HomePageModel with
        /// user and note access information for the admin view.</returns>
        [Authorize(Roles = "Admin")]
        public override async Task<HomePageModel> GetAdminPageModel(NoRequest request, ServerCallContext context)
        {
            HomePageModel homepageModel = await GetBaseHomePageModelAsync(request, context);

            List<ApplicationUser> udl = _db.Users.ToList();
            homepageModel.UserDataList = new GAppUserList();
            foreach (ApplicationUser userx in udl)
            {
                GAppUser ud = userx.GetGAppUser();
                homepageModel.UserDataList.List.Add(ud);
            }

            GAppUser user = homepageModel.UserData;
            homepageModel.NoteAccesses = new GNoteAccessList();
            foreach (GNotefile nf in homepageModel.NoteFiles.List)
            {
                NoteAccess na = await AccessManager.GetAccess(_db, user.Id, nf.Id, 0);
                homepageModel.NoteAccesses.List.Add(na.GetGNoteAccess());
            }

            return homepageModel;
        }

        /// <summary>
        /// Asynchronously constructs a base home page model containing general messages and, if available,
        /// user-specific data and accessible note files.
        /// </summary>
        /// <remarks>If the user is authenticated, the returned model includes personalized data and a
        /// list of note files the user can access. Otherwise, only general home page messages are included. This method
        /// does not throw exceptions for missing user data; it returns a model with available information.</remarks>
        /// <param name="request">A request object representing an empty or default request. This parameter is not used to influence the
        /// returned model.</param>
        /// <param name="context">The server call context associated with the current gRPC request. Used to access user information and
        /// authentication details.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a populated HomePageModel with
        /// general messages and, if the user is authenticated, user-specific data and accessible note files.</returns>
        private async Task<HomePageModel> GetBaseHomePageModelAsync(NoRequest request, ServerCallContext context)
        {
            HomePageModel homepageModel = new();

            logger.LogInformation("Received GetBaseHomePageModelAsync request");

            NoteFile? hpmf = _db.NoteFile.Where(p => p.NoteFileName == "homepagemessages").FirstOrDefault();
            if (hpmf is not null)
            {
                NoteHeader? hpmh = _db.NoteHeader.Where(p => p.NoteFileId == hpmf.Id && !p.IsDeleted).OrderByDescending(p => p.CreateDate).FirstOrDefault();
                if (hpmh is not null)
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    homepageModel.Message = _db.NoteContent.Where(p => p.NoteHeaderId == hpmh.Id).FirstOrDefault().NoteBody;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                }
            }

            if (context.GetHttpContext().User != null)
            {
                try
                {
                    ClaimsPrincipal user = context.GetHttpContext().User;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    if (user.FindFirst(ClaimTypes.NameIdentifier) is not null && user.FindFirst(ClaimTypes.NameIdentifier).Value != null)
                    {
                        ApplicationUser appUser = await GetAppUser(context);
                        homepageModel.UserData = appUser.GetGAppUser();

                        List<NoteFile> allFiles = _db.NoteFile.ToList().OrderBy(p => p.NoteFileTitle).ToList();
                        List<NoteAccess> myAccesses = _db.NoteAccess.Where(p => p.UserID == appUser.Id).ToList();
                        List<NoteAccess> otherAccesses = _db.NoteAccess.Where(p => p.UserID == Globals.AccessOtherId).ToList();

                        List<NoteFile> myNoteFiles = new();

                        bool isAdmin = await _userManager.IsInRoleAsync(appUser, UserRoles.Admin);
                        foreach (NoteFile file in allFiles)
                        {
                            NoteAccess? x = myAccesses.SingleOrDefault(p => p.NoteFileId == file.Id);
                            if (x is null)
                                x = otherAccesses.Single(p => p.NoteFileId == file.Id);

                            if (isAdmin || x.ReadAccess || x.Write || x.ViewAccess)
                            {
                                myNoteFiles.Add(file);
                            }
                        }

                        homepageModel.NoteFiles = NoteFile.GetGNotefileList(myNoteFiles);
                    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                }
                catch (Exception)
                {
                }
            }

            return homepageModel;
        }

        /// <summary>
        /// Updates an existing note file with new name and title information.
        /// </summary>
        /// <remarks>This method requires the caller to have the "Admin" role. The note file is updated in
        /// the database based on the provided <c>Id</c>. Changes are persisted asynchronously.</remarks>
        /// <param name="noteFile">The note file containing the updated name and title. The <c>Id</c> property must correspond to an existing
        /// note file.</param>
        /// <param name="context">The server call context for the current gRPC request.</param>
        /// <returns>A <see cref="GNotefile"/> representing the updated note file.</returns>
        [Authorize(Roles = "Admin")]
        public override async Task<GNotefile> UpdateNoteFile(GNotefile noteFile, ServerCallContext context)
        {
            NoteFile nf = await NoteDataManager.GetFileById(_db, noteFile.Id);
            nf.NoteFileName = noteFile.NoteFileName;
            nf.NoteFileTitle = noteFile.NoteFileTitle;
            _db.NoteFile.Update(nf);
            await _db.SaveChangesAsync();
            return noteFile;
        }

        /// <summary>
        /// Deletes the specified note file and all associated data from the database.
        /// </summary>
        /// <remarks>This method removes the note file along with all related tags, headers, content, and
        /// access records. Only users with the 'Admin' role are authorized to perform this operation.</remarks>
        /// <param name="noteFile">The note file to delete. Must contain a valid identifier for an existing note file.</param>
        /// <param name="context">The server call context for the current request.</param>
        /// <returns>A <see cref="NoRequest"/> object indicating that the operation completed successfully.</returns>
        [Authorize(Roles = "Admin")]
        public override async Task<NoRequest> DeleteNoteFile(GNotefile noteFile, ServerCallContext context)
        {
            NoteFile nf = await NoteDataManager.GetFileById(_db, noteFile.Id);

            // remove tags
            List<Tags> tl = _db.Tags.Where(p => p.NoteFileId == nf.Id).ToList();
            _db.Tags.RemoveRange(tl);

            // remove content
            List<NoteHeader> hl = _db.NoteHeader.Where(p => p.NoteFileId == nf.Id).ToList();
            foreach (NoteHeader h in hl)
            {
                NoteContent c = _db.NoteContent.Single(p => p.NoteHeaderId == h.Id);
                _db.NoteContent.Remove(c);
            }

            // remove headers
            _db.NoteHeader.RemoveRange(hl);

            // remove access
            List<NoteAccess> al = _db.NoteAccess.Where(p => p.NoteFileId == nf.Id).ToList();
            _db.NoteAccess.RemoveRange(al);

            // remove file
            _db.NoteFile.Remove(nf);
            await _db.SaveChangesAsync();
            return new NoRequest();
        }


        /// <summary>
        /// Imports data from the specified payload into the system using the provided import request.
        /// The payload is a notefile.
        /// </summary>
        /// <remarks>This method can only be accessed by users with the 'Admin' role. The import operation
        /// processes the payload contained in the request and may affect the system's data state.</remarks>
        /// <param name="request">The import request containing the payload data and associated metadata to be imported.</param>
        /// <param name="context">The server call context for the current gRPC operation, providing information about the call and its
        /// environment.</param>
        /// <returns>A <see cref="NoRequest"/> instance indicating that the import operation has completed.</returns>
        [Authorize(Roles = "Admin")]
        public override async Task<NoRequest> Import(ImportRequest request, ServerCallContext context)
        {
            MemoryStream? input = new MemoryStream(request.Payload.ToArray());
            StreamReader file = new StreamReader(input);

            Importer? imp = new();
            _ = await imp.Import(_db, file, request.NoteFile);

            file.DiscardBufferedData();
            file.Dispose();
            input.Dispose();
            GC.Collect();
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
            return new NoRequest();
        }

        /// <summary>
        /// Retrieves index data for a note file, including access information, note headers, linked files, tags, and
        /// user details, based on the specified request and the current user's permissions.
        /// </summary>
        /// <remarks>Only users with the appropriate access rights can retrieve note file index data.
        /// Administrators have additional viewing privileges. If the user lacks access or the file is missing, the
        /// returned model will indicate the reason in the message property.</remarks>
        /// <param name="request">The request containing the note file identifier and archive ID for which to retrieve index data.</param>
        /// <param name="context">The server call context that provides user authentication and request metadata.</param>
        /// <returns>A NoteDisplayIndexModel containing the note file's index data, user access details, notes, tags, and any
        /// relevant messages. If the user does not have access or the file does not exist, the model will include an
        /// appropriate message.</returns>
        [Authorize]
        public override async Task<NoteDisplayIndexModel> GetNoteFileIndexData(NoteFileRequest request, ServerCallContext context)
        {
            ClaimsPrincipal user;
            ApplicationUser appUser;
            NoteDisplayIndexModel idxModel = new();
            bool isAdmin;
            bool isUser;

            int arcId = request.ArcId;

            user = context.GetHttpContext().User;
            try
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                if (user.FindFirst(ClaimTypes.NameIdentifier) != null && user.FindFirst(ClaimTypes.NameIdentifier).Value != null)
                {
                    try
                    {
                        appUser = await GetAppUser(context);

                        isAdmin = await _userManager.IsInRoleAsync(appUser, "Admin");
                        isUser = await _userManager.IsInRoleAsync(appUser, "User");
                        if (!isUser)
                            return idxModel;    // not a User?  You get NOTHING!

                        NoteAccess noteAccess = await AccessManager.GetAccess(_db, appUser.Id, request.NoteFileId, arcId);
                        if (noteAccess is null)
                        {
                            idxModel.Message = "File does not exist";
                            return idxModel;
                        }
                        if (isAdmin)
                        {
                            noteAccess.ViewAccess = true;    // Admins can always view access list
                        }
                        idxModel.MyAccess = noteAccess.GetGNoteAccess();

                        idxModel.NoteFile = (await NoteDataManager.GetFileById(_db, request.NoteFileId)).GetGNotefile();

                        if (!idxModel.MyAccess.ReadAccess && !idxModel.MyAccess.Write)
                        {
                            idxModel.Message = "You do not have access to file " + idxModel.NoteFile.NoteFileName;
                            return idxModel;
                        }

                        List<LinkedFile> linklist = await _db.LinkedFile.Where(p => p.HomeFileId == request.NoteFileId).ToListAsync();
                        if (linklist is not null && linklist.Count > 0)
                            idxModel.LinkedText = " (Linked)";

                        List<NoteHeader> allhead = await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == arcId).ToListAsync(); // await NoteDataManager.GetAllHeaders(_db, request.NoteFileId, arcId);
                        idxModel.AllNotes = NoteHeader.GetGNoteHeaderList(allhead);

                        List<NoteHeader> notes = allhead.FindAll(p => p.ResponseOrdinal == 0).OrderBy(p => p.NoteOrdinal).ToList();
                        idxModel.Notes = NoteHeader.GetGNoteHeaderList(notes);

                        idxModel.UserData = appUser.GetGAppUser();

                        List<Tags> tags = await _db.Tags.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == arcId).ToListAsync();
                        idxModel.Tags = Tags.GetGTagsList(tags);

                        idxModel.ArcId = arcId;
                    }
                    catch (Exception ex1)
                    {
                        idxModel.Message = ex1.Message;
                    }
                }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            catch (Exception ex)
            {
                idxModel.Message = ex.Message;
            }

            return idxModel;
        }

        /// <summary>
        /// Retrieves the content and metadata of a note specified by the request, including header, body, tags, file
        /// information, and access permissions.
        /// </summary>
        /// <remarks>Only users with appropriate read access can retrieve note content. Edit permissions
        /// are granted to administrators and the note's author. The returned model includes information about whether
        /// the user can edit the note and whether they have administrative privileges.</remarks>
        /// <param name="request">An object containing the note identifier and version to specify which note content to retrieve.</param>
        /// <param name="context">The server call context for the current gRPC request, used to identify and authorize the user.</param>
        /// <returns>A DisplayModel containing the note's header, content, tags, file information, access details, and edit
        /// permissions. Returns an empty DisplayModel if the user does not have read access to the note.</returns>
        [Authorize]
        public override async Task<DisplayModel> GetNoteContent(DisplayModelRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            bool isAdmin = await _userManager.IsInRoleAsync(appUser, "Admin");

            NoteHeader nh = await _db.NoteHeader.SingleAsync(p => p.Id == request.NoteId && p.Version == request.Vers);
            NoteContent c = await _db.NoteContent.SingleAsync(p => p.NoteHeaderId == nh.Id);
            List<Tags> tags = await _db.Tags.Where(p => p.NoteHeaderId == nh.Id).ToListAsync();
            NoteFile nf = await _db.NoteFile.SingleAsync(p => p.Id == nh.NoteFileId);
            NoteAccess access = await AccessManager.GetAccess(_db, appUser.Id, nh.NoteFileId, nh.ArchiveId);

            bool canEdit = isAdmin;         // admins can always edit a note
            if (appUser.Id == nh.AuthorID)  // otherwise only the author can edit
                canEdit = true;

            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, nh.NoteFileId, nh.ArchiveId);

            if (!na.ReadAccess)
                return new DisplayModel();

            DisplayModel model = new()
            {
                Header = nh.GetGNoteHeader(),
                Content = c.GetGNoteContent(),
                Tags = Tags.GetGTagsList(tags),
                NoteFile = nf.GetGNotefile(),
                Access = access.GetGNoteAccess(),
                CanEdit = canEdit,
                IsAdmin = isAdmin
            };

            return model;
        }

        /// <summary>
        /// Retrieves the access list for a specified note file and archive for the requesting user.
        /// </summary>
        /// <param name="request">An object containing the identifiers for the note file and archive, as well as user information used to
        /// determine access rights.</param>
        /// <param name="context">The server call context for the current gRPC request.</param>
        /// <returns>A <see cref="GNoteAccessList"/> representing the access permissions for the specified note file and archive.</returns>
        [Authorize]
        public override async Task<GNoteAccessList> GetAccessList(AccessAndUserListRequest request, ServerCallContext context)
        {
            return NoteAccess.GetGNoteAccessList(await _db.NoteAccess.Where(p => p.NoteFileId == request.FileId && p.ArchiveId == request.ArcId).ToListAsync());
        }

        /// <summary>
        /// Retrieves the access permissions and user list associated with the specified note file and archive for a
        /// given user.
        /// </summary>
        /// <param name="request">An object containing the identifiers for the note file, archive, and user for which to retrieve access and
        /// user information. Cannot be null.</param>
        /// <param name="context">The server call context for the current gRPC request. Provides information about the call, such as
        /// cancellation and metadata.</param>
        /// <returns>An AccessAndUserList object containing the access permissions, the list of application users, and the user's
        /// specific access for the requested note file and archive.</returns>
        [Authorize]
        public override async Task<AccessAndUserList> GetAccessAndUserList(AccessAndUserListRequest request, ServerCallContext context)
        {
            AccessAndUserList accessAndUserList = new()
            {
                AccessList = NoteAccess.GetGNoteAccessList(_db.NoteAccess.Where(p => p.NoteFileId == request.FileId && p.ArchiveId == request.ArcId).ToList()),
                AppUsers = ApplicationUser.GetGAppUserList((await _userManager.GetUsersInRoleAsync("User")).ToList()),
                UserAccess = (await AccessManager.GetAccess(_db, request.UserId, request.FileId, request.ArcId)).GetGNoteAccess()
            };

            return accessAndUserList;
        }

        /// <summary>
        /// Updates the access item.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNoteAccess.</returns>
        [Authorize]
        public override async Task<GNoteAccess> UpdateAccessItem(GNoteAccess request, ServerCallContext context)
        {
            NoteAccess access = NoteAccess.GetNoteAccess(request);
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, access.NoteFileId, access.ArchiveId);
            if (na.EditAccess)
            {
                _db.NoteAccess.Update(access);
                await _db.SaveChangesAsync();
            }

            return request;
        }

        /// <summary>
        /// Deletes the access item.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoRequest.</returns>
        [Authorize]
        public override async Task<NoRequest> DeleteAccessItem(GNoteAccess request, ServerCallContext context)
        {
            NoteAccess access = NoteAccess.GetNoteAccess(request);
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, access.NoteFileId, access.ArchiveId);
            if (na.EditAccess)
            {
                _db.NoteAccess.Remove(access);
                await _db.SaveChangesAsync();
            }

            return new NoRequest();
        }

        /// <summary>
        /// Adds an access item.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNoteAccess.</returns>
        [Authorize]
        public override async Task<GNoteAccess> AddAccessItem(GNoteAccess request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, request.NoteFileId, request.ArchiveId);
            if (na.EditAccess)
            {
                await _db.NoteAccess.AddAsync(NoteAccess.GetNoteAccess(request));
                await _db.SaveChangesAsync();
            }

            return request;
        }

        /// <summary>
        /// Gets the user data.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GAppUser.</returns>
        [Authorize]
        public override async Task<GAppUser> GetUserData(NoRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            return appUser.GetGAppUser();
        }

        /// <summary>
        /// Updates the user data.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GAppUser.</returns>
        [Authorize]
        public override async Task<GAppUser> UpdateUserData(GAppUser request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            if (appUser.Id != request.Id)   // can onlt update self
                return request;

            ApplicationUser? appUserBase = await _userManager.FindByIdAsync(request.Id);
            ApplicationUser merged = ApplicationUser.MergeApplicationUser(request, appUserBase);

            await _userManager.UpdateAsync(merged);

            return request;
        }

        /// <summary>
        /// Gets the versions.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNoteHeaderList.</returns>
        [Authorize]
        public override async Task<GNoteHeaderList> GetVersions(GetVersionsRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, request.FileId, request.ArcId);
            if (!na.ReadAccess)
                return new GNoteHeaderList();

            List<NoteHeader> hl;

            hl = _db.NoteHeader.Where(p => p.NoteFileId == request.FileId && p.Version != 0
                    && p.NoteOrdinal == request.NoteOrdinal && p.ResponseOrdinal == request.ResponseOrdinal && p.ArchiveId == request.ArcId)
                .OrderBy(p => p.Version)
                .ToList();

            return NoteHeader.GetGNoteHeaderList(hl);
        }

        /// <summary>
        /// Gets the sequencer list.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GSequencerList.</returns>
        [Authorize]
        public override async Task<GSequencerList> GetSequencer(NoRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            // My list
            List<Sequencer> mine = await _db.Sequencer.Where(p => p.UserId == appUser.Id).OrderBy(p => p.Ordinal).ThenBy(p => p.LastTime).ToListAsync();

            if (mine is null)
                mine = new List<Sequencer>();

            List<Sequencer> avail = new();

            foreach (Sequencer m in mine)
            {
                NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, m.NoteFileId, 0);
                if (na.ReadAccess)
                    avail.Add(m);   // ONLY if you have current read access!!
            }
            var ret = Sequencer.GetGSequencerList(avail.OrderBy(p => p.Ordinal).ToList());
            return ret;
        }

        /// <summary>
        /// Creates the sequencer item.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoRequest.</returns>
        [Authorize]
        public override async Task<NoRequest> CreateSequencer(SCheckModel request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            List<Sequencer> mine = await _db.Sequencer.Where(p => p.UserId == appUser.Id).OrderByDescending(p => p.Ordinal).ToListAsync();

            int ord;
            if (mine is null || mine.Count == 0)
            {
                ord = 1;
            }
            else
            {
                ord = mine[0].Ordinal + 1;
            }

            Sequencer tracker = new()   // make a starting entry
            {
                Active = true,
                NoteFileId = request.FileId,
                LastTime = DateTime.UtcNow,
                UserId = appUser.Id,
                Ordinal = ord,
                StartTime = DateTime.UtcNow
            };

            _db.Sequencer.Add(tracker);
            await _db.SaveChangesAsync();

            return new NoRequest();
        }

        /// <summary>
        /// Deletes the sequencer item.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoRequest.</returns>
        [Authorize]
        public override async Task<NoRequest> DeleteSequencer(SCheckModel request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Sequencer mine = await _db.Sequencer.SingleOrDefaultAsync(p => p.UserId == appUser.Id && p.NoteFileId == request.FileId);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            if (mine is null)
                return new NoRequest();

            _db.Sequencer.Remove(mine);
            await _db.SaveChangesAsync();

            return new NoRequest();
        }

        /// <summary>
        /// Updates the sequencer item ordinal.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoRequest.</returns>
        [Authorize]
        public override async Task<NoRequest> UpdateSequencerOrdinal(GSequencer request, ServerCallContext context)
        {
            Sequencer modified = await _db.Sequencer.SingleAsync(p => p.UserId == request.UserId && p.NoteFileId == request.NoteFileId);

            modified.LastTime = request.LastTime.ToDateTime();
            modified.Ordinal = request.Ordinal;

            _db.Entry(modified).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return new NoRequest();
        }

        /// <summary>
        /// Updates the sequencer item.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoRequest.</returns>
        [Authorize]
        public override async Task<NoRequest> UpdateSequencer(GSequencer request, ServerCallContext context)
        {
            Sequencer modified = await _db.Sequencer.SingleAsync(p => p.UserId == request.UserId && p.NoteFileId == request.NoteFileId);

            modified.Active = request.Active;
            if (request.Active)  // starting to seq - set start time
            {
                modified.StartTime = DateTime.UtcNow;
            }
            else            // end of a file - copy start time to LastTime so we do not miss notes
            {
                modified.LastTime = modified.StartTime;  //request.StartTime.ToDateTime();
            }

            _db.Entry(modified).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return new NoRequest();
        }

        /// <summary>
        /// Gets the note file.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNotefile.</returns>
        [Authorize]
        public override async Task<GNotefile> GetNoteFile(NoteFileRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, request.NoteFileId, 0);
            if (na.Write || na.ReadAccess || na.EditAccess || na.Respond)      // TODO is this right??
            { }
            else
                return new GNotefile();

            NoteFile nf = _db.NoteFile.Single(p => p.Id == request.NoteFileId);

            return nf.GetGNotefile();
        }

        /// <summary>
        /// Creates the new note.
        /// </summary>
        /// <param name="TextViewModel">The TVM.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNoteHeader.</returns>
        [Authorize]
        public override async Task<GNoteHeader> CreateNewNote(TextViewModel tvm, ServerCallContext context)
        {
            if (tvm.MyNote is null || tvm.MySubject is null)
                return new GNoteHeader();

            ApplicationUser appUser = await GetAppUser(context);
            bool test = await _userManager.IsInRoleAsync(appUser, "User");
            if (!test)  // Must be in a User Role
                return new GNoteHeader();

            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, tvm.NoteFileID, 0);
            if (na.Write || na.Respond)
            { }
            else
                return new GNoteHeader();

            ApplicationUser me = appUser;
            DateTime now = DateTime.UtcNow;
            NoteHeader nheader = new()  // construct a new NoteHeader
            {
                LastEdited = now,
                ThreadLastEdited = now,
                CreateDate = now,
                NoteFileId = tvm.NoteFileID,
                AuthorName = me.DisplayName,
                AuthorID = me.Id,
                NoteSubject = tvm.MySubject,
                DirectorMessage = tvm.DirectorMessage,
                ResponseOrdinal = 0,
                ResponseCount = 0
            };

            NoteHeader created;

            if (tvm.BaseNoteHeaderID == 0)  // a base note
            {
                created = await NoteDataManager.CreateNote(_db, nheader, tvm.MyNote, tvm.TagLine, tvm.DirectorMessage, true, false);
            }
            else        // a response
            {
                nheader.BaseNoteId = tvm.BaseNoteHeaderID;
                nheader.RefId = tvm.RefId;
                created = await NoteDataManager.CreateResponse(_db, nheader, tvm.MyNote, tvm.TagLine, tvm.DirectorMessage, true, false);
            }

            //// Process any linked note file
            //await ProcessLinkedNotes();

            //// Send copy to subscribers
            //await SendNewNoteToSubscribers(created);

            return created.GetGNoteHeader();
        }

        /// <summary>
        /// Updates the note.
        /// </summary>
        /// <param name="tvm">The TVM.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNoteHeader.</returns>
        [Authorize]
        public override async Task<GNoteHeader> UpdateNote(TextViewModel tvm, ServerCallContext context)
        {
            if (tvm.MyNote is null || tvm.MySubject is null)
                return new GNoteHeader();

            NoteHeader nheader = await NoteDataManager.GetBaseNoteHeaderById(_db, tvm.NoteID);

            ApplicationUser appUser = await GetAppUser(context);
            bool isAdmin = await _userManager.IsInRoleAsync(appUser, "Admin");
            bool canEdit = isAdmin;         // admins can always edit a note
            if (appUser.Id == nheader.AuthorID)  // otherwise only the author can edit
                canEdit = true;

            if (!canEdit)
                return new GNoteHeader();

            // update header
            DateTime now = DateTime.UtcNow;
            nheader.NoteSubject = tvm.MySubject;
            nheader.DirectorMessage = tvm.DirectorMessage;
            //nheader.LastEdited = now;
            nheader.ThreadLastEdited = now;

            NoteContent nc = new()
            {
                NoteHeaderId = tvm.NoteID,
                NoteBody = tvm.MyNote
            };

            NoteHeader newheader = await NoteDataManager.EditNote(_db, _userManager, nheader, nc, tvm.TagLine);

            //await ProcessLinkedNotes();

            return newheader.GetGNoteHeader();
        }

        /// <summary>
        /// Gets the header for note identifier.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNoteHeader.</returns>
        [Authorize]
        public override async Task<GNoteHeader> GetHeaderForNoteId(NoteId request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            bool isAdmin = await _userManager.IsInRoleAsync(appUser, "Admin");

            GNoteHeader gnh = (await _db.NoteHeader.SingleAsync(p => p.Id == request.Id)).GetGNoteHeader();

            if (isAdmin)
            { }
            else
            {
                NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, gnh.NoteFileId, gnh.ArchiveId);
                if (!na.ReadAccess)
                {
                    return new GNoteHeader();
                }
            }

            return gnh;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        /// <summary>
        /// Gets the about.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>AboutModel.</returns>
        public override async Task<AboutModel> GetAbout(NoRequest request, ServerCallContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return new AboutModel()
            {
                PrimeAdminEmail = _configuration["PrimeAdminEmail"],
                PrimeAdminName = _configuration["PrimeAdminName"]
            };
        }

        ///// <summary>
        ///// The throttle
        ///// </summary>
        //private static int throttle = 0;

        ///// <summary>
        ///// The time of throttle set
        ///// </summary>
        //private static DateTime? TimeOfThrottle = null;

        ///// <summary>
        ///// Send an email.
        ///// unauthenticated - slower - use it too much and it really hurts you!
        ///// </summary>
        ///// <param name="request">The request received from the client.</param>
        ///// <param name="context">The context of the server-side call handler being invoked.</param>
        ///// <returns>The response to send back to the client (wrapped by a task).</returns>
        //public override async Task<NoRequest> SendEmail(GEmail request, ServerCallContext context)
        //{
        //    try
        //    {
        //        ApplicationUser appUser = await GetAppUser(context);
        //    }
        //    catch (Exception)
        //    {
        //        // was not authenticated - slow them up

        //        if (throttle++ >= 100)
        //        {
        //            // some real potential abuse??
        //            Thread.Sleep(1000 * throttle);

        //            if (TimeOfThrottle is null)
        //            {
        //                TimeOfThrottle = DateTime.UtcNow;
        //            }
        //            else
        //            {
        //                TimeSpan? diff = DateTime.UtcNow - TimeOfThrottle;
        //                if (diff > new TimeSpan(0, 30, 0)) // backoff in 30 minutes
        //                {
        //                    throttle = 0;
        //                    TimeOfThrottle = null;
        //                }
        //            }
        //        }

        //        Thread.Sleep(1000);
        //    }

        //    await _emailSender.SendEmailAsync(request.Address, request.Subject, request.Body);
        //    return new NoRequest();
        //}

        /// <summary>
        /// Send email authenticated.
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        [Authorize]
        public override async Task<NoRequest> SendEmailAuth(GEmail request, ServerCallContext context)
        {
            await _emailSender.SendEmailAsync(request.Address, request.Subject, request.Body);
            return new NoRequest();
        }

        ///// <summary>
        ///// Gets the export info for phase 1.
        ///// </summary>
        ///// <param name="request">The request.</param>
        ///// <param name="context">The context.</param>
        ///// <returns>GNoteHeaderList.</returns>
        //[Authorize]
        //public override async Task<GNoteHeaderList> GetExport(ExportRequest request, ServerCallContext context)
        //{
        //    ApplicationUser appUser = await GetAppUser(context);
        //    NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, request.FileId, request.ArcId);
        //    if (!na.ReadAccess)
        //        return new GNoteHeaderList();

        //    List<NoteHeader> nhl;

        //    if (request.NoteOrdinal == 0)   // All base notes
        //    {
        //        nhl = await _db.NoteHeader
        //            .Where(p => p.NoteFileId == request.FileId && p.ArchiveId == request.ArcId && p.ResponseOrdinal == 0)
        //            .OrderBy(p => p.NoteOrdinal)
        //            .ToListAsync();
        //    }
        //    else                // Just one base note/response
        //    {
        //        nhl = await _db.NoteHeader
        //            .Where(p => p.NoteFileId == request.FileId && p.ArchiveId == request.ArcId && p.NoteOrdinal == request.NoteOrdinal && p.ResponseOrdinal == request.ResponseOrdinal)
        //            .ToListAsync();
        //    }

        //    return NoteHeader.GetGNoteHeaderList(nhl);
        //}

        /// <summary>
        /// Gets the export info for phase 2. (note content)
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNoteContent.</returns>
        [Authorize]
        public override async Task<GNoteContent> GetExport2(NoteId request, ServerCallContext context)
        {
            NoteContent? nc = await _db.NoteContent
                .Where(p => p.NoteHeaderId == request.Id)
                .FirstOrDefaultAsync();

            NoteHeader? nh = await _db.NoteHeader
                .Where(p => p.Id == request.Id)
                .FirstOrDefaultAsync();

            ApplicationUser appUser = await GetAppUser(context);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, nh.NoteFileId, nh.ArchiveId);
            if (!na.ReadAccess)
                return new GNoteContent();

            return nc.GetGNoteContent();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        /// <summary>
        /// Does the forward of note(s) to an email address.
        /// </summary>
        /// <param name="fv">The fv.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoRequest.</returns>
        [Authorize]
        public override async Task<NoRequest> DoForward(ForwardViewModel fv, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, fv.FileID, fv.ArcID);
            if (!na.ReadAccess)
                return new NoRequest();

#pragma warning disable CS8604 // Possible null reference argument.
            string myEmail = await LocalService.MakeNoteForEmail(fv, fv.NoteFile, _db, appUser.Email, appUser.DisplayName);
#pragma warning restore CS8604 // Possible null reference argument.

            await _emailSender.SendEmailAsync(appUser.Email, fv.NoteSubject, myEmail);
            return new NoRequest();
        }

        /// <summary>
        /// Gets the note files ordered by name.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNotefileList.</returns>
        [Authorize]
        public override async Task<GNotefileList> GetNoteFilesOrderedByName(NoRequest request, ServerCallContext context)
        {
            List<NoteFile> noteFiles = await _db.NoteFile.OrderBy(p => p.NoteFileName).ToListAsync();
            return NoteFile.GetGNotefileList(noteFiles);
        }

        /// <summary>
        /// Copies note(s) from one file to another
        /// </summary>
        /// <param name="Model">The model.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoRequest.</returns>
        [Authorize]
        public override async Task<NoRequest> CopyNote(CopyModel Model, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, Model.Note.NoteFileId, Model.Note.ArchiveId);
            if (!na.ReadAccess)
                return new NoRequest();         // can not read file

            int fileId = Model.FileId;

            // Can I write to the target file?
            string uid = appUser.Id;
            NoteAccess myAccess = await AccessManager.GetAccess(_db, uid, fileId, 0);
            if (!myAccess.Write)
                return new NoRequest();         // can not write to file

            // Prepare to copy
            NoteHeader Header = NoteHeader.GetNoteHeader(Model.Note);
            bool whole = Model.WholeString;
            NoteFile noteFile = await _db.NoteFile.SingleAsync(p => p.Id == fileId);

            // Just the note
            if (!whole)
            {
                NoteContent cont = await _db.NoteContent.SingleAsync(p => p.NoteHeaderId == Header.Id);
                //cont.NoteHeader = null;
                List<Tags> tags = await _db.Tags.Where(p => p.NoteHeaderId == Header.Id).ToListAsync();

                string Body = string.Empty;
                Body = MakeHeader(Header, noteFile);
                Body += cont.NoteBody;

                Header = Header.CloneForLink();

                Header.Id = 0;
                Header.ArchiveId = 0;
                Header.LinkGuid = string.Empty;
                Header.NoteOrdinal = 0;
                Header.ResponseCount = 0;
                Header.NoteFileId = fileId;
                Header.BaseNoteId = 0;
                //Header.NoteFile = null;
                Header.AuthorID = appUser.Id;
                Header.AuthorName = appUser.DisplayName;

                Header.CreateDate = Header.ThreadLastEdited = Header.LastEdited = DateTime.Now.ToUniversalTime();

#pragma warning disable CS8604 // Possible null reference argument.
                _ = await NoteDataManager.CreateNote(_db, Header, Body, Tags.ListToString(tags), Header.DirectorMessage, true, false);
#pragma warning restore CS8604 // Possible null reference argument.
            }
            else    // whole note string
            {
                // get base note first
                NoteHeader BaseHeader;
                BaseHeader = await _db.NoteHeader.SingleAsync(p => p.NoteFileId == Header.NoteFileId
                    && p.ArchiveId == Header.ArchiveId
                    && p.NoteOrdinal == Header.NoteOrdinal
                    && p.ResponseOrdinal == 0);

                Header = BaseHeader.CloneForLink();

                NoteContent cont = await _db.NoteContent.SingleAsync(p => p.NoteHeaderId == Header.Id);
                //cont.NoteHeader = null;
                List<Tags> tags = await _db.Tags.Where(p => p.NoteHeaderId == Header.Id).ToListAsync();

                string Body = string.Empty;
                Body = MakeHeader(Header, noteFile);
                Body += cont.NoteBody;

                Header.Id = 0;
                Header.ArchiveId = 0;
                Header.LinkGuid = string.Empty;
                Header.NoteOrdinal = 0;
                Header.ResponseCount = 0;
                Header.NoteFileId = fileId;
                Header.BaseNoteId = 0;
                //Header.NoteFile = null;
                Header.AuthorID = appUser.Id;
                Header.AuthorName = appUser.DisplayName;

                Header.CreateDate = Header.ThreadLastEdited = Header.LastEdited = DateTime.Now.ToUniversalTime();

                Header.NoteContent = null;

#pragma warning disable CS8604 // Possible null reference argument.
                NoteHeader NewHeader = await NoteDataManager.CreateNote(_db, Header, Body, Tags.ListToString(tags), Header.DirectorMessage, true, false);
#pragma warning restore CS8604 // Possible null reference argument.

                // now deal with any responses
                for (int i = 1; i <= BaseHeader.ResponseCount; i++)
                {
                    NoteHeader RHeader = await _db.NoteHeader.SingleAsync(p => p.NoteFileId == BaseHeader.NoteFileId
                        && p.ArchiveId == BaseHeader.ArchiveId
                        && p.NoteOrdinal == BaseHeader.NoteOrdinal
                        && p.ResponseOrdinal == i);

                    Header = RHeader.CloneForLinkR();

                    cont = await _db.NoteContent.SingleAsync(p => p.NoteHeaderId == Header.Id);
                    tags = await _db.Tags.Where(p => p.NoteHeaderId == Header.Id).ToListAsync();

                    Body = string.Empty;
                    Body = MakeHeader(Header, noteFile);
                    Body += cont.NoteBody;

                    Header.Id = 0;
                    Header.ArchiveId = 0;
                    Header.LinkGuid = string.Empty;
                    Header.NoteOrdinal = NewHeader.NoteOrdinal;
                    Header.ResponseCount = 0;
                    Header.NoteFileId = fileId;
                    Header.BaseNoteId = NewHeader.Id;
                    //Header.NoteFile = null;
                    Header.ResponseOrdinal = 0;
                    Header.AuthorID = appUser.Id;
                    Header.AuthorName = appUser.DisplayName;

                    Header.CreateDate = Header.ThreadLastEdited = Header.LastEdited = DateTime.Now.ToUniversalTime();

#pragma warning disable CS8604 // Possible null reference argument.
                    _ = await NoteDataManager.CreateResponse(_db, Header, Body, Tags.ListToString(tags), Header.DirectorMessage, true, false);
#pragma warning restore CS8604 // Possible null reference argument.
                }
            }
            return new NoRequest();
        }

        // Utility method - makes a viewable header for the copied note
        /// <summary>
        /// Makes the header.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="noteFile">The note file.</param>
        /// <returns>System.String.</returns>
        private static string MakeHeader(NoteHeader header, NoteFile noteFile)
        {
            StringBuilder sb = new();

            sb.Append("<div class=\"copiednote\">From: ");
            sb.Append(noteFile.NoteFileName);
            sb.Append(" - ");
            sb.Append(header.NoteSubject);
            sb.Append(" - ");
            sb.Append(header.AuthorName);
            sb.Append(" - ");
            sb.Append(header.CreateDate.ToShortDateString());
            sb.AppendLine("</div>");
            return sb.ToString();
        }

        /// <summary>
        /// Deletes the note.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoRequest.</returns>
        [Authorize]
        public override async Task<NoRequest> DeleteNote(NoteId request, ServerCallContext context)
        {
            NoteHeader note = await NoteDataManager.GetNoteByIdWithFile(_db, request.Id);

            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, note.NoteFileId, note.ArchiveId);
            if (!na.DeleteEdit)
            {
                return new();
            }

            await NoteDataManager.DeleteNote(_db, note);

            return new();
        }

        /// <summary>
        /// Gets the export json.  Well it's called json here because it was intended to be used to export a
        /// file as json.  But the fact is this is a good way to grab every thing the file contains 
        /// relevant to the requesting user.  In here you have the file object, all headers including their 
        /// content object and tag objects.  Finally, the users access token.
        /// Grab this and you have all you need to display a file.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>JsonExport.</returns>
        [Authorize]
        public override async Task<JsonExport> GetExportJson(ExportRequest request, ServerCallContext context)
        {
            JsonExport stuff = new()
            {
                NoteFile = _db.NoteFile.Single(p => p.Id == request.FileId).GetGNotefile()
            };

            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, stuff.NoteFile.Id, 0);
            if (!na.ReadAccess)
                return new JsonExport();

            stuff.NoteAccess = na.GetGNoteAccess();

            NoteHeadersRequest request2 = new()
            {
                NoteFileId = request.FileId,
                ArcId = request.ArcId,
                NoteOrdinal = -1,
                ContentAndTags = true,
                NestResponses = request.NestResponses
            };
            if (request.NoteOrdinal > 0)
                request2.NoteOrdinal = request.NoteOrdinal;

            stuff.NoteHeaders = await GetNoteHeaders(request2, context);

            return stuff;
        }

        /// <summary>
        /// Gets text from server for display in client.
        /// files: about.html | help.html | helpdialog.html | helpdialog2.html | license.html
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        public override async Task<AString> GetTextFile(AString request, ServerCallContext context)
        {
            AString stuff = new()
            {
                Val = string.Empty
            };
            /*
                        if (request.Val == "syncfusionkey.rsghjjsrsrj43632353")
                        {
                            stuff.Val = _configuration["SyncfusionKey"];
                            return stuff;
                        }
            */
            string myFileInput = Globals.ImportRoot + "Text\\" + request.Val;
            // Get the input file
            StreamReader file;
            try
            {
                file = new StreamReader(myFileInput);
            }
            catch
            {
                return stuff;
            }

            StringBuilder sb = new();
            string? line;
            while ((line = await file.ReadLineAsync()) is not null)
            {
                sb.AppendLine(line);
            }

            stuff.Val = sb.ToString();

            return stuff;
        }

        /// <summary>
        /// Gets the current homepage message if any
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<AString> GetHomePageMessage(NoRequest request, ServerCallContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var message = new AString();

            NoteFile? hpmf = _db.NoteFile.Where(p => p.NoteFileName == "homepagemessages").FirstOrDefault();
            if (hpmf is not null)
            {
                NoteHeader? hpmh = _db.NoteHeader.Where(p => p.NoteFileId == hpmf.Id && !p.IsDeleted).OrderByDescending(p => p.CreateDate).FirstOrDefault();
                if (hpmh is not null)
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    message.Val = _db.NoteContent.Where(p => p.NoteHeaderId == hpmh.Id).FirstOrDefault().NoteBody;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                }
            }

            return message;
        }

        /// <summary>
        /// Get a list of notes w or wo content and tags as specified by the request.
        /// Returns the same stuff as a JsonExport but without the Notefile and Access token.
        /// Also permits filtering to a limted degree.
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        [Authorize]
        public override async Task<GNoteHeaderList> GetNoteHeaders(NoteHeadersRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, request.NoteFileId, request.ArcId);
            if (!na.ReadAccess)
                return new();

            List<NoteHeader> work = new();

            if (request.NoteOrdinal == -1 && request.MinNote > 0 && request.MaxNote >= request.MinNote)      // base notes and responses
            {
                work = await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId
                    && p.NoteOrdinal >= request.MinNote && p.NoteOrdinal <= request.MaxNote
                    && !p.IsDeleted && p.Version == 0)
                    .OrderBy(p => p.NoteOrdinal).ThenBy(p => p.ResponseOrdinal).ToListAsync();
            }
            else if (request.NoteOrdinal == -1) // base notes and responses
            {
                work = await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId
                    && !p.IsDeleted && p.Version == 0)
                    .OrderBy(p => p.NoteOrdinal).ThenBy(p => p.ResponseOrdinal).ToListAsync();
            }
            else if (request.NoteOrdinal == 0 && request.MinNote > 0 && request.MaxNote >= request.MinNote)  // base notes only
            {
                work = await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId && p.ResponseOrdinal == 0
                    && p.NoteOrdinal >= request.MinNote && p.NoteOrdinal <= request.MaxNote
                    && !p.IsDeleted && p.Version == 0)
                    .OrderBy(p => p.NoteOrdinal).ToListAsync();
            }
            else if (request.NoteOrdinal == 0)  // base notes only
            {
                work = await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId && p.ResponseOrdinal == 0
                    && !p.IsDeleted && p.Version == 0)
                    .OrderBy(p => p.NoteOrdinal).ToListAsync();
            }
            else if (request.ResponseOrdinal <= 0) // specifc base note plus all responses
            {
                work = await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId && p.NoteOrdinal == request.NoteOrdinal
                    && !p.IsDeleted && p.Version == 0)
                    .OrderBy(p => p.ResponseOrdinal).ToListAsync();
            }
            else if (request.ResponseOrdinal == 0) // specifc base note 
            {
                work = await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId
                    && p.ResponseOrdinal == 0 && p.NoteOrdinal == request.NoteOrdinal
                    && !p.IsDeleted && p.Version == 0).ToListAsync();
            }
            else    // specific response
            {
                work = await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId && p.ResponseOrdinal == request.ResponseOrdinal
                    && p.NoteOrdinal == request.NoteOrdinal
                    && !p.IsDeleted && p.Version == 0).ToListAsync();
            }

            GNoteHeaderList returnval = NoteHeader.GetGNoteHeaderList(work);

            if (request.ContentAndTags)
            {
                long[] items = work.Select(p => p.Id).ToArray();
                List<NoteContent> cont = await _db.NoteContent.Where(p => items.Contains(p.NoteHeaderId)).ToListAsync();
                List<Tags> tags = await (_db.Tags.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId)).ToListAsync();

                foreach (GNoteHeader item in returnval.List)
                {
                    item.Content = cont.Single(p => p.NoteHeaderId == item.Id).GetGNoteContent();
                    List<Tags> x = tags.Where(p => p.NoteHeaderId == item.Id).ToList();
                    item.Tags = Tags.GetGTagsList(x);
                }
            }

            if (request.NestResponses)
            {
                List<GNoteHeader> bases = returnval.List.Where(p => p.ResponseOrdinal == 0).ToList();
                foreach (GNoteHeader bn in bases)
                {
                    List<GNoteHeader> rn = returnval.List.Where(p => p.NoteOrdinal == bn.NoteOrdinal && p.ResponseOrdinal > 0).OrderBy(p => p.ResponseOrdinal).ToList();
                    if (rn.Count == 0)
                        continue;
                    bn.Responses = new();
                    bn.Responses.List.AddRange(rn);
                    foreach (GNoteHeader ln in rn)
                    {
                        returnval.List.Remove(ln);
                    }
                }
            }

            return returnval;
        }

        [Authorize]
        public override async Task<NoteCount> GetNoteCount(NoteFileRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, request.NoteFileId, request.ArcId);
            if (!na.ReadAccess)
                return new();

            NoteCount returnval = new()
            {
                Count = await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId && p.ResponseOrdinal == 0
                   && !p.IsDeleted && p.Version == 0).CountAsync()
            };

            return returnval;
        }

        [Authorize]
        public override async Task<SearchResult> ContentSearch(ContentSearchRequest request, ServerCallContext context)
        {
            //ApplicationUser appUser = await GetAppUser(context);

            List<NoteHeader> stuff = 
                await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId 
                && p.ArchiveId == request.ArcId 
                && !p.IsDeleted
                && p.Version == 0)
                .Include(s => s.NoteContent)
                .Where(s => s.NoteContent.NoteBody.ToLower().Contains(request.SearchText)
                && s.Id == s.NoteContent.NoteHeaderId
                ).ToListAsync();

            SearchResult result = new();

            foreach (NoteHeader stuffItem in stuff) {
                result.List.Add(stuffItem.GetGNoteHeader());
            }

            return result;
        }
    }
}