namespace CaixaMercado.Domain.Entities;

public class Produto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public string EAN { get; set; } = string.Empty;
    public string PLU { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Unidade { get; set; } = "UN";
    public decimal PrecoCusto { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal Estoque { get; set; }
    public decimal EstoqueMinimo { get; set; }
    public bool ProdutoPesavel { get; set; }
    public bool Ativo { get; set; } = true;
}
