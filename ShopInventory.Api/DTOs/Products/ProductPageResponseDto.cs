namespace ShopInventory.Api.DTOs.Products;

public class ProductPageResponseDto
{
    public IEnumerable<ProductResponseDto> Products { get; set; } = [];

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
