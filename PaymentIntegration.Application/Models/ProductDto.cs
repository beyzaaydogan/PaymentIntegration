namespace PaymentIntegration.API.Models;

public class ProductDto
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public double Price { get; set; }

    public string Currency { get; set; }

    public string Category { get; set; }

    public int Stock { get; set; }

}