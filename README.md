# China vs Gregorian

Single-page web app (Blazor WebAssembly, .NET 10, no MAUI) for converting between the Gregorian calendar and the Chinese lunisolar calendar. Runs in any modern browser on Windows, macOS, Linux, Android and iOS.

The UI is in Dutch.

## Features

- **Chinese birthday** – enter a Gregorian date of birth and a year to see on which Gregorian date the Chinese (lunar) birthday falls in that year, alongside the regular birthday, plus a 10-year overview.
- **Gregorian → Chinese** – Chinese date (month/day, also in characters), zodiac animal, element, yin/yang, stem-branch (干支) and Chinese New Year.
- **Chinese → Gregorian** – including leap months.
- **Chinese New Year** – table covering 12 years; click a row to jump to that year.

Range: 1901 – 2100 (limits of `System.Globalization.ChineseLunisolarCalendar`).

Birthday rules: if the birth day is day 30 and the month has only 29 days that year, day 29 is used. If born in a leap month that does not exist that year, the regular month is used.

## Project structure

```
ChinaVsGregoriaans.slnx
ChinaVsGregoriaans/
  ChinaVsGregoriaans.csproj    Version number (<Version>)
  ChineseCalendarService.cs    Calendar calculations
  CalendarApp.razor            UI
  wwwroot/index.html           Styling, auto-update and cache handling
.github/workflows/deploy.yml   Deployment to GitHub Pages
```

## Running locally

Open `ChinaVsGregoriaans.slnx` in Visual Studio and press F5, or:

```
dotnet run --project ChinaVsGregoriaans
```

App: https://localhost:7331 / http://localhost:5431

> Windows may reserve port ranges (Hyper-V/WSL), which causes `SocketException 10013`. Check with
> `netsh int ipv4 show excludedportrange protocol=tcp` and adjust `Properties/launchSettings.json` if needed.

## Deployment (GitHub Pages)

1. Push to GitHub (branch `main` or `master`).
2. In the repository, set **Settings → Pages → Source** to **GitHub Actions**.
3. Every push publishes automatically via `.github/workflows/deploy.yml`.

The workflow rewrites `<base href>` to match the repository name, creates `404.html` and `.nojekyll`, and writes `version.json` containing the version number from the csproj.

## Versioning and automatic updates

- The version number lives only in `<Version>` in `ChinaVsGregoriaans.csproj` and is shown in the top-right corner. Increase it with every change.
- The app checks `version.json` every 60 seconds and whenever the tab regains focus (bypassing the cache).
- When a new version is found, the page reloads with `?v=<version>`, bypassing the GitHub Pages cache (~10 min).
- WASM files are always revalidated with the server (`cache: 'no-cache'`).
- If loading stalls because of stale HTML, a watchdog reloads the page automatically (max. 2×).
