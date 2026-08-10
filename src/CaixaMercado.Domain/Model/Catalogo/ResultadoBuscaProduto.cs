namespace CaixaMercado.Domain.Model.Catalogo;

public enum SituacaoBuscaProduto
{
    Encontrado = 1,
    NaoEncontrado = 2,
    IdentificadorAmbiguo = 3
}

public sealed record ResultadoBuscaProduto(SituacaoBuscaProduto Situacao, Produto? Produto)
{
    public static ResultadoBuscaProduto Encontrado(Produto produto) => new(SituacaoBuscaProduto.Encontrado, produto);
    public static ResultadoBuscaProduto NaoEncontrado() => new(SituacaoBuscaProduto.NaoEncontrado, null);
    public static ResultadoBuscaProduto Ambiguo() => new(SituacaoBuscaProduto.IdentificadorAmbiguo, null);
}
