using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JurisApp.Infrastructure.Persistence.Repositories;

public class FolderRepository : IFolderRepository
{
    private readonly AppDbContext _context;

    public FolderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Folders.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Folder>> GetByLawyerProfileIdAsync(Guid lawyerProfileId, CancellationToken cancellationToken = default)
        => await _context.Folders
            .Where(f => f.LawyerProfileId == lawyerProfileId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Folder folder, CancellationToken cancellationToken = default)
        => await _context.Folders.AddAsync(folder, cancellationToken);

    public void Update(Folder folder)
        => _context.Folders.Update(folder);

    public void Delete(Folder folder)
        => _context.Folders.Remove(folder);
}