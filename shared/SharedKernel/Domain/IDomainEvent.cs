using System;

namespace SharedKernel.Domain;

/// <summary>
/// Marker interface for Domain Events
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
