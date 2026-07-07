// ============================================================
// MessagesController.cs — Handles all /Messages/* URLs
// ============================================================
//
// A controller is like a waiter: the browser makes a request, this
// controller receives it, asks the kitchen (MessageService + database)
// for what it needs, and hands back a View (the HTML page).
//
// This controller powers the built-in inbox. Every logged-in user
// (Supplier, StoreManager, Driver) can use it, so we authorize all three.
//
// The screens:
//   /Messages/Inbox           → list of conversations
//   /Messages/Thread?partnerId=..  → the chat with one person + reply box
//   /Messages/Compose         → start a brand-new message
//
// Keep in mind the CLAUDE.md rule: controllers stay THIN. The real work
// (grouping conversations, marking read) lives in MessageService.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SupplyChainMS.Models;
using SupplyChainMS.Services.Interfaces;
using SupplyChainMS.ViewModels;

namespace SupplyChainMS.Controllers;

// All three roles can message each other, so all three are authorized.
[Authorize(Roles = "Supplier,StoreManager,Driver")]
public class MessagesController : Controller
{
    private readonly IMessageService _messageService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<MessagesController> _logger;

    // Dependency injection: ASP.NET hands us these ready-made objects.
    public MessagesController(
        IMessageService messageService,
        UserManager<ApplicationUser> userManager,
        ILogger<MessagesController> logger)
    {
        _messageService = messageService;
        _userManager = userManager;
        _logger = logger;
    }

    // -------------------------------------------------------
    // GET /Messages/Inbox
    // The list of conversations (one row per person you've talked to).
    // -------------------------------------------------------
    public async Task<IActionResult> Inbox()
    {
        ViewData["ActivePage"] = "Messages";

        // GetUserId reads the id straight from the login cookie — no DB hit.
        var userId = _userManager.GetUserId(User)!;

        var conversations = await _messageService.GetInboxAsync(userId);
        return View(conversations);
    }

    // -------------------------------------------------------
    // GET /Messages/Thread?partnerId=abc
    // The full chat with ONE person. Opening it marks their messages read.
    // -------------------------------------------------------
    public async Task<IActionResult> Thread(string partnerId)
    {
        ViewData["ActivePage"] = "Messages";

        if (string.IsNullOrEmpty(partnerId))
        {
            TempData["Error"] = "No conversation selected.";
            return RedirectToAction(nameof(Inbox));
        }

        var userId = _userManager.GetUserId(User)!;

        // Who am I chatting with? (for the header)
        var partner = await _messageService.GetUserAsync(partnerId);
        if (partner == null)
        {
            TempData["Error"] = "That user no longer exists.";
            return RedirectToAction(nameof(Inbox));
        }

        // This call ALSO marks the partner's messages to me as read.
        var messages = await _messageService.GetThreadAsync(userId, partnerId);

        // Pass extra info to the view via ViewBag (small, screen-only values).
        ViewBag.Partner = partner;
        ViewBag.CurrentUserId = userId;

        return View(messages);
    }

    // -------------------------------------------------------
    // POST /Messages/Reply
    // The little reply box at the bottom of a thread posts here.
    // -------------------------------------------------------
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(string receiverId, string content, int? orderId, int? shipmentId)
    {
        var userId = _userManager.GetUserId(User)!;

        // Basic guard: don't save empty replies.
        if (string.IsNullOrWhiteSpace(content) || string.IsNullOrEmpty(receiverId))
        {
            TempData["Error"] = "Message can't be empty.";
            return RedirectToAction(nameof(Thread), new { partnerId = receiverId });
        }

        try
        {
            await _messageService.SendAsync(new Message
            {
                SenderId = userId,
                ReceiverId = receiverId,
                Subject = "Reply",          // replies inherit the ongoing conversation
                Content = content,
                OrderId = orderId,
                ShipmentId = shipmentId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reply to {ReceiverId}", receiverId);
            TempData["Error"] = "Your message couldn't be sent. Please try again.";
        }

        // Back to the same conversation — the new message appears at the bottom.
        return RedirectToAction(nameof(Thread), new { partnerId = receiverId });
    }

    // -------------------------------------------------------
    // GET /Messages/Compose
    // Start a brand-new message. Can be pre-filled via query string, e.g.
    //   /Messages/Compose?to=abc&orderId=45
    // (used by "Message supplier about this order" buttons elsewhere).
    // -------------------------------------------------------
    public async Task<IActionResult> Compose(string? to, int? orderId, int? shipmentId)
    {
        ViewData["ActivePage"] = "Messages";

        var userId = _userManager.GetUserId(User)!;

        // Fill the "To:" dropdown with everyone except me.
        await PopulateContactsAsync(userId, to);

        // If the message is about an order/shipment, show a friendly label.
        string? contextLabel = null;
        if (orderId.HasValue) contextLabel = $"About Order #{orderId}";
        else if (shipmentId.HasValue) contextLabel = $"About Shipment #{shipmentId}";

        return View(new ComposeMessageViewModel
        {
            ReceiverId = to ?? string.Empty,
            OrderId = orderId,
            ShipmentId = shipmentId,
            ContextLabel = contextLabel
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Compose(ComposeMessageViewModel vm)
    {
        ViewData["ActivePage"] = "Messages";

        var userId = _userManager.GetUserId(User)!;

        // Can't message yourself.
        if (vm.ReceiverId == userId)
            ModelState.AddModelError(nameof(vm.ReceiverId), "You can't send a message to yourself.");

        // If validation failed, redraw the form with the errors shown.
        if (!ModelState.IsValid)
        {
            await PopulateContactsAsync(userId, vm.ReceiverId);
            return View(vm);
        }

        try
        {
            await _messageService.SendAsync(new Message
            {
                SenderId = userId,
                ReceiverId = vm.ReceiverId,
                Subject = vm.Subject,
                Content = vm.Content,
                OrderId = vm.OrderId,
                ShipmentId = vm.ShipmentId
            });

            TempData["Success"] = "Message sent!";
            // Jump straight into the conversation we just started.
            return RedirectToAction(nameof(Thread), new { partnerId = vm.ReceiverId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error composing message to {ReceiverId}", vm.ReceiverId);
            TempData["Error"] = "Your message couldn't be sent. Please try again.";
            await PopulateContactsAsync(userId, vm.ReceiverId);
            return View(vm);
        }
    }

    // -------------------------------------------------------
    // Helper: fill ViewBag.Contacts with a dropdown of users.
    // We show "Full Name (Role)" so it's easy to pick the right person.
    // -------------------------------------------------------
    private async Task PopulateContactsAsync(string currentUserId, string? selectedId)
    {
        var contacts = await _messageService.GetContactsAsync(currentUserId);
        ViewBag.Contacts = new SelectList(
            contacts.Select(u => new { u.Id, Display = $"{u.FullName} ({u.Role})" }),
            "Id", "Display", selectedId);
    }
}
