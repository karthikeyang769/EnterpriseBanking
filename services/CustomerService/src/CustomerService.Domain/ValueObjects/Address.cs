using System.Collections.Generic;
using SharedKernel.Domain;

namespace CustomerService.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; } = string.Empty;
    public string City { get; } = string.Empty;
    public string State { get; } = string.Empty;
    public string ZipCode { get; } = string.Empty;
    public string Country { get; } = string.Empty;

    // EF Core deserialization constructor
    private Address() { }

    public Address(string street, string city, string state, string zipCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
    }
}
