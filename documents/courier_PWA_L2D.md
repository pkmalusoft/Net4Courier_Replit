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
**Status:** COMPLETED
**Priority:** Critical — everything depends on this

**Implemented:**
- [x] PWA manifest (`manifest.json`) with LinkDel branding, icons, theme colors
- [x] Service worker registration (`linkdel/sw.js`) for installability
- [x] Mobile-optimized LinkDel layout (`LinkDelLayout.razor`) separate from main Net4Courier layout
  - [x] Bottom navigation bar (Home, Pickups, Deliveries, Expenses, More)
  - [x] No sidebar — thumb-friendly one-hand operation
  - [x] Responsive design for regular mobiles and industrial devices
- [x] Dedicated routes under `/l2d/*`
- [x] Courier login page (`/l2d/login`) with modern mobile-first design
  - [x] Uses existing `AuthService` for authentication
  - [x] Role validation — only Courier role can access (courier-scoped authorization)
  - [x] Session management
- [x] Home screen (`/l2d`) with daily summary dashboard (operations stats, collections, expenses, net position)
- [x] JavaScript interop module (`linkdel.js`) for device features (GPS, camera, barcode scanning, map navigation)
- [x] "Add to Home Screen" / install support

**Pending:**
- [ ] L2D-specific branding icons (currently using placeholder icons)

**Reuses:**
- `AuthService` (login, role check)
- `IsCourierByUsernameAsync()` for role validation
- Existing user/session infrastructure

**Delivers:** Couriers can install the PWA, log in, and see the L2D home screen with daily summary

---

### Phase 2: Pickup Flow
**Status:** COMPLETED
**Priority:** High — core courier workflow

**Implemented:**
- [x] **Pickup Request List** (`/l2d/pickups` — `Pickups.razor`)
  - [x] Shows available pickup requests assigned to the courier
  - [x] Accept/Reject functionality for new pickup requests
  - [x] Accepted pickups shown in "My Pickups" tab
  - [x] Status-based filtering and display

- [x] **Pickup Detail** (`/l2d/pickups/{id}` — `PickupDetail.razor`)
  - [x] Full pickup info: customer name, address, contact, scheduled time
  - [x] **Navigate button** — opens device map app with customer address
  - [x] **Collect button** — opens collection flow
  - [x] GPS auto-capture on collection
  - [x] Actual pieces and weight entry on collection
  - [x] Confirm collection — updates pickup status

- [x] **Pickup Attempt** (within `PickupDetail.razor`)
  - [x] Attempt recording with status history tracking
  - [x] Reason selection from predefined list
  - [x] GPS + timestamp auto-captured
  - [x] Updates pickup status and attempt fields
  - [x] Multiple attempts per pickup allowed

- [x] **Offline caching** — Pickups page caches data to IndexedDB, falls back to cached data when offline

**Pending:**
- [ ] Store Handover page (`/l2d/pickups/handover`) — batch handover to warehouse
- [ ] AWB barcode scanning via device camera during collection
- [ ] Parcel/premises image capture during collection
- [ ] Zone-based pickup notification flow (push notification to zone couriers on new request)
- [ ] First-accept-wins via `PickupCommitmentService` (current implementation uses direct accept/reject)

**Reuses:**
- `PickupRequest` entity and status workflow
- Courier-scoped authorization

**Delivers:** Couriers can see pickup requests, accept/reject them, navigate to customers, collect with GPS/pieces/weight, and handle failed attempts

---

### Phase 3: Delivery Flow & POD
**Status:** COMPLETED
**Priority:** High — core courier workflow

**Implemented:**
- [x] **My Deliveries** (`/l2d/deliveries` — `Deliveries.razor`)
  - [x] Shows today's assigned shipments from DRS for logged-in courier
  - [x] Queries via `DRS` → `DRSDetail` → `InscanMaster`
  - [x] Each shipment shows: AWB, consignee name, address, contact, COD amount
  - [x] Status indicators: pending, attempted, delivered, RTS

- [x] **Delivery Detail** (`/l2d/deliveries/{id}` — `DeliveryDetail.razor`)
  - [x] Full shipment info with consignee address and contact
  - [x] **Navigate button** — opens device map app with delivery address
  - [x] **Deliver button** — opens POD capture flow
  - [x] **Attempt button** — opens attempt dialog
  - [x] **RTS button** — marks as return-to-shipper

- [x] **POD Capture** (within `DeliveryDetail.razor`)
  - [x] Receiver name entry
  - [x] Receiver relation (self, family, colleague, security, etc.)
  - [x] Collected amount entry
  - [x] Updates `DRSDetail` with delivery info
  - [x] DRS counter updates on delivery

