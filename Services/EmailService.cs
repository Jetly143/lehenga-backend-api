using System.Net;
using System.Net.Mail;
using System.Text;

namespace Rent_Lehenga_Project.Services;

public interface IEmailService
{
    Task<bool> SendVerificationEmailAsync(string toEmail, string userName, string token, string role);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendVerificationEmailAsync(string toEmail, string userName, string token, string role)
    {
        var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
        var verificationLink = $"{frontendUrl}/?action=confirm-email&token={token}&email={Uri.EscapeDataString(toEmail)}";
        var roleLabel = role == "SHOP_OWNER" ? "B2B Boutique Partner" : "P2P Lady Closet Partner";

        var subject = "Action Required: Confirm Your LehengaVerse Partner Email";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset=""utf-8"">
  <title>Confirm Your LehengaVerse Account</title>
</head>
<body style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #FAF7F2; margin: 0; padding: 30px; color: #360A10;"">
  <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 580px; background-color: #ffffff; border-radius: 16px; border: 1px solid #EBE3D7; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.05);"">
    <!-- Header -->
    <tr>
      <td style=""background: linear-gradient(135deg, #420B13 0%, #5D131E 100%); padding: 32px 24px; text-align: center;"">
        <div style=""display: inline-block; background-color: #E5A93C; color: #420B13; font-weight: bold; font-size: 14px; padding: 6px 14px; border-radius: 999px; margin-bottom: 12px; letter-spacing: 1px;"">LEHENGAVERSE LUXURY</div>
        <h1 style=""color: #ffffff; font-family: Georgia, serif; font-size: 26px; margin: 0; font-weight: bold;"">Confirm Your Partner Email</h1>
        <p style=""color: #EED9B3; font-size: 13px; margin: 6px 0 0;"">Step 1 of Partner Onboarding ({roleLabel})</p>
      </td>
    </tr>

    <!-- Body -->
    <tr>
      <td style=""padding: 32px 28px;"">
        <p style=""font-size: 15px; line-height: 1.6; margin: 0 0 16px;"">
          Hi <strong style=""color: #420B13;"">{userName}</strong>,
        </p>
        <p style=""font-size: 14px; line-height: 1.6; color: #614D4B; margin: 0 0 24px;"">
          Thank you for joining <strong>LehengaVerse</strong> as a <strong>{roleLabel}</strong>. To safeguard our high-value bridal and designer lehenga marketplace, please confirm your email address. Once verified, you will be directed to submit your mandatory identity documents for Admin verification.
        </p>

        <!-- CTA Button -->
        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""margin: 28px auto;"">
          <tr>
            <td align=""center"" style=""border-radius: 12px; background-color: #1E7E34;"">
              <a href=""{verificationLink}"" target=""_blank"" style=""display: inline-block; padding: 14px 32px; font-size: 15px; font-weight: bold; color: #ffffff; text-decoration: none; border-radius: 12px; letter-spacing: 0.5px;"">
                👉 Confirm Email & Proceed to KYC
              </a>
            </td>
          </tr>
        </table>

        <!-- Security Notice -->
        <div style=""background-color: #FAF4EA; border: 1px solid #EDE4D5; border-radius: 12px; padding: 16px; margin-top: 24px;"">
          <p style=""margin: 0; font-size: 12px; color: #7A6260; line-height: 1.5;"">
            🔒 <strong>Dashboard Lockout Policy:</strong> Your partner dashboard and outfit listing rights remain strictly locked until email confirmation is complete and Admin approves your KYC credentials.
          </p>
        </div>

        <p style=""font-size: 12px; color: #8C7A78; margin-top: 28px; line-height: 1.5;"">
          Or copy and paste this verification URL in your browser:<br />
          <a href=""{verificationLink}"" style=""color: #D99B35; word-break: break-all;"">{verificationLink}</a>
        </p>
      </td>
    </tr>

    <!-- Footer -->
    <tr>
      <td style=""background-color: #FAF7F2; padding: 20px; text-align: center; border-top: 1px solid #EDE4D5; font-size: 11px; color: #8C7A78;"">
        LehengaVerse Luxury Bridal Rentals &copy; 2026 • Verified Partner Compliance Center
      </td>
    </tr>
  </table>
</body>
</html>";

        // Save email copy to local outbox for instant verification/testing
        try
        {
            var outboxDir = Path.Combine(AppContext.BaseDirectory, "outbox");
            if (!Directory.Exists(outboxDir))
            {
                Directory.CreateDirectory(outboxDir);
            }
            var fileName = $"Verification_{toEmail.Replace("@", "_").Replace(".", "_")}_{DateTime.UtcNow:yyyyMMddHHmmss}.html";
            var filePath = Path.Combine(outboxDir, fileName);
            await File.WriteAllTextAsync(filePath, htmlBody, Encoding.UTF8);
            _logger.LogInformation("📧 Verification email saved to local outbox: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to write to outbox folder: {Message}", ex.Message);
        }

        // Attempt Real SMTP Sending if SMTP configured
        var smtpHost = _configuration["Smtp:Host"];
        if (!string.IsNullOrWhiteSpace(smtpHost))
        {
            try
            {
                var smtpPort = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 587;
                var smtpUser = _configuration["Smtp:UserName"];
                var smtpPass = _configuration["Smtp:Password"];
                var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@lehengaverse.com";
                var fromName = _configuration["Smtp:FromName"] ?? "LehengaVerse Luxury Rentals";
                var enableSsl = bool.TryParse(_configuration["Smtp:EnableSsl"], out var ssl) && ssl;

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = enableSsl
                };

                if (!string.IsNullOrWhiteSpace(smtpUser) && !string.IsNullOrWhiteSpace(smtpPass))
                {
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                }

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(new MailAddress(toEmail, userName));

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("✅ Real SMTP email dispatched successfully to {ToEmail}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("SMTP dispatch failed (falling back to generated link): {Message}", ex.Message);
            }
        }
        else
        {
            _logger.LogInformation("ℹ️ SMTP Host not configured in appsettings.json. Verification link generated: {Link}", verificationLink);
        }

        return true;
    }
}
