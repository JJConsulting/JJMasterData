using JJMasterData.Commons.Data; 
using Microsoft.Data.Sqlite;

var dataAccess = new DataAccess("Data Source=:memory:", SqliteFactory.Instance);
var command = new DataAccessCommand("SELECT 1 AS product_id, 'Sample' AS Description, 19.95 AS Price");
var products = await dataAccess.GetListAsync<Product>(command);
var product = await dataAccess.GetAsync<Product>(command);

Console.WriteLine($"{products.Count} product(s): {product!.Description}, {product.Price}");

[DataAccessModel]
public partial class Product
{
    [DataAccessColumn("product_id")]
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
}

