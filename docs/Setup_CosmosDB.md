
# ☁️ Cosmos DB Setup Guide - Market Data Platform

This guide walks through the steps required to set up the Azure Cosmos DB resources for the Market Data Platform.

---

## 📋 Prerequisites

- Azure Subscription
- Access to Azure Portal or Azure CLI
- Resource Group created for your environment (e.g., `rg-marketdata-dev`)

---

## 🏛 Step 1: Create Cosmos DB Account

**Service:** Azure Cosmos DB for NoSQL (Core SQL API)

**Steps:**

1. Navigate to **Azure Portal** ➔ **Create a Resource** ➔ **Databases** ➔ **Azure Cosmos DB for NoSQL**.
2. Fill in the required fields:
   - **Subscription:** (Select your subscription)
   - **Resource Group:** `dev-saf-rg-mktdatastore` (or your group)
   - **Account Name:** `cosmos-marketdata-dev`
   - **API:** Core (SQL)
   - **Region:** Choose the region closest to your app (e.g., `North Europe` or `West Europe`)
   - **Capacity Mode:** Provisioned Throughput (start small, scale later)
3. Enable the following options:
   - **Geo-Redundancy:** Disabled (for development)
   - **Multi-region Writes:** Disabled (for development)
   - **Backup Policy:** Continuous (recommended)
4. Review + Create.

---

## 🗂 Step 2: Create Database and Containers

### Database Creation

- **Database ID:** `MarketData`
- **Throughput:** 400 RU/s (adjustable later)
- **Provisioning Mode:** Manual (can enable autoscale later)
- **Encryption:** Microsoft-managed key (default)

### Container: `marketdata-history`

| Setting | Value |
|:--------|:------|
| **Container ID** | `marketdata-history` |
| **Partition Key Path** | `/assetId` |
| **Unique Key Constraints** | (Optional, none for now) |
| **Indexing Policy** | Custom (see below) |
| **Default TTL** | Off (for temporal data) |

#### Custom Indexing Policy

- **Composite Indexes:**

```json
[
  {
    "path": "/assetId/?",
    "order": "ascending"
  },
  {
    "path": "/timestamp/?",
    "order": "ascending"
  },
  {
    "path": "/dataType/?",
    "order": "ascending"
  },
  {
    "path": "/timestamp/?",
    "order": "ascending"
  }
]
```

- **Exclude Large Payloads (Optional):**  
  E.g., if vol surfaces become too large:

```json
{
  "path": "/surface/?",
  "indexingMode": "none"
}
```

---

### (Later Phase 2) Container: `marketdata-live`

| Setting | Value |
|:--------|:------|
| **Container ID** | `marketdata-live` |
| **Partition Key Path** | `/assetId` |
| **Default TTL** | 1 day (optional - to auto-expire old live data) |

---

## 🛡 Step 3: Networking and Security

- Allow access from selected networks if necessary.
- Use **Managed Identity** or **Key Vault** to store the Cosmos DB connection strings securely.

---

## 🧹 Step 4: Cleanup for Dev/Test

If setting up for dev/test environments:
- Set **cost alerts** on the resource group.
- Set **automatic backup retention**.
- Periodically prune old market data if necessary.

---

# 🚀 Quick Connection Details for Development

Once CosmosDB is provisioned:

- **Connection String:** (available under *Keys* blade in Azure Portal)
- **Primary Endpoint:** https://cosmos-marketdata-dev.documents.azure.com:443/
- **Primary Key:** (securely stored)
- **Database Name:** `MarketData`
- **Container Name:** `marketdata-history`

---

# 🎯 Summary

✅ Azure Cosmos DB for NoSQL (Core SQL API) account created.  
✅ `MarketData` database with `marketdata-history` container initialized.  
✅ Proper partitioning, indexing, and performance optimization ready.  
✅ Future phases (live, archival) supported seamlessly.

---

# 📢 Status: 🔵 Ready to integrate Cosmos DB into C# services
