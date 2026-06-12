using JurisApp.Application.Common;
using JurisApp.Application.DTOs.LawyerProfiles;

namespace JurisApp.Application.Interfaces.Services;

public interface ILawyerProfileService
{
    Task<Result<LawyerProfileDto>> CreateAsync(Guid userId, CreateLawyerProfileRequest request, CancellationToken cancellationToken = default);
    Task<Result<LawyerProfileDto>> UpdateAsync(Guid userId, UpdateLawyerProfileRequest request, CancellationToken cancellationToken = default);
    Task<Result<LawyerProfileDto>> VerifyAsync(VerifyLawyerRequest request, CancellationToken cancellationToken = default);
    Task<Result<LawyerProfileDto>> RejectAsync(RejectLawyerRequest request, CancellationToken cancellationToken = default);
    Task<Result<LawyerProfileDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
