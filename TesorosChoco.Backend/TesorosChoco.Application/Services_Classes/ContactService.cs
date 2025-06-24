using AutoMapper;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.DTOs.Responses;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Application.Services;

public class ContactService : IContactService
{
    private readonly IContactMessageRepository _contactMessageRepository;
    private readonly IMapper _mapper;

    public ContactService(IContactMessageRepository contactMessageRepository, IMapper mapper)
    {
        _contactMessageRepository = contactMessageRepository;
        _mapper = mapper;
    }

    public async Task<GenericResponse> SubmitContactMessageAsync(ContactRequest request)
    {
        try
        {
            var contactMessage = new ContactMessage
            {
                Name = request.Name,
                Email = request.Email,
                Subject = request.Subject,
                Message = request.Message,
                CreatedAt = DateTime.UtcNow
            };

            await _contactMessageRepository.CreateAsync(contactMessage);

            return new GenericResponse
            {
                Success = true,
                Message = "Your message has been sent successfully. We will get back to you soon."
            };
        }
        catch (Exception)
        {
            return new GenericResponse
            {
                Success = false,
                Message = "There was an error sending your message. Please try again later."
            };
        }
    }
}
