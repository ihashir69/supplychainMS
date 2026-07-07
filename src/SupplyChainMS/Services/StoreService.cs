// ============================================================
// StoreService.cs — Implementation of IStoreService
// ============================================================
//
// Handles all database queries for stores and inventory.
// Uses EF Core with .Include() to do SQL JOINs automatically.
//
// Key EF Core pattern used here:
//   .ThenInclude() — for loading a "grandchild" relationship.
//   Example: InventoryItem → Product → Supplier
//   Normal .Include() only goes one level deep.
//   .ThenInclude() lets you go further:
//     .Include(i => i.Product)
//         .ThenInclude(p => p.Supplier)

using Microsoft.EntityFrameworkCore;
using SupplyChainMS.Data;
using SupplyChainMS.Models;
using SupplyChainMS.Services.Interfaces;

namespace SupplyChainMS.Services;

public class StoreService : IStoreService
{
    private readonly AppDbContext _context;

    public StoreService(AppDbContext context)
    {
        _context = context;
    }

    // -------------------------------------------------------
    // Get store by the manager's Identity UserId (string GUID)
    // -------------------------------------------------------
    public async Task<Store?> GetByUserIdAsync(string userId)
    {
        return await _context.Stores
            .Include(s => s.Manager)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ManagerUserId == userId);
    }

    // -------------------------------------------------------
    // Get store by its integer database Id
    // -------------------------------------------------------
    public async Task<Store?> GetByIdAsync(int id)
    {
        return await _context.Stores
            .Include(s => s.Manager)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    // -------------------------------------------------------
    // Get all active stores
    // -------------------------------------------------------
    // We also load InventoryItems here (just the quantity/threshold
    // numbers, no Product/Supplier details needed) so the branch
    // directory cards can show a quick stock-level summary without
    // a separate database trip per store.
    public async Task<List<Store>> GetAllActiveAsync()
    {
        return await _context.Stores
            .Where(s => s.IsActive)
            .Include(s => s.Manager)
            .Include(s => s.InventoryItems)
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    // -------------------------------------------------------
    // Create a new store branch
    // -------------------------------------------------------
    public async Task CreateStoreAsync(Store store)
    {
        store.CreatedAt = DateTime.UtcNow;
        _context.Stores.Add(store);
        await _context.SaveChangesAsync();
    }

    // -------------------------------------------------------
    // Save changes to a store profile
    // -------------------------------------------------------
    public async Task UpdateProfileAsync(Store store)
    {
        _context.Update(store);
        await _context.SaveChangesAsync();
    }

    // -------------------------------------------------------
    // Get all inventory for a store, with product + supplier info
    // -------------------------------------------------------
    public async Task<List<InventoryItem>> GetInventoryAsync(int storeId)
    {
        return await _context.InventoryItems
            .Where(i => i.StoreId == storeId)
            // Load Product (so we can show name, price, unit)
            .Include(i => i.Product)
                // Load the Product's Supplier (so we know who supplies it)
                .ThenInclude(p => p.Supplier)
            .AsNoTracking()
            // Sort: items AT or BELOW threshold come first (most urgent)
            // then alphabetically by product name
            .OrderBy(i => i.QuantityInStock > i.LowStockThreshold)
            .ThenBy(i => i.Product.Name)
            .ToListAsync();
    }

    // -------------------------------------------------------
    // Get a single inventory item
    // -------------------------------------------------------
    public async Task<InventoryItem?> GetInventoryItemAsync(int inventoryItemId)
    {
        return await _context.InventoryItems
            .Include(i => i.Product)
                .ThenInclude(p => p.Supplier)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == inventoryItemId);
    }

    // -------------------------------------------------------
    // Add a product to inventory for the first time
    // -------------------------------------------------------
    public async Task AddInventoryItemAsync(InventoryItem item)
    {
        item.LastUpdated = DateTime.UtcNow;
        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync();
    }

    // -------------------------------------------------------
    // Update stock quantity and/or threshold
    // -------------------------------------------------------
    public async Task UpdateInventoryItemAsync(InventoryItem item)
    {
        item.LastUpdated = DateTime.UtcNow;
        _context.Update(item);
        await _context.SaveChangesAsync();
    }

    // -------------------------------------------------------
    // Remove a product from inventory entirely
    // -------------------------------------------------------
    public async Task RemoveInventoryItemAsync(int inventoryItemId)
    {
        var item = await _context.InventoryItems.FindAsync(inventoryItemId);
        if (item != null)
        {
            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    // -------------------------------------------------------
    // Check if a product is already in this store's inventory
    // -------------------------------------------------------
    public async Task<bool> IsProductInInventoryAsync(int storeId, int productId)
    {
        return await _context.InventoryItems
            .AnyAsync(i => i.StoreId == storeId && i.ProductId == productId);
    }

    // -------------------------------------------------------
    // Get products not yet in the store's inventory
    // -------------------------------------------------------
    public async Task<List<Product>> GetAvailableProductsToAddAsync(int storeId)
    {
        // First, get all productIds already tracked in this store
        var trackedProductIds = await _context.InventoryItems
            .Where(i => i.StoreId == storeId)
            .Select(i => i.ProductId)  // SELECT ProductId only (no full object load)
            .ToListAsync();

        // Then get all active products NOT in that list, with their supplier info
        return await _context.Products
            .Where(p => p.IsActive && !trackedProductIds.Contains(p.Id))
            .Include(p => p.Supplier)
            .AsNoTracking()
            .OrderBy(p => p.Supplier.CompanyName)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }
}
