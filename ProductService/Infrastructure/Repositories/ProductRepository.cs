using Domain;
using Infrastructure.EF;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure.Repositories;

public sealed class ProductRepository(ProductDbContext db) : IProductRepository
{
    private readonly ProductDbContext _db = db;

    public Task<Product?> GetByIdAsync(long productId, bool includeInactive = false)
    {
        return _db.Products.FirstOrDefaultAsync(p =>
            p.Id == productId && (includeInactive || p.IsActive));
    }

    public Task<List<Product>> GetAllAsync(bool includeInactive = false)
    {
        return _db.Products
            .Where(p => includeInactive || p.IsActive)
            .OrderBy(p => p.Id)
            .ToListAsync();
    }

    public async Task AddAsync(Product product)
    {
        await _db.Products.AddAsync(product);
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(long productId, bool includeInactive = false)
    {
        return _db.Products.AnyAsync(p =>
            p.Id == productId && (includeInactive || p.IsActive));
    }
}
