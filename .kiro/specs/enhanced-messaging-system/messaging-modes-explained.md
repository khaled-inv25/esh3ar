# Messaging Modes Explained

## Overview

The Esh3arTech messaging system supports two distinct modes of communication:

---

## Mode 1: One-Way Read-Only Messages

### Purpose
Send announcements, alerts, and updates from business to mobile users without expecting replies.

### Participants
- **Sender:** Business User (with send message permission)
- **Recipient:** Mobile User(s)

### Characteristics
- ✅ Read-only (mobile users cannot reply)
- ✅ Real-time delivery if user is online
- ✅ Queued delivery if user is offline
- ✅ Can be sent to single user or broadcast to multiple users
- ✅ No conversation context needed
- ✅ Supports media attachments

### Use Cases
- System announcements
- Promotional messages
- Service alerts
- Account notifications
- Marketing campaigns

### Example Flow
```
Business User creates announcement
    ↓
System checks if mobile user is online
    ↓
If ONLINE: Deliver via SignalR immediately
If OFFLINE: Queue message
    ↓
When user comes online: Deliver queued messages
    ↓
Mobile user views message (read-only, no reply button)
```

---

## Mode 2: Bidirectional Chat

### Purpose
Enable interactive conversations between business users and mobile users for support, sales, and assistance.

### Participants
- **Business User:** Customer support agent, sales rep, account manager
- **Mobile User:** Customer, end-user

### Characteristics
- ✅ Bidirectional (both can send and receive)
- ✅ Conversation context (messages grouped by conversation)
- ✅ Real-time delivery via SignalR
- ✅ Push notifications when offline
- ✅ Message status tracking (sent, delivered, read)
- ✅ Typing indicators
- ✅ Supports media attachments
- ✅ Unread message counts

### Use Cases
- Customer support inquiries
- Sales conversations
- Account assistance
- Technical support
- Order inquiries

### Example Flow
```
Mobile User: "I need help with my order"
    ↓
System creates conversation (Business User ↔ Mobile User)
    ↓
Business User receives message via SignalR
    ↓
Business User: "Sure, what's your order number?"
    ↓
Mobile User receives reply via SignalR
    ↓
Conversation continues...
```

---

## Key Differences

| Aspect | One-Way Messages | Chat Messages |
|--------|-----------------|---------------|
| **Direction** | Business → Mobile (one-way) | Business ↔ Mobile (bidirectional) |
| **Reply Capability** | No (read-only) | Yes (both can reply) |
| **Conversation** | No conversation context | Grouped in conversations |
| **Recipients** | Single or multiple | Single mobile user per conversation |
| **Use Case** | Announcements, alerts | Support, sales, assistance |
| **Delivery** | Real-time or queued | Real-time with push notifications |
| **Status Tracking** | Basic (pending, delivered) | Full (sent, delivered, read) |
| **Typing Indicators** | No | Yes |
| **Unread Counts** | No | Yes |

---

## Data Model Comparison

### One-Way Message
```csharp
{
    "id": "guid",
    "type": "OneWay",
    "recipientPhoneNumber": "+1234567890",
    "subject": "New Feature Available",
    "messageContent": "Check out our new feature...",
    "status": "Delivered",
    "conversationId": null,  // No conversation
    "senderPhoneNumber": null,  // Business user (use CreatorId)
    "createdAt": "2025-01-15T10:30:00Z"
}
```

### Chat Message
```csharp
{
    "id": "guid",
    "type": "Chat",
    "conversationId": "conv-guid",  // Links to conversation
    "senderPhoneNumber": "+1234567890",  // Could be business or mobile
    "recipientPhoneNumber": "+0987654321",
    "messageContent": "How can I help you?",
    "status": "Read",
    "subject": null,  // Not used in chat
    "createdAt": "2025-01-15T10:35:00Z"
}
```

---

## UI/UX Differences

### Mobile User View

#### One-Way Messages Tab
```
┌─────────────────────────────────┐
│ 📢 Announcements                │
├─────────────────────────────────┤
│ New Feature Available           │
│ Check out our new feature...    │
│ 10:30 AM                        │
├─────────────────────────────────┤
│ System Maintenance Notice       │
│ Scheduled maintenance on...     │
│ Yesterday                       │
└─────────────────────────────────┘
[No reply button - read-only]
```

#### Chat Conversations Tab
```
┌─────────────────────────────────┐
│ 💬 Support Chats                │
├─────────────────────────────────┤
│ Customer Support (2 unread)     │
│ Sure, what's your order...      │
│ 10:35 AM                        │
├─────────────────────────────────┤
│ Sales Team                      │
│ You: I'm interested in...       │
│ Yesterday                       │
└─────────────────────────────────┘
[Tap to open conversation and reply]
```

### Business User View

#### Send One-Way Message
```
┌─────────────────────────────────┐
│ Send Announcement               │
├─────────────────────────────────┤
│ Recipients: [Select users]      │
│ Subject: [Enter subject]        │
│ Message: [Enter message]        │
│ [Attach Media]                  │
│                                 │
│ [Send to All] [Send to Selected]│
└─────────────────────────────────┘
```

#### Chat Conversations
```
┌─────────────────────────────────┐
│ Active Conversations (5)        │
├─────────────────────────────────┤
│ John Doe (+1234567890) 🔴       │
│ I need help with my order       │
│ 2 min ago                       │
├─────────────────────────────────┤
│ Jane Smith (+0987654321)        │
│ You: Sure, I can help...        │
│ 1 hour ago                      │
└─────────────────────────────────┘
[Click to open conversation]
```

---

## Permission Model

### One-Way Messages
- **Required Permission:** `Esh3arTechPermissions.Esh3arSendMessages`
- **Who Can Send:** Business users with permission
- **Who Can Receive:** All verified mobile users
- **Who Can Reply:** No one (read-only)

### Chat Messages
- **Required Permission:** `Esh3arTechPermissions.Esh3arChat` (new)
- **Who Can Send:** 
  - Business users with chat permission
  - Mobile users (to business users only)
- **Who Can Receive:**
  - Business users (from mobile users)
  - Mobile users (from business users)
- **Who Can Reply:** Both participants

---

## Implementation Priority

### Phase 1: Keep Existing One-Way (Week 1)
- ✅ Already implemented
- Enhance with media support
- Add better offline queuing

### Phase 2: Add Chat Mode (Weeks 2-4)
- Create Conversation entity
- Add MessageType field
- Implement bidirectional routing
- Build chat UI for both business and mobile

### Phase 3: Enhance Both Modes (Weeks 5-6)
- Add media sharing to both
- Implement push notifications
- Add retry logic
- Performance optimizations

---

## Summary

**One-Way Messages:** Think of these as "announcements" or "broadcasts" - business sends, mobile users read. No conversation, no replies.

**Chat Messages:** Think of these as "customer support tickets" or "sales conversations" - business and mobile user have a back-and-forth conversation.

Both modes coexist in the same system, serving different communication needs!
