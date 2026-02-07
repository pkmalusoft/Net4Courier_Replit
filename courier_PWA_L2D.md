# L2D - Linked To Deliver (Courier PWA)

## Progressive Web Application for Courier Operations

### Integrated with Net4Courier Web Application

---

## Overview

L2D is a Progressive Web Application (PWA) designed for courier field operations. It provides a mobile-optimized interface for pickup collection, delivery, POD capture, expense tracking, and RTS processing — all tightly integrated with the Net4Courier backend.

**App Name:** L2D (Linked To Deliver)
**Technology:** Blazor Server PWA (within the existing Net4Courier project)
**Access URL:** `https://<client-domain>/l2d`
**Target Users:** Couriers using regular mobiles or industrial devices
**Access Control:** Courier role only (not all staff)

---

## Deployment Model

- **Net4Courier_Main** — Source/master repository
- **Net4Courier_Gateex, Net4Courier_Rainbow, Net4Courier_Highway, etc.** — Separate client projects, each with their own database
- Code is pushed from Main and pulled to each client project
- The L2D PWA is part of the same codebase — no separate deployment needed
- Each client's couriers access their own URL (e.g., `https://gateex-domain.com/l2d`)

---

## Existing Net4Courier Features (Reuse — Do Not Recreate)

The following features already exist in Net4Courier and will be consumed by the PWA via existing services/APIs:

| Feature | Existing Entity/Service | Location |
|---------|------------------------|----------|
| Customer Zones | `CustomerZone`, `CustomerZoneCourier`, `CustomerZoneCity` | Masters.Entities |
| Courier Zone Assignment | `CustomerZoneCourier` (links UserId to CustomerZoneId) | Masters.Entities |
| Pickup Requests | `PickupRequest` entity with status workflow | Operations.Entities |
| Pickup Commitment (Accept/Release) | `PickupCommitment` entity + `PickupCommitmentService` | Operations.Entities / Web.Services |
| DRS (Delivery Run Sheet) | `DRS`, `DRSDetail` entities with delivery assignment | Operations.Entities |
| POD Fields | `InscanMaster.PODSignature`, `DeliveredDate`, `DeliveredTo` | Operations.Entities |
| Delivery Attempts | `DRSDetail.AttemptNo`, `AttemptedAt`, `Remarks`, `ReceivedBy` | Operations.Entities |
| Pickup Attempts | `PickupRequest.AttemptedAt`, `AttemptedByUserId`, `AttemptRemarks`, `PickupStatus.Attempted` | Operations.Entities |
| Courier Expenses | `CourierExpense` entity (DRS-linked, with approval workflow) | Operations.Entities |
| RTS Processing | `InscanMaster.IsRTS`, `RTSReason`, `RTSCreatedAt`, `RTSCreatedByUserId` | Operations.Entities |
| Authentication | `AuthService.IsCourierByUsernameAsync()`, role-based access | Infrastructure.Services |
| COD/Payment Collection | `DRSDetail.CODAmount`, `CollectedAmount`, `CourierCashSubmission` | Operations.Entities |
| DRS Reconciliation | `DRSReconciliationService` | Web.Services |
| AWB Tracking | `AWBTracking` entity with status history | Operations.Entities |

---

## Menu Structure

### Bottom Navigation Bar (always visible)

| Icon | Label | Description |
|------|-------|-------------|
| Home | Home | Dashboard / daily summary |
| Pickups | Pickups | Pickup requests & collection |
| Deliveries | Deliveries | Out-for-delivery & POD |
| Expenses | Expenses | Expense entry & history |
| More | More | Additional options |

### Home Screen
- Today's summary (pending pickups, pending deliveries, collections, expenses)
- Active notifications / alerts
- Quick action buttons (Scan AWB, New Expense)

### Pickups Section
- New Pickup Requests (zone-based notifications, accept/reject)
- My Pickups (accepted, in-progress)
- Pickup Collection (scan/manual AWB entry, capture images, GPS)
- Pickup Attempts (mark attempted with reason + photo + GPS + timestamp)
- Store Handover (mark batch as handed over)

### Deliveries Section
- My Deliveries (today's assigned shipments)
- Shipment Details (address, COD, charges, instructions)
- Navigate (open map to customer location)
- Deliver (signature capture, POD image, payment split)
- Delivery Attempts (mark attempted with reason + photo + GPS + timestamp)
- RTS (initiate return to shipper with reason + photo evidence)

### Expenses Section
- New Expense (category, amount, receipt photo)
- Expense History (past entries with sync status)

### More Menu
- My Profile (courier info, zone assignment)
- Vehicle Transfer (transfer shipments between vehicles)
- History (past pickups, deliveries, earnings)
- Sync Status (pending uploads, offline queue)
- Settings (notifications, camera preferences)
- About L2D
- Logout

---

## Zone-Based Pickup Notification Flow

1. **Setup (already in Net4Courier):**
   - Customer zones configured (`CustomerZone`)
   - Couriers assigned to zones (`CustomerZoneCourier`)
   - Customers mapped to zones (`Party.CustomerZoneId`)

2. **Pickup request created** (by office staff or customer self-service):
   - System identifies the customer's zone
   - Finds all couriers assigned to that zone via `CustomerZoneCourier`
   - Sends notification to ALL those couriers' devices

3. **First-accept-wins:**
   - Uses existing `PickupCommitmentService.CommitToPickupAsync()`
   - First courier to accept gets the pickup (commitment created)
   - Pickup disappears from other couriers' screens
   - Other couriers see "Already accepted" if they try

4. **Cancellation re-triggers:**
   - Uses existing `PickupCommitmentService.ReleaseCommitmentAsync()`
   - Released pickup goes back to available pool
   - All zone couriers get notified again

---

## Attempt Flow (Pickup & Delivery)

Applies identically to both pickup and delivery scenarios:

1. Courier arrives at location
2. Cannot complete operation (customer unavailable, goods not ready, etc.)
3. Marks as **"Attempted"** — captures:
   - Reason for failure (from predefined list)
   - Photo of premises (device camera)
   - GPS coordinates (auto-captured)
   - Date & time (auto-captured)
4. Attempt record syncs to Net4Courier
5. Multiple attempts allowed per shipment
6. After max attempts, courier can initiate RTS

---

## Development Phases

### Phase 1: PWA Foundation & Authentication
**Status:** Not Started
**Priority:** Critical — everything depends on this

**Scope:**
- PWA manifest (`manifest.json`) with L2D branding, icons, theme colors
- Service worker registration for installability
- Mobile-optimized L2D layout (separate from main Net4Courier layout)
  - Bottom navigation bar (Home, Pickups, Deliveries, Expenses, More)
  - No sidebar — thumb-friendly one-hand operation
  - Responsive design for regular mobiles and industrial devices
- Dedicated routes under `/l2d/*`
- Courier login page (`/l2d/login`)
  - Uses existing `AuthService` for authentication
  - Role validation — only Courier role can access
  - Session/token management
- Home screen (`/l2d`) with daily summary placeholder
- "Add to Home Screen" prompt and install support

**Reuses:**
- `AuthService` (login, role check)
- `IsCourierByUsernameAsync()` for role validation
- Existing user/session infrastructure

**Delivers:** Couriers can install the PWA, log in, and see the L2D home screen

---

### Phase 2: Pickup Flow
**Status:** Not Started
**Priority:** High — core courier workflow

**Scope:**
- **Pickup Request List** (`/l2d/pickups`)
  - Shows available pickup requests for courier's assigned zones
  - Queries via `CustomerZoneCourier` → `CustomerZone` → pickup requests
  - Real-time refresh / polling for new requests
  - Accept button — calls `PickupCommitmentService.CommitToPickupAsync()`
  - Accepted pickups move to "My Pickups" tab

- **My Pickups** (`/l2d/pickups/mine`)
  - Shows courier's accepted/committed pickups
  - Each pickup shows: customer name, address, contact, scheduled time, shipment count
  - **Navigate button** — opens device map app (Google Maps / Apple Maps) with customer address
  - **Collect button** — opens collection screen
  - **Cancel** — releases commitment via `ReleaseCommitmentAsync()` (reason required)

- **Pickup Collection** (`/l2d/pickups/collect/{id}`)
  - AWB barcode scanning via device camera (using JavaScript interop)
  - Manual AWB number entry as fallback
  - Parcel image capture (device camera)
  - Premises image capture (device camera)
  - Auto-capture: GPS coordinates, timestamp
  - Confirm collection — updates pickup status

- **Pickup Attempt** (`/l2d/pickups/attempt/{id}`)
  - Reason selection (predefined list: customer unavailable, goods not ready, address not found, etc.)
  - Premises photo capture (mandatory)
  - GPS + timestamp auto-captured
  - Updates `PickupRequest.AttemptedAt`, `AttemptedByUserId`, `AttemptedByUserName`, `AttemptRemarks`
  - Sets `PickupRequest.Status = PickupStatus.Attempted`
  - Multiple attempts per pickup allowed

- **Store Handover** (`/l2d/pickups/handover`)
  - Select collected shipments to hand over
  - Mark batch as handed over to warehouse/store
  - Capture handover timestamp and courier ID
  - Prevents re-processing of handed-over shipments

**Reuses:**
- `PickupCommitmentService` (accept, release, expire)
- `PickupRequest` entity and status workflow
- `CustomerZone`, `CustomerZoneCourier` for zone filtering

**Delivers:** Couriers can see zone-based pickup requests, accept them, navigate to customers, collect shipments with barcode scanning, and handle failed attempts

---

### Phase 3: Delivery Flow & POD
**Status:** Not Started
**Priority:** High — core courier workflow

**Scope:**
- **My Deliveries** (`/l2d/deliveries`)
  - Shows today's assigned shipments from DRS
  - Queries via `DRS` → `DRSDetail` → `InscanMaster` for logged-in courier
  - Each shipment shows: AWB, customer name, address, contact, COD amount, material cost, courier charges, special instructions
  - Status indicators: pending, attempted, delivered, RTS

- **Delivery Detail** (`/l2d/deliveries/{id}`)
  - Full shipment info with delivery address
  - **Navigate button** — opens map app with delivery address
  - **Deliver button** — opens POD capture screen
  - **Attempt button** — opens attempt screen
  - **RTS button** — opens return-to-shipper screen

- **POD Capture** (`/l2d/deliveries/pod/{id}`)
  - Customer signature capture (touch-based drawing canvas via JS interop)
  - POD photo capture (parcel handed over, doorstep, etc.)
  - Receiver name entry
  - Receiver relation (self, family, colleague, security, etc.)
  - Payment collection split:
    - COD amount
    - Courier charges
    - Material cost
    - Pending amount (if any)
  - Auto-capture: GPS, timestamp
  - Saves to `InscanMaster.PODSignature`, `DeliveredDate`, `DeliveredTo`
  - Updates `DRSDetail` with delivery info (`CollectedAmount`, `ReceivedBy`, `Relation`)

- **Delivery Attempt** (`/l2d/deliveries/attempt/{id}`)
  - Same flow as pickup attempt:
    - Reason (customer unavailable, wrong address, refused, etc.)
    - Premises photo (mandatory)
    - GPS + timestamp auto-captured
  - Updates `DRSDetail.AttemptNo`, `AttemptedAt`
  - Multiple attempts allowed

- **RTS (Return to Shipper)** (`/l2d/deliveries/rts/{id}`)
  - Reason selection (refused, wrong address, max attempts, damaged, etc.)
  - Photo evidence capture
  - GPS + timestamp
  - Sets `InscanMaster.IsRTS = true`, `RTSReason`, `RTSCreatedAt`, `RTSCreatedByUserId`
  - Updates shipment status via existing RTS workflow

**Reuses:**
- `DRS`, `DRSDetail` entities
- `InscanMaster` POD fields (`PODSignature`, `DeliveredDate`, `DeliveredTo`)
- `AWBTracking` for status updates
- `InscanMaster` RTS fields (`IsRTS`, `RTSReason`, `RTSCreatedAt`, `RTSCreatedByUserId`)

**Delivers:** Couriers can view delivery assignments, navigate to customers, capture POD with signature, collect payments, handle failed delivery attempts, and initiate RTS

---

### Phase 4: Expense Management & Vehicle Transfer
**Status:** Not Started
**Priority:** Medium

**Scope:**
- **New Expense** (`/l2d/expenses/new`)
  - Expense category selection (fuel, toll, parking, food, etc.)
  - Amount entry
  - Date (defaults to today)
  - Remarks / description
  - Receipt photo capture (optional)
  - Links to current DRS via `CourierExpense.DRSId`
  - Status: Pending (approval by office)

- **Expense History** (`/l2d/expenses`)
  - List of past expenses with status (Pending / Approved / Rejected)
  - Filter by date range
  - Shows approval remarks if rejected

- **Vehicle Transfer** (`/l2d/transfer`)
  - Transfer shipments between vehicles in emergencies
  - Select source vehicle, destination vehicle
  - Select shipments to transfer
  - Reason for transfer (mandatory)
  - Creates audit trail in Net4Courier

**Reuses:**
- `CourierExpense` entity (DRS-linked, with `ExpenseStatus` workflow)
- `ExpenseType` enum
- Existing expense approval workflow in office dashboard

**Delivers:** Couriers can log expenses with receipt photos, view expense history, and transfer shipments between vehicles

---

### Phase 5: Offline Support & Push Notifications
**Status:** Not Started
**Priority:** Medium-High

**Scope:**
- **Service Worker Enhancement**
  - Cache critical pages for offline access
  - Cache API responses for offline reading (delivery list, pickup list)

- **Offline Data Storage (IndexedDB)**
  - Store pending pickups and deliveries locally
  - Queue offline actions: attempts, POD captures, expense entries
  - Track sync status per record

- **Background Sync**
  - On network restoration, sync queued actions to server
  - Conflict resolution: server is source of truth
  - Visual indicator: sync status (synced / pending / failed)

- **Push Notifications**
  - New pickup request in courier's zone → push notification
  - Pickup cancellation re-notification
  - Delivery assignment notification
  - Notification click opens relevant screen
  - Foreground and background notification handling

**Delivers:** Couriers can work offline (scanning, attempts, photos, expenses), with automatic sync when connectivity returns. Push notifications for new pickups.

---

### Phase 6: Dashboard, History & Performance
**Status:** Not Started
**Priority:** Low-Medium

**Scope:**
- **Home Dashboard Enhancement** (`/l2d`)
  - Today's stats: pickups completed, deliveries done, COD collected, expenses
  - Pending items count with quick-action links
  - Recent activity feed

- **Historical Data** (`/l2d/history`)
  - Past pickups (with dates and outcomes)
  - Past deliveries (with POD status)
  - Earnings summary (COD collected, expenses)
  - Date-range filtering

- **Courier Performance Metrics**
  - Delivery success rate
  - Average attempts per delivery
  - On-time delivery percentage
  - Daily/weekly/monthly summaries

- **Integration with Office Dashboard**
  - Real-time courier location display (if GPS consent given)
  - Live delivery status updates visible to operations team
  - Courier activity log accessible from office dashboard

**Delivers:** Couriers see daily summaries and historical performance; office staff see real-time courier activity

---

## Technical Architecture

### Route Structure
```
/l2d                    → Home dashboard
/l2d/login              → Courier login
/l2d/pickups            → Pickup request list (available + mine)
/l2d/pickups/collect/{id} → Pickup collection
/l2d/pickups/attempt/{id} → Pickup attempt
/l2d/pickups/handover   → Store handover
/l2d/deliveries         → Delivery list
/l2d/deliveries/{id}    → Delivery detail
/l2d/deliveries/pod/{id} → POD capture
/l2d/deliveries/attempt/{id} → Delivery attempt
/l2d/deliveries/rts/{id} → Return to shipper
/l2d/expenses           → Expense list
/l2d/expenses/new       → New expense
/l2d/transfer           → Vehicle transfer
/l2d/history            → Historical data
/l2d/profile            → Courier profile
/l2d/settings           → App settings
```

### Layout
- Separate `L2DLayout.razor` (not MainLayout)
- Bottom navigation bar with 5 tabs
- Top app bar with L2D logo and notification bell
- Pull-to-refresh on list pages
- Touch-optimized — minimum 48px tap targets
- Dark mode support (optional)

### JavaScript Interop Requirements
- Camera access for barcode scanning and image capture
- GPS/geolocation for auto-location capture
- Touch canvas for signature capture
- Push notification subscription
- IndexedDB for offline storage (Phase 5)
- "Add to Home Screen" prompt

### Security
- Courier role validation on all `/l2d/*` routes
- JWT/session-based authentication
- API-level role enforcement
- Device binding (optional, future)
- HTTPS required

---

## Change Log

| Date | Phase | Changes | Status |
|------|-------|---------|--------|
| | | | |

---

*Document Version: 1.0*
*Created: February 7, 2026*
*Last Updated: February 7, 2026*
