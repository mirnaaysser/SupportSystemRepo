using System.Net;
using System.Net.Mail;

namespace BizSparkSupport.Mailing
{
    /// <summary>
    /// Email configuration class.
    /// </summary>
    public class EmailConfig
    {
        #region " Public properties "
        /// <summary>
        /// Gets or sets the name or IP address of the host.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the port used by the host.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Specify whether the Emailconfig uses SSL to encrypt the connection.
        /// </summary>
        public bool EnableSsl { get; set; }
        #endregion

        #region " Private/Internal Properties "
        /// <summary>
        /// Specifies how outgoing email messages will be handled.
        /// </summary>
        private SmtpDeliveryMethod deliveryMethod = SmtpDeliveryMethod.Network;

        /// <summary>
        /// Gets or sets boolean value that controls whether the 
        /// default credentials are sent with requests.
        /// </summary>
        private bool useDefaultCredentials = false;

        /// <summary>
        /// Provides credentials for the sender email.
        /// </summary>
        private NetworkCredential credentials;

        /// <summary>
        /// Gets or sets the sender mail address info.
        /// </summary>
        internal MailAddress Sender { get; set; }
        #endregion

        #region " Constructor/Destructor "
        /// <summary>
        /// Initializes a new instance of the EmailConfig class by using 
        /// the givin sender information.
        /// </summary>
        /// <param name="senderAddress">Sender email address.</param>
        /// <param name="displayName">The display name for the sender.</param>
        /// <param name="password">Sender email password.</param>
        public EmailConfig(string senderAddress, string displayName, string password)
        {
            this.Sender = new MailAddress(senderAddress, displayName);
            this.credentials = new NetworkCredential(senderAddress, password);
        }
        #endregion

        #region " Public functions "
        /// <summary>
        /// Gets the configured sender client.
        /// </summary>
        /// <returns>Ready to use SMTPClient object.</returns>
        public SmtpClient GetSenderClient()
        {
            SmtpClient smtp = new SmtpClient
            {
                Host = this.Host,
                Port = this.Port,
                EnableSsl = this.EnableSsl,
                DeliveryMethod = this.deliveryMethod,
                UseDefaultCredentials = this.useDefaultCredentials,
                Credentials = this.credentials
            };

            return smtp;
        }
        #endregion
    }
}
