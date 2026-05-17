// ============================================================
// IStoreService.cs — Contract for store and inventory operations
// ============================================================
//
// Phase 3 covers two related things:
//   1. Store profile (name, address, contact info)
//   2. Inventory (which products the store stocks, and how many)
//
// The "inventory" in this system is the store's own stock tracker —
// separate from the supplier's StockQuantity (which is what the
// supplier has available to sell). This tracks what's physically
// sitting on the store's shelves.

using SupplyChainMS.Models;

namespace SupplyChainMS.Services.Interfaces;

public interface IStoreService
{
    // -------------------------------------------------------
    // STORE profile operations
    // -------------------------------------------------------

    // Find the store managed by this user (by Identity UserId).
    // Returns null if the user has no store assigned.
    Task<Store?> GetByUserIdAsync(string userId);

    // Find a store by its database Id.
    Task<Store?> GetByIdAsync(int id);

    // Get all active stores (used to show a store directory).
    Task<List<Store>> GetAllActiveAsync();

    // Save changes to a store profile.
    Task UpdateProfileAsync(Store store);

    // Create a brand new store branch.
    Task CreateStoreAsync(Store store);

    // -------------------------------------------------------
    // INVENTORY operations
    // -------------------------------------------------------

    // Get all inventory items for a store, including product + supplier info.
    // Ordered so low-stock items appear first (most urgent at the top).
    Task<List<InventoryItem>> GetInventoryAsync(int storeId);

    // Get a single inventory item by its Id.
    Task<InventoryItem?> GetInventoryItemAsync(int inventoryItemId);

    // Add a product to the store's inventory for the first time.
    Task AddInventoryItemAsync(InventoryItem item);

    // Update the stock quantity and/or low stock threshold for an item.
    Task UpdateInventoryItemAsync(InventoryItem item);

    // Remove a product line from inventory entirely.
    Task RemoveInventoryItemAsync(int inventoryItemId);

    // Check if a product is already tracked in this store's inventory.
    // Used to prevent adding duplicates.
    Task<bool> IsProductInInventoryAsync(int storeId, int productId);

    // Get all products NOT yet in this store's inventory,
    // so the "Add Product" form only shows products the store can still add.
    Task<List<Product>> GetAvailableProductsToAddAsync(int storeId);
}
