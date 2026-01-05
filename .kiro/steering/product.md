# Product Overview

Esh3arTech is a messaging platform built on the ABP Framework that provides reliable message delivery services with support for multiple communication channels.

## Core Features

- **Message Management**: Send one-way messages with attachment support
- **Mobile User Management**: User registration, verification, and tracking
- **Plan & Subscription System**: Feature-based subscription management
- **Message Reliability**: Retry mechanisms with exponential backoff for failed messages
- **Real-time Communication**: SignalR integration for live updates
- **Multi-channel Support**: WhatsApp and email integration

## Key Business Domains

- **Messages**: Core messaging functionality with status tracking, attachments, and delivery confirmation
- **Mobile Users**: User registration, OTP verification, and online status tracking
- **Plans & Subscriptions**: Feature-based subscription management with renewal tracking
- **Background Processing**: Message ingestion, retry handling, and subscription renewals

## Architecture Approach

The application follows Domain Driven Design (DDD) principles with a layered architecture, emphasizing message reliability and scalability through event-driven patterns and background processing.