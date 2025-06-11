# WORK IN PROGRESS - Not yet functional

# CoreAccess User Manager

Ein schlanker, containerisierter User- & Rollenmanager mit REST-API, Vue-Frontend und pluggable Datenbank (SQLite oder PostgreSQL). Ideal für Projekte, die User-Verwaltung, Login und Rechteverwaltung benötigen.

## 🚀 Features

- 🧑‍💼 Benutzerverwaltung mit JWT-Auth
- 🔐 Rollen & Rechte (optional erweiterbar)
- 🧩 SQLite oder PostgreSQL automatisch wählbar
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
  -e COREACCESS_DB_CONNECTION="Host=host;Port=5432;Database=db;Username=user;Password=pass" \
  --name coreaccess \
  ghcr.io/bexy315/coreaccess:latest
```

Frontend: http://localhost:8080  
API: http://localhost:8080/api

---

## ⚙️ Environment Variablen

| Variable                    | Beschreibung                             | Default          |
|-----------------------------|------------------------------------------|------------------|
| `COREACCESS_DB_CONNECTION`  | PostgreSQL ConnectionString (wenn nötig) | -                |
| `COREACCESS_ADMIN_USERNAME` | Initialer Admin-Login (optional)         | root             |
| `COREACCESS_ADMIN_PASSWORD` | Initiales Passwort (optional)            | changeme123      |

Wenn keine `COREACCESS_DB_CONNECTION`-Variable gesetzt ist, wird automatisch SQLite verwendet.

---

## 🧪 Beispiel Login mit SDK (JavaScript) (---W.I.P.---)

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
git clone https://github.com/bexy315/coreaccess.git
cd coreaccess

docker build -t coreaccess .
```

---

## 📚 API Referenz

Die API ist unter `/api` verfügbar. Eine API-Doku gibt es im WIKI bereich.

---

## ✅ Lizenz

MIT License – Feel free to use and contribute.

---

## 🧠 Inspiration / Ziele

- Firebase-ähnliche Login-Integration via SDK
- "Plug & Play"-Userverwaltung für Microservices
- Vollständig eigenständig lauffähiger Container
