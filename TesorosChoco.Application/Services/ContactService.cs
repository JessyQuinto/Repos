using AutoMapper;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.DTOs.Responses;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Application.Services;

/// <summary>
/// Contact service implementation for handling contact messages
/// Stores contact messages and provides basic validation
/// </summary>
public class ContactService : IContactService
{
    private readonly IContactMessageRepository _contactMessageRepository;
    private readonly IMapper _mapper;

    public ContactService(IContactMessageRepository contactMessageRepository, IMapper mapper)
    {
        _contactMessageRepository = contactMessageRepository ?? throw new ArgumentNullException(nameof(contactMessageRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<GenericResponse> SendContactMessageAsync(ContactRequest request)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name is required");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required");

            if (string.IsNullOrWhiteSpace(request.Message))
                throw new ArgumentException("Message is required");

            // Create contact message entity
            var contactMessage = new ContactMessage
            {
                Name = request.Name.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                Subject = request.Subject?.Trim() ?? string.Empty,
                Message = request.Message.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _contactMessageRepository.CreateAsync(contactMessage);

            return new GenericResponse
            {
                Success = true,
                Message = "Your message has been sent successfully. We'll get back to you soon!"
            };
        }
        catch (Exception ex)
        {
            return new GenericResponse
            {
                Success = false,
                Message = $"Error sending message: {ex.Message}"
            };        }
    }
}
