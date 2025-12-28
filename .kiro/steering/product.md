# Product Overview

Esh3arTech is a messaging and subscription management platform built on ABP Framework. The system enables:

- **One-way messaging**: Send messages (with optional attachments) to mobile users via Custom mobile app
- **Subscription management**: Manage user plans, subscriptions, billing intervals, and renewals
- **User registration**: Mobile user registration with OTP verification
- **Real-time communication**: SignalR hub for tracking online mobile users and message delivery
- **Message tracking**: Queue, deliver, and track message status with retry mechanisms

## Core Business Domains

- **Messages**: One-way messaging with attachment support, status tracking, and delivery confirmation
- **Plans & Subscriptions**: Feature-based plans with billing intervals, renewal history, and expiration management
- **Mobile Users**: User registration, verification, and connection tracking
- **Registrations**: OTP-based registration flow for mobile users

## Key Features

- Multi-language support (Arabic/English)
- Permission-based access control
- Background workers for subscription renewals
- Distributed event bus (RabbitMQ) for message processing
- Blob storage for file attachments
- Email and SMS provider integration
