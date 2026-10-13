using JJMasterData.Commons.Data; 
using Microsoft.Data.Sqlite;

var dataAccess = new DataAccess("Data Source=:memory:", SqliteFactory.Instance);
var command = new DataAccessCommand("SELECT 1 AS product_id, 'Sample' AS Description, 19.95 AS Price");
var products = await dataAccess.GetProductListAsync(command);
var product = await dataAccess.GetProductAsync(command);

Console.WriteLine($"{products.Count} product(s): {product!.Description}, {product.Price}");

[DataAccessModel]
public class Product
{
    [DataAccessColumn("product_id")]
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
}

