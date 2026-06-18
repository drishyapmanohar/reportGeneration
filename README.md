# Report Generation System – Architecture Overview

## Overview

This solution demonstrates a scalable asynchronous report generation system hosted on Azure.

Instead of making users wait for long-running report generation, the application immediately returns a Job ID and processes the report in the background.

---

## Architecture

```text
Angular UI
    |
    v
ASP.NET Core API
    |
    +----> Azure SQL Database
    |
    +----> Azure Service Bus Queue
                    |
                    v
          Azure Function Worker
                    |
         +----------+----------+
         |                     |
         v                     v
   Azure SQL          Azure Blob Storage
         |
         v
      SignalR
         |
         v
    Angular UI
```

---

## Workflow

### Step 1 – Submit Report

The user clicks "Generate Report".

The API:

* Creates a ReportJob record in Azure SQL
* Sets status = Pending
* Publishes JobId to Azure Service Bus
* Returns JobId immediately

### Step 2 – Queue Processing

Azure Service Bus stores the message until it is processed.

Benefits:

* Decouples API from report generation
* Handles spikes in traffic
* Improves reliability

### Step 3 – Background Processing

Azure Function consumes the message.

The worker:

* Updates status to InProgress
* Generates report data
* Creates CSV report
* Uploads report to Azure Blob Storage

### Step 4 – Store Result

After successful generation:

* Status = Completed
* File path stored in SQL
* Completion timestamp recorded

### Step 5 – Real-Time Updates

Azure Function notifies the API.

SignalR pushes status updates to connected Angular clients.

The UI updates automatically:

Pending → InProgress → Completed

### Step 6 – Secure Download

Reports are stored in private Azure Blob Storage.

When a user clicks Download:

* API validates the report
* Generates a temporary SAS URL
* Redirects the browser to Blob Storage

Benefits:

* Blob container remains private
* Download links expire automatically
* Reduced API bandwidth

---

## Scalability Features

### Azure Service Bus

Provides reliable asynchronous messaging and supports retry and dead-letter queues.

### Azure Functions

Allows background processing and can scale independently of the API.

### Azure Blob Storage

Low-cost storage for generated reports.

### Server-Side Pagination

Report history is paginated at the database level using:

* Skip()
* Take()

This avoids loading large datasets into the UI.

### Database Indexing

Created index:

IX_ReportJobs_CreatedAt

Used to improve pagination performance and reduce table scans.

### SignalR

Provides real-time report status updates without continuous polling.

---

## Security Improvements

### SAS Downloads

Temporary signed URLs are generated for report downloads.

Benefits:

* Private blob containers
* Time-limited access
* No storage credentials exposed to clients

---

## Technologies Used

Frontend

* Angular 20
* Signals
* SignalR

Backend

* ASP.NET Core
* Entity Framework Core

Azure

* Azure SQL Database
* Azure Service Bus
* Azure Functions
* Azure Blob Storage

---

## Future Enhancements

* Redis caching
* Application Insights monitoring
* Retry policies with exponential backoff
* Multiple report types
* User authentication and authorization
