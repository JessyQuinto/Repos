using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Infrastructure.Services;

/// <summary>
/// Email service implementation for sending various types of emails
/// Simple text-based email service for backend API
/// HTML templates should be handled by frontend or separate template service
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly bool _enableSsl;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _smtpHost = configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
        _smtpUsername = configuration["Email:SmtpUsername"] ?? throw new InvalidOperationException("SMTP Username not configured");
        _smtpPassword = configuration["Email:SmtpPassword"] ?? throw new InvalidOperationException("SMTP Password not configured");
        _fromEmail = configuration["Email:FromEmail"] ?? _smtpUsername;
        _fromName = configuration["Email:FromName"] ?? "Tesoros del Chocó";
        _enableSsl = bool.Parse(configuration["Email:EnableSsl"] ?? "true");
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        try
        {
            using var client = new SmtpClient(_smtpHost, _smtpPort);
            client.EnableSsl = _enableSsl;
            client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

            using var message = new MailMessage();
            message.From = new MailAddress(_fromEmail, _fromName);
            message.To.Add(new MailAddress(to));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHtml;

            await client.SendMailAsync(message);
            
            _logger.LogInformation("Email sent successfully to {Email}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            return false;
        }
    }

    /// <summary>
    /// Send order confirmation email
    /// </summary>
    public async Task<bool> SendOrderConfirmationAsync(string email, string customerName, int orderId, decimal total)
    {
        var subject = $"Confirmación de pedido #{orderId} - Tesoros del Chocó";
        var body = $@"Hola {customerName},

¡Gracias por tu pedido!

Detalles del pedido:
- Número de pedido: #{orderId}
- Total: ${total:N0} COP
- Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}

Tu pedido ha sido recibido y está siendo procesado.

Te notificaremos cuando tu pedido sea enviado.

Puedes consultar el estado de tu pedido en cualquier momento desde tu cuenta.

Saludos cordiales,
El equipo de Tesoros del Chocó

© 2025 Tesoros del Chocó. Todos los derechos reservados.";
        
        return await SendEmailAsync(email, subject, body);
    }

    /// <summary>
    /// Send order status update email
    /// </summary>
    public async Task<bool> SendOrderStatusUpdateAsync(string email, string customerName, int orderId, string status)
    {
        var subject = $"Actualización de pedido #{orderId} - Tesoros del Chocó";
        var body = $@"Hola {customerName},

Tu pedido #{orderId} ha sido actualizado.

Nuevo estado: {status}

Puedes consultar más detalles en tu cuenta.

Saludos cordiales,
El equipo de Tesoros del Chocó";
        
        return await SendEmailAsync(email, subject, body);
    }

    /// <summary>
    /// Send newsletter email
    /// </summary>
    public async Task<bool> SendNewsletterAsync(string email, string subject, string content)
    {
        return await SendEmailAsync(email, subject, content, true);
    }

    /// <summary>
    /// Send contact form response
    /// </summary>
    public async Task<bool> SendContactResponseAsync(string email, string name, string response)
    {
        var subject = "Respuesta a tu consulta - Tesoros del Chocó";
        var body = $@"Hola {name},

Gracias por contactarnos. Aquí está nuestra respuesta:

{response}

Si tienes más preguntas, no dudes en contactarnos.

Saludos cordiales,
El equipo de Tesoros del Chocó";
        
        return await SendEmailAsync(email, subject, body);
    }

    /// <summary>
    /// Send password reset email
    /// </summary>
    public async Task<bool> SendPasswordResetAsync(string email, string resetToken)
    {
        var subject = "Código para restablecer contraseña - Tesoros del Chocó";
        var body = $@"Hola,

Recibimos una solicitud para restablecer la contraseña de tu cuenta en Tesoros del Chocó.

Usa el siguiente código para restablecer tu contraseña:

Código de restablecimiento: {resetToken}

Este código expirará en 1 hora por razones de seguridad.

Si no solicitaste este cambio, puedes ignorar este correo.

Saludos cordiales,
El equipo de Tesoros del Chocó";
        
        return await SendEmailAsync(email, subject, body);
    }

    /// <summary>
    /// Send email confirmation
    /// </summary>
    public async Task<bool> SendEmailConfirmationAsync(string email, string confirmationToken)
    {
        var subject = "Confirma tu dirección de correo electrónico - Tesoros del Chocó";
        var body = $@"Hola,

¡Bienvenido a Tesoros del Chocó!

Para completar tu registro, por favor usa el siguiente código de confirmación:

Código de confirmación: {confirmationToken}

Este código expirará en 24 horas por razones de seguridad.

Si no creaste esta cuenta, puedes ignorar este correo.

Saludos cordiales,
El equipo de Tesoros del Chocó";
        
        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendWelcomeEmailAsync(string email, string firstName)
    {
        var subject = "¡Bienvenido a Tesoros del Chocó!";
        var body = $@"Hola {firstName},

¡Bienvenido a Tesoros del Chocó!

Tu cuenta ha sido creada exitosamente. Ahora puedes:
- Explorar nuestros productos artesanales únicos del Chocó
- Realizar pedidos de manera segura
- Seguir el estado de tus envíos
- Contactar directamente con nuestros artesanos

Gracias por unirte a nuestra comunidad.

Saludos cordiales,
El equipo de Tesoros del Chocó";
        
        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendContactFormNotificationAsync(string adminEmail, string customerName, string customerEmail, string message)
    {
        var subject = "Nuevo mensaje de contacto - Tesoros del Chocó";
        var body = $@"Nuevo mensaje de contacto recibido:

Nombre: {customerName}
Email: {customerEmail}
Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}

Mensaje:
{message}

Responder a: {customerEmail}";
        
        return await SendEmailAsync(adminEmail, subject, body);
    }

    public async Task<bool> SendContactMessageConfirmationAsync(string email, string customerName)
    {
        var subject = "Hemos recibido tu mensaje - Tesoros del Chocó";
        var body = $@"Hola {customerName},

Gracias por contactarnos. Hemos recibido tu mensaje y nuestro equipo lo revisará pronto.

Nos pondremos en contacto contigo en las próximas 24-48 horas.

¡Gracias por tu interés en Tesoros del Chocó!

Saludos cordiales,
El equipo de Tesoros del Chocó";
        
        return await SendEmailAsync(email, subject, body);
    }
}

/// <summary>
/// Email configuration settings for dependency injection
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