- [x] **Delivery Attempt** (within `DeliveryDetail.razor`)
  - [x] Reason selection from predefined list (customer unavailable, wrong address, refused, etc.)
  - [x] Attempt recording with history tracking
  - [x] DRS counter updates on attempt
  - [x] Multiple attempts allowed

- [x] **RTS Marking** (within `DeliveryDetail.razor`)
  - [x] RTS marking with reason
  - [x] Updates shipment status

- [x] **Offline caching** — Deliveries page caches data to IndexedDB, falls back to cached data when offline

**Pending:**
- [ ] Customer signature capture (touch-based drawing canvas via JS interop)
- [ ] POD photo capture (parcel handed over, doorstep, etc.)
- [ ] Premises photo capture on delivery attempt
- [ ] GPS auto-capture on delivery/attempt
- [ ] Photo evidence capture on RTS
- [ ] Payment collection split (COD, courier charges, material cost, pending amount)
- [ ] Saving to `InscanMaster.PODSignature`, `DeliveredDate`, `DeliveredTo`
- [ ] AWBTracking status updates on delivery/attempt/RTS

**Reuses:**
- `DRS`, `DRSDetail` entities
- Courier-scoped authorization

**Delivers:** Couriers can view delivery assignments, navigate to customers, capture POD (receiver name/relation/amount), handle failed delivery attempts with predefined reasons, and mark RTS

---

### Phase 4: Expense Management & Cash
**Status:** COMPLETED
**Priority:** Medium

**Implemented:**
- [x] **Expense List** (`/l2d/expenses` — `Expenses.razor`)
  - [x] Today / Week / All tab filtering
  - [x] Expense list with status indicators (Pending / Approved / Rejected)
  - [x] Shows expense type, amount, date, description, and DRS reference

- [x] **Add Expense** (dialog within `Expenses.razor`)
  - [x] DRS selection dropdown
  - [x] Expense type selection (fuel, toll, parking, food, etc.)
  - [x] Amount entry
  - [x] Description / remarks
  - [x] Links to DRS via `CourierExpense.DRSId`
  - [x] Status: Pending (approval by office)

- [x] **COD Cash Submission** (`/l2d/cash` — `CashSubmission.razor`)
  - [x] Submit cash collected against specific DRS
  - [x] Server-side balance validation (prevents over-submission)
  - [x] Shows already submitted vs remaining amounts

- [x] **Daily Summary** (`/l2d/summary` — `DailySummary.razor`)
  - [x] Operations stats (pickups completed, deliveries done)
  - [x] Collections summary (COD collected)
  - [x] Expenses summary
  - [x] Net position calculation

- [x] **Offline caching** — Expenses page caches data to IndexedDB, falls back to cached data when offline
- [x] **Offline sync** — add_expense and submit_cash actions queued offline and synced when back online

**Pending:**
- [ ] Receipt photo capture on expense entry
- [ ] Expense approval remarks display when rejected
- [ ] Date range filter for expense history
- [ ] Vehicle Transfer page (`/l2d/transfer`) — transfer shipments between vehicles

**Reuses:**
- `CourierExpense` entity (DRS-linked, with `ExpenseStatus` workflow)
- `ExpenseType` enum
- `CourierCashSubmission` entity
- Courier-scoped authorization

**Delivers:** Couriers can log expenses against DRS, submit COD cash with balance validation, view daily summary with net position, and work offline with automatic sync

---

### Phase 5: Offline Support & Push Notifications
**Status:** COMPLETED
**Priority:** Medium-High

**Implemented:**
- [x] **Service Worker** (`linkdel/sw.js`)
  - [x] Service worker registration for PWA installability
  - [x] Push notification handling (push event + notificationclick)
  - [x] Background sync event support

- [x] **Offline Data Storage (IndexedDB)** (`linkdel-offline.js`)
  - [x] IndexedDB stores: pickups, deliveries, expenses, outbox
  - [x] Data caching wired into Pickups, Deliveries, and Expenses pages
  - [x] Cache data after successful server load (`cacheData`)
  - [x] Fall back to cached data when network fails (`getCachedData`)
  - [x] JSON-friendly serialization for IndexedDB storage

- [x] **Offline Mutation Queue**
  - [x] Queue offline actions to IndexedDB outbox (accept_pickup, reject_pickup, add_expense, submit_cash)
  - [x] Track sync status per record (pending / synced / failed)

- [x] **Background Sync** (in `LinkDelLayout.razor`)
  - [x] `ProcessOutboxQueue()` — reads pending actions from IndexedDB, processes each
  - [x] `ProcessSingleAction()` — server-side mutation handling with authorization checks
  - [x] Authorization enforcement: verifies courier owns pickup/DRS before allowing mutations
  - [x] Triggered automatically on connectivity restored (`OnConnectivityChanged`)
  - [x] Triggered on service worker sync event (`OnSyncRequested`)
  - [x] Snackbar notifications showing sync results (success/failure counts)
  - [x] Cleanup of synced actions from outbox

