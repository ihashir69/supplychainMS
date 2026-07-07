// ============================================================
// MessageService.cs — Implementation of IMessageService
// ============================================================
//
// This is the "brain" of the inbox. The controller stays thin and just
// calls these methods; all the real logic (grouping messages into
// conversations, marking things read, counting unread) lives here.

using Microsoft.EntityFrameworkCore;
using SupplyChainMS.Data;
using SupplyChainMS.Models;
using SupplyChainMS.Services.Interfaces;
using SupplyChainMS.ViewModels;

namespace SupplyChainMS.Services;

public class MessageService : IMessageService
{
    private readonly AppDbContext _context;

    public MessageService(AppDbContext context)
    {
        _context = context;
    }

    // -------------------------------------------------------
    // Build the inbox list (one row per conversation partner)
    // -------------------------------------------------------
    public async Task<List<ConversationSummary>> GetInboxAsync(string currentUserId)
    {
        // STEP 1: Pull every message where I'm EITHER the sender OR receiver.
        // .Include() loads the related Sender/Receiver user rows too, so we
        // can show names without extra database trips.
        // AsNoTracking() = read-only, faster (we're not editing these).
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
            .AsNoTracking()
            .OrderByDescending(m => m.SentAt)   // newest first
            .ToListAsync();

        // STEP 2: Group the flat list into conversations.
        // The "key" of each group is THE OTHER PERSON in the message:
        //   - if I sent it, the other person is the Receiver
        //   - if I received it, the other person is the Sender
        // So all messages with the same partner land in the same group.
        var conversations = messages
            .GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
            .Select(group =>
            {
                var partnerId = group.Key;

                // group is already newest-first, so First() = latest message
                var latest = group.First();

                // Figure out which side of the latest message is the partner
                var partner = latest.SenderId == partnerId ? latest.Sender : latest.Receiver;

                return new ConversationSummary
                {
                    PartnerId = partnerId,
                    PartnerName = partner?.FullName ?? "Unknown user",
                    PartnerRole = partner?.Role ?? "",
                    LastSubject = latest.Subject,
                    LastMessage = latest.Content,
                    LastSentAt = latest.SentAt,
                    LastFromMe = latest.SenderId == currentUserId,

                    // Unread = messages in this conversation that I received
                    // and haven't opened yet.
                    UnreadCount = group.Count(m => m.ReceiverId == currentUserId && !m.IsRead)
                };
            })
            .OrderByDescending(c => c.LastSentAt)   // most recent conversation on top
            .ToList();

        return conversations;
    }

    // -------------------------------------------------------
    // Get the full conversation with one person + mark as read
    // -------------------------------------------------------
    public async Task<List<Message>> GetThreadAsync(string currentUserId, string partnerId)
    {
        // Notice: NO AsNoTracking() here. We WANT EF Core to track these
        // rows because we're about to change some (mark them read) and save.
        var thread = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Include(m => m.Order)       // optional context link
            .Include(m => m.Shipment)    // optional context link
            .Where(m =>
                (m.SenderId == currentUserId && m.ReceiverId == partnerId) ||
                (m.SenderId == partnerId && m.ReceiverId == currentUserId))
            .OrderBy(m => m.SentAt)       // oldest first = chat order (top → bottom)
            .ToListAsync();

        // Mark the messages I RECEIVED (and haven't read) as read now that
        // I'm looking at them. This is what makes the unread badge go away.
        var justRead = thread.Where(m => m.ReceiverId == currentUserId && !m.IsRead).ToList();
        if (justRead.Count > 0)
        {
            foreach (var m in justRead)
            {
                m.IsRead = true;
                m.ReadAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }

        return thread;
    }

    // -------------------------------------------------------
    // Total unread count (for the sidebar badge)
    // -------------------------------------------------------
    public async Task<int> GetUnreadCountAsync(string currentUserId)
    {
        return await _context.Messages
            .CountAsync(m => m.ReceiverId == currentUserId && !m.IsRead);
    }

    // -------------------------------------------------------
    // Everyone I can message (all users except me)
    // -------------------------------------------------------
    public async Task<List<ApplicationUser>> GetContactsAsync(string currentUserId)
    {
        // _context.Users comes from IdentityDbContext — it's the AspNetUsers table.
        return await _context.Users
            .Where(u => u.Id != currentUserId)
            .AsNoTracking()
            .OrderBy(u => u.Role).ThenBy(u => u.FullName)
            .ToListAsync();
    }

    // -------------------------------------------------------
    // Look up one user (for the "Message to: X" header)
    // -------------------------------------------------------
    public async Task<ApplicationUser?> GetUserAsync(string userId)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    // -------------------------------------------------------
    // Send (save) a new message
    // -------------------------------------------------------
    public async Task<Message> SendAsync(Message message)
    {
        // Stamp the send time and mark it unread for the recipient.
        message.SentAt = DateTime.UtcNow;
        message.IsRead = false;
        message.ReadAt = null;

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }
}
