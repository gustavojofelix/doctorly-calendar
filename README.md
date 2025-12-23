# Calendar Tech Test (Minimal APIs, .NET 10, EF Core, SQLite)

## Overview

A simple event calendar API supporting event CRUD, attendee management, filters, and search. Includes a notification abstraction (console implementation) and optimistic concurrency via RowVersion.

## Architecture

- `Calendar.Domain`: Aggregate + domain rules (CalendarEvent, Attendee)
- `Calendar.Application`: Use cases (EventService), DTOs, abstractions (repository, notifications)
- `Calendar.Infrastructure`: EF Core DbContext + repository + notification implementation
- `Calendar.Api`: Minimal APIs + Swagger

## Assumptions

- Authentication/authorization is out of scope for the assessment.
- Cancel = soft cancel (EventStatus = Cancelled), not hard delete.
- Notifications are implemented as an interface + console logger; production could use email/iCal/message bus/outbox.
- Concurrency uses RowVersion optimistic locking; update/cancel requires the RowVersion from the last read.

## Run locally

```bash
dotnet restore
dotnet ef database update -p src/Calendar.Infrastructure -s src/Calendar.Api
dotnet run --project src/Calendar.Api
```

## Scope & prioritisation

Given the time-boxed nature of this assessment, I focused on delivering a complete vertical slice of functionality: clear domain modelling, EF Core persistence, API endpoints, basic testing, and documentation. The goal was to demonstrate approach, structure, and decision-making rather than exhaustive feature completeness. The areas below outline what I would implement next if more time were available.

## Next steps (with more time)

If I had additional time beyond the assessment window, I would extend the solution in the following areas:

1. Paging & richer filtering

   - Add pagination (`page`, `pageSize`) and sorting parameters.
   - Expand filtering (status, attendee email, title contains, time overlap queries).
   - Return a standard paged response contract (items + totalCount + page metadata).

2. Input validation

   - Introduce FluentValidation for request DTOs (title/description length, time ranges, attendee email format).
   - Standardize validation error responses (ProblemDetails) across all endpoints.

3. Reliable notifications (Outbox pattern)

   - Implement an Outbox table persisted in the same transaction as event changes.
   - Add a background worker to publish notifications (email/iCal/message bus) with retries and idempotency.

4. Authentication & RBAC

   - Add authentication (JWT/OIDC) and role-based access control.
   - Introduce tenant/user context if needed (depending on product requirements).

5. Testing & performance hardening
   - Add more integration tests covering concurrency, edge cases, and negative paths.
   - Add contract tests for the API surface and schema.
   - Add basic load/performance tests for list/search endpoints and EF query tuning.

## Run locally

Clone this repo from the root of the project run the following commands:

```bash
dotnet restore
dotnet ef database update -p src/Calendar.Infrastructure -s src/Calendar.Api
dotnet run --project src/Calendar.Api
```
