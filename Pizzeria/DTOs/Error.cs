namespace Pizzeria.DTOs
{
    public sealed record Error(string Code, string? Message = null);
}
