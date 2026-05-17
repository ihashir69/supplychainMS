// ============================================================
// ISupplierService.cs — The "contract" for supplier operations
// ============================================================
//
// An "interface" in C# is like a promise or a menu.
// It says: "Whoever implements me MUST provide these methods."
//
// Why use an interface instead of calling the database directly in the controller?
//   - Separation of concerns: Controller asks WHAT to do. Service figures out HOW.
//   - Testability: You can swap the real service for a fake one in tests.
//   - Clarity: Reading the interface tells you everything the service can do,
//     without having to read the implementation code.
//
// Think of it like an electrical socket:
//   The interface = the socket shape (contract)
//   The service   = the actual electrical system behind the wall (implementation)
//   The controller= the device you plug in (the consumer)

using SupplyChainMS.Models;

namespace SupplyChainMS.Services.Interfaces;

public interface ISupplierService
{
    // -------------------------------------------------------
    // SUPPLIER operations
    // -------------------------------------------------------

    // Get a supplier profile by the UserId (Identity user's Id string).
    // Returns null if this user doesn't have a supplier profile.
    // Used to find "my profile" when a Supplier logs in.
    Task<Supplier?> GetByUserIdAsync(string userId);

    // Get a supplier by their database Id number.
    // Returns null if not found.
    Task<Supplier?> GetByIdAsync(int id);

    // Get all active suppliers (IsActive = true).
    // Used by StoreManagers to browse the supplier catalog.
    Task<List<Supplier>> GetAllActiveAsync();

    // Save changes to an existing supplier profile.
    // The supplier object must already exist in the DB (it has an Id).
    Task UpdateProfileAsync(Supplier supplier);

    // -------------------------------------------------------
    // PRODUCT operations
    // -------------------------------------------------------

    // Get all products for a given supplier.
    // "supplierId" is the Supplier table's Id (int), not the UserId (string).
    Task<List<Product>> GetProductsBySupplierIdAsync(int supplierId);

    // Get a single product by its Id.
    // Returns null if not found.
    Task<Product?> GetProductByIdAsync(int productId);

    // Add a brand new product to the database.
    Task AddProductAsync(Product product);

    // Save changes to an existing product.
    Task UpdateProductAsync(Product product);

    // Delete a product permanently from the database.
    // (We only call this if the product has no order history.)
    Task DeleteProductAsync(int productId);

    // Security check: does this product belong to this supplier?
    // Used to prevent a supplier from editing another supplier's products.
    // Returns true if the productId's SupplierId matches the given supplierId.
    Task<bool> IsProductOwnedBySupplierAsync(int productId, int supplierId);
}
