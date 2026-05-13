# Contributing

Thank you for contributing to Logistic Project. This backend contains business-critical logistics workflows, so changes should be small, intentional, and easy to review.

## Getting Started

1. Clone the repository.
2. Install the .NET 10 SDK.
3. Configure local secrets for SQL Server, JWT, integrations, SendGrid, Twilio, Lookuptax, and ClamAV.
4. Restore dependencies:

```bash
dotnet restore
```

5. Apply database migrations:

```bash
dotnet ef database update --project Infrastructure --startup-project API
```

6. Build the solution:

```bash
dotnet build
```

7. Run the API:

```bash
dotnet run --project API
```

## Branching

Use short, descriptive branch names:

```text
feature/shipment-documents
fix/invoice-payment-rule
refactor/rate-import-service
docs/readme-setup
```

## Commit Style

Use clear commits that explain the change:

```text
feat: add shipment document upload validation
fix: prevent invoices for cancelled shipments
refactor: split shipment lifecycle service
docs: update setup instructions
```

## Architecture Guidelines

Keep changes aligned with the current layered structure:

- `Domain`: entities, enums, and domain exceptions only.
- `Application`: DTOs, interfaces, validators, query models, and business rule helpers.
- `Infrastructure`: EF Core, repositories, service implementations, migrations, external providers, and persistence logic.
- `API`: controllers, filters, middleware, dependency injection, rate limiting, authentication, and mapping.

Controllers should stay thin. Put business behavior in services and shared rules in `Application/ApplicationRules`.

## Feature Workflow

For a new backend feature:

1. Add or update domain entities/enums when the data model changes.
2. Add DTOs and request/response contracts in `Application/DTOs`.
3. Add service and repository interfaces in `Application/Interfaces`.
4. Add FluentValidation validators for public request models.
5. Implement repositories and services in `Infrastructure`.
6. Add EF Core configuration and migrations when persistence changes.
7. Add API endpoints in the matching controller.
8. Register new services and repositories in `API/Program.cs`.
9. Update AutoMapper mappings in `API/Mapping/MappingProfile.cs`.
10. Update documentation when behavior, setup, or API surface changes.

## Business Rules

Respect existing logistics rules:

- Shipment status transitions must go through `ShipmentStatusRules`.
- Shipment tracking updates must go through `ShipmentTrackingRules`.
- Rates must honor active state, valid date ranges, allowed currencies, and one-active-rate behavior per carrier/route/container combination.
- Quotes must be created from active, currently valid rates.
- Shipments created from quotes must preserve quote snapshot data.
- Shipment items, charges, invoices, and tracking updates must respect the current shipment status.
- Invoice operations must respect payment status and shipment status.
- Integration imports must preserve idempotency and transactional processing.

## Validation

Use FluentValidation for API request validation. Validators should:

- Reject missing required fields.
- Validate enum values.
- Validate date ordering.
- Validate numeric ranges.
- Normalize or validate codes such as carrier SCAC codes, port codes, and currency codes.
- Keep user-facing error messages clear and specific.

## Database Changes

When changing persistence:

- Add or update entity configuration classes under `Infrastructure/Data/Configuration`.
- Add indexes for fields used in search, filtering, sorting, joins, or uniqueness checks.
- Preserve global soft-delete behavior where appropriate.
- Use `DeleteBehavior.Restrict` or `DeleteBehavior.NoAction` for relationships where accidental cascade delete would be dangerous.
- Create migrations with descriptive names.

Migration command:

```bash
dotnet ef migrations add MigrationName --project Infrastructure --startup-project API
```

## Security Guidelines

- Do not commit secrets, API keys, JWT signing keys, connection strings for production, certificates, or private credentials.
- Use role-based authorization for admin, staff, user, and integration workflows.
- Validate ownership before returning customer-specific shipments, invoices, charges, items, documents, or quotes.
- Keep refresh tokens hashed at rest.
- Keep integration endpoints protected by integration keys and idempotency metadata.
- Validate and scan uploaded files before storing them.

## Pull Request Checklist

Before opening a pull request:

- The solution builds successfully with `dotnet build`.
- New request models have validators.
- New services and repositories are registered in `API/Program.cs`.
- New DTO/entity mappings are added to `MappingProfile`.
- Database changes include migrations.
- Business rules are enforced in services or application rules.
- Sensitive configuration is not committed.
- README or API documentation is updated when behavior changes.

## Code Style

- Follow existing C# style and naming conventions.
- Keep methods focused and readable.
- Prefer explicit business-rule methods over duplicated inline conditions.
- Use async EF Core APIs for database work.
- Use `DateTimeOffset.UtcNow` for persisted timestamps.
- Keep comments short and useful.

## Reporting Issues

When reporting a bug, include:

- What endpoint or workflow failed.
- Expected behavior.
- Actual behavior.
- Request payload or reproduction steps.
- Relevant status, role, shipment status, invoice status, or integration source.
- Error response or log snippet with secrets removed.
