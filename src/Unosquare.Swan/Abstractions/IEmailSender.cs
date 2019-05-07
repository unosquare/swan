namespace Unosquare.Swan.Abstractions
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an interface for e-mail sender.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if the message was sent, otherwise <c>false</c>.</returns>
        Task<bool> SendMessage(string recipient, string subject, string message);
    }
}
