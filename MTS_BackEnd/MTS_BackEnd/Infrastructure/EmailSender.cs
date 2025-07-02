using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MimeKit.Text;

namespace MTS.BackEnd.Infrastructure
{
	public class EmailSender : IEmailSender, IDisposable
	{
		private readonly IConfiguration _configuration;
		private SmtpClient? _smtpClient;
		private bool _isConnected = false;

		public EmailSender(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			var emailMessage = new MimeMessage();
			emailMessage.From.Add(MailboxAddress.Parse(_configuration["Email:Sender"]!));
			emailMessage.To.Add(MailboxAddress.Parse(email));
			emailMessage.Subject = subject;
			emailMessage.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

			if (_smtpClient == null)
			{
				_smtpClient = new SmtpClient();
			}

			if (!_isConnected || !_smtpClient.IsConnected)
			{
				int port = _configuration.GetValue<int>("Email:Port");
				await _smtpClient.ConnectAsync(_configuration["Email:SmtpServer"], port, SecureSocketOptions.StartTls);
				await _smtpClient.AuthenticateAsync(_configuration["Email:Sender"], _configuration["Email:Password"]);
				_isConnected = true;
			}

			await _smtpClient.SendAsync(emailMessage);
		}

		public void Dispose()
		{
			if (_smtpClient != null && _smtpClient.IsConnected)
			{
				_smtpClient.Disconnect(true);
				_smtpClient.Dispose();
			}
		}
	}
}
