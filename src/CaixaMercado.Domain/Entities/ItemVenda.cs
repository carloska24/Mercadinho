namespace CaixaMercado.Domain.Entities;

public class ItemVenda
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Sequencial { get; set; }
    public Guid ProdutoId { get; set; }
    public string CodigoProduto { get; set; } = string.Empty;
    public string EAN { get; set; } = string.Empty;
    public string DescricaoProduto { get; set; } = string.Empty;
    public decimal Quantidade { get; set; } = 1m;
    public string Unidade { get; set; } = "UN";
    public decimal PrecoUnitario { get; set; }
    public decimal Desconto { get; set; }
    public decimal Total => Math.Max(0m, (Quantidade * PrecoUnitario) - Desconto);
}
