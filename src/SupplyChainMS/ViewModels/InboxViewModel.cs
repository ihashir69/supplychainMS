// ============================================================
// InboxViewModel.cs — Data shaped for the messaging screens
// ============================================================
//
// Remember the rule from CLAUDE.md: NEVER hand raw database entities
// to a view. Instead we build small "ViewModel" classes that carry
// exactly the data the screen needs, in a ready-to-display shape.
//
// This file holds TWO small classes used by the inbox:
//   1. ConversationSummary — one row in the inbox list
//   2. ComposeMessageViewModel — the "new message" form
//
// (Keeping both in one file because they're tiny and closely related.)

using System.ComponentModel.DataAnnotations;

namespace SupplyChainMS.ViewModels;

// -------------------------------------------------------
// ConversationSummary
// -------------------------------------------------------
// One entry in the inbox list — like one chat row in WhatsApp.
// It summarises the whole back-and-forth with ONE other person:
// who they are, the last thing said, and how many you haven't read.
public class ConversationSummary
{
    // The OTHER person's user id (not you). Clicking the row opens
    // the full thread with this person via /Messages/Thread?partnerId=...
    public string PartnerId { get; set; } = string.Empty;

    // The other person's display name and role (for a nice label + badge)
    public string PartnerName { get; set; } = string.Empty;
    public string PartnerRole { get; set; } = string.Empty;

    // A preview of the most recent message in this conversation
    public string LastSubject { get; set; } = string.Empty;
    public string LastMessage { get; set; } = string.Empty;

    // When the last message was sent (to show "2h ago" style timestamps)
    public DateTime LastSentAt { get; set; }

    // Was the last message sent BY me? (so we can show "You: ...")
    public bool LastFromMe { get; set; }

    // How many messages in this conversation I haven't opened yet.
    // 0 = nothing new. > 0 = show a red badge.
    public int UnreadCount { get; set; }
}

// -------------------------------------------------------
// ComposeMessageViewModel
// -------------------------------------------------------
// Backs the "write a new message" form. The [Required] / [StringLength]
// attributes are VALIDATION RULES — ASP.NET checks them automatically
// and shows error messages if the user leaves something blank.
public class ComposeMessageViewModel
{
    // Who am I sending to? (their user id, chosen from a dropdown)
    [Required(ErrorMessage = "Please choose who to send this to.")]
    [Display(Name = "To")]
    public string ReceiverId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a subject.")]
    [StringLength(150)]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please type a message.")]
    [StringLength(2000)]
    [Display(Name = "Message")]
    public string Content { get; set; } = string.Empty;

    // Optional context links — a message can be "about" a specific
    // order or shipment. These come in via the URL (e.g. a
    // "Message supplier about this order" button on the order page).
    public int? OrderId { get; set; }
    public int? ShipmentId { get; set; }

    // A friendly label shown at the top of the form when the message
    // is pre-linked to something, e.g. "About Order #45".
    public string? ContextLabel { get; set; }
}
