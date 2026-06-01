# Logistic Project

Logistic Project is a modular ASP.NET Core backend for freight forwarding and shipment operations. It covers customer onboarding, carrier and route management, pricing, quotation workflows, shipment lifecycle automation, invoicing, document handling, and external rate-import integrations.

## Live Demo

The GitHub live demo points to the backend Swagger UI:

https://unmultipliable-kelsey-unloyal.ngrok-free.dev/swagger

## Features

- Shipment lifecycle management across logistics states such as created, client confirmed, booking requested, booking confirmed, shipping instructions submitted, draft bill of lading received/approved, payment pending/completed, telex released, delivered, closed, cancelled, and on hold.
- Role-based shipment commands for Admin, Staff, User, and Integration workflows.
- Pricing engine for carriers, routes, container types, active rates, validity windows, and searchable rate catalogs.
- Quotation workflow that calculates final prices from active rates and quote items, then preserves shipment snapshots from accepted quotes.
- Invoice and charge workflows with subtotal, tax, total amount, payer type, payment status, cancellation, partial payment, refund, and unique invoice numbering.
- Shipment item management for cargo details including weight, volume, hazardous cargo, required temperature, and marks/numbers.
- Shipment document upload with document types, file validation, local storage, ClamAV scanning, integration message linking, and soft delete.
- External rate import endpoint with integration key validation, idempotency metadata, duplicate message detection, transactional item processing, and per-item import results.
- Authentication with ASP.NET Core Identity, JWT access tokens, hashed refresh token rotation, email confirmation, and Twilio phone OTP verification.
- API rate limiting, centralized exception handling, FluentValidation request validation, AutoMapper DTO mapping, EF Core global soft-delete filters, pagination, filtering, sorting, and SQL Server indexes.

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- ASP.NET Core Identity
- Entity Framework Core
- SQL Server
- FluentValidation
- AutoMapper
- JWT Bearer Authentication
- SendGrid
- Twilio Verify
- ClamAV via nClam
- Swagger / OpenAPI

## Architecture

The solution follows a layered backend structure:

```text
Domain/
  Entities, enums, and domain exceptions

Application/
  DTOs, interfaces, validation rules, query models, and application rules

Infrastructure/
  EF Core DbContext, repositories, services, migrations, integrations, and persistence logic

API/
  Controllers, middleware, filters, dependency injection, authentication, rate limiting, and mapping
```

Key patterns used in the project:

- Repository pattern for persistence boundaries
- Unit of Work for coordinated transactional operations
- Application rules for shipment status, tracking, rate, quote, carrier, and port validation
- Global query filters for soft-deleted entities
- DTO mapping between API contracts and domain entities
- Role-based authorization at controller and action level

## API Areas

The API includes endpoints for:

- Auth: register, login, confirm email, confirm phone, refresh token, logout, logout all
- User Profile: profile retrieval, profile update, password update, email change confirmation, phone change verification
- Customers: customer profile creation, update, delete, current-user profile, admin/staff search
- Shipping Core: carriers, ports, routes, and container types
- Pricing: rates, active-rate toggling, searchable pricing catalog
- Quotation: quote creation, lookup by customer, lookup by route, details, deletion
- Shipments: create, update, delete, user shipments, admin/staff shipments, shipment timeline
- Shipment Lifecycle: confirm client, request booking, confirm booking, submit shipping instructions, receive/approve draft BL, mark payment pending, confirm payment, release telex, complete delivery, close, hold, resume, cancel
- Shipment Tracking: booking number, vessel, voyage, checkpoint, ETA/ETD, ATA/ATD
- Shipment Items: cargo items and weight/volume details
- Shipment Charges: operational charges and payer classification
- Invoices: invoice creation, cancellation, payment status updates, refund handling
- Shipment Documents: upload, list, retrieve, and delete documents
- Integrations: rate import for external sources such as N8N, carrier APIs, and email imports

## Prerequisites

- .NET 10 SDK
- SQL Server
- EF Core CLI tools
- Optional: ClamAV service for document scanning
- Optional: SendGrid account for email delivery
- Optional: Twilio Verify service for phone OTP
- Optional: Lookuptax API credentials for tax verification

Install EF Core tools if needed:

```bash
dotnet tool install --global dotnet-ef
```

## Configuration

Set the required configuration values in `API/appsettings.json`, user secrets, or environment variables.

Important settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=LogisticDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "replace-with-a-secure-signing-key",
    "Issuer": "https://localhost:7100",
    "Audience": "https://localhost:7100",
    "ExpiryMinutes": 60
  },
  "Integrations": {
    "N8n": {
      "ApiKey": "replace-with-integration-key"
    }
  },
  "SendGridSettings": {
    "ApiKey": "replace-with-sendgrid-key",
    "FromEmail": "no-reply@example.com",
    "ReplyTo": "support@example.com",
    "FromName": "Logistics App"
  },
  "Twilio": {
    "AccountSid": "replace-with-account-sid",
    "AuthToken": "replace-with-auth-token",
    "VerifyServiceSid": "replace-with-verify-service-sid",
    "PhoneNumber": "replace-with-phone-number"
  },
  "TaxVerification": {
    "Provider": "Lookuptax",
    "Lookuptax": {
      "BaseUrl": "replace-with-base-url",
      "ApiKey": "replace-with-api-key",
      "TimeoutInSeconds": 10
    }
  },
  "ClamAV": {
    "Host": "localhost",
    "Port": 3310
  }
}
```

Do not commit production secrets to source control.

## Database

Apply migrations using the API project as the startup project:

```bash
dotnet ef database update --project Infrastructure --startup-project API
```

To add a new migration:

```bash
dotnet ef migrations add MigrationName --project Infrastructure --startup-project API
```

## Running Locally

Restore packages:

```bash
dotnet restore
```

Build the solution:

```bash
dotnet build
```

Run the API:

```bash
dotnet run --project API
```

Swagger is enabled in development. Open the URL shown in the terminal and navigate to `/swagger`.

## Development Notes

- Keep domain entities and enums in `Domain`.
- Put DTOs, interfaces, validators, query parameters, and application rules in `Application`.
- Implement repository and service logic in `Infrastructure`.
- Keep controllers thin in `API`; controllers should delegate business behavior to services.
- Add FluentValidation validators for public request DTOs.
- Add EF Core indexes for high-traffic filters and lookup fields.
- Use transactions for multi-entity changes such as rate import, active-rate changes, and financial operations.
- Respect shipment lifecycle rules before modifying shipment items, charges, invoices, or tracking fields.

## Useful Commands

```bash
dotnet restore
dotnet build
dotnet run --project API
dotnet ef database update --project Infrastructure --startup-project API
```

## License

This project is licensed under the MIT License. See `LICENSE` for details.
