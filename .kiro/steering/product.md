# Product Overview

Esh3arTech is a subscription and messaging management platform built on the ABP Framework. The application manages user plans, subscriptions, and real-time messaging capabilities.

## Core Features

- **User Plan Management**: Create and manage subscription plans with pricing tiers (daily, weekly, monthly, annual), trial periods, and expiration policies
- **Subscription System**: Handle user subscriptions with auto-renewal, billing intervals, and subscription history tracking
- **Messaging System**: Real-time message delivery using SignalR with support for one-way messages, broadcast messages, and message status tracking (pending, sent, delivered, read, queued, failed)
- **Mobile User Management**: Track mobile users with registration requests, OTP verification, and online status tracking
- **Feature Management**: ABP feature system for plan-based feature access control

## Domain Concepts

- **UserPlan**: Subscription plans with pricing, trial periods, and expiration fallback plans
- **Subscription**: User subscriptions with billing intervals, renewal history, and payment status
- **Message**: Messages with content types, priorities, and delivery status
- **MobileUser**: Mobile application users with registration status and device information
- **RegistrationRequest**: OTP-based user registration workflow