- [x] **Online/Offline Status Indicator** (in `LinkDelLayout.razor`)
  - [x] Visual indicator showing current connectivity status
  - [x] Connectivity change detection

- [x] **Push Notification Subscription**
  - [x] VAPID-based subscription creation via `subscribePush` in `linkdel.js`
  - [x] `urlBase64ToUint8Array` helper for VAPID key conversion
  - [x] Notification permission request flow
  - [x] Service worker push event handling with notification display
  - [x] Notification click opens relevant L2D screen

**Pending:**
- [ ] Push subscription persistence to server backend (requires API endpoint to store subscriptions per user/device)
- [ ] Server-side push notification sending (requires web-push library integration)
- [ ] Zone-based push notifications for new pickup requests
- [ ] Pickup cancellation re-notification
- [ ] Delivery assignment push notification
- [ ] Offline sync for POD capture and delivery attempt actions
- [ ] Conflict resolution strategy documentation

**Delivers:** Couriers can work offline with automatic data caching and action queuing. When connectivity returns, queued actions sync automatically with authorization-checked server-side processing. Push notification subscription infrastructure is in place.

---

### Phase 6: Dashboard, History & Performance
**Status:** PARTIALLY COMPLETED
**Priority:** Low-Medium

**Implemented:**
- [x] **Home Dashboard** (`/l2d` — `Home.razor`)
  - [x] Today's stats: pickups completed, deliveries done, COD collected, expenses
  - [x] Quick action buttons

- [x] **Daily Summary** (`/l2d/summary` — `DailySummary.razor`)
  - [x] Operations stats, collections, expenses, net position

**Pending:**
- [ ] Recent activity feed on home dashboard
- [ ] Pending items count with quick-action links
- [ ] Historical Data page (`/l2d/history`) — past pickups, deliveries, earnings with date-range filtering
- [ ] Courier Performance Metrics — delivery success rate, average attempts, on-time percentage
- [ ] Integration with Office Dashboard — real-time courier location, live status updates, courier activity log

**Delivers:** Couriers see daily summaries; historical performance and office integration still pending

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

## Overall Progress Summary

| Phase | Status | Completion |
|-------|--------|------------|
| Phase 1: PWA Foundation & Authentication | COMPLETED | ~95% |
| Phase 2: Pickup Flow | COMPLETED | ~75% |
| Phase 3: Delivery Flow & POD | COMPLETED | ~65% |
| Phase 4: Expense Management & Cash | COMPLETED | ~85% |
| Phase 5: Offline Support & Push Notifications | COMPLETED | ~70% |
| Phase 6: Dashboard, History & Performance | PARTIALLY COMPLETED | ~30% |

### Key Pending Items Across All Phases
1. **Camera/Image Capture** — Barcode scanning, parcel/premises photos, receipt photos, POD photos (Phases 2, 3, 4)
2. **Signature Capture** — Touch-based drawing canvas for POD (Phase 3)
3. **GPS on Delivery** — Auto-capture GPS on delivery/attempt/RTS (Phase 3)
4. **Push Notification Backend** — Subscription persistence, server-side sending, zone-based notifications (Phase 5)
5. **AWB Tracking Updates** — Status history updates on delivery/attempt/RTS events (Phase 3)
6. **Store Handover** — Batch handover page for collected shipments (Phase 2)
7. **Vehicle Transfer** — Emergency shipment transfer between vehicles (Phase 4)
8. **History & Performance** — Historical data, courier metrics, office dashboard integration (Phase 6)
9. **PickupCommitmentService** — First-accept-wins zone-based pickup assignment (Phase 2)

---

## Change Log

| Date | Phase | Changes | Status |
|------|-------|---------|--------|
| Feb 7, 2026 | 1 | PWA foundation, manifest, service worker, LinkDelLayout, courier login, home dashboard, JS interop | Completed |
| Feb 7, 2026 | 2 | Pickup list, pickup detail, accept/reject, collect with GPS/pieces/weight, attempt recording | Completed |
| Feb 7, 2026 | 3 | Delivery list, delivery detail, POD capture (name/relation/amount), delivery attempt, RTS marking | Completed |
| Feb 7, 2026 | 4 | Expense list with tabs, add expense dialog, COD cash submission, daily summary | Completed |
| Feb 8, 2026 | 5 | IndexedDB offline caching wired into pages, offline mutation queue, sync processing with auth checks, push subscription | Completed |
| Feb 8, 2026 | 6 | Home dashboard with today's stats, daily summary page | Partial |

---

*Document Version: 2.0*
*Created: February 7, 2026*
*Last Updated: February 8, 2026*
