/*--------------------------------------------------------------------------
    **
    **  Copyright © 2026, Dale Sinder
    **
    **  Name: NotesService.cs
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

using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Notes.Client;
using Notes.Client.Comp;
using Notes.Data;
using Notes.Entities;
using Notes.Manager;
using Notes.Protos;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime;
using System.Security.Claims;
using System.Text;

/// <summary>
/// The Notes.Services namespace contains gRPC service implementations for managing notes, user access,
/// Email, and related operations in the notes application.
/// </summary>
namespace Notes.Services
{
    /// <summary>
    /// Provides gRPC service methods for managing notes, note files, user access, and related operations in the
    /// application. Supports authentication, authorization, and administrative features for note collaboration and user
    /// management.
    /// </summary>
    /// <remarks>This service implements the core server-side logic for the application's note management
    /// system, exposing endpoints for note creation, editing, access control, user administration, and related
    /// features. Most methods require the caller to be authenticated, and some are restricted to users with
    /// administrative privileges. The service integrates with ASP.NET Core Identity for user and role management, and
    /// uses dependency injection for configuration, logging, and email services. Thread safety and authorization are
    /// enforced according to .NET and gRPC best practices.</remarks>
    /// <param name="logger">The logger instance used to record diagnostic and operational information for the NotesService.</param>
    /// <param name="_db">The database context for accessing and manipulating note, user, and related data entities.</param>
    /// <param name="_configuration">The application configuration provider used to access settings such as JWT secrets and administrative contact
    /// information.</param>
    /// <param name="_roleManager">The role manager used to query and manage user roles within the application.</param>
    /// <param name="_emailSender">The email sender service used to send notification and system emails to users.</param>
    /// <param name="_userManager">The user manager responsible for user identity operations, including user lookup and role assignment.</param>
    public class NotesService(
        //ILogger<NotesService> logger,
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
            // logger.LogInformation("Received GetServerTime request for ID: {Id}", 1);
            DateTime now = DateTime.UtcNow;
            var response = new ServerTime()
            {
                UtcTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(now),
                OffsetHours = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalHours,
                OffsetMinutes = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Minutes % 60,
                Status = new RequestStatus()
                {
                    Success = true,
                    Status = 0,
                    Message = $"Server time: {DateTime.Now}"
                }
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
        public override async Task<RequestStatus> NoOp(NoRequest request, ServerCallContext context)
        {
            return new RequestStatus()
            {
                Success = true,
                Status = 0,
                Message = "No operation performed."
            };
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
                RolesList = new CheckedUserList(),
                Status = new RequestStatus()
                {
                    Success = true,
                    Status = 0,
                    Message = "User roles retrieved."
                }
            };
            string Id = request.Subject;
            ApplicationUser user = await _userManager.FindByIdAsync(Id);

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
        public override async Task<RequestStatus> UpdateUserRoles(EditUserViewModel model, ServerCallContext context)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(model.UserData.Id);
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

            return new RequestStatus()
                { Message = "User roles updated", Status = 0, Success = true };
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
            ApplicationUser? appUser = await _userManager.FindByIdAsync(user.FindFirst(ClaimTypes.NameIdentifier).Value);
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

            List<NoteFile> x = [.. _db.NoteFile.OrderBy(x => x.Id)];
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

            List<ApplicationUser> udl = [.. _db.Users];
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

            homepageModel.Status = new()
            {
                Success = true,
                Status = 0,
                Message = "Admin page model retrieved."
            };

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
#pragma warning disable IDE0060 // Remove unused parameter
        private async Task<HomePageModel> GetBaseHomePageModelAsync(NoRequest request, ServerCallContext context)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            HomePageModel homepageModel = new()
            {Status = new()
                { Success = true, Status = 0, Message = "Home page model retrieved." }
            };

            NoteFile? hpmf = _db.NoteFile.Where(p => p.NoteFileName == "homepagemessages").FirstOrDefault();
            if (hpmf is not null)
            {
                NoteHeader? hpmh = _db.NoteHeader.Where(p => p.NoteFileId == hpmf.Id
                    && !p.IsDeleted && p.Version == 0)
                    .OrderByDescending(p => p.Id).FirstOrDefault();
                if (hpmh is not null && !hpmh.IsDeleted)
                {
                    homepageModel.Message = _db.NoteContent.Where(p => p.NoteHeaderId == hpmh.Id).FirstOrDefault().NoteBody;
                }
            }

            if (context.GetHttpContext().User != null)
            {
                try
                {
                    ClaimsPrincipal user = context.GetHttpContext().User;
                    if (user.FindFirst(ClaimTypes.NameIdentifier) is not null && user.FindFirst(ClaimTypes.NameIdentifier).Value != null)
                    {
                        ApplicationUser appUser = await GetAppUser(context);
                        homepageModel.UserData = appUser.GetGAppUser();

                        List<NoteFile> allFiles = [.. _db.NoteFile.ToList().OrderBy(p => p.NoteFileTitle)];
                        List<NoteAccess> myAccesses = [.. _db.NoteAccess.Where(p => p.UserID == appUser.Id)];
                        List<NoteAccess> otherAccesses = [.. _db.NoteAccess.Where(p => p.UserID == Globals.AccessOtherId)];

                        List<NoteFile> myNoteFiles = [];

                        bool isAdmin = await _userManager.IsInRoleAsync(appUser, UserRoles.Admin);
                        foreach (NoteFile file in allFiles)
                        {
                            NoteAccess? x = myAccesses.SingleOrDefault(p => p.NoteFileId == file.Id);
                            x ??= otherAccesses.Single(p => p.NoteFileId == file.Id);

                            if (isAdmin || x.ReadAccess || x.Write || x.ViewAccess)
                            {
                                myNoteFiles.Add(file);
                            }
                        }

                        homepageModel.NoteFiles = NoteFile.GetGNotefileList(myNoteFiles);
                    }
                }
                catch (Exception ex)
                {
                    homepageModel.Status = new()
                    {
                        Success = false,
                        Status = -1,
                        Message = ex.Message
                    };
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

        [Authorize]
        public override async Task<GNotefile> ClearNoteFilePolicy(GNotefile noteFile, ServerCallContext context)
        {
            NoteFile nf = await NoteDataManager.GetFileById(_db, noteFile.Id);
            nf.PolicyId = 0;
            _db.NoteFile.Update(nf);
            await _db.SaveChangesAsync();
            return noteFile;
        }

        [Authorize]
        public override async Task<GNotefile> SetNoteFilePolicy(GNotefile noteFile, ServerCallContext context)
        {
            NoteFile nf = await NoteDataManager.GetFileById(_db, noteFile.Id);
            nf.PolicyId = noteFile.PolicyId;
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
        public override async Task<RequestStatus> DeleteNoteFile(GNotefile noteFile, ServerCallContext context)
        {
            NoteFile nf = await NoteDataManager.GetFileById(_db, noteFile.Id);

            // remove tags
            List<Tags> tl = [.. _db.Tags.Where(p => p.NoteFileId == nf.Id)];
            _db.Tags.RemoveRange(tl);

            // remove content
            List<NoteHeader> hl = [.. _db.NoteHeader.Where(p => p.NoteFileId == nf.Id)];
            foreach (NoteHeader h in hl)
            {
                NoteContent c = _db.NoteContent.Single(p => p.NoteHeaderId == h.Id);
                _db.NoteContent.Remove(c);
            }

            // remove headers
            _db.NoteHeader.RemoveRange(hl);

            // remove access
            List<NoteAccess> al = [.. _db.NoteAccess.Where(p => p.NoteFileId == nf.Id)];
            _db.NoteAccess.RemoveRange(al);

            // remove file
            _db.NoteFile.Remove(nf);
            await _db.SaveChangesAsync();
            return new RequestStatus()
                { Message = "Note file deleted", Status = 0, Success = true };
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
        public override async Task<RequestStatus> Import(ImportRequest request, ServerCallContext context)
        {
            MemoryStream? input = new ([.. request.Payload]);
            StreamReader file = new (input);

            Importer? imp = new();
            RequestStatus result = await imp.Import(_db, file, request.NoteFile);

            file.DiscardBufferedData();
            file.Dispose();
            input.Dispose();
            GC.Collect();
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
            return result;
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
            NoteDisplayIndexModel idxModel = new()
            { Status = new() { Success = false } };
            bool isAdmin;
            bool isUser;

            int arcId = request.ArcId;

            user = context.GetHttpContext().User;
            try
            {
                if (user.FindFirst(ClaimTypes.NameIdentifier) is not null 
                    && user.FindFirst(ClaimTypes.NameIdentifier).Value is not null)
                {
                    try
                    {
                        appUser = await GetAppUser(context);

                        isAdmin = await _userManager.IsInRoleAsync(appUser, "Admin");
                        isUser = await _userManager.IsInRoleAsync(appUser, "User");
                        if (!isUser)
                        {
                            idxModel.Status.Message = idxModel.Message = "You are not authorized to access notes.";
                            
                            return idxModel;    // not a User?  You get NOTHING!
                        }

                        NoteAccess noteAccess = await AccessManager.GetAccess(_db, appUser.Id, request.NoteFileId, arcId);
                        if (noteAccess is null)
                        {
                            idxModel.Status.Message = idxModel.Message = "File does not exist";
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
                            idxModel.Status.Message = idxModel.Message = "You do not have access to file " + idxModel.NoteFile.NoteFileName;
                            return idxModel;
                        }

                        List<LinkedFile> linklist = await _db.LinkedFile
                            .Where(p => p.HomeFileId == request.NoteFileId)
                            .ToListAsync();
                        if (linklist is not null && linklist.Count > 0)
                            idxModel.LinkedText = " (Linked)";

                        List<NoteHeader> allhead = await _db.NoteHeader
                            .Where(p => p.NoteFileId == request.NoteFileId 
                                && p.ArchiveId == arcId
                                && p.Id != idxModel.NoteFile.PolicyId
                                ).ToListAsync(); // await NoteDataManager.GetAllHeaders(_db, request.NoteFileId, arcId);
                        idxModel.AllNotes = NoteHeader.GetGNoteHeaderList(allhead);

                        List<NoteHeader> notes = [.. allhead.FindAll(p => p.ResponseOrdinal == 0).OrderBy(p => p.NoteOrdinal)];
                        idxModel.Notes = NoteHeader.GetGNoteHeaderList(notes);

                        idxModel.UserData = appUser.GetGAppUser();

                        List<Tags> tags = await _db.Tags
                            .Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == arcId)
                            .ToListAsync();
                        idxModel.Tags = Tags.GetGTagsList(tags);

                        idxModel.ArcId = arcId;

                        if (idxModel.NoteFile.PolicyId > 0)
                        {
                            idxModel.Policy = notes.FirstOrDefault(p => p.Id == idxModel.NoteFile.PolicyId)?.GetGNoteHeader();
                        }
                    }
                    catch (Exception ex1)
                    {
                        idxModel.Status.Message = idxModel.Message = ex1.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                idxModel.Status.Message = idxModel.Message = ex.Message;
            }

            idxModel.Status.Success = true;
            idxModel.Status.Message = "Note file index data retrieved.";

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

            NoteHeader nh = await _db.NoteHeader
                .SingleAsync(p => p.Id == request.NoteId && p.Version == request.Vers);
            NoteContent c = await _db.NoteContent
                .SingleAsync(p => p.NoteHeaderId == nh.Id);
            List<Tags> tags = await _db.Tags
                .Where(p => p.NoteHeaderId == nh.Id)
                .ToListAsync();
            NoteFile nf = await _db.NoteFile
                .SingleAsync(p => p.Id == nh.NoteFileId);
            NoteAccess access = await AccessManager.GetAccess(_db, appUser.Id, nh.NoteFileId, nh.ArchiveId);

            bool canEdit = isAdmin;         // admins can always edit a note
            if (appUser.Id == nh.AuthorID)  // otherwise only the author can edit
                canEdit = true;

            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, nh.NoteFileId, nh.ArchiveId);

            if (!na.ReadAccess)
                return new DisplayModel()
                { Status = new() { Message = "You do not have read access", Success = false } };

            DisplayModel model = new()
            {
                Header = nh.GetGNoteHeader(),
                Content = c.GetGNoteContent(),
                Tags = Tags.GetGTagsList(tags),
                NoteFile = nf.GetGNotefile(),
                Access = access.GetGNoteAccess(),
                CanEdit = canEdit,
                IsAdmin = isAdmin,
                Status = new()
                { Message = "Note content retrieved", Success = true }
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
                AccessList = NoteAccess.GetGNoteAccessList([.. _db.NoteAccess
                    .Where(p => p.NoteFileId == request.FileId && p.ArchiveId == request.ArcId)]),
                AppUsers = ApplicationUser.GetGAppUserList([.. (await _userManager.GetUsersInRoleAsync("User"))]),
                UserAccess = (await AccessManager.GetAccess(_db, request.UserId, request.FileId, request.ArcId))
                    .GetGNoteAccess()
            };

            return accessAndUserList;
        }

        /// <summary>
        /// Updates the access permissions for a note file if the current user has edit access.
        /// </summary>
        /// <remarks>This method requires the caller to be authorized. The update is performed only if the
        /// current user has edit access to the specified note file. No changes are made if edit access is not
        /// granted.</remarks>
        /// <param name="request">The access item containing the updated permissions and identifiers for the note file.</param>
        /// <param name="context">The server call context that provides information about the current gRPC request and user.</param>
        /// <returns>The updated access item reflecting the requested changes. If the user does not have edit access, the item is
        /// returned unchanged.</returns>
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
        /// Deletes the specified access item for a note if the current user has edit permissions.
        /// </summary>
        /// <remarks>The access item is only deleted if the current user has edit access to the note. If
        /// the user does not have sufficient permissions, no changes are made.</remarks>
        /// <param name="request">The access item to delete, containing information about the note and associated access rights.</param>
        /// <param name="context">The server call context that provides information about the current request and user.</param>
        /// <returns>A NoRequest object indicating that the operation has completed.</returns>
        [Authorize]
        public override async Task<RequestStatus> DeleteAccessItem(GNoteAccess request, ServerCallContext context)
        {
            NoteAccess access = NoteAccess.GetNoteAccess(request);
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, access.NoteFileId, access.ArchiveId);
            if (na.EditAccess)
            {
                _db.NoteAccess.Remove(access);
                await _db.SaveChangesAsync();
                return new RequestStatus()
                { Message = "Access item deleted", Status = 0, Success = true };

            }

            return new RequestStatus()
                { Message = "Permission denied", Status = 0, Success = false };
        }

        /// <summary>
        /// Adds a new access item for a note if the current user has edit permissions.
        /// </summary>
        /// <remarks>This method requires the caller to have edit access to the specified note. If edit
        /// access is not granted, no changes are made to the database and the request is returned unchanged.</remarks>
        /// <param name="request">The access item to add, containing note and archive identifiers and access details.</param>
        /// <param name="context">The server call context that provides information about the current request and user.</param>
        /// <returns>The access item that was requested to be added. If the user does not have edit access, the item is not
        /// added.</returns>
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
        /// Retrieves user data for the authenticated user associated with the current server call context.
        /// </summary>
        /// <remarks>This method requires the caller to be authenticated. If the caller is not
        /// authenticated, an authorization error will be returned.</remarks>
        /// <param name="request">A request object containing no parameters. This value is ignored.</param>
        /// <param name="context">The context for the current server call, providing access to user identity and request metadata.</param>
        /// <returns>A <see cref="GAppUser"/> instance containing information about the authenticated user.</returns>
        [Authorize]
        public override async Task<GAppUser> GetUserData(NoRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            return appUser.GetGAppUser();
        }

        /// <summary>
        /// Updates the user data for the authenticated user based on the provided information.
        /// </summary>
        /// <remarks>This method only allows users to update their own data. Attempts to update data for
        /// other users will be ignored. The operation requires authentication and is subject to authorization
        /// policies.</remarks>
        /// <param name="request">An object containing the updated user data. Only fields corresponding to the authenticated user will be
        /// applied.</param>
        /// <param name="context">The server call context that provides information about the current request and authenticated user.</param>
        /// <returns>A <see cref="GAppUser"/> object containing the user data after the update operation. If the request does not
        /// pertain to the authenticated user, returns the original request object without changes.</returns>
        [Authorize]
        public override async Task<GAppUser> UpdateUserData(GAppUser request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            if (appUser.Id != request.Id)   // can only update self
                return request;

            ApplicationUser? appUserBase = await _userManager.FindByIdAsync(request.Id);
            ApplicationUser merged = ApplicationUser.MergeApplicationUser(request, appUserBase);

            await _userManager.UpdateAsync(merged);

            return request;
        }

        /// <summary>
        /// Retrieves a list of all available versions of a note that match the specified criteria.
        /// </summary>
        /// <remarks>The caller must have read access to the specified note file and archive to receive
        /// results. Only note versions with a non-zero version number are included in the response.</remarks>
        /// <param name="request">The request containing the identifiers and ordinals used to filter note versions. Must specify valid values
        /// for FileId, ArcId, NoteOrdinal, and ResponseOrdinal.</param>
        /// <param name="context">The server call context for the current gRPC request. Used to identify the authenticated user.</param>
        /// <returns>A GNoteHeaderList containing the headers of all note versions that match the request parameters. Returns an
        /// empty list if the user does not have read access or if no matching versions are found.</returns>
        [Authorize]
        public override async Task<GNoteHeaderList> GetVersions(GetVersionsRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, request.FileId, request.ArcId);
            if (!na.ReadAccess)
                return new GNoteHeaderList();

            List<NoteHeader> hl;

            hl = [.. _db.NoteHeader
                    .Where(p => p.NoteFileId == request.FileId && p.Version != 0
                        && p.NoteOrdinal == request.NoteOrdinal 
                        && p.ResponseOrdinal == request.ResponseOrdinal 
                        && p.ArchiveId == request.ArcId)
                    .OrderBy(p => p.Version)];

            return NoteHeader.GetGNoteHeaderList(hl);
        }

        /// <summary>
        /// Retrieves a list of sequencers owned by the current user for which read access is granted.
        /// </summary>
        /// <remarks>Only sequencers for which the user currently has read access are included in the
        /// returned list. Sequencers are ordered by their ordinal value.</remarks>
        /// <param name="request">A request object containing no parameters. This value is ignored.</param>
        /// <param name="context">The server call context that provides information about the current RPC call, including user authentication
        /// details.</param>
        /// <returns>A <see cref="GSequencerList"/> containing sequencers owned by the user and accessible for reading. The list
        /// will be empty if no accessible sequencers are found.</returns>
        [Authorize]
        public override async Task<GSequencerList> GetSequencer(NoRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            // My list
            List<Sequencer> mine = await _db.Sequencer
                .Where(p => p.UserId == appUser.Id)
                .OrderBy(p => p.Ordinal)
                .ThenBy(p => p.LastTime)
                .ToListAsync();

            mine ??= [];

            List<Sequencer> avail = [];

            foreach (Sequencer m in mine)
            {
                NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, m.NoteFileId, 0);
                if (na.ReadAccess)
                    avail.Add(m);   // ONLY if you have current read access!!
            }
            var ret = Sequencer.GetGSequencerList([.. avail.OrderBy(p => p.Ordinal)]);
            return ret;
        }

        /// <summary>
        /// Creates a new sequencer entry for the authenticated user using the specified check model.
        /// </summary>
        /// <remarks>This method requires the caller to be authenticated. The new sequencer entry is
        /// created with an incremented ordinal value and is marked as active. The entry is associated with the note
        /// file specified in <paramref name="request"/> and the current user.</remarks>
        /// <param name="request">The check model containing information used to initialize the sequencer entry. The <see
        /// cref="SCheckModel.FileId"/> property must specify the note file to associate with the sequencer.</param>
        /// <param name="context">The server call context that provides information about the current gRPC call, including authentication
        /// details.</param>
        /// <returns>A <see cref="NoRequest"/> instance indicating that the operation completed successfully.</returns>
        [Authorize]
        public override async Task<RequestStatus> CreateSequencer(SCheckModel request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            List<Sequencer> mine = await _db.Sequencer
                .Where(p => p.UserId == appUser.Id)
                .OrderByDescending(p => p.Ordinal)
                .ToListAsync();

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

            return new RequestStatus()
                { Message = "Sequencer created", Status = 0, Success = true };
        }

        /// <summary>
        /// Deletes the sequencer associated with the specified note file for the current user.
        /// </summary>
        /// <remarks>This method requires the caller to be authorized. If the sequencer does not exist for
        /// the given note file and user, no changes are made.</remarks>
        /// <param name="request">The model containing the identifier of the note file whose sequencer is to be deleted.</param>
        /// <param name="context">The server call context for the current request, used to identify the authenticated user.</param>
        /// <returns>A <see cref="NoRequest"/> instance indicating the completion of the operation. If no sequencer is found for
        /// the specified note file and user, the operation completes without error.</returns>
        [Authorize]
        public override async Task<RequestStatus> DeleteSequencer(SCheckModel request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            Sequencer mine = await _db.Sequencer.SingleOrDefaultAsync(p => p.UserId == appUser.Id && p.NoteFileId == request.FileId);
            if (mine is null)
                return new RequestStatus() 
                    { Message = "Sequencer not found", Status = 0, Success = false };

            _db.Sequencer.Remove(mine);
            await _db.SaveChangesAsync();

            return new RequestStatus() 
                { Message = "Sequencer deleted", Status = 0, Success = true };
        }

        /// <summary>
        /// Updates the ordinal and last modification time of a sequencer for the specified user and note file.
        /// </summary>
        /// <remarks>Requires authorization. The sequencer is identified by the combination of user ID and
        /// note file ID provided in the request. If no matching sequencer is found, an exception will be
        /// thrown.</remarks>
        /// <param name="request">The sequencer update request containing the user ID, note file ID, new ordinal, and last modification time.</param>
        /// <param name="context">The server call context for the current gRPC request.</param>
        /// <returns>A <see cref="NoRequest"/> instance indicating that the update operation has completed.</returns>
        [Authorize]
        public override async Task<RequestStatus> UpdateSequencerOrdinal(GSequencer request, ServerCallContext context)
        {
            Sequencer modified = await _db.Sequencer
                .SingleAsync(p => p.UserId == request.UserId && p.NoteFileId == request.NoteFileId);

            modified.LastTime = request.LastTime.ToDateTime();
            modified.Ordinal = request.Ordinal;

            _db.Entry(modified).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return new RequestStatus() 
                { Message = "Sequencer ordinal updated", Status = 0, Success = true };
        }

        /// <summary>
        /// Updates the sequencer state for the specified user and note file, setting its active status and relevant
        /// timestamps.
        /// </summary>
        /// <remarks>If the sequencer is activated, its start time is set to the current UTC time. If
        /// deactivated, the last time is updated to match the previous start time. This method requires authorization
        /// and persists changes to the database.</remarks>
        /// <param name="request">The sequencer update request containing the user ID, note file ID, and the desired active status.</param>
        /// <param name="context">The server call context for the current gRPC request.</param>
        /// <returns>A NoRequest instance indicating that the operation completed successfully and no additional data is
        /// returned.</returns>
        [Authorize]
        public override async Task<RequestStatus> UpdateSequencer(GSequencer request, ServerCallContext context)
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

            return new RequestStatus() 
                { Message = "Sequencer updated", Status = 0, Success = true };
        }

        /// <summary>
        /// Retrieves the note file specified by the request if the current user has appropriate access permissions.
        /// </summary>
        /// <remarks>Access is determined based on the user's permissions for the specified note file. The
        /// returned object will be empty if the user does not have read, write, edit, or respond access.</remarks>
        /// <param name="request">The request containing the identifier of the note file to retrieve.</param>
        /// <param name="context">The server call context for the current gRPC request, used to identify and authorize the user.</param>
        /// <returns>A <see cref="GNotefile"/> representing the requested note file if access is granted; otherwise, an empty
        /// <see cref="GNotefile"/>.</returns>
        [Authorize]
        public override async Task<GNotefile> GetNoteFile(NoteFileRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            if (appUser is null)
                return new GNotefile();

            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, request.NoteFileId, 0);
            if (na is null || !(na.Write || na.ReadAccess || na.EditAccess || na.Respond))      // TODO is this right??
                return new GNotefile();

            NoteFile? nf = await _db.NoteFile
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == request.NoteFileId);

            if (nf is null)
                return new GNotefile();

            return nf.GetGNotefile();
        }

        /// <summary>
        /// Creates a new note or response in the system using the provided text and subject information.
        /// </summary>
        /// <remarks>The caller must have write or respond access to the specified note file and be
        /// assigned the 'User' role. If these conditions are not met, the method returns an empty note header. This
        /// method is protected by authorization and should be called by authenticated users only.</remarks>
        /// <param name="tvm">The text view model containing the note content, subject, and related metadata. Must not have a null value
        /// for MyNote or MySubject.</param>
        /// <param name="context">The server call context associated with the current request, used to identify and authorize the user.</param>
        /// <returns>A GNoteHeader representing the newly created note or response. Returns an empty GNoteHeader if the user is
        /// not authorized or required information is missing.</returns>
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
        /// Updates an existing note with new content and subject information if the caller is authorized to edit the
        /// note.
        /// </summary>
        /// <remarks>Only the note's author or users with the 'Admin' role are permitted to update a note.
        /// If the caller lacks permission or required fields are null, the method returns an empty result.</remarks>
        /// <param name="tvm">A model containing the updated note content, subject, and related metadata to apply to the note.</param>
        /// <param name="context">The server call context that provides information about the current gRPC request and caller identity.</param>
        /// <returns>A GNoteHeader representing the updated note if the operation succeeds; otherwise, an empty GNoteHeader if
        /// the caller is not authorized or required data is missing.</returns>
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
        /// Retrieves the header information for the specified note identifier, subject to access permissions.
        /// </summary>
        /// <remarks>If the caller is not an administrator and lacks read access to the note, the method returns an empty
        /// header object. Administrators can retrieve headers for any note regardless of access restrictions.</remarks>
        /// <param name="request">An object containing the unique identifier of the note for which the header is requested.</param>
        /// <param name="context">The server call context associated with the current request, providing user and request metadata.</param>
        /// <returns>A <see cref="GNoteHeader"/> representing the header of the requested note. If the caller does not have read access,
        /// an empty <see cref="GNoteHeader"/> is returned.</returns>
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
        public override async Task<RequestStatus> SendEmailAuth(GEmail request, ServerCallContext context)
        {
            await _emailSender.SendEmailAsync(request.Address, request.Subject, request.Body);
            return new RequestStatus() { Success = true };
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
        /// Sends a note email to the current user if they have read access to the specified file and archive.
        /// </summary>
        /// <remarks>If the user does not have read access to the specified file and archive, no email is
        /// sent and a default response is returned. This method requires the caller to be authorized.</remarks>
        /// <param name="fv">The view model containing information about the note, including file and archive identifiers, note content,
        /// and subject.</param>
        /// <param name="context">The server call context that provides information about the current gRPC request and user.</param>
        /// <returns>A <see cref="NoRequest"/> instance indicating the completion of the operation.</returns>
        [Authorize]
        public override async Task<RequestStatus> DoForward(ForwardViewModel fv, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, fv.FileID, fv.ArcID);
            if (!na.ReadAccess)
                return new RequestStatus() { Success = false, Message = "No read access" };
            string myEmail = await LocalService.MakeNoteForEmail(fv, fv.NoteFile, _db, appUser.Email, appUser.DisplayName);
            await _emailSender.SendEmailAsync(appUser.Email, fv.NoteSubject, myEmail);
            return new RequestStatus() { Success = true, Message = "Email sent" };
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
        /// Copies a note or an entire note thread to a specified target file for the current user, preserving content
        /// and associated tags.
        /// </summary>
        /// <remarks>The method requires the caller to have read access to the source note and write
        /// access to the target file. If copying the entire note thread, all responses associated with the note are
        /// also copied. The copied note will be attributed to the current user. This operation is performed
        /// asynchronously.</remarks>
        /// <param name="Model">The model containing information about the note to copy, the target file ID, and whether to copy the entire
        /// note thread. Must not be null.</param>
        /// <param name="context">The server call context for the current request, used to identify and authorize the user.</param>
        /// <returns>A <see cref="NoRequest"/> object indicating the completion of the copy operation. Returns an empty response
        /// if the user does not have read access to the source note or write access to the target file.</returns>
        [Authorize]
        public override async Task<RequestStatus> CopyNote(CopyModel Model, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, Model.Note.NoteFileId, Model.Note.ArchiveId);
            if (!na.ReadAccess)
                return new RequestStatus() { Success = false, Message = "No read access" };         // can not read file

            int fileId = Model.FileId;

            // Can I write to the target file?
            string uid = appUser.Id;
            NoteAccess myAccess = await AccessManager.GetAccess(_db, uid, fileId, 0);
            if (!myAccess.Write)
                return new RequestStatus() { Success = false, Message = "No write access" };

            // Prepare to copy
            NoteHeader Header = NoteHeader.GetNoteHeader(Model.Note);
            bool whole = Model.WholeString;
            NoteFile noteFile = await _db.NoteFile
                .SingleAsync(p => p.Id == fileId);

            // Just the note
            if (!whole)
            {
                NoteContent cont = await _db.NoteContent
                    .SingleAsync(p => p.NoteHeaderId == Header.Id);
                //cont.NoteHeader = null;
                List<Tags> tags = await _db.Tags
                    .Where(p => p.NoteHeaderId == Header.Id)
                    .ToListAsync();

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
                _ = await NoteDataManager.CreateNote(_db, Header, Body, Tags.ListToString(tags), Header.DirectorMessage, true, false);
            }
            else    // whole note string
            {
                // get base note first
                NoteHeader BaseHeader;
                BaseHeader = await _db.NoteHeader
                    .SingleAsync(p => p.NoteFileId == Header.NoteFileId
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
                NoteHeader NewHeader = await NoteDataManager
                    .CreateNote(_db, Header, Body, Tags.ListToString(tags), Header.DirectorMessage, true, false);

                // now deal with any responses
                for (int i = 1; i <= BaseHeader.ResponseCount; i++)
                {
                    NoteHeader RHeader = await _db.NoteHeader
                        .SingleAsync(p => p.NoteFileId == BaseHeader.NoteFileId
                            && p.ArchiveId == BaseHeader.ArchiveId
                            && p.NoteOrdinal == BaseHeader.NoteOrdinal
                            && p.ResponseOrdinal == i);

                    Header = RHeader.CloneForLinkR();

                    cont = await _db.NoteContent
                        .SingleAsync(p => p.NoteHeaderId == Header.Id);
                    tags = await _db.Tags
                        .Where(p => p.NoteHeaderId == Header.Id)
                        .ToListAsync();

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
                    _ = await NoteDataManager.CreateResponse(_db, Header, Body, Tags.ListToString(tags), Header.DirectorMessage, true, false);
                }
            }
            return new RequestStatus() { Success = true  };
        }

        // Utility method - makes a viewable header for the copied note
        /// <summary>
        /// Generates an HTML header string for a copied note, including the note file name, subject, author, and
        /// creation date.
        /// </summary>
        /// <remarks>The returned string includes the note file name, subject, author, and creation date,
        /// separated by hyphens and wrapped in a <div> element with the class 'copiednote'. The method does not perform
        /// HTML encoding; ensure that input values are safe for HTML output if used in untrusted contexts.</remarks>
        /// <param name="header">The header information for the note, including subject, author, and creation date. Cannot be null.</param>
        /// <param name="noteFile">The note file containing the note's file name. Cannot be null.</param>
        /// <returns>A formatted HTML string representing the note header, suitable for display in a web view.</returns>
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
        /// Deletes the specified note if the current user has delete permissions.
        /// </summary>
        /// <remarks>If the user does not have permission to delete the note, the operation completes
        /// without deleting the note and without throwing an exception.</remarks>
        /// <param name="request">An object containing the identifier of the note to delete.</param>
        /// <param name="context">The server call context for the current request, providing user and request metadata.</param>
        /// <returns>A <see cref="NoRequest"/> object indicating the completion of the delete operation.</returns>
        [Authorize]
        public override async Task<RequestStatus> DeleteNote(NoteId request, ServerCallContext context)
        {
            NoteHeader note = await NoteDataManager.GetNoteByIdWithFile(_db, request.Id);

            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, note.NoteFileId, note.ArchiveId);
            if (!na.DeleteEdit)
            {
                return new() { Success = false, Message = "No delete access" };
            }

            await NoteDataManager.DeleteNote(_db, note);

            return new() { Success = true, Message = "Note deleted" };
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
        /// Retrieves the current home page message to be displayed to users.
        /// </summary>
        /// <param name="request">A request object containing no parameters. This value is ignored.</param>
        /// <param name="context">The context for the server-side call, providing information about the RPC environment.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="AString"/> with
        /// the home page message text. If no message is available, the value will be empty.</returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<AString> GetHomePageMessage(NoRequest request, ServerCallContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var message = new AString();

            NoteFile? hpmf = _db.NoteFile.Where(p => p.NoteFileName == "homepagemessages")
                .FirstOrDefault();
            if (hpmf is not null)
            {
                NoteHeader? hpmh = _db.NoteHeader
                    .Where(p => p.NoteFileId == hpmf.Id && !p.IsDeleted)
                    .OrderByDescending(p => p.CreateDate)
                    .FirstOrDefault();
                if (hpmh is not null)
                {
                    message.Val = _db.NoteContent
                        .Where(p => p.NoteHeaderId == hpmh.Id)
                        .FirstOrDefault()
                        .NoteBody;
                }
            }

            return message;
        }

        /// <summary>
        /// Retrieves a list of note headers for the specified note file and archive, based on the criteria provided in
        /// the request.
        /// </summary>
        /// <remarks>The returned list may include base notes, responses, or both, depending on the values
        /// specified in the request. If the ContentAndTags option is set, each note header will include its associated
        /// content and tags. If the NestResponses option is set, responses will be nested under their corresponding
        /// base notes. The method requires the caller to have read access to the specified note file and
        /// archive.</remarks>
        /// <param name="request">An object containing the criteria for selecting note headers, including note file ID, archive ID, note
        /// ordinal, response ordinal, and additional options such as whether to include content, tags, or nested
        /// responses. Cannot be null.</param>
        /// <param name="context">The context for the server call, providing user and request metadata. Cannot be null.</param>
        /// <returns>A list of note headers matching the specified criteria. If the user does not have read access, the list will
        /// be empty.</returns>
        [Authorize]
        public override async Task<GNoteHeaderList> GetNoteHeaders(NoteHeadersRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, request.NoteFileId, request.ArcId);
            if (!na.ReadAccess)
                return new();

            List<NoteHeader> work = [];

            if (request.NoteOrdinal == -1 && request.MinNote > 0 && request.MaxNote >= request.MinNote)      // base notes and responses
            {
                work = await _db.NoteHeader
                    .Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId
                        && p.NoteOrdinal >= request.MinNote && p.NoteOrdinal <= request.MaxNote
                        && !p.IsDeleted && p.Version == 0)
                    .OrderBy(p => p.NoteOrdinal)
                    .ThenBy(p => p.ResponseOrdinal)
                    .ToListAsync();
            }
            else if (request.NoteOrdinal == -1) // base notes and responses
            {
                work = await _db.NoteHeader
                    .Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId
                        && !p.IsDeleted && p.Version == 0)
                    .OrderBy(p => p.NoteOrdinal)
                    .ThenBy(p => p.ResponseOrdinal)
                    .ToListAsync();
            }
            else if (request.NoteOrdinal == 0 && request.MinNote > 0 && request.MaxNote >= request.MinNote)  // base notes only
            {
                work = await _db.NoteHeader
                    .Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId && p.ResponseOrdinal == 0
                        && p.NoteOrdinal >= request.MinNote && p.NoteOrdinal <= request.MaxNote
                        && !p.IsDeleted && p.Version == 0)
                    .OrderBy(p => p.NoteOrdinal)
                    .ToListAsync();
            }
            else if (request.NoteOrdinal == 0)  // base notes only
            {
                work = await _db.NoteHeader
                    .Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId && p.ResponseOrdinal == 0
                        && !p.IsDeleted && p.Version == 0)
                    .OrderBy(p => p.NoteOrdinal)
                    .ToListAsync();
            }
            else if (request.ResponseOrdinal <= 0) // specifc base note plus all responses
            {
                work = await _db.NoteHeader
                    .Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId && p.NoteOrdinal == request.NoteOrdinal
                        && !p.IsDeleted && p.Version == 0)
                    .OrderBy(p => p.ResponseOrdinal)
                    .ToListAsync();
            }
            else if (request.ResponseOrdinal == 0) // specifc base note 
            {
                work = await _db.NoteHeader
                    .Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId
                        && p.ResponseOrdinal == 0 && p.NoteOrdinal == request.NoteOrdinal
                        && !p.IsDeleted && p.Version == 0)
                    .ToListAsync();
            }
            else    // specific response
            {
                work = await _db.NoteHeader
                    .Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId && p.ResponseOrdinal == request.ResponseOrdinal
                        && p.NoteOrdinal == request.NoteOrdinal
                        && !p.IsDeleted && p.Version == 0)
                    .ToListAsync();
            }

            GNoteHeaderList returnval = NoteHeader.GetGNoteHeaderList(work);

            if (request.ContentAndTags)
            {
                long[] items = [.. work.Select(p => p.Id)];
                List<NoteContent> cont = await _db.NoteContent
                    .Where(p => items.Contains(p.NoteHeaderId))
                    .ToListAsync();

                List<Tags> tags = await (_db.Tags
                    .Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId))
                    .ToListAsync();

                foreach (GNoteHeader item in returnval.List)
                {
                    item.Content = cont.Single(p => p.NoteHeaderId == item.Id).GetGNoteContent();
                    List<Tags> x = [.. tags.Where(p => p.NoteHeaderId == item.Id)];
                    item.Tags = Tags.GetGTagsList(x);
                }
            }

            if (request.NestResponses)
            {
                List<GNoteHeader> bases = [.. returnval.List.Where(p => p.ResponseOrdinal == 0)];
                foreach (GNoteHeader bn in bases)
                {
                    List<GNoteHeader> rn = [.. returnval.List
                        .Where(p => p.NoteOrdinal == bn.NoteOrdinal && p.ResponseOrdinal > 0)
                        .OrderBy(p => p.ResponseOrdinal)];
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

        /// <summary>
        /// Retrieves the number of notes in the specified note file and archive that are not deleted and are not
        /// responses.
        /// </summary>
        /// <remarks>Only notes that are not deleted, are not responses, and are in the initial version
        /// are included in the count. The caller must have read access to the specified note file and archive;
        /// otherwise, the result will indicate a count of zero.</remarks>
        /// <param name="request">The request containing the identifiers of the note file and archive for which to count notes.</param>
        /// <param name="context">The context for the server call, which provides user and request information.</param>
        /// <returns>A <see cref="NoteCount"/> object containing the count of notes matching the specified criteria. If the user
        /// does not have read access, the count will be zero.</returns>
        [Authorize]
        public override async Task<NoteCount> GetNoteCount(NoteFileRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, request.NoteFileId, request.ArcId);
            if (!na.ReadAccess)
                return new();

            NoteCount returnval = new()
            {
                Count = await _db.NoteHeader
                    .Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId && p.ResponseOrdinal == 0
                       && !p.IsDeleted && p.Version == 0)
                    .CountAsync()
            };

            return returnval;
        }

        /// <summary>
        /// Searches note content for matches to the specified search criteria and returns the results.
        /// </summary>
        /// <remarks>The search can be performed with case sensitivity and/or whole word matching,
        /// depending on the values specified in the request. Only notes that are not deleted and have a version of 0
        /// are included in the search results.</remarks>
        /// <param name="request">The search parameters, including the search text, case sensitivity, whole word matching, and identifiers for
        /// the note file and archive. Cannot be null.</param>
        /// <param name="context">The server call context for the current gRPC request. Provides information about the call and its
        /// environment.</param>
        /// <returns>A SearchResult containing a list of note headers that match the search criteria. Returns an empty result if
        /// no matches are found or if the search text is null or empty.</returns>
        [Authorize]
        public override async Task<SearchResult> ContentSearch(ContentSearchRequest request, ServerCallContext context)
        {
            if (request.SearchText is null || request.SearchText.Trim() == string.Empty)
                return new SearchResult();

            if (request.CaseSensitive)
            {
                if (request.WholeWords)
                {
                    request.SearchText = " " + request.SearchText.Trim() + " ";
                    List<NoteHeader> stuff =
                        await _db.NoteHeader
                        .Where(p => p.NoteFileId == request.NoteFileId
                            && p.ArchiveId == request.ArcId
                            && !p.IsDeleted
                            && p.Version == 0)
                        .Include(s => s.NoteContent)
                        .Where(s => s.NoteContent.NoteBody.Contains(request.SearchText)
                            && s.Id == s.NoteContent.NoteHeaderId)
                        .ToListAsync();

                    SearchResult result = new();

                    foreach (NoteHeader stuffItem in stuff)
                    {
                        result.List.Add(stuffItem.GetGNoteHeader());
                    }

                    return result;
                }
                else
                {
                    // case sensitive search
                    List<NoteHeader> stuff =
                        await _db.NoteHeader
                        .Where(p => p.NoteFileId == request.NoteFileId
                            && p.ArchiveId == request.ArcId
                            && !p.IsDeleted
                            && p.Version == 0)
                        .Include(s => s.NoteContent)
                        .Where(s => s.NoteContent.NoteBody.Contains(request.SearchText)
                            && s.Id == s.NoteContent.NoteHeaderId)
                        .ToListAsync();
                    SearchResult result = new();
                    foreach (NoteHeader stuffItem in stuff)
                    {
                        result.List.Add(stuffItem.GetGNoteHeader());
                    }
                    return result;
                }
            }
            else
            {
                if (request.WholeWords)
                {
                    request.SearchText = " " + request.SearchText.Trim() + " ";
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
                    List<NoteHeader> stuff =
                        await _db.NoteHeader
                        .Where(p => p.NoteFileId == request.NoteFileId
                            && p.ArchiveId == request.ArcId
                            && !p.IsDeleted
                            && p.Version == 0)
                        .Include(s => s.NoteContent)
                        .Where(s => s.NoteContent.NoteBody.ToLower().Contains(request.SearchText.ToLower())
                            && s.Id == s.NoteContent.NoteHeaderId)
                        .ToListAsync();
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

                    SearchResult result = new();

                    foreach (NoteHeader stuffItem in stuff)
                    {
                        result.List.Add(stuffItem.GetGNoteHeader());
                    }

                    return result;
                }
                else
                {

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
                    List<NoteHeader> stuff =
                        await _db.NoteHeader
                        .Where(p => p.NoteFileId == request.NoteFileId
                            && p.ArchiveId == request.ArcId
                            && !p.IsDeleted
                            && p.Version == 0)
                        .Include(s => s.NoteContent)
                        .Where(s => s.NoteContent.NoteBody.ToLower().Contains(request.SearchText.ToLower())
                            && s.Id == s.NoteContent.NoteHeaderId)
                        .ToListAsync();
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

                    SearchResult result = new();

                    foreach (NoteHeader stuffItem in stuff)
                    {
                        result.List.Add(stuffItem.GetGNoteHeader());
                    }

                    return result;
                }
            }
        }
    }
}