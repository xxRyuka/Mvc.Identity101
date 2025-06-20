using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Mvc.Identity101.Services.Abstract;
using Mvc.Identity101.Services.Data.Dto;

namespace Mvc.Identity101.Services.Concrete;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _emailSettings = options.Value;
    }

    public async Task SendResetPasswordEmailAsync(string toUserMail, string resetPasswordLink)
    {
        var smtpClient = new SmtpClient();
        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        smtpClient.UseDefaultCredentials = false;
        smtpClient.Port = 587;
        smtpClient.Credentials = new NetworkCredential(_emailSettings.Email, password: _emailSettings.Password);
        smtpClient.Host = _emailSettings.SmtpHost;
        smtpClient.EnableSsl = true;

        var mailMessage = new MailMessage();
        mailMessage.From = new MailAddress(_emailSettings.Email);
        mailMessage.To.Add(new MailAddress(toUserMail));
        mailMessage.Subject = "AspNetCore Identity Project Reset Password Linki";
        mailMessage.Body = @$"Sifre Yenileme linkiniz : {resetPasswordLink}
                        <h1>

                            <p>
                                <a href='{resetPasswordLink}'>Sifre Yenileme linki</a>
                            </p>
                        </h1>
                        ";      

        mailMessage.IsBodyHtml = true;

        await smtpClient.SendMailAsync(mailMessage);
    }
}