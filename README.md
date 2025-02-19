# Pollitic

Pollitic to kompleksowa aplikacja webowa służąca do prezentacji wyników sondaży, analizy danych politycznych oraz wizualizacji kompozycji parlamentu i wykresów poglądów partii. Projekt składa się z części backendowej, frontendowej oraz zestawu testów integracyjnych i end‑to‑end.

---

## Spis treści

- [Funkcjonalności](#funkcjonalności)
- [Architektura](#architektura)
- [Wymagania](#wymagania)
- [Instalacja](#instalacja)
- [Konfiguracja](#konfiguracja)
- [Uruchomienie](#uruchomienie)
- [Testy](#testy)
- [CI/CD](#cicd)
- [Struktura projektu](#struktura-projektu)
- [Licencja](#licencja)

---

## Funkcjonalności

- **Backend API**: Aplikacja backendowa napisana w .NET, korzystająca z bazy MySQL do przechowywania danych o sondażach, krajach, partiach oraz grupach.
- **Frontend**: Interaktywny interfejs użytkownika zbudowany przy użyciu Angular, prezentujący dane z backendu w formie wykresów, list i map.
- **Wizualizacje**: Prezentacja wykresów poparcia, wykresów poglądów partii oraz kompozycji parlamentu.
- **Popup Informacyjny**: Pełnoekranowy, przewijany popup z informacjami o działaniu aplikacji, wykresach, ikonach i grupach.
- **Testy**: Integracyjne testy backendu oraz end‑to‑end (E2E) testy frontendowe przy użyciu Cypress.
- **Docker & Docker-Compose**: Cała aplikacja jest konteneryzowana i uruchamiana za pomocą docker-compose.

---

## Architektura

Aplikacja została podzielona na trzy główne warstwy:
- **Backend**: Implementacja API w .NET, komunikacja z bazą danych MySQL oraz logika migracji i seeding danych.
- **Frontend**: Aplikacja Angular odpowiedzialna za wyświetlanie interfejsu użytkownika, wizualizację danych oraz interakcję z API.
- **Testy**: Testy integracyjne backendu oraz E2E testy frontendowe, zapewniające wysoką jakość i spójność działania aplikacji.

---

## Wymagania

- **Docker** i **Docker-Compose**
- **Node.js** (zalecana wersja 18.x)
- **.NET SDK** (wersja 9.0)
- Dostęp do bazy MySQL (lokalnie lub w chmurze)
- Git (dla klonowania repozytorium)

---

## Instalacja

1. **Klonowanie Repozytorium:**

   ```bash
   git clone https://github.com/twoje_uzytkownik/pollitic.eu.git
   cd pollitic.eu
   ```

2. **Backend:**
    - Przejdź do folderu `backend` i zainstaluj zależności:
      ```bash
      cd backend
      dotnet restore
      ```

3. **Frontend:**
    - Przejdź do folderu `frontend` i zainstaluj zależności:
      ```bash
      cd ../frontend
      npm install
      ```

4. **Testy:**
    - Jeśli używasz integracyjnych testów backendowych, przejdź do folderu `Backend.IntegrationTests` i zainstaluj zależności:
      ```bash
      cd ../Backend.IntegrationTests
      dotnet restore
      ```

---

## Konfiguracja

### Pliki Konfiguracyjne

- **Backend:**
    - `appsettings.json` – podstawowa konfiguracja.
    - `appsettings.https.json` – konfiguracja SSL (jeśli jest używana).
    - Zmienne środowiskowe (np. `MYSQL_ROOT_PASSWORD`, `MYSQL_DATABASE`, `MYSQL_USER`, `MYSQL_PASSWORD`, `APP_DOMAIN`) są przekazywane przez plik `.env` lub zmienne środowiskowe systemu.

- **Frontend:**
    - `environment.ts` i `environment.prod.ts` – konfiguracja środowiskowa Angulara.
    - Cypress: konfiguracja w pliku `cypress.config.js`.

### Docker-Compose

Plik `docker-compose.yml` definiuje kontenery dla:
- bazy danych MySQL,
- backendu (aplikacja .NET),
- frontendu (aplikacja Angular serwowana przez Nginx).

---

## Uruchomienie

### Lokalnie z Docker-Compose

W głównym folderze projektu (np. `pollitic`) uruchom:

```bash
docker-compose up --build -d
```

Spowoduje to uruchomienie wszystkich kontenerów. Frontend będzie dostępny na porcie 4200 (lub według konfiguracji).

### Lokalnie bez Dockera

- **Backend:**
  ```bash
  cd backend
  dotnet run
  ```
- **Frontend:**
  ```bash
  cd frontend
  ng serve
  ```

---

## Testy

### Testy Backendowe (Integracyjne)

Testy integracyjne backendu znajdują się w folderze `Backend.IntegrationTests`. Aby je uruchomić, przejdź do tego folderu i wykonaj:

```bash
dotnet test --configuration Release
```

Testy wykorzystują WebApplicationFactory oraz InMemoryDatabase do symulacji bazy danych.

### Testy Frontendowe (E2E)

Testy E2E frontendowe są napisane w Cypress. Aby je uruchomić:

1. Przejdź do folderu `frontend`:
   ```bash
   cd frontend
   ```
2. Uruchom Cypress:
   ```bash
   npm run cypress:run
   ```

Lub, aby otworzyć interfejs graficzny:

```bash
npm run cypress:open
```

---

## CI/CD

### GitHub Actions

- **integration-tests.yml** – Uruchamia testy integracyjne backendu i E2E testy frontendowe na gałęzi `develop`.
- **deploy.yml** – Pipeline CI/CD, który wdraża aplikację na serwerze OVH przy pushu do gałęzi `main`. Wdrożenie wykorzystuje GitHub Secrets do przekazywania zmiennych środowiskowych (używane są mechanizmy, takie jak tworzenie lokalnego pliku `.env`).

---

## Struktura Projektu

```
pollitic/
├── backend/                 # Aplikacja backendowa (.NET)
│   ├── Program.cs
│   ├── appsettings.json
│   └── ... 
├── frontend/                # Aplikacja frontendowa (Angular)
│   ├── src/
│   │   ├── app/
│   │   │   ├── components, services, models, etc.
│   │   └── environments/
│   ├── cypress/
│   │   ├── integration/
│   │   │   └── *.spec.ts
│   │   └── fixtures/
│   │       └── *.json
│   ├── package.json
│   └── cypress.config.js
├── Backend.IntegrationTests/  # Testy integracyjne backendu (xUnit)
│   └── ... 
├── docker-compose.yml       # Definicje kontenerów
├── .env                     # Lokalny plik z konfiguracją (nie publikowany)
├── .github/
│   └── workflows/           # Pliki GitHub Actions
└── README.md                # Ten plik
```

---

## Licencja

Projekt jest udostępniany na warunkach [MIT License](LICENSE).

---
