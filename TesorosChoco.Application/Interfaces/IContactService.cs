using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.DTOs.Responses;

namespace TesorosChoco.Application.Interfaces;

public interface IContactService
{
    Task<GenericResponse> SendContactMessageAsync(ContactRequest request);
}
