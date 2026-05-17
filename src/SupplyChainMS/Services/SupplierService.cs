// ============================================================
// SupplierService.cs — Implementation of ISupplierService
// ============================================================
//
// This is the "real" version of the service — the actual code
// that talks to the database via Entity Framework Core (EF Core).
//
// "Implementing" an interface = providing the actual code for
// each method that was declared in the interface.
//
// This class is registered in Program.cs so that ASP.NET can
// inject it into controllers automatically (Dependency Injection).
//
// EF Core query tips used here:
//   .AsNoTracking()     — tells EF "don't watch this object for changes"
//                         = faster reads when we're NOT going to save changes
//   .Include(x => x.Y) — loads a related table (like a SQL JOIN)
//   .FirstOrDefaultAsync() — returns first match or null (never throws)
//   .SingleOrDefaultAsync() — same, but errors if more than one match

using Microsoft.EntityFrameworkCore;
using SupplyChainMS.Data;
using SupplyChainMS.Models;
using SupplyChainMS.Services.Interfaces;

namespace SupplyChainMS.Services;

public class SupplierService : ISupplierService
{
    // The database context — our gateway to PostgreSQL.
    // Injected automatically by ASP.NET's DI container.
    private readonly AppDbContext _context;

    public SupplierService(AppDbContext context)
    {
        _context = context;
    }

    // -------------------------------------------------------
    // Get a supplier profile by the Identity UserId (string GUID)
    // -------------------------------------------------------
    public async Task<Supplier?> GetByUserIdAsync(string userId)
    {
        // Include(s => s.Products) = also load the supplier's products
        // in the same query (one SQL JOIN, not two queries).
        // AsNoTracking() = we're only reading, not editing, so no need to track.
        return await _context.Suppliers
            .Include(s => s.Products)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    // -------------------------------------------------------
    // Get a supplier by their integer database Id
    // -------------------------------------------------------
    public async Task<Supplier?> GetByIdAsync(int id)
    {
        return await _context.Suppliers
            .Include(s => s.Products.Where(p => p.IsActive))  // only show active products
            .Include(s => s.User)                              // also load the User account
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    // -------------------------------------------------------
    // Get all active suppliers (for StoreManagers to browse)
    // -------------------------------------------------------
    public async Task<List<Supplier>> GetAllActiveAsync()
    {
        return await _context.Suppliers
            .Where(s => s.IsActive)
            // Count products without loading them all into memory.
            // We use a separate property in the view instead.
            .Include(s => s.Products)
            .AsNoTracking()
            .OrderBy(s => s.CompanyName)  // alphabetical order
            .ToListAsync();
    }

    // -------------------------------------------------------
    // Update an existing supplier profile
    // -------------------------------------------------------
    public async Task UpdateProfileAsync(Supplier supplier)
    {
        // _context.Update() tells EF Core: "this object already exists in the DB,
        // mark ALL its fields as modified so they get saved."
        _context.Update(supplier);
        await _context.SaveChangesAsync();
    }

    // -------------------------------------------------------
    // Get all products for a specific supplier
    // -------------------------------------------------------
    public async Task<List<Product>> GetProductsBySupplierIdAsync(int supplierId)
    {
        return await _context.Products
            .Where(p => p.SupplierId == supplierId && p.IsActive)
            .AsNoTracking()
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    // -------------------------------------------------------
    // Get a single product by Id
    // -------------------------------------------------------
    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        return await _context.Products
            .Include(p => p.Supplier)  // also load who owns this product
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId);
    }

    // -------------------------------------------------------
    // Add a new product
    // -------------------------------------------------------
    public async Task AddProductAsync(Product product)
    {
        // _context.Products.Add() = INSERT INTO Products (...)
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }

    // -------------------------------------------------------
    // Update an existing product
    // -------------------------------------------------------
    public async Task UpdateProductAsync(Product product)
    {
        _context.Update(product);
        await _context.SaveChangesAsync();
    }

    // -------------------------------------------------------
    // Delete a product by Id
    // -------------------------------------------------------
    public async Task DeleteProductAsync(int productId)
    {
        // Find the product first, then remove it.
        // We can't delete something EF Core doesn't know about.
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }

    // -------------------------------------------------------
    // Check ownership: does productId belong to supplierId?
    // -------------------------------------------------------
    public async Task<bool> IsProductOwnedBySupplierAsync(int productId, int supplierId)
    {
        // AnyAsync = returns true if at least one row matches, false otherwise.
        // We don't need to load the full product — just check if the row exists.
        return await _context.Products
            .AnyAsync(p => p.Id == productId && p.SupplierId == supplierId);
    }
}
