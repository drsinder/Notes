using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using Notes.Client;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Notes.Services
{
    /// <summary>
    /// Class EmailSender.
    /// Implements the <see cref="IEmailSender" />
    /// </summary>
    /// <seealso cref="IEmailSender" />
    public class EmailSender : IEmailSender
    {
        //public StreamWriter StreamWriter { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSender"/> class.
        /// </summary>
        /// <param name="optionsAccessor">The options accessor.</param>
        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }
    
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSender"/> class.
        /// </summary>
        public EmailSender()
        {
        }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        public AuthMessageSenderOptions Options { get; } //set only via Secret Manager

        /// <summary>
        /// Send email as an asynchronous operation.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var apiKey = Globals.SendGridApiKey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(Globals.SendGridEmail, Globals.SendGridName);
            var to = new EmailAddress(email);
            var htmlStart = "<!DOCTYPE html>";
            var isHtml = message.StartsWith(htmlStart);

            SendGridMessage msg;

            if (email.Contains(';')) // multiple targets
            {
                string[] who = email.Split(';');

                List<EmailAddress> addresses = new List<EmailAddress>();
                foreach (string a in who)
                {
                    addresses.Add(new EmailAddress(a.Trim()));
                }
                msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, addresses, subject, isHtml ? "See Html Attachment." : message, isHtml ? "See Html Attachment." : message);
            }
            else // single target
            {
                msg = MailHelper.CreateSingleEmail(from, to, subject, isHtml ? "See Html Attachment." : message, isHtml ? "See Html Attachment." : message);
            }

            if (isHtml)
            {
                MemoryStream ms = new();
                StreamWriter sw = new(ms);
                await sw.WriteAsync(message);
                await sw.FlushAsync();
                ms.Seek(0, SeekOrigin.Begin);
                await msg.AddAttachmentAsync(subject + ".html", ms);
                ms.Dispose();
            }

            await client.SendEmailAsync(msg);
        }


    }

    /// <summary>
    /// Class AuthMessageSenderOptions.
    /// </summary>
    public class AuthMessageSenderOptions
    {
        /// <summary>
        /// Gets or sets the send grid key.
        /// </summary>
        /// <value>The send grid key.</value>
        public string SendGridKey { get; set; }
    }
}
