using Calendar.Core.Entities;
using Calendar.Core.Interfaces;
using Calendar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly CalendarDbContext _context;

    public UserRepository(CalendarDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Username == username, ct);
    }

    public async Task<bool> ExistsAsync(string username, CancellationToken ct = default)
    {
        return await _context.Users.AnyAsync(x => x.Username == username, ct);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users.AnyAsync(x => x.Email == email, ct);
    }

    public void Add(User user)
    {
        _context.Users.Add(user);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
