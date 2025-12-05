using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;
using Notes.Client.Layout;
using Notes.Client.Menus;
using Notes.Protos;
using System.Text;

/// <summary>
/// The Notes.Client namespace contains classes and components for the client-side application of the Notes system,
/// </summary>
namespace Notes.Client
{
    /// <summary>
    /// Provides global constants, configuration values, and utility methods used throughout the application.
    /// </summary>
    /// <remarks>The Globals class contains static properties for application-wide identifiers, configuration
    /// settings, and helper methods such as Base64 encoding/decoding and time zone conversions. These members are
    /// intended for use across different components to ensure consistent access to shared values and functionality. All
    /// members are static and thread-safe for read operations; however, callers should ensure thread safety when
    /// modifying settable properties in multi-threaded scenarios.</remarks>
    public static class Globals
    {
        /// <summary>
        /// Gets the access other identifier.
        /// </summary>
        /// <value>The access other identifier.</value>
        public static string AccessOtherId { get; } = "Other";

        /// <summary>
        /// Gets the imported author identifier.
        /// </summary>
        /// <value>The imported author identifier.</value>
        public static string ImportedAuthorId { get; } = "*imported*";

        /// <summary>
        /// Gets or sets the guest identifier.
        /// </summary>
        /// <value>The guest identifier.</value>
        public static string GuestId { get; set; } = "x";

        /// <summary>
        /// Gets or sets the time zone default identifier.
        /// </summary>
        /// <value>The time zone default identifier.</value>
        public static int TimeZoneDefaultID { get; set; } = 54;

        /// <summary>
        /// Gets or sets the import root.
        /// </summary>
        /// <value>The import root.</value>
        public static string ImportRoot { get; set; } = "E:\\Projects\\2022gRPC\\Notes2022GRPC\\Notes2022\\Server\\wwwroot\\Import\\";

        /// <summary>
        /// Gets or sets the send grid email.
        /// </summary>
        /// <value>The send grid email.</value>
        public static string SendGridEmail { get; set; } = "";

        /// <summary>
        /// Gets or sets the name of the send grid.
        /// </summary>
        /// <value>The name of the send grid.</value>
        public static string SendGridName { get; set; } = "";

        /// <summary>
        /// Gets or sets the send grid API key.
        /// </summary>
        /// <value>The send grid API key.</value>
        public static string SendGridApiKey { get; set; } = "";

        public static string HangfireAddress { get; set; } = "";

        public static string AppVirtDir { get; set; } = "";

        public static string PrimeAdminEmail { get; set; } = "";

        public static string PrimeAdminName { get; set; } = "";

        public static int ImportMailInterval { get; set; } = 200;



        /// <summary>
        /// Base64s the encode.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <returns>System.String.</returns>
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Base64s the decode.
        /// </summary>
        /// <param name="encodedString">The encoded string.</param>
        /// <returns>System.String.</returns>
        public static string Base64Decode(string encodedString)
        {
            byte[] data = Convert.FromBase64String(encodedString);
            string decodedString = Encoding.UTF8.GetString(data);
            return decodedString;
        }


        /// <summary>
        /// us the time blazor.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <returns>DateTime.</returns>
        public static DateTime UTimeBlazor(DateTime dt)
        {
            //int OHours = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Hours;
            //int OMinutes = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Minutes;

            return dt; //.AddHours(-OHours).AddMinutes(-OMinutes);
        }

        /// <summary>
        /// GMT to Local.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <returns>DateTime.</returns>
        public static DateTime LocalTimeBlazor(DateTime dt)
        {
            int OHours = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Hours;
            int OMinutes = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Minutes;

            return dt.AddHours(OHours * 2).AddMinutes(OMinutes * 2);    // *2 needed because we go in and out of unix utc time
        }

        public static Notes.Client.Menus.MainMenu? MainMenu { get; set; }

        public static Notes.Client.Menus.LoginDisplay? LoginDisplay { get; set; }

        public static long GotoNote { get; set; }
        public static string returnUrl { get; set; }

        public static string CookieName { get; set; } = "notesfoodie2026";

        public static string AppUrl { get; set; } = "https://localhost:7093";

        public static string ValidIssuerURL { get; set; } = "https://localhost:7093";
        public static string ValidAudienceURL { get; set; } = "https://localhost:7093";

        public static string SecretKey { get; set; } = "%TrwmbxYunsJWT8972jbgsfdh02h4gHTwwmkOOmNVDRNBSTmmdshgsynsnb%$$@2mNggsshh&";

        public static MainMenu? NavMenu = null;

        public static NotesServer.NotesServerClient? NotesClient { get; set; }

        public static NotesServer.NotesServerClient GetNotesClient(NavigationManager serviceprov)
        {
            string AppVirtDir = ""; // preset for localhost
            string baseUri = serviceprov.BaseUri;
            string[] parts = baseUri.Split('/');
            if (!baseUri.Contains("localhost")) // not localhost - assume it is in a virtual directory ONLY ONE LEVEL DOWN from root of site
                AppVirtDir = "/" + parts[^2];


            return new NotesServer.NotesServerClient(
                GrpcChannel.ForAddress(baseUri,
                new GrpcChannelOptions
                {
                    HttpHandler = new GrpcWebHandler(new SubdirectoryHandler(new HttpClientHandler(), AppVirtDir)),
                    MaxReceiveMessageSize = 50 * 1024 * 1024,
                    MaxSendMessageSize = 50 * 1024 * 1024
                })
                );
        }

    }
}
