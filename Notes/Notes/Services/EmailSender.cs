/*--------------------------------------------------------------------------
    **
    **  Copyright © 2026, Dale Sinder
    **
    **  Name: EmailSender.cs
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

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using Notes.Client;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Notes.Services
{
    /// <summary>
    /// Provides functionality to send email messages asynchronously using configured options.
    /// </summary>
    /// <remarks>This class implements the <see cref="IEmailSender"/> interface and is typically used to send
    /// emails in applications that require notification or communication features. The email sending behavior is
    /// configured via <see cref="AuthMessageSenderOptions"/>, which may be set through dependency injection or secret
    /// management. Instances of <see cref="EmailSender"/> are thread-safe for concurrent use.</remarks>
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
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public EmailSender()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
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

                List<EmailAddress> addresses = [];
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
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string SendGridKey { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }
}
