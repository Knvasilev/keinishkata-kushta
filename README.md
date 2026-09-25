# Keinishkata Kushta

A Bulgarian guest-house website built with ASP.NET Core MVC and Entity Framework
Core. The project combines a bilingual public website with an authenticated admin
area for managing accommodation, photographs, enquiries, and availability.

This is an actively developed portfolio project, not an online payment or instant
booking service. No guest database, admin account, or runtime uploads are included.

## Features

- Bulgarian and English public interface with a responsive layout.
- Room listings, euro prices, room details, and cover-image management.
- Room and property galleries with click/swipe navigation and lazy-loaded images.
- Nearby attractions and a contact form that stores enquiries for the admin.
- Whole-property availability with in-place month navigation using AJAX.
- Admin login with hashed passwords, cookie authentication, login rate limiting,
  failed-attempt lockout, and antiforgery-protected form submissions.
- Admin management of rooms, gallery images, contact messages, and blocked dates.

## Technical Decisions

**MVC and Razor:** controllers load data and pass view models to server-rendered
views. Small JavaScript enhancements add interaction without a separate SPA.

**Calendar updates:** JavaScript `fetch()` requests a Razor partial and replaces
only the calendar. The address bar and surrounding page remain unchanged. Failed
requests preserve the existing month; ordinary links remain usable without JS.

**Availability policy:** one guest group occupies the property at a time, even
when using only one room. Departure is exclusive, so another stay can begin on
checkout day. Overlap validation and optimistic concurrency protect admin edits.

**Separation of concerns:** the data project owns entities, EF configuration, and
migrations. The web project owns controllers, view models, views, and static assets.

## Stack and Structure

- C# / .NET 8, ASP.NET Core MVC, Razor
- EF Core 8 with SQL Server / LocalDB
- Bootstrap, CSS, JavaScript; jQuery validation for forms
- Dependency-free Node.js interaction tests and a .NET console test harness

```text
keinishkata-kushta/
├── KeinishkataKushta.sln               # Visual Studio solution
├── KeinishkataKushta/                  # ASP.NET Core MVC web project
│   ├── Areas/
│   │   └── Admin/
│   │       ├── Controllers/           # Login and content management
│   │       ├── Models/                # Admin form and list view models
│   │       └── Views/                 # Admin Razor views and layout
│   ├── Controllers/                   # Public page and form endpoints
│   ├── Infrastructure/                # Translations, currency, calendar helpers
│   ├── Models/                        # Public page view models
│   ├── Views/
│   │   ├── Availability/              # Calendar page and AJAX partial
│   │   ├── Gallery/                   # Property photo gallery
│   │   ├── Home/                      # Homepage, contact, and privacy
│   │   ├── Places/                    # Nearby attractions
│   │   ├── Rooms/                     # Room listings and details
│   │   └── Shared/                    # Layout and shared partials
│   ├── wwwroot/
│   │   ├── css/                       # Public and admin styles
│   │   ├── images/                    # Branding and social icons
│   │   ├── js/                        # Gallery and calendar interactions
│   │   └── lib/                       # Third-party frontend libraries
│   ├── appsettings.example.json       # Safe local configuration template
│   ├── KeinishkataKushta.Web.csproj    # Web project dependencies
│   └── Program.cs                     # Services, middleware, and routes
├── KeinishkataKushta.Data/
│   ├── Entities/                      # EF Core entity classes
│   ├── Migrations/                    # Database schema history
│   ├── AppDbContext.cs                # Entity mappings and database sets
│   ├── AvailabilityRules.cs           # Reservation overlap rules
│   └── KeinishkataKushta.Data.csproj   # Data project dependencies
├── tests/
│   ├── Availability.Tests/            # .NET checks and isolated visual preview
│   ├── availability-navigation.test.cjs
│   └── gallery-interaction.test.cjs
├── AVAILABILITY.md                    # Calendar policy and manual test guide
├── SOCIAL-ICONS.md                     # Icon sources and attribution
└── README.md
```

## Run Locally

Prerequisites: .NET 8 SDK and SQL Server. The simplest setup is Windows with SQL
Server Express LocalDB installed. Node.js with the built-in test runner is needed
only for the JavaScript tests.

Run these commands from the folder containing the solution, after cloning:

```powershell
# Fresh clone only: create local settings without overwriting an existing file.
if (-not (Test-Path .\KeinishkataKushta\appsettings.json)) {
    Copy-Item .\KeinishkataKushta\appsettings.example.json .\KeinishkataKushta\appsettings.json
}
dotnet restore .\KeinishkataKushta.sln
dotnet build .\KeinishkataKushta.sln --no-restore
dotnet dev-certs https --trust
dotnet run --project .\KeinishkataKushta\KeinishkataKushta.Web.csproj --launch-profile https
```

Open <https://localhost:7015>. If those ports are in use, stop your other local
instance or adjust your launch settings before starting.

The example uses a separate `KeinishkataKushtaPortfolio` database with Windows
authentication. Startup automatically applies migrations. Point it only at a
development database you intend to initialize or update. For another SQL Server,
set `ConnectionStrings:DefaultConnection` using .NET User Secrets or an environment
variable; never commit database credentials. HTTPS is required for admin cookies.

For a fresh database, visit <https://localhost:7015/Admin/Setup> locally to choose
your own admin username and strong password. Setup is available only in Development,
from a loopback connection, while no admin account exists. There are no default
credentials. Afterwards use <https://localhost:7015/Admin>.

Create and publish sample rooms through Admin, then upload photos you own. A fresh
database starts without room listings or reservations. Contact submissions are
stored in the database; the application does not currently send enquiry emails.

## Tests

From the solution folder:

```powershell
dotnet run --project .\tests\Availability.Tests\Availability.Tests.csproj
node --test .\tests\availability-navigation.test.cjs .\tests\gallery-interaction.test.cjs
```

The .NET checks cover date boundaries, overlap rules, SQL translation, model and
migration consistency, validation, and authorization attributes. JavaScript tests
cover gallery gestures and calendar navigation, focus, and error recovery.

These do not replace database-backed integration tests, a deployment security
review, or manual browser checks. See [AVAILABILITY.md](AVAILABILITY.md) for the
manual checklist and a sample-data calendar preview that needs no database.

## Status and Next Steps

- Availability is manually maintained, not synchronized with booking platforms.
- Online reservation requests and payments are not implemented.
- A reviewed set of desktop/mobile screenshots is planned for this README.
- Production deployment, backups, and a full security review remain separate work.

## Assets and Credits

The project was developed with AI-assisted coding and iterative testing.

Bundled Bootstrap, jQuery, and validation libraries retain their license files.
Social icons are from Icons8; attribution details are in
[SOCIAL-ICONS.md](SOCIAL-ICONS.md). Keep the required icon credit.

Guest-house branding and photographs are project-specific assets. Their inclusion
does not grant permission to reuse them. No open-source license has been selected
for the original project code; third-party components retain their own licenses.
