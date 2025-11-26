using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notes.Entities
{
    /// <summary>
    /// This class defines a table in the database.
    /// Not currently in use.
    /// </summary>
    public class Audit
    {
        /// <summary>
        /// Gets or sets the audit identifier.
        /// </summary>
        /// <value>The audit identifier.</value>
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AuditID { get; set; }

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        /// <value>The type of the event.</value>
        [Required]
        [StringLength(20)]
        [Display(Name = "Event Type")]
        public string? EventType { get; set; }

        // Name of the user did made it
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        [Required]
        [StringLength(256)]
        [Display(Name = "User Name")]
        public string? UserName { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        [Required]
        [StringLength(450)]
        public string? UserID { get; set; }

        /// <summary>
        /// Gets or sets the event time.
        /// </summary>
        /// <value>The event time.</value>
        [Required]
        [Display(Name = "Event Time")]
        public DateTime EventTime { get; set; }

        /// <summary>
        /// Gets or sets the event.
        /// </summary>
        /// <value>The event.</value>
        [Required]
        [StringLength(1000)]
        [Display(Name = "Event")]
        public string? Event { get; set; }
    }
}
