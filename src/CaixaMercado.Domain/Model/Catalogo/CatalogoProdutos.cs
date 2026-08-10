namespace CaixaMercado.Domain.Model.Catalogo;

public sealed class CatalogoProdutos
{
    private readonly List<Produto> _produtos = new();
    public IReadOnlyList<Produto> Produtos => _produtos.AsReadOnly();

    public void Adicionar(Produto produto)
    {
        ArgumentNullException.ThrowIfNull(produto);
        if (_produtos.Any(p => p.Id == produto.Id)) throw new InvalidOperationException("Já existe produto com o mesmo identificador.");
        if (_produtos.Any(p => Comparar(p.CodigoInterno, produto.CodigoInterno))) throw new InvalidOperationException("Já existe produto com o mesmo código interno.");
        if (produto.Ean is not null && _produtos.Any(p => Comparar(p.Ean, produto.Ean))) throw new InvalidOperationException("Já existe produto com o mesmo EAN.");
        if (produto.Plu is not null && _produtos.Any(p => Comparar(p.Plu, produto.Plu))) throw new InvalidOperationException("Já existe produto com o mesmo PLU.");
        _produtos.Add(produto);
    }

    public ResultadoBuscaProduto BuscarPorIdentificador(string identificador)
    {
        if (string.IsNullOrWhiteSpace(identificador)) return ResultadoBuscaProduto.NaoEncontrado();
        var normalizado = identificador.Trim();
        var encontrados = _produtos
            .Where(p => p.Ativo && (Comparar(p.CodigoInterno, normalizado) || Comparar(p.Ean, normalizado) || Comparar(p.Plu, normalizado)))
            .DistinctBy(p => p.Id).Take(2).ToArray();

        return encontrados.Length switch
        {
            0 => ResultadoBuscaProduto.NaoEncontrado(),
            1 => ResultadoBuscaProduto.Encontrado(encontrados[0]),
            _ => ResultadoBuscaProduto.Ambiguo()
        };
    }

    private static bool Comparar(string? esquerdo, string? direito) =>
        esquerdo is not null && direito is not null && esquerdo.Equals(direito, StringComparison.OrdinalIgnoreCase);
}
