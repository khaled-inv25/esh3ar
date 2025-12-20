# Esh3arTech Product Overview

Esh3arTech is a messaging and notification platform built on the ABP Framework. It enables businesses to send messages to mobile users through a subscription-based model.

## Core Features

- **Messaging System**: Send messages to registered mobile users with delivery tracking
- **Mobile User Management**: Registration, OTP verification, and user status tracking
- **Subscription Plans**: Tiered subscription system with feature-based access control
- **Real-time Communication**: SignalR-based hub for online user tracking and live messaging
- **Multi-tenancy**: Support for multiple tenants with isolated data

## Business Domain

- Users subscribe to plans that grant messaging capabilities
- Mobile users register via phone number with OTP verification
- Messages are routed to recipients and tracked for delivery status
- Background jobs handle subscription renewals and email notifications
