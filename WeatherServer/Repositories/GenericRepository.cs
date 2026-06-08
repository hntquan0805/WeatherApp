using Microsoft.EntityFrameworkCore;
using WeatherApp.Models;
using WeatherApp.API.Repositories.Interfaces;

namespace WeatherApp.API.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly WeatherContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(WeatherContext context)
    {
        _context = context;
        _dbSet   = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) =>
        await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() =>
        await _dbSet.ToListAsync();

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity is not null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _dbSet.FindAsync(id) is not null;
}