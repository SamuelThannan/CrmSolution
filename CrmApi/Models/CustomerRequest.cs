namespace CrmApi.Models;

public record CustomerRequest(
    string Name,
    string Title,
    string Phone,
    string Email,
    string Address,
    SalesRepresentativeRequest SalesRepresentative
);

public record SalesRepresentativeRequest(
    string Name,
    string Phone,
    string Email
);