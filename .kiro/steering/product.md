# Product Overview

Esh3arTech is a messaging platform built on the ABP Framework that provides reliable message delivery services with support for multiple communication channels. currently not implemented.

## Core Features

- **Message Management**: Send one-way messages with retry mechanisms and delivery tracking
- **Multi-channel Support**: WhatsApp integration with extensible provider architecture
- **File Attachments**: Support for media attachments with blob storage
- **User Management**: Mobile user registration with OTP verification
- **Subscription Plans**: Feature-based subscription management with billing intervals
- **Real-time Communication**: SignalR integration for live message delivery
- **Background Processing**: Hangfire-based job processing for reliable message delivery

## Key Business Domains

- **Messages**: Core messaging functionality with status tracking, retry policies, and attachment support
- **Mobile Users**: User registration, verification, and management
- **Plans & Subscriptions**: Feature-based subscription management with renewal tracking
- **Registration**: OTP-based user verification system

## Architecture

The application follows Domain Driven Design (DDD) principles with a layered monolith architecture, built on the ABP Framework for enterprise-grade features like multi-tenancy, permissions, and localization.