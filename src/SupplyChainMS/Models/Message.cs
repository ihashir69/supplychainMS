// ============================================================
// Message.cs — An inbox message between two users
// ============================================================
//
// This is like a simple email system built into the app.
// Any user can send a message to any other user.
// Messages can optionally be linked to an Order or Shipment
// to give context (e.g., "About your order #45: when will it arrive?")

namespace SupplyChainMS.Models;

public class Message
{
    public int Id { get; set; }

    // Foreign Key → who sent this message? (their UserId)
    public string SenderId { get; set; } = string.Empty;

    // Foreign Key → who is receiving this message? (their UserId)
    public string ReceiverId { get; set; } = string.Empty;

    // The actual message text
    public string Content { get; set; } = string.Empty;

    // Short subject line (like email subject)
    public string Subject { get; set; } = string.Empty;

    // Has the recipient read this message yet?
    public bool IsRead { get; set; } = false;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // When did the recipient read it? (null until they do)
    public DateTime? ReadAt { get; set; }

    // -------------------------------------------------------
    // Optional Context Links
    // These let you attach a message to an order or shipment.
    // Both are nullable — messages don't HAVE to be linked to anything.
    // -------------------------------------------------------

    // Optional: is this message about a specific order?
    public int? OrderId { get; set; }

    // Optional: is this message about a specific shipment?
    public int? ShipmentId { get; set; }

    // -------------------------------------------------------
    // Navigation Properties
    // -------------------------------------------------------

    public ApplicationUser Sender { get; set; } = null!;
    public ApplicationUser Receiver { get; set; } = null!;

    // These might be null (message doesn't have to be about an order/shipment)
    public Order? Order { get; set; }
    public Shipment? Shipment { get; set; }
}
