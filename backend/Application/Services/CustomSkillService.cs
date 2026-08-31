using JurisApp.Application.Common;
using JurisApp.Application.DTOs.CustomSkills;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Mappings;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Domain.Entities;

namespace JurisApp.Application.Services;

public class CustomSkillService : ICustomSkillService
{
    private readonly ICustomSkillRepository _customSkillRepository;
    private readonly ILawyerProfileRepository _lawyerProfileRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CustomSkillService(
        ICustomSkillRepository customSkillRepository,
        ILawyerProfileRepository lawyerProfileRepository,
        IChatRepository chatRepository,
        IUnitOfWork unitOfWork)
    {
        _customSkillRepository = customSkillRepository;
        _lawyerProfileRepository = lawyerProfileRepository;
        _chatRepository = chatRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomSkillDto>> CreateAsync(Guid userId, CreateCustomSkillRequest request, CancellationToken cancellationToken = default)
    {
        var ownershipError = await EnsureVerifiedLawyerOwnershipAsync(userId, request.LawyerProfileId, cancellationToken);
        if (ownershipError is not null)
        {
            return Result<CustomSkillDto>.Failure(ownershipError);
        }

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Instructions))
        {
            return Result<CustomSkillDto>.Failure(Error.Validation("Nombre e instrucciones son obligatorios."));
        }

        var skill = new CustomSkill(
            Guid.NewGuid(),
            request.LawyerProfileId,
            request.Name,
            request.WhenToUse,
            request.Instructions,
            request.Examples,
            request.RedFlags,
            request.OutputFormat);

        await _customSkillRepository.AddAsync(skill, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CustomSkillDto>.Success(skill.ToDto());
    }

    public async Task<Result<CustomSkillDto>> UpdateAsync(Guid userId, Guid customSkillId, UpdateCustomSkillRequest request, CancellationToken cancellationToken = default)
    {
        var skill = await _customSkillRepository.GetByIdAsync(customSkillId, cancellationToken);
        if (skill is null)
        {
            return Result<CustomSkillDto>.Failure(Error.NotFound("Custom skill no encontrada."));
        }

        var ownershipError = await EnsureVerifiedLawyerOwnershipAsync(userId, skill.LawyerProfileId, cancellationToken);
        if (ownershipError is not null)
        {
            return Result<CustomSkillDto>.Failure(ownershipError);
        }

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Instructions))
        {
            return Result<CustomSkillDto>.Failure(Error.Validation("Nombre e instrucciones son obligatorios."));
        }

        skill.Update(
            request.Name,
            request.WhenToUse,
            request.Instructions,
            request.Examples,
            request.RedFlags,
            request.OutputFormat);

        _customSkillRepository.Update(skill);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CustomSkillDto>.Success(skill.ToDto());
    }

    public async Task<Result<IReadOnlyList<CustomSkillDto>>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var verifiedError = await FolderOwnershipValidator.EnsureVerifiedLawyerAsync(
            userId, _lawyerProfileRepository, cancellationToken);
        if (verifiedError is not null)
            return Result<IReadOnlyList<CustomSkillDto>>.Failure(verifiedError);

        var profile = await _lawyerProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null)
            return Result<IReadOnlyList<CustomSkillDto>>.Failure(Error.NotFound("Perfil de abogado no encontrado."));

        var skills = await _customSkillRepository.GetByLawyerProfileIdAsync(profile.Id, cancellationToken);
        var dtos = skills.Select(s => s.ToDto()).ToList();
        return Result<IReadOnlyList<CustomSkillDto>>.Success(dtos);
    }

    public async Task<Result> ApplyToChatAsync(Guid userId, ApplyCustomSkillToChatRequest request, CancellationToken cancellationToken = default)
    {
        var chatError = await ApplyOrRemoveSkillAsync(userId, request, apply: true, cancellationToken);
        return chatError is null ? Result.Success() : Result.Failure(chatError);
    }

    public async Task<Result> RemoveFromChatAsync(Guid userId, ApplyCustomSkillToChatRequest request, CancellationToken cancellationToken = default)
    {
        var chatError = await ApplyOrRemoveSkillAsync(userId, request, apply: false, cancellationToken);
        return chatError is null ? Result.Success() : Result.Failure(chatError);
    }

    public async Task<Result> DeleteAsync(Guid userId, Guid customSkillId, CancellationToken cancellationToken = default)
    {
        var skill = await _customSkillRepository.GetByIdAsync(customSkillId, cancellationToken);
        if (skill is null)
        {
            return Result.Failure(Error.NotFound("Custom skill no encontrada."));
        }

        var ownershipError = await EnsureVerifiedLawyerOwnershipAsync(userId, skill.LawyerProfileId, cancellationToken);
        if (ownershipError is not null)
        {
            return Result.Failure(ownershipError);
        }

        _customSkillRepository.Delete(skill);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<CustomSkillDto>> SetActiveAsync(
        Guid userId,
        Guid customSkillId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var skill = await _customSkillRepository.GetByIdAsync(customSkillId, cancellationToken);
        if (skill is null)
            return Result<CustomSkillDto>.Failure(Error.NotFound("Custom skill no encontrada."));

        var ownershipError = await EnsureVerifiedLawyerOwnershipAsync(userId, skill.LawyerProfileId, cancellationToken);
        if (ownershipError is not null)
            return Result<CustomSkillDto>.Failure(ownershipError);

        if (isActive)
            skill.Activate();
        else
            skill.Deactivate();

        _customSkillRepository.Update(skill);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CustomSkillDto>.Success(skill.ToDto());
    }

    private async Task<Error?> ApplyOrRemoveSkillAsync(
        Guid userId,
        ApplyCustomSkillToChatRequest request,
        bool apply,
        CancellationToken cancellationToken)
    {
        if (request.ChatId == Guid.Empty || request.CustomSkillId == Guid.Empty)
        {
            return Error.Validation("Chat y skill son obligatorios.");
        }

        var chat = await _chatRepository.GetByIdLightAsync(request.ChatId, cancellationToken);
        if (chat is null)
        {
            return Error.NotFound("Chat no encontrado.");
        }

        if (chat.UserId != userId)
        {
            return Error.Unauthorized("No tenés acceso a este chat.");
        }

        var skill = await _customSkillRepository.GetByIdAsync(request.CustomSkillId, cancellationToken);
        if (skill is null)
        {
            return Error.NotFound("Custom skill no encontrada.");
        }

        var ownershipError = await EnsureVerifiedLawyerOwnershipAsync(userId, skill.LawyerProfileId, cancellationToken);
        if (ownershipError is not null)
        {
            return ownershipError;
        }

        if (apply && !skill.IsActive)
            return Error.Validation("La skill está desactivada.");

        if (apply)
            await _customSkillRepository.ApplyToChatAsync(request.ChatId, request.CustomSkillId, cancellationToken);
        else
            await _customSkillRepository.RemoveFromChatAsync(request.ChatId, request.CustomSkillId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return null;
    }

    private async Task<Error?> EnsureVerifiedLawyerOwnershipAsync(Guid userId, Guid lawyerProfileId, CancellationToken cancellationToken)
    {
        if (lawyerProfileId == Guid.Empty || userId == Guid.Empty)
        {
            return Error.Validation("Identificadores inválidos.");
        }

        var profile = await _lawyerProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null || profile.Id != lawyerProfileId)
        {
            return Error.Unauthorized("No tenés acceso a este perfil de abogado.");
        }

        if (!profile.IsVerifiedLawyer)
            return Error.Unauthorized("Solo los abogados verificados pueden gestionar skills.");

        return null;
    }
}
