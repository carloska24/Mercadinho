namespace CaixaMercado.Domain.Model.Catalogo;

public sealed class Produto
{
    private Produto()
    {
        CodigoInterno = null!;
        Descricao = null!;
    }

    public Produto(Guid id, string codigoInterno, string? ean, string? plu, string descricao,
        UnidadeMedida unidadeMedida, decimal precoVenda, bool produtoPesavel, bool ativo = true)
    {
        if (id == Guid.Empty) throw new ArgumentException("O identificador do produto é obrigatório.", nameof(id));
        if (string.IsNullOrWhiteSpace(codigoInterno)) throw new ArgumentException("O código interno é obrigatório.", nameof(codigoInterno));
        if (string.IsNullOrWhiteSpace(descricao)) throw new ArgumentException("A descrição é obrigatória.", nameof(descricao));
        if (!Enum.IsDefined(unidadeMedida)) throw new ArgumentOutOfRangeException(nameof(unidadeMedida));
        if (precoVenda < 0m) throw new ArgumentOutOfRangeException(nameof(precoVenda), "O preço não pode ser negativo.");
        if (decimal.Round(precoVenda, 2) != precoVenda) throw new ArgumentException("O preço deve ter no máximo duas casas decimais.", nameof(precoVenda));
        if (produtoPesavel && unidadeMedida != UnidadeMedida.Quilograma)
            throw new ArgumentException("Produto pesável deve usar quilograma como unidade.", nameof(unidadeMedida));

        Id = id;
        CodigoInterno = Normalizar(codigoInterno);
        Ean = NormalizarOpcional(ean);
        Plu = NormalizarOpcional(plu);
        Descricao = descricao.Trim();
        UnidadeMedida = unidadeMedida;
        PrecoVenda = precoVenda;
        ProdutoPesavel = produtoPesavel;
        Ativo = ativo;
    }

    public Guid Id { get; }
    public string CodigoInterno { get; }
    public string? Ean { get; }
    public string? Plu { get; }
    public string Descricao { get; }
    public UnidadeMedida UnidadeMedida { get; }
    public decimal PrecoVenda { get; }
    public bool ProdutoPesavel { get; }
    public bool Ativo { get; }

    private static string Normalizar(string valor) => valor.Trim().ToUpperInvariant();
    private static string? NormalizarOpcional(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : Normalizar(valor);
}
