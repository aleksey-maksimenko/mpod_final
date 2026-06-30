using System.Globalization;
using HtmlAgilityPack;

namespace DistributedWebCrawler.Parsing;

public class ProductParser
{
    public IEnumerable<Product> Parse(string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var products = new List<Product>();
        var productNodes = document.DocumentNode.SelectNodes("//div[@itemtype='https://schema.org/Product']");

        if (productNodes == null)
            return products;
        foreach (var node in productNodes)
        {
            try
            {
                var article = node.GetAttributeValue("id", "").Trim();
                var nameNode = node.SelectSingleNode(".//span[@itemprop='name']");
                if (nameNode == null)
                    continue;
                var nameHtml = nameNode.InnerHtml.Replace("</b><b>", "</b> <b>");
                var tempDoc = new HtmlDocument();
                tempDoc.LoadHtml($"<span>{nameHtml}</span>");
                var fullName = HtmlEntity.DeEntitize(
                    tempDoc.DocumentNode.InnerText).Trim();
                if (string.IsNullOrWhiteSpace(article) || string.IsNullOrWhiteSpace(fullName))
                {
                    continue;
                }
                var parts = fullName.Split(',', 2);
                var name = parts[0].Trim();
                var description = parts.Length > 1
                    ? parts[1].Trim()
                    : string.Empty;
                var priceNode = node.SelectSingleNode(".//div[contains(@class,'product-price__price--discounted')]");
                priceNode ??= node.SelectSingleNode(".//div[contains(@class,'product-price__price')]");
                double price = 0;
                if (priceNode != null)
                {
                    var priceText = new string(
                        priceNode.InnerText
                            .Where(c => char.IsDigit(c) || c == ',')
                            .ToArray());
                    double.TryParse(
                        priceText.Replace(',', '.'),
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out price);
                }
                products.Add(new Product
                {
                    Article = article,
                    Name = name,
                    Description = description,
                    Price = price,
                    ParsedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка разбора товара: {ex.Message}");
            }
        }
        return products;
    }
}