# Zarichney API - Personal Website Backend

This repository contains the source code for the backend API powering Steven Zarichney's personal website (zarichney.com). Initially developed to support a sophisticated Cookbook AI application, the API is designed to be extensible for future website features and app development.

## Overview

The Zarichney API is a feature-rich .NET backend handling everything from user authentication and payments to complex AI-driven recipe generation and web scraping. It serves as the engine for interactive applications on the website.

## Key Features

* **Cookbook AI Application:**
    * Processes user cookbook orders based on specific recipes or general criteria.
    * Scrapes recipe data from various web sources using a custom engine (Playwright & selectors).
    * Cleans and standardizes scraped recipe data.
    * Ranks scraped recipes for relevance using AI.
    * Synthesizes new, unique recipes using OpenAI's GPT models, tailored to user preferences and cookbook themes.
    * Analyzes synthesized recipes for quality and alignment with user requests.
    * Generates downloadable PDF cookbooks using QuestPDF.
* **Authentication & Authorization:**
    * Secure user registration with email verification.
    * Password reset functionality.
    * Robust JWT-based authentication using secure, HttpOnly cookies.
    * Refresh token mechanism with sliding expiration.
    * Support for API Key authentication.
    * Role-based access control (e.g., 'admin' role).
* **AI Services:**
    * Integration with OpenAI GPT models for recipe synthesis, analysis, and ranking.
    * Audio transcription capabilities using OpenAI Whisper.
* **Payment Processing:**
    * Integration with Stripe for handling payments for cookbook orders and recipe credits.
    * Webhook handling for payment events.
* **External Integrations:**
    * Microsoft Graph API for sending transactional emails (verification, password reset, cookbook delivery).
    * GitHub API for logging LLM interactions and potentially storing data.
* **System Features:**
    * Background task processing for handling long-running operations like order fulfillment.
    * Session management linked to user authentication.
    * File-based storage for recipe and order data.
    * PostgreSQL database for user identity management (via EF Core).
    * Structured logging with Serilog.
    * Configuration managed via `appsettings.json` and AWS Systems Manager.

## Technology Stack

* **Backend:** .NET / ASP.NET Core
* **Database:** PostgreSQL (for Identity), File System (for Recipe/Order data)
* **Authentication:** ASP.NET Core Identity, JWT, API Keys
* **AI:** OpenAI API (GPT-4o Mini, Whisper)
* **PDF Generation:** QuestPDF
* **Web Scraping:** Playwright
* **Payments:** Stripe API
* **Email:** Microsoft Graph API
* **Logging:** Serilog, Seq (optional)
* **ORM:** Entity Framework Core
* **Other Libraries:** AutoMapper, MediatR, Polly, Handlebars.NET, Octokit

## API Documentation

Detailed API documentation generated via Swagger/OpenAPI is available at the `/api/swagger` endpoint when the application is running.

---

*This is a personal project and is not currently seeking external contributions directly to this GitHub Project.*