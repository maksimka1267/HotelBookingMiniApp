````md
# 🏨 HotelBooking — міні-застосунок «Бронювання готелів»

Рішення складається з трьох підпроєктів:
- **HotelBooking.Data** — домен + доступ до даних (EF Core + MySQL) і приклад Dapper (статистика), DTO та сервіси.
- **HotelBooking.Api** — Web API (JWT + ролі, Swagger, CRUD для адміністратора, бронювання для клієнта).
- **HotelBooking.Web** — інтерфейс на **Razor Pages**, який викликає API через `GatewayClient` (Bearer JWT).

> Усі `Id` у системі — **GUID**.

---

## ✅ Функціонал

### Клієнт (Client)
- Реєстрація / Вхід (JWT)
- Перегляд готелів
- Пошук доступних номерів (місто, дати, мін. місткість)
- Створення бронювання
- Перегляд **моїх бронювань**

### Адміністратор (Admin)
- CRUD готелів
- CRUD номерів
- Перегляд **усіх бронювань**
- Статистика по готелях за період (кількість бронювань, ночі, виручка)

---

## 🧱 Архітектура (коротко)

**Web (Razor Pages)** → `GatewayClient` → **Api** → `Services (Data)` → EF Core / Dapper → **MySQL**

- **JWT** видається в `Api/AuthController`.
- **Web** зберігає токен у cookie-claims (`access_token`) та додає його в запити через `BearerTokenHandler`.

---

## 🛠️ Технології

- **.NET 9**
- **ASP.NET Core Web API**
- **Razor Pages**
- **EF Core + Pomelo MySQL**
- **ASP.NET Core Identity** (Guid)
- **JWT Bearer**
- **Dapper** (для статистики)
- **Swagger (Swashbuckle)**

---

## ⚙️ Налаштування конфігів

### 1) HotelBooking.Api — `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=hotelbooking;user=root;password=pass;"
  },
  "Jwt": {
    "Key": "YOUR_SUPER_SECRET_KEY_32+_CHARS",
    "Issuer": "HotelBooking.Api",
    "Audience": "HotelBooking.Web",
    "ExpireMinutes": "120"
  },
  "AdminSeed": {
    "Email": "admin@gmail.com",
    "Password": "Admin123!"
  }
}
````

> На старті API виконує `Migrate()` та сідує ролі/адміна.

---

### 2) HotelBooking.Web — `appsettings.json`

```json
{
  "Gateway": {
    "BaseUrl": "https://localhost:7030"
  }
}
```

> `BaseUrl` має вказувати на **домен/порт API**.

---

## ▶️ Запуск локально

### 1) Підготувати MySQL

Створи БД (наприклад `hotelbooking`) та користувача/пароль.

### 2) Запустити API

1. Встановити `DefaultConnection` у `HotelBooking.Api/appsettings.json`
2. Запустити:

   * `HotelBooking.Api`

Swagger:

* `https://localhost:7030/swagger`

### 3) Запустити Web

1. Вказати `Gateway:BaseUrl` на API (`https://localhost:7030`)
2. Запустити:

   * `HotelBooking.Web`

Web:

* `https://localhost:7110`

---

## 🔐 Ролі та доступ

* **Admin**: доступ до `/Admin/*` сторінок та `api/admin/*` endpoint-ів.
* **Client**: бронювання, перегляд своїх бронювань.

Ролі додаються у токен як `ClaimTypes.Role` і використовуються через:

```csharp
[Authorize(Roles = "Admin")]
```

---

## 📡 Основні API маршрути (скорочено)

### Auth

* `POST /api/auth/register`
* `POST /api/auth/login`
* `GET /api/auth/me` (Authorize)

### Public

* `GET /api/hotels`
* `GET /api/hotels/{id}`
* `GET /api/rooms/search?city=...&checkIn=...&checkOut=...&minCapacity=...`
* `GET /api/rooms/{id}`

### Client

* `POST /api/bookings`
* `GET /api/bookings/my`

### Admin

* `POST /api/admin/hotels`

* `PUT /api/admin/hotels/{id}`

* `DELETE /api/admin/hotels/{id}`

* `POST /api/admin/rooms`

* `PUT /api/admin/rooms/{id}`

* `DELETE /api/admin/rooms/{id}`

* `GET /api/admin/bookings`

* `GET /api/admin/stats/hotels?from=YYYY-MM-DD&to=YYYY-MM-DD`

---

## 🧩 Важливо про токен у Web

У Web токен додається автоматично через `BearerTokenHandler`, який читає його з claims:

* claim: `"access_token"`

Це зроблено, щоб токен **не губився після перезапуску Web**, на відміну від `Session` у пам’яті.

---

## 🚀 Деплой (monsterASP / IIS)

1. Публікувати **кожен сайт окремо**:

* `HotelBooking.Api` → publish folder → залити на API-сайт
* `HotelBooking.Web` → publish folder → залити на Web-сайт

2. У `HotelBooking.Web/appsettings.json`:

* `Gateway:BaseUrl` = **URL API-сайта** (не Web!)

3. Якщо хостинг не підтримує `.NET 9`:

* перевести `TargetFramework` на **net8.0** і перепублікувати.

---

## 📌 Примітки

* Дати бронювання: `CheckIn < CheckOut`
* Конфлікт бронювання: `new.CheckIn < existing.CheckOut && new.CheckOut > existing.CheckIn`
* Статистика рахується через Dapper + SQL (агрегація по готелях)

---

## 👤 Автор

Chystikov Maksim

```
```
