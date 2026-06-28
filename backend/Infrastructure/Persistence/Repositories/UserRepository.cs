using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JurisApp.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    => await _context.Users.ToListAsync(cancellationToken);
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Users.FirstOrDefaultAsync(
            u => u.Email.ToLower() == email.ToLower(),
            cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Users.AnyAsync(
            u => u.Email.ToLower() == email.ToLower(),
            cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, Guid excludeUserId, CancellationToken cancellationToken = default)
        => await _context.Users.AnyAsync(
            u => u.Email.ToLower() == email.ToLower() && u.Id != excludeUserId,
            cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await _context.Users.AddAsync(user, cancellationToken);

    public void Update(User user)
        => _context.Users.Update(user);

    public void Delete(User user)
        => _context.Users.Remove(user);
}