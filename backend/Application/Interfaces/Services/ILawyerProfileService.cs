using JurisApp.Application.Common;
using JurisApp.Application.DTOs.LawyerProfiles;
using JurisApp.Domain.Enums;

namespace JurisApp.Application.Interfaces.Services;

public interface ILawyerProfileService
{
    Task<Result<LawyerProfileDto>> CreateVerificationRequestAsync(
        Guid userId,
        CreateLawyerProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<LawyerProfileDto>> UpdateAsync(
        Guid userId,
        UpdateLawyerProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<LawyerProfileDto>> ApproveAsync(
        Guid requestId,
        Guid adminUserId,
        CancellationToken cancellationToken = default);

    Task<Result<LawyerProfileDto>> RejectAsync(
        Guid requestId,
        Guid adminUserId,
        string? reason,
        CancellationToken cancellationToken = default);

    Task<Result<LawyerProfileDto>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LawyerVerificationRequestSummaryDto>>> GetAllRequestsAsync(
        LawyerVerificationStatus? status,
        CancellationToken cancellationToken = default);

    Task<Result<LawyerVerificationRequestDetailDto>> GetRequestDetailAsync(
        Guid requestId,
        CancellationToken cancellationToken = default);
}
