# Availability Calendar

## Policy

Only one guest group may stay at a time. Every reservation blocks the entire
property, even if the group uses only one bedroom. Maintenance closures also block
the property. "Rooms included" and notes are private descriptive fields; they do
not change availability. These fields have no dependency on room records, so
renaming or deleting a room cannot accidentally release occupied nights.

Arrival is inclusive and departure is exclusive. For example, 10-12 October blocks
the nights of 10 and 11 October. A second group may arrive on 12 October. This
assumes checkout and cleaning finish before the next check-in; add extra blocked
nights when needed.

The calendar covers the current month and the following 11 months. Dates use
Bulgarian local time. Entries outside the visible calendar are still stored and
participate in overlap checking. Admin can also enter an already-started stay.

This is manual availability, not online booking or synchronization with other
platforms. Unblocked future nights appear available, subject to confirmation.
Enter existing phone/platform reservations before relying on the public calendar.
No sample reservations have been inserted into the database.

## Start and Migrate

Open the updated `KeinishkataKushta.sln`, rebuild, and start the web project.
The existing `Program.cs` already calls `Database.MigrateAsync()` at startup, so
it will apply `20260918161952_AddPropertyAvailability` automatically. No additional
migration needs to be generated. Back up a production database before deploying.

Alternatively, from the folder containing `KeinishkataKushta.sln`:

```powershell
dotnet ef database update --project .\KeinishkataKushta.Data\KeinishkataKushta.Data.csproj --startup-project .\KeinishkataKushta\KeinishkataKushta.Web.csproj
```

If `dotnet ef` is not installed:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.11
```

Public calendar: `/Availability`. The header's green action and the footer link
lead there. Room details also link to the shared calendar.

Admin: `/Admin/Availability`, using the existing admin login. Add, edit, and remove
reservations or maintenance closures there. Removal requires a confirmation page
and an antiforgery-protected POST. The public calendar has no booking forms and
receives no private notes or room-use information.

## Manual Checks

Month navigation uses JavaScript `fetch()` (AJAX) and a Razor partial view.
Only the calendar is replaced; the page and address stay unchanged. Each change
fetches fresh availability. Loading temporarily disables navigation; an error
keeps the current month and lets the visitor retry. Without JavaScript, the
original navigation links still work with a full-page request. No React or new
database migration is needed for this enhancement.

1. Sign in, open `/Admin/Availability`, and add a future two-night reservation.
   Enter a single room in "Rooms included". Verify both nights are occupied on
   `/Availability`, while checkout day remains free.
2. Attempt another reservation for the same dates with a different room name.
   Saving should show an overlap error. A stay beginning on checkout day should
   be accepted.
3. Edit the first stay to different free dates. The old nights should become free
   and the new nights occupied. A conflicting edit should be rejected.
4. Add a maintenance closure. Its nights should appear unavailable, not occupied.
5. Remove a test entry through its confirmation page. Its dates should be free
   again. Only remove disposable entries created for this check.
6. Open the same entry in two tabs. Save a change in one, then try saving or
   deleting from the stale tab. The stale operation should show a warning without
   overwriting the newer entry; reopen it from the list to continue.
7. Check a month boundary, next/previous month, Today, Bulgarian/English, and a
   narrow phone screen. Two calendars show on desktop, one on mobile. Statuses use
   symbols and accessible text as well as colour.
8. In a signed-out browser, visit `/Admin/Availability`; it must require login.
   The public page must never show private notes or room-use descriptions.
9. Try saving overlapping entries simultaneously from two tabs. One should be
   rejected or asked to retry, not create an overlapping stay. This requires a
   real SQL Server/LocalDB instance to verify transaction behaviour end to end.
10. Open `/Availability`, then use Next, Previous, and Today repeatedly. The
    address should remain unchanged, with no document reload or scroll reset.
    Repeat in English and on mobile. In DevTools Network, month changes should
    appear as fetch requests, not new document requests. Test offline after the
    page loads: the current month should remain visible with an error message;
    reconnect and try again.

## Automated Checks

From the solution folder:

```powershell
dotnet run --project .\tests\Availability.Tests\Availability.Tests.csproj
node --test .\tests\gallery-interaction.test.cjs
node --test .\tests\availability-navigation.test.cjs
```

The availability checks cover interval rules, maintenance, leap days, month/year
boundaries, validation, SQL translation, migration/model consistency, privacy of
the public projection, and admin authorization/antiforgery attributes. They do not
connect to a real database or replace the SQL concurrency and login checks above.
The JavaScript navigation tests cover partial replacement, repeated navigation,
rapid clicks, timeout/error recovery, keyboard focus, and native-link fallbacks.

A separate test-only visual host renders the real public Razor view with sample
dates and no database access:

```powershell
dotnet run --project .\tests\Availability.Tests\Availability.Tests.csproj -- --preview --contentRoot "$PWD\KeinishkataKushta"
```

It listens only on `http://127.0.0.1:5192/Availability`, blocks other actions, and
is not the real booking calendar. Stop it with Ctrl+C after previewing.

## Files Added or Changed

Paths below are relative to the solution folder. Build output and restore caches
are excluded.

- `KeinishkataKushta.Data/Entities/AvailabilityBlock.cs`
- `KeinishkataKushta.Data/AvailabilityRules.cs`
- `KeinishkataKushta.Data/AppDbContext.cs`
- `KeinishkataKushta.Data/Migrations/20260918161952_AddPropertyAvailability.cs`
- `KeinishkataKushta.Data/Migrations/20260918161952_AddPropertyAvailability.Designer.cs`
- `KeinishkataKushta.Data/Migrations/AppDbContextModelSnapshot.cs`
- `KeinishkataKushta/Infrastructure/AvailabilityCalendar.cs`
- `KeinishkataKushta/Infrastructure/SiteText.cs`
- `KeinishkataKushta/Models/AvailabilityViewModel.cs`
- `KeinishkataKushta/Controllers/AvailabilityController.cs`
- `KeinishkataKushta/Views/Availability/Index.cshtml`
- `KeinishkataKushta/Views/Availability/_Calendar.cshtml`
- `KeinishkataKushta/wwwroot/css/availability.css`
- `KeinishkataKushta/wwwroot/js/availability.js`
- `KeinishkataKushta/Views/Shared/_Layout.cshtml`
- `KeinishkataKushta/Views/Rooms/Details.cshtml`
- `KeinishkataKushta/Areas/Admin/Models/AvailabilityFormViewModel.cs`
- `KeinishkataKushta/Areas/Admin/Controllers/AvailabilityController.cs`
- `KeinishkataKushta/Areas/Admin/Views/Availability/Index.cshtml`
- `KeinishkataKushta/Areas/Admin/Views/Availability/Form.cshtml`
- `KeinishkataKushta/Areas/Admin/Views/Availability/Delete.cshtml`
- `KeinishkataKushta/Areas/Admin/Views/Shared/_Layout.cshtml`
- `tests/Availability.Tests/Availability.Tests.csproj`
- `tests/Availability.Tests/Program.cs`
- `tests/Availability.Tests/PreviewHost.cs`
- `tests/availability-navigation.test.cjs`
- `AVAILABILITY.md`
