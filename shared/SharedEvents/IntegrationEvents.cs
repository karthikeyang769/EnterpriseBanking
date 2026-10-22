using System;

namespace SharedEvents;

public record CustomerRegisteredEvent(
    Guid CustomerId,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateTime RegisteredAt);

public record AccountCreatedEvent(
    Guid AccountId,
    Guid CustomerId,
    string AccountNumber,
    string AccountType,
    decimal InitialBalance,
    DateTime CreatedAt);

public record BeneficiaryAddedEvent(
    Guid BeneficiaryId,
    Guid CustomerId,
    string Name,
    string AccountNumber,
    string BankName,
    DateTime AddedAt);

public record MoneyTransferredEvent(
    Guid TransactionId,
    string SourceAccountNumber,
    string TargetAccountNumber,
    decimal Amount,
    string Description,
    string ReferenceNumber,
    DateTime TransferredAt);

public record MoneyTransferFailedEvent(
    Guid TransactionId,
    string SourceAccountNumber,
    string TargetAccountNumber,
    decimal Amount,
    string ErrorMessage,
    DateTime FailedAt);

public record DepositCompletedEvent(
    Guid TransactionId,
    string AccountNumber,
    decimal Amount,
    string ReferenceNumber,
    DateTime CompletedAt);

public record WithdrawalCompletedEvent(
    Guid TransactionId,
    string AccountNumber,
    decimal Amount,
    string ReferenceNumber,
    DateTime CompletedAt);

public record PaymentCompletedEvent(
    Guid PaymentId,
    Guid CustomerId,
    string PaymentType,
    string ReferenceNumber,
    decimal Amount,
    string Status,
    DateTime CompletedAt);

public record DocumentUploadedEvent(
    Guid DocumentId,
    Guid CustomerId,
    string DocumentType,
    string FileUrl,
    DateTime UploadedAt);

public record StatementGeneratedEvent(
    Guid ReportId,
    Guid CustomerId,
    string AccountNumber,
    string FileUrl,
    DateTime GeneratedAt);

public record NotificationRequestedEvent(
    Guid NotificationId,
    Guid CustomerId,
    string Target, // Email address or phone number
    string NotificationType, // "Email" or "SMS"
    string Title,
    string Message);

public record EmailSentEvent(
    Guid NotificationId,
    string EmailAddress,
    string Subject,
    DateTime SentAt);

public record SMSSentEvent(
    Guid NotificationId,
    string PhoneNumber,
    DateTime SentAt);

public record AuditCreatedEvent(
    Guid AuditId,
    string UserId,
    string Action,
    string ServiceName,
    string Details,
    string IpAddress,
    string Browser,
    DateTime CreatedAt);
