using Microsoft.AspNetCore.Identity;
using Notes.Protos;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// The Notes.Entities namespace contains the Entity Framework Core Table
/// definitions for the Notes application.  These classes define the schema.
/// </summary>
namespace Notes.Entities
{
    /// <summary>
    /// Extentions to the base IdentityUser
    /// Contains fields mirrored locally in UserData
    /// These fields are accessed and edited there and then written back
    /// enmass.  By contrast the predefined field not seen here are
    /// almost always accessed via methods.  These methods create a Validation
    /// Stamp for the predefined fields.  Tinker with those directly and
    /// you will probably make the user "Unusable".
    /// </summary>
    public class ApplicationUser : IdentityUser
    {

        //[Required]
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        [Display(Name = "Display Name")]
        [StringLength(50)]
        [PersonalData]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the ipref0.
        /// </summary>
        /// <value>The ipref0.</value>
        [PersonalData]
        public int Ipref0 { get; set; }

        /// <summary>
        /// Gets or sets the ipref1.
        /// </summary>
        /// <value>The ipref1.</value>
        [PersonalData]
        public int Ipref1 { get; set; }

        /// <summary>
        /// Gets or sets the ipref2.
        /// user choosen page size
        /// </summary>
        /// <value>The ipref2.</value>
        [PersonalData]
        public int Ipref2 { get; set; } // user choosen page size

        /// <summary>
        /// Gets or sets the ipref3.
        /// </summary>
        /// <value>The ipref3.</value>
        [PersonalData]
        public int Ipref3 { get; set; }

        /// <summary>
        /// Gets or sets the ipref4.
        /// </summary>
        /// <value>The ipref4.</value>
        [PersonalData]
        public int Ipref4 { get; set; }

        /// <summary>
        /// Gets or sets the ipref5.
        /// </summary>
        /// <value>The ipref5.</value>
        [PersonalData]
        public int Ipref5 { get; set; }

        /// <summary>
        /// Gets or sets the ipref6.
        /// </summary>
        /// <value>The ipref6.</value>
        [PersonalData]
        public int Ipref6 { get; set; }

        /// <summary>
        /// Gets or sets the ipref7.
        /// </summary>
        /// <value>The ipref7.</value>
        [PersonalData]
        public int Ipref7 { get; set; }

        /// <summary>
        /// Gets or sets the ipref8.
        /// </summary>
        /// <value>The ipref8.</value>
        [PersonalData]
        public int Ipref8 { get; set; }

        /// <summary>
        /// Gets or sets the ipref9.
        /// bits extend bool properties
        /// </summary>
        /// <value>The ipref9.</value>
        [PersonalData]
        public int Ipref9 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationUser"/> is pref0.
        /// </summary>
        /// <value><c>true</c> if pref0; otherwise, <c>false</c>.</value>
        [PersonalData]
        public bool Pref0 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationUser"/> is pref1.
        /// </summary>
        /// <value><c>true</c> if pref1; otherwise, <c>false</c>.</value>
        [PersonalData]
        public bool Pref1 { get; set; } // false = use paged note index, true= scrolled

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationUser"/> is pref2.
        /// </summary>
        /// <value><c>true</c> if pref2; otherwise, <c>false</c>.</value>
        [PersonalData]
        public bool Pref2 { get; set; } // use alternate editor

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationUser"/> is pref3.
        /// show responses by default
        /// </summary>
        /// <value><c>true</c> if pref3; otherwise, <c>false</c>.</value>
        [PersonalData]
        public bool Pref3 { get; set; } // show responses by default

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationUser"/> is pref4.
        /// Flip Response order by default
        /// </summary>
        /// <value><c>true</c> if pref4; otherwise, <c>false</c>.</value>
        [PersonalData]
        public bool Pref4 { get; set; } // multiple expanded responses

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationUser"/> is pref5.
        /// expanded responses
        /// </summary>
        /// <value><c>true</c> if pref5; otherwise, <c>false</c>.</value>
        [PersonalData]
        public bool Pref5 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationUser"/> is pref6.
        /// </summary>
        /// <value><c>true</c> if pref6; otherwise, <c>false</c>.</value>
        [PersonalData]
        public bool Pref6 { get; set; } // alternate text editor

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationUser"/> is pref7.
        /// show content when expanded
        /// </summary>
        /// <value><c>true</c> if pref7; otherwise, <c>false</c>.</value>
        [PersonalData]
        public bool Pref7 { get; set; } // show content when expanded

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationUser"/> is pref8.
        /// </summary>
        /// <value><c>true</c> if pref8; otherwise, <c>false</c>.</value>
        [PersonalData]
        public bool Pref8 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationUser"/> is pref9.
        /// </summary>
        /// <value><c>true</c> if pref9; otherwise, <c>false</c>.</value>
        [PersonalData]
        public bool Pref9 { get; set; }

