using Notes.Client.Layout;
using Notes.Client.Menus;
using System.Text;

namespace Notes.Client
{
    /// <summary>
    /// Class Globals.
    /// </summary>
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

        public static string ValidIssuerURL { get; set; } = "https://localhost:7093";
        public static string ValidAudienceURL { get; set; } = "https://localhost:7093";

        public static string SecretKey { get; set; } = "%TrwmbxYunsJWT8972jbgsfdh02h4gHTwwmkOOmNVDRNBSTmmdshgsynsnb%$$@2mNggsshh&";

        public static MainMenu? NavMenu = null;

    }
}
