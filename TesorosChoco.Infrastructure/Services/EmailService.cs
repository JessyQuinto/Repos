using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace TesorosChoco.Infrastructure.Services;

/// <summary>
/// Email service for sending notifications, confirmations, and marketing emails
/// Configurable for different providers (SMTP, SendGrid, etc.)
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendEmailConfirmationAsync(string to, string confirmationLink);
    Task SendPasswordResetAsync(string to, string resetLink);
    Task SendOrderConfirmationAsync(string to, string orderNumber, decimal total);
    Task SendContactMessageConfirmationAsync(string to, string name);
}

public class EmailService : IEmailService
{
    private readonly EmailConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailConfiguration> config, ILogger<EmailService> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            using var client = new SmtpClient(_config.SmtpHost, _config.SmtpPort)
            {
                EnableSsl = _config.EnableSsl,
                Credentials = new NetworkCredential(_config.Username, _config.Password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config.FromEmail, _config.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {To} with subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} with subject: {Subject}", to, subject);
            throw;
        }
    }    public async Task SendEmailConfirmationAsync(string to, string confirmationLink)
    {
        var subject = "Confirma tu cuenta - Tesoros del Chocó";
        var body = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #8B4513, #A0522D); color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
                    .button {{ display: inline-block; background: #8B4513; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""header"">
                        <h1>¡Bienvenido a Tesoros del Chocó!</h1>
                    </div>
                    <div class=""content"">
                        <p>Gracias por registrarte en nuestra plataforma. Para completar tu registro, por favor confirma tu dirección de correo electrónico haciendo clic en el siguiente enlace:</p>
                        
                        <div style=""text-align: center;"">
                            <a href=""{confirmationLink}"" class=""button"">Confirmar Email</a>
                        </div>
                        
                        <p>Si no puedes hacer clic en el botón, copia y pega el siguiente enlace en tu navegador:</p>
                        <p style=""word-break: break-all; background: #e9e9e9; padding: 10px; border-radius: 4px;"">{confirmationLink}</p>
                        
                        <p>Este enlace expirará en 24 horas por razones de seguridad.</p>
                        
                        <p>Si no creaste esta cuenta, puedes ignorar este correo.</p>
                    </div>
                    <div class=""footer"">
                        <p>© 2025 Tesoros del Chocó. Todos los derechos reservados.</p>
                    </div>
                </div>
            </body>
            </html>";

        await SendEmailAsync(to, subject, body);
    }

    public async Task SendPasswordResetAsync(string to, string resetLink)
    {
        var subject = "Restablece tu contraseña - Tesoros del Chocó";
        var body = $"""
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #8B4513, #A0522D); color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
                    .button {{ display: inline-block; background: #8B4513; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                    .warning {{ background: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 4px; margin: 20px 0; }}
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>Restablece tu contraseña</h1>
                    </div>
                    <div class="content">
                        <p>Hemos recibido una solicitud para restablecer la contraseña de tu cuenta en Tesoros del Chocó.</p>
                        
                        <div style="text-align: center;">
                            <a href="{resetLink}" class="button">Restablecer Contraseña</a>
                        </div>
                        
                        <p>Si no puedes hacer clic en el botón, copia y pega el siguiente enlace en tu navegador:</p>
                        <p style="word-break: break-all; background: #e9e9e9; padding: 10px; border-radius: 4px;">{resetLink}</p>
                        
                        <div class="warning">
                            <strong>Importante:</strong> Este enlace expirará en 1 hora por razones de seguridad. Si no solicitaste este cambio, puedes ignorar este correo de forma segura.
                        </div>
                    </div>
                    <div class="footer">
                        <p>© 2025 Tesoros del Chocó. Todos los derechos reservados.</p>
                    </div>
                </div>
            </body>
            </html>
            """;

        await SendEmailAsync(to, subject, body);
    }

    public async Task SendOrderConfirmationAsync(string to, string orderNumber, decimal total)
    {
        var subject = $"Confirmación de pedido #{orderNumber} - Tesoros del Chocó";
        var body = $"""
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #8B4513, #A0522D); color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
                    .order-info {{ background: white; padding: 20px; border-radius: 4px; margin: 20px 0; }}
                    .total {{ font-size: 18px; font-weight: bold; color: #8B4513; }}
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>¡Pedido Confirmado!</h1>
                    </div>
                    <div class="content">
                        <p>Gracias por tu pedido en Tesoros del Chocó. Hemos recibido tu orden y la estamos procesando.</p>
                        
                        <div class="order-info">
                            <h3>Detalles del pedido:</h3>
                            <p><strong>Número de pedido:</strong> #{orderNumber}</p>
                            <p><strong>Total:</strong> <span class="total">${total:N0} COP</span></p>
                            <p><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                        </div>
                        
                        <p>Te notificaremos cuando tu pedido sea enviado. Puedes hacer seguimiento de tu pedido en tu cuenta.</p>
                        
                        <p>Si tienes alguna pregunta sobre tu pedido, no dudes en contactarnos.</p>
                    </div>
                    <div class="footer">
                        <p>© 2025 Tesoros del Chocó. Todos los derechos reservados.</p>
                    </div>
                </div>
            </body>
            </html>
            """;

        await SendEmailAsync(to, subject, body);
    }

    public async Task SendContactMessageConfirmationAsync(string to, string name)
    {
        var subject = "Hemos recibido tu mensaje - Tesoros del Chocó";
        var body = $"""
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #8B4513, #A0522D); color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>Mensaje Recibido</h1>
                    </div>
                    <div class="content">
                        <p>Hola {name},</p>
                        
                        <p>Gracias por contactarnos. Hemos recibido tu mensaje y nuestro equipo lo revisará pronto.</p>
                        
                        <p>Nos pondremos en contacto contigo en las próximas 24-48 horas.</p>
                        
                        <p>¡Gracias por tu interés en Tesoros del Chocó!</p>
                    </div>
                    <div class="footer">
                        <p>© 2025 Tesoros del Chocó. Todos los derechos reservados.</p>
                    </div>
                </div>
            </body>
            </html>
            """;

        await SendEmailAsync(to, subject, body);
    }
}

/// <summary>
/// Email configuration settings
/// </summary>
public class EmailConfiguration
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}
