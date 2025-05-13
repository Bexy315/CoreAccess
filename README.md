# CoreAccess

CoreAccess ist ein leichtgewichtiges, konfigurierbares User- und Role-Management für ASP.NET Core Web APIs.  
Es lässt sich in wenigen Zeilen in der `Program.cs` aktivieren und bietet eine einfache, moderne Alternative zu ASP.NET Core Identity — basierend auf JWT Tokens, ohne unnötigen Overhead.

---

## ⚙️ Features

- 🔐 Benutzer- und Rollenverwaltung
- 🔧 Konfigurierbare Datenquelle: SQLite-Datei oder PostgreSQL
- 🔑 Authentifizierung via JWT Token
- 🛡️ Autorisierungs-Attribute für Controller/Actions
- 🧩 Einfach integrierbar via NuGet-Package
- 🧪 Inklusive Sample-Projekt zum Testen & Ausprobieren

---

## 🚀 Installation

Installiere das NuGet-Paket über die .NET CLI:

```bash
dotnet add package CoreAccess
```

---

## 🧪 Quick Start

Füge in deiner `Program.cs` folgende Zeile hinzu:

```csharp
builder.Services.AddCoreAccess(options =>
{
    options.UsePostgreSql("Host=localhost;Database=coreaccess;Username=postgres;Password=yourpw");
    // oder:
    // options.UseFileBasedSqlite("data/coreaccess.db");

    options.EnableRoles = true;
    options.JwtSecret = "your-super-secret-key";
    options.JwtIssuer = "your-api";
    options.JwtAudience = "your-client";
});
```

Und aktiviere die Middleware:

```csharp
app.UseCoreAccess();
```

---

## 📦 Sample-Projekt

Ein vollständiges Beispielprojekt ist im Repository enthalten unter:

```
/samples/CoreAccess.SampleApi
```

Dort kannst du CoreAccess direkt ausprobieren, inklusive vorgefüllter Beispieldaten und CRUD-Endpunkten.

---

## 🛠 Konfiguration

Verfügbare Optionen:

- `UsePostgreSql(string connectionString)`
- `UseFileBasedSqlite(string filePath)`
- `EnableRoles` (bool)
- `JwtSecret` (string)
- `JwtIssuer` (string)
- `JwtAudience` (string)
- `AllowedRoles` (string[])

---

## 📁 Beispiel: appsettings.json

CoreAccess lässt sich auch über die `appsettings.json` konfigurieren.  
Siehe `/samples/CoreAccess.SampleApi/appsettings.json` für ein Beispiel.

---

## 🔒 Endpunkt-Schutz via Attribute

Nutze das `[CoreAccessAuthorize]` Attribut, um Endpunkte abzusichern:

```csharp
[CoreAccessAuthorize]
[HttpGet("secure-data")]
public IActionResult GetSecureData() => Ok("Only authenticated users can see this");
```

---

## 📄 Lizenz

Dieses Projekt steht unter der MIT-Lizenz.  
Details siehe [LICENSE](https://github.com/Bexy315/CoreAccess/blob/main/LICENSE).

---

## 🤝 Beiträge willkommen

Pull Requests, Bug Reports und Feature-Vorschläge sind herzlich willkommen!  
Siehe `CONTRIBUTING.md` (in Vorbereitung).

---

## 📫 Kontakt

Projekt von [@Bexy315](https://github.com/Bexy315)
