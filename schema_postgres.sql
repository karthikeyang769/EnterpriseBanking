-- =========================================================================
-- ENTERPRISE SMART BANKING SYSTEM - CASE-SENSITIVE DATABASE SCHEMA SCRIPT
-- Target Database: PostgreSQL 15+
-- =========================================================================

-- -------------------------------------------------------------------------
-- 0. CLEANUP (DROP ALL EXISTING TABLES CASCADE)
-- -------------------------------------------------------------------------
DROP TABLE IF EXISTS "RefreshTokens" CASCADE;
DROP TABLE IF EXISTS "Users" CASCADE;
DROP TABLE IF EXISTS "Customers" CASCADE;
DROP TABLE IF EXISTS "Accounts" CASCADE;
DROP TABLE IF EXISTS "Transactions" CASCADE;
DROP TABLE IF EXISTS "Beneficiaries" CASCADE;
DROP TABLE IF EXISTS "DocumentLogs" CASCADE;
DROP TABLE IF EXISTS "ReportLogs" CASCADE;
DROP TABLE IF EXISTS "AuditLogs" CASCADE;
DROP TABLE IF EXISTS "NotificationLogs" CASCADE;

-- -------------------------------------------------------------------------
-- 1. USERS & CREDENTIALS
-- -------------------------------------------------------------------------
CREATE TABLE "Users" (
    "Id" UUID PRIMARY KEY,
    "Email" VARCHAR(256) NOT NULL,
    "PasswordHash" VARCHAR(512) NOT NULL,
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "PhoneNumber" VARCHAR(50) NOT NULL,
    "Role" VARCHAR(50) NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE
);

CREATE UNIQUE INDEX "ix_users_email" ON "Users"("Email");

CREATE TABLE "RefreshTokens" (
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL,
    "Token" VARCHAR(500) NOT NULL,
    "ExpiresAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "RevokedAt" TIMESTAMP WITH TIME ZONE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    CONSTRAINT "fk_refreshtokens_users" FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE
);

-- -------------------------------------------------------------------------
-- 2. CUSTOMER PROFILE & ADDRESS
-- -------------------------------------------------------------------------
CREATE TABLE "Customers" (
    "Id" UUID PRIMARY KEY,
    "FirstName" VARCHAR(150) NOT NULL,
    "LastName" VARCHAR(150) NOT NULL,
    "Email" VARCHAR(256) NOT NULL,
    "PhoneNumber" VARCHAR(50) NOT NULL,
    "KycStatus" INT NOT NULL,
    "Street" VARCHAR(200),
    "City" VARCHAR(100),
    "State" VARCHAR(100),
    "ZipCode" VARCHAR(20),
    "Country" VARCHAR(100),
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE
);

CREATE UNIQUE INDEX "ix_customers_email" ON "Customers"("Email");

-- -------------------------------------------------------------------------
-- 3. BANK ACCOUNTS
-- -------------------------------------------------------------------------
CREATE TABLE "Accounts" (
    "Id" UUID PRIMARY KEY,
    "CustomerId" UUID NOT NULL,
    "AccountNumber" VARCHAR(50) NOT NULL,
    "AccountType" VARCHAR(50) NOT NULL,
    "Balance" DECIMAL(18,2) DEFAULT 0.00 NOT NULL,
    "Status" VARCHAR(50) DEFAULT 'Active' NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE
);

CREATE UNIQUE INDEX "ix_accounts_number" ON "Accounts"("AccountNumber");
CREATE INDEX "ix_accounts_customer" ON "Accounts"("CustomerId");

-- -------------------------------------------------------------------------
-- 4. FINANCIAL TRANSACTIONS
-- -------------------------------------------------------------------------
CREATE TABLE "Transactions" (
    "Id" UUID PRIMARY KEY,
    "SourceAccountNumber" VARCHAR(50),
    "TargetAccountNumber" VARCHAR(50),
    "Amount" DECIMAL(18,2) NOT NULL,
    "TransactionType" VARCHAR(50) NOT NULL,
    "ReferenceNumber" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500),
    "Status" VARCHAR(50) DEFAULT 'Success' NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE
);

CREATE UNIQUE INDEX "ix_transactions_reference" ON "Transactions"("ReferenceNumber");
CREATE INDEX "ix_transactions_source" ON "Transactions"("SourceAccountNumber");
CREATE INDEX "ix_transactions_target" ON "Transactions"("TargetAccountNumber");

-- -------------------------------------------------------------------------
-- 5. PAYMENT BENEFICIARIES
-- -------------------------------------------------------------------------
CREATE TABLE "Beneficiaries" (
    "Id" UUID PRIMARY KEY,
    "CustomerId" UUID NOT NULL,
    "BeneficiaryName" VARCHAR(150) NOT NULL,
    "BeneficiaryAccountNumber" VARCHAR(50) NOT NULL,
    "Nickname" VARCHAR(100),
    "TransferLimit" DECIMAL(18,2) NOT NULL
);

CREATE INDEX "ix_beneficiaries_customer" ON "Beneficiaries"("CustomerId");

-- -------------------------------------------------------------------------
-- 6. DOCUMENT STORAGE LOGS (KYC)
-- -------------------------------------------------------------------------
CREATE TABLE "DocumentLogs" (
    "Id" UUID PRIMARY KEY,
    "CustomerId" UUID NOT NULL,
    "DocumentType" VARCHAR(100) NOT NULL,
    "FileName" VARCHAR(256) NOT NULL,
    "ObjectName" VARCHAR(256) NOT NULL,
    "UploadedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX "ix_documents_customer" ON "DocumentLogs"("CustomerId");

-- -------------------------------------------------------------------------
-- 7. GENERATED REPORT STATEMENTS
-- -------------------------------------------------------------------------
CREATE TABLE "ReportLogs" (
    "Id" UUID PRIMARY KEY,
    "CustomerId" UUID NOT NULL,
    "AccountNumber" VARCHAR(50) NOT NULL,
    "ReportUrl" VARCHAR(512) NOT NULL,
    "GeneratedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX "ix_reports_customer" ON "ReportLogs"("CustomerId");

-- -------------------------------------------------------------------------
-- 8. SYSTEM AUDITING
-- -------------------------------------------------------------------------
CREATE TABLE "AuditLogs" (
    "Id" UUID PRIMARY KEY,
    "EventName" VARCHAR(100) NOT NULL,
    "EventData" TEXT NOT NULL,
    "OccurredOn" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX "ix_audit_event_name" ON "AuditLogs"("EventName");

-- -------------------------------------------------------------------------
-- 9. DISPATCHED NOTIFICATIONS
-- -------------------------------------------------------------------------
CREATE TABLE "NotificationLogs" (
    "Id" UUID PRIMARY KEY,
    "CustomerId" UUID NOT NULL,
    "Target" VARCHAR(256) NOT NULL,
    "NotificationType" VARCHAR(50) NOT NULL,
    "Title" VARCHAR(200) NOT NULL,
    "Message" TEXT NOT NULL,
    "SentAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX "ix_notifications_customer" ON "NotificationLogs"("CustomerId");
