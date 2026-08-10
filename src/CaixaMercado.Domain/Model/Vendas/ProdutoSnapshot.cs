using CaixaMercado.Domain.Model.Catalogo;

namespace CaixaMercado.Domain.Model.Vendas;

public sealed record ProdutoSnapshot
{
    private ProdutoSnapshot()
    {
        CodigoInterno = null!;
        Descricao = null!;
    }

    private ProdutoSnapshot(Produto produto)
    {
        ProdutoId = produto.Id;
        CodigoInterno = produto.CodigoInterno;
        Ean = produto.Ean;
        Plu = produto.Plu;
        Descricao = produto.Descricao;
        UnidadeMedida = produto.UnidadeMedida;
        PrecoUnitario = produto.PrecoVenda;
    }

    public Guid ProdutoId { get; private init; }
    public string CodigoInterno { get; private init; }
    public string? Ean { get; private init; }
    public string? Plu { get; private init; }
    public string Descricao { get; private init; }
    public UnidadeMedida UnidadeMedida { get; private init; }
    public decimal PrecoUnitario { get; private init; }

    public static ProdutoSnapshot Criar(Produto produto)
    {
        ArgumentNullException.ThrowIfNull(produto);
        return new ProdutoSnapshot(produto);
    }
}
