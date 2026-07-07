// ============================================================
// IMessageService.cs — Contract for the inbox / messaging system
// ============================================================
//
// This is like a simple email system built into the app.
// Any user can message any other user (Supplier ↔ StoreManager ↔ Driver).
//
// The MENTAL MODEL to understand messaging:
//
//   A single "Message" row is one note from ONE person to ANOTHER.
//
//   A "Conversation" (or "thread") is NOT a table — it's just ALL the
//   messages exchanged between two specific people, shown in time order.
//   We calculate conversations on the fly by grouping messages by
//   "who is the OTHER person in this message?".
//
//   The "Inbox" is a summary list — one row per person you've talked to,
//   showing the latest message and how many you haven't read yet.
//   (Exactly like WhatsApp's chat list or Gmail's inbox.)
//
// An interface is a CONTRACT: it lists WHAT the service can do,
// but not HOW. The controller depends on this interface, and ASP.NET
// injects the real MessageService at runtime (dependency injection).

using SupplyChainMS.Models;
using SupplyChainMS.ViewModels;

namespace SupplyChainMS.Services.Interfaces;

public interface IMessageService
{
    // -------------------------------------------------------
    // READ
    // -------------------------------------------------------

    // Build the inbox: one summary per conversation partner,
    // newest conversation first, with unread counts.
    Task<List<ConversationSummary>> GetInboxAsync(string currentUserId);

    // Get every message exchanged between the current user and one other
    // person, oldest → newest (chat order). As a SIDE EFFECT this also
    // marks the messages the current user RECEIVED as "read".
    Task<List<Message>> GetThreadAsync(string currentUserId, string partnerId);

    // How many unread messages does this user have in total?
    // Used for the little red badge in the sidebar (like email).
    Task<int> GetUnreadCountAsync(string currentUserId);

    // Everyone the current user is allowed to message (everyone but themself).
    // Used to fill the "To:" dropdown on the compose form.
    Task<List<ApplicationUser>> GetContactsAsync(string currentUserId);

    // Look up a single user by id (to show "Message to: Jane" headers).
    Task<ApplicationUser?> GetUserAsync(string userId);

    // -------------------------------------------------------
    // WRITE
    // -------------------------------------------------------

    // Save a new message to the database and return it.
    Task<Message> SendAsync(Message message);
}
