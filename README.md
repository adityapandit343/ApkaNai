# CutBook API — Real-time Salon Queue System

## Tech Stack
- **.NET 8** Web API
- **PostgreSQL** via Neon (configured in appsettings.json)
- **Entity Framework Core 8** with Npgsql
- **SignalR** for real-time communication
- **JWT Bearer** authentication
- **Swagger UI** at root `/`

---

## Quick Start

```bash
# 1. Restore packages
dotnet restore

# 2. Run (auto-migrates DB on startup)
dotnet run
```

Open **http://localhost:5000** — Swagger UI loads automatically.

---

## Architecture

### Roles
| Role | Description |
|------|-------------|
| `Customer` | Searches nearby shops, sends haircut requests |
| `ShopOwner` | Creates shop, manages queue, accepts/rejects requests |

### Flow

```
1. ShopOwner registers → gets 1-month free trial
2. ShopOwner creates shop with services (salon type, opening hours, etc.)
3. ShopOwner hits /api/shop/go-live with lat/long → shop becomes visible
4. Customer registers → searches nearby shops (by lat/long + radius)
5. Customer selects shop + services → sends real-time request
6. ShopOwner receives SignalR notification → accepts or rejects
7. On accept → customer gets token number via SignalR
8. Customer enters queue with position and token
9. ShopOwner calls POST /api/queue/next after each haircut
10. Next customer gets "YourTurn" notification via SignalR
```

---

## API Endpoints

### Auth
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register/customer` | Customer registration |
| POST | `/api/auth/register/shopowner` | Shop owner registration |
| POST | `/api/auth/login` | Login (both roles) |

### Shop (ShopOwner)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/shop` | Create shop |
| PUT | `/api/shop` | Update shop details |
| POST | `/api/shop/go-live` | Go live with lat/long |
| POST | `/api/shop/go-offline` | Take shop offline |
| GET | `/api/shop/my-shop` | View my shop |
| POST | `/api/shop/services` | Add a service |
| DELETE | `/api/shop/services/{id}` | Remove a service |

### Shop (Customer)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/shop/search-nearby` | Search live shops by location |

### Queue (Customer)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/queue/request` | Send haircut request to shop |
| GET | `/api/queue/my-request` | Check my request status & token |

### Queue (ShopOwner)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/queue/pending` | View pending requests |
| POST | `/api/queue/accept/{id}` | Accept request → adds to queue |
| POST | `/api/queue/reject/{id}` | Reject request |
| GET | `/api/queue/live-queue` | View current queue |
| POST | `/api/queue/next` | Complete current & call next |

---

## SignalR Hub: `/hubs/shop`

Connect with JWT token as query string:
```
ws://localhost:5000/hubs/shop?access_token=YOUR_JWT
```

### Client Methods (listen for these)
| Event | Sent To | Payload |
|-------|---------|---------|
| `NewRequest` | Shop owner group | `HaircutRequestResponseDto` |
| `RequestAccepted` | Customer | `HaircutRequestResponseDto` (includes TokenNumber) |
| `RequestRejected` | Customer | `HaircutRequestResponseDto` |
| `QueueUpdated` | Shop owner group | `List<QueueEntryResponseDto>` |
| `YourTurn` | Customer | `{ TokenNumber }` |
| `HaircutCompleted` | Customer | `{ TokenNumber }` |

### Server Methods (call these from client)
| Method | Args | Description |
|--------|------|-------------|
| `JoinShopGroup` | `shopId` | Shop owner joins their shop group |
| `LeaveShopGroup` | `shopId` | Leave shop group |

> **Note:** Customers auto-join their personal group `customer_{userId}` on connect.

---

## Database Schema

```
Users (id, fullName, email, passwordHash, phoneNumber, role, isActive)
  └── Shops (id, ownerId, shopName, address, lat, long, salonType, openingTime, closingTime, isLive, trialEndsAt)
        ├── ShopServices (id, shopId, serviceName, category, estimatedMinutes, price)
        ├── HaircutRequests (id, customerId, shopId, requestedServices, hairStyle, status, tokenNumber)
        └── QueueEntries (id, shopId, requestId, customerId, position, tokenNumber, status)
```

---

## Salon Types
`Male` | `Female` | `Unisex`

## Service Categories
`Hair` | `Beard` | `Skin` | `Other`

## Request Statuses
`Pending` → `Accepted` / `Rejected` → `Completed` / `Cancelled`

## Queue Statuses
`Waiting` → `InProgress` → `Done`
