namespace TesorosChoco.Domain.Interfaces;

/// <summary>
/// Service interface for email notifications
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send order confirmation email
    /// </summary>
    Task<bool> SendOrderConfirmationAsync(string email, string customerName, int orderId, decimal total);
    
    /// <summary>
    /// Send order status update email
    /// </summary>
    Task<bool> SendOrderStatusUpdateAsync(string email, string customerName, int orderId, string status);
    
    /// <summary>
    /// Send newsletter email
    /// </summary>
    Task<bool> SendNewsletterAsync(string email, string subject, string content);
    
    /// <summary>
    /// Send contact form response
    /// </summary>
    Task<bool> SendContactResponseAsync(string email, string name, string response);
    
    /// <summary>
    /// Send password reset email
    /// </summary>
    Task<bool> SendPasswordResetAsync(string email, string resetToken);
    
    /// <summary>
    /// Send email confirmation
    /// </summary>
    Task<bool> SendEmailConfirmationAsync(string email, string confirmationToken);
}
