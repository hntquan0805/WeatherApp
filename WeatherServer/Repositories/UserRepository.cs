using Microsoft.EntityFrameworkCore;
using WeatherApp.Models;
using WeatherApp.API.Repositories.Interfaces;

namespace WeatherApp.API.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(WeatherContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email) =>
        await _dbSet
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<User?> GetByUsernameAsync(string username) =>
        await _dbSet
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

    public async Task<bool> EmailExistsAsync(string email) =>
        await _dbSet
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<bool> UsernameExistsAsync(string username) =>
        await _dbSet
            .AnyAsync(u => u.Username.ToLower() == username.ToLower());
}