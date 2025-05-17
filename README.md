# CoreAccess User Manager

Ein schlanker, containerisierter User- & Rollenmanager mit REST-API, Vue-Frontend und pluggable Datenbank (SQLite oder PostgreSQL). Ideal für Projekte, die User-Verwaltung, Login und Rechteverwaltung benötigen.

## 🚀 Features

- 🧑‍💼 Benutzerverwaltung mit JWT-Auth
- 🔐 Rollen & Rechte (optional erweiterbar)
- 🧩 SQLite oder PostgreSQL per ENV wählbar
- 📦 Als einzelner Docker-Container deploybar
- 🎨 Modernes Vue 3 + PrimeVue Admin-Frontend
- ⚙️ REST API unter `/api`
- 🧰 JavaScript/TypeScript SDK für einfache Integration

---

## 🐳 Quickstart via Docker

```bash
docker run -d \
  -p 8080:80 \
  -v ./data:/app/data \
  -e COREACCESS_DB_TYPE=sqlite \
  --name coreaccess \
  ghcr.io/dein-user/coreaccess:latest
```

Oder mit PostgreSQL:

```bash
docker run -d \
  -p 8080:80 \
  -e COREACCESS_DB_TYPE=postgres \
  -e COREACCESS_DB_CONNECTION="Host=host;Port=5432;Database=db;Username=user;Password=pass" \
  --name coreaccess \
  ghcr.io/dein-user/coreaccess:latest
```

Frontend: http://localhost:8080  
API: http://localhost:8080/api

---

## ⚙️ Environment Variablen

| Variable                    | Beschreibung                                | Default    |
|----------------------------|---------------------------------------------|------------|
| `COREACCESS_DB_TYPE`       | `sqlite` oder `postgres`                    | `sqlite`   |
| `COREACCESS_DB_CONNECTION` | PostgreSQL ConnectionString (wenn nötig)    | -          |
| `COREACCESS_ADMIN_EMAIL`   | Initialer Admin-Login (optional)            | -          |
| `COREACCESS_ADMIN_PASSWORD`| Initiales Passwort (optional)               | -          |

---

## 🧪 Beispiel Login mit SDK (JavaScript)

```bash
npm install @coreaccess/sdk
```

```ts
import { createUserManager } from '@coreaccess/sdk';
import { coreAccessConfig } from './coreaccess.config';

const userManager = createUserManager(coreAccessConfig);

await userManager.login("admin@example.com", "mypassword");
```

Beispiel-Konfiguration:

```ts
export const coreAccessConfig = {
  endpoint: "http://localhost:8080",
  apiKey: "xyz-123",
  clientId: "myfrontend",
};
```

---

## 🖥️ Admin UI

Über das integrierte Vue-Frontend kannst du:

- Benutzer anlegen, bearbeiten, löschen
- Rollen definieren (optional)
- API-Keys & Projekte verwalten
- SDK-Konfig generieren für andere Projekte

---

## 📦 Build selbst durchführen

```bash
git clone https://github.com/dein-user/coreaccess.git
cd coreaccess

docker build -t coreaccess .
```

---

## 📚 API Referenz

Die API ist unter `/api` verfügbar. Eine OpenAPI/Swagger-Doku folgt.

---

## ✅ Lizenz

MIT License – Feel free to use and contribute.

---

## 🧠 Inspiration / Ziele

- Firebase-ähnliche Login-Integration via SDK
- "Plug & Play"-Userverwaltung für Microservices
- Vollständig eigenständig lauffähiger Container
