Given your scope (Courier Operations \+ Financial Accounting, 120 concurrent users, mobile integration, 20 GB DB, migration from SQL Server to PostgreSQL, reporting stack change), this is no longer a “tuning” exercise but a **full-scale data-centric re-architecture and performance refactor**.

Below is a structured, industry-aligned optimization and refactoring task plan.

---

## **1\. Data Model Re-Engineering (Highest Priority)**

### **a) Normalize and Relate Core Domains**

Redesign schema around proper domain separation:

**Operations**

* Consignment / AWB  
* Pickup  
* Delivery  
* Route / Run Sheet  
* Hub / Branch  
* Vehicle / Rider  
* POD (images, signatures)

**Finance**

* Customer  
* Invoice  
* Payment  
* Ledger  
* GL Entries  
* Tax (GST/VAT)  
* Costing (fuel, linehaul, rider payouts)

Introduce:

* Surrogate keys (BIGINT)  
* Foreign key constraints  
* Referential integrity  
* Proper transaction boundaries

### **b) Year & Volume Strategy**

Implement from day one:

* FinancialYear dimension  
* Partition large tables by:  
  * TransactionDate (monthly or yearly)  
  * BranchId (if very high volume)

In PostgreSQL:

* Native table partitioning  
* Hot partitions for current month  
* Cold partitions for closed periods (read-only)

---

## **2\. Query & Transaction Optimization**

### **a) Eliminate Legacy Anti-Patterns**

During refactor:

* No cursors  
* No RBAR loops  
* No scalar UDFs in filters  
* No dynamic SQL concatenation  
* No “SELECT \*”

Replace with:

* Set-based operations  
* Window functions  
* Materialized views for heavy reports  
* Pre-aggregated summary tables (daily, monthly KPIs)

### **b) Hot Path Optimization**

Profile and optimize:

| Module | Typical Hot Queries |
| ----- | ----- |
| Operations | AWB tracking, POD fetch, route assignment |
| Mobile | Pickup sync, delivery sync, status updates |
| Finance | Aging, outstanding, ledger, tax reports |
| Management | Daily MIS, branch performance, revenue |

These must be indexed and partition-aware.

---

## **3\. PostgreSQL-Specific Performance Design**

### **a) Index Strategy**

* B-Tree on all FK columns  
* Composite indexes for:  
  * (BranchId, Date)  
  * (CustomerId, Date)  
  * (AWBNo, Status)  
* Partial indexes for:  
  * Open consignments  
  * Undelivered shipments  
  * Unpaid invoices

### **b) Storage & IO**

* Tablespaces:  
  * Hot data on SSD  
  * Archive partitions on slower disks  
* Separate WAL disk  
* Autovacuum tuning (critical at 120 users)

---

## **4\. Mobile Integration Performance**

### **a) Sync Architecture**

Avoid:

* Full table sync  
* Date range brute force pulls

Implement:

* Change-tracking tables  
* Delta sync by LastModified  
* Message queue for status updates (RabbitMQ / Kafka optional)

### **b) API Optimization**

* Pagination mandatory  
* Field projection (no overfetching)  
* Caching of master data  
* Async write-behind for POD uploads

---

## **5\. Reporting Stack Replacement (Crystal → Linux PDF)**

### **a) Report Architecture**

Replace monolithic Crystal queries with:

* Reporting database or read replica  
* Materialized views for:  
  * Daily revenue  
  * Aging  
  * Branch-wise performance  
  * Tax summaries

PDF generation layer:

* QuestPDF (best for .NET microservices)  
* wkhtmltopdf (HTML-to-PDF, easier migration)

### **b) Report Performance Strategy**

* Pre-calculate heavy numbers  
* Parameterized report queries  
* Snapshot tables for closed periods  
* Cache PDFs for regulatory reports

---

## **6\. Financial Year & Archive Strategy**

Implement:

* Current FY → Read/Write  
* Previous FY → Read-mostly  
* Closed FY → Read-only partition / archive DB

This alone will cut:

* Index bloat  
* Vacuum time  
* Query scan range  
* Backup time

---

## **7\. Migration Tasks (SQL Server → PostgreSQL)**

### **a) Schema Migration**

* Data type mapping (datetime → timestamptz, nvarchar → text, money → numeric)  
* Identity → sequences  
* Triggers → PL/pgSQL  
* Stored procedures → functions

### **b) Data Migration**

* Bulk load via COPY  
* Partition-wise migration  
* Validate counts, balances, trial balance totals

### **c) Query Rewrite**

* Replace:  
  * T-SQL functions  
  * CROSS APPLY  
  * MERGE  
  * ISNULL  
    with PostgreSQL equivalents.

---

## **8\. Performance Governance After Go-Live**

### **a) Observability**

* pg\_stat\_statements  
* Slow query log  
* Lock monitoring  
* Vacuum / bloat monitoring

### **b) SLA Metrics**

Track:

* AWB search \< 200 ms  
* POD fetch \< 300 ms  
* Invoice generation \< 2 sec  
* Trial balance \< 5 sec

---

## **Executive Summary – Refactor Workstreams**

| Stream | Objective |
| ----- | ----- |
| Data Architecture | Industry-standard relational model |
| Partitioning | Year & month based isolation |
| Query Engineering | Eliminate legacy inefficiencies |
| PostgreSQL Tuning | Indexing, vacuum, WAL, memory |
| Mobile Sync | Delta-based, async, cached |
| Reporting | Materialized views \+ Linux PDF |
| Archival | Closed year read-only |
| Migration | Zero-data-loss, performance-first |

---

## **Most Important Design Principle**

**Courier systems are time-series \+ ledger systems.**  
They must be engineered as:

* Write-optimized for live operations  
* Read-optimized for MIS and finance  
* Partitioned by time  
* Immutable once closed  
* Audit-compliant

---

If you confirm:

* Annual shipment volume  
* Daily peak transactions  
* Average invoices per day  
* Current largest table row count  
* Whether finance must be statutory-compliant (GST, VAT, TDS, etc.)

I can provide you a **detailed target architecture document** including:

* PostgreSQL partition design  
* Core table schemas  
* Index templates  
* Reporting data mart layout  
* Migration sequence plan  
* Performance acceptance benchmarks