        /// <summary>
        /// Gets or sets my unique identifier.
        /// </summary>
        /// <value>My unique identifier.</value>
        [StringLength(100)]
        [PersonalData]
        public string? MyGuid { get; set; }

        /// <summary>
        /// Gets the application user.
        /// Conversions between Db Entity space and gRPC space.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>ApplicationUser.</returns>
        public static ApplicationUser GetApplicationUser(GAppUser other)
        {
            ApplicationUser u = new ApplicationUser();
            u.Id = other.Id;
            u.DisplayName = other.DisplayName;
           // u.TimeZoneID = other.TimeZoneID;
            u.Ipref0 = other.Ipref0;
            u.Ipref1 = other.Ipref1;
            u.Ipref2 = other.Ipref2;
            u.Ipref3 = other.Ipref3;
            u.Ipref4 = other.Ipref4;
            u.Ipref5 = other.Ipref5;
            u.Ipref6 = other.Ipref6;
            u.Ipref7 = other.Ipref7;
            u.Ipref8 = other.Ipref8;
            u.Ipref9 = other.Ipref9;
            u.Pref0 = other.Pref0;
            u.Pref1 = other.Pref1;
            u.Pref2 = other.Pref2;
            u.Pref3 = other.Pref3;
            u.Pref4 = other.Pref4;
            u.Pref5 = other.Pref5;
            u.Pref6 = other.Pref6;
            u.Pref7 = other.Pref7;
            u.Pref8 = other.Pref8;
            u.Pref9 = other.Pref9;
            //u.MyGuid = other.MyGuid;  // null
            return u;
        }

        /// <summary>
        /// Merges the application user.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <param name="u">The u.</param>
        /// <returns>ApplicationUser.</returns>
        public static ApplicationUser MergeApplicationUser(GAppUser other, ApplicationUser u)
        {
            u.Id = other.Id;
            u.DisplayName = other.DisplayName;
         //   u.TimeZoneID = other.TimeZoneID;
            u.Ipref0 = other.Ipref0;
            u.Ipref1 = other.Ipref1;
            u.Ipref2 = other.Ipref2;
            u.Ipref3 = other.Ipref3;
            u.Ipref4 = other.Ipref4;
            u.Ipref5 = other.Ipref5;
            u.Ipref6 = other.Ipref6;
            u.Ipref7 = other.Ipref7;
            u.Ipref8 = other.Ipref8;
            u.Ipref9 = other.Ipref9;
            u.Pref0 = other.Pref0;
            u.Pref1 = other.Pref1;
            u.Pref2 = other.Pref2;
            u.Pref3 = other.Pref3;
            u.Pref4 = other.Pref4;
            u.Pref5 = other.Pref5;
            u.Pref6 = other.Pref6;
            u.Pref7 = other.Pref7;
            u.Pref8 = other.Pref8;
            u.Pref9 = other.Pref9;
            //u.MyGuid = other.MyGuid;  // null
            return u;
        }


        /// <summary>
        /// Gets the g application user.
        /// Conversions between Db Entity space and gRPC space.
        /// </summary>
        /// <returns>GAppUser.</returns>
        public GAppUser GetGAppUser()
        {
            GAppUser u = new GAppUser();
            u.Id = Id;
            u.DisplayName = DisplayName;
            //u.TimeZoneID = TimeZoneID;
            u.Ipref0 = Ipref0;
            u.Ipref1 = Ipref1;
            u.Ipref2 = Ipref2;
            u.Ipref3 = Ipref3;
            u.Ipref4 = Ipref4;
            u.Ipref5 = Ipref5;
            u.Ipref6 = Ipref6;
            u.Ipref7 = Ipref7;
            u.Ipref8 = Ipref8;
            u.Ipref9 = Ipref9;
            u.Pref0 = Pref0;
            u.Pref1 = Pref1;
            u.Pref2 = Pref2;
            u.Pref3 = Pref3;
            u.Pref4 = Pref4;
            u.Pref5 = Pref5;
            u.Pref6 = Pref6;
            u.Pref7 = Pref7;
            u.Pref8 = Pref8;
            u.Pref9 = Pref9;
            //u.MyGuid = MyGuid;    //null
            return u;
        }

        /// <summary>
        /// Gets the application users.
        /// Conversions between Db Entity space and gRPC space.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>List&lt;ApplicationUser&gt;.</returns>
        public static List<ApplicationUser> GetApplicationUsers(GAppUserList other)
        {
            List<ApplicationUser> list = new List<ApplicationUser>();
            foreach (GAppUser user in other.List)
            {
                list.Add(GetApplicationUser(user));
            }
            return list;
        }

        /// <summary>
        /// Gets the g application user list.
        /// Conversions between Db Entity space and gRPC space.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>GAppUserList.</returns>
        public static GAppUserList GetGAppUserList(List<ApplicationUser> other)
        {
            GAppUserList list = new GAppUserList();
            foreach (ApplicationUser u in other)
            {
                list.List.Add(u.GetGAppUser());
            }
            return list;
        }
    }

}
