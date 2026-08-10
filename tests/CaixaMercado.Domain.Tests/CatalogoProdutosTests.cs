using CaixaMercado.Domain.Model.Catalogo;

namespace CaixaMercado.Domain.Tests;

public class CatalogoProdutosTests
{
    [Fact]
    public void Adicionar_EanDuplicado_DeveRejeitar()
    {
        var catalogo = new CatalogoProdutos();
        catalogo.Adicionar(CriarProduto("001", "7891", "101"));
        Assert.Throws<InvalidOperationException>(() => catalogo.Adicionar(CriarProduto("002", "7891", "102")));
    }

    [Fact]
    public void Buscar_IdentificadorQueCoincideComCamposDeProdutosDiferentes_DeveIndicarAmbiguidade()
    {
        var catalogo = new CatalogoProdutos();
        catalogo.Adicionar(CriarProduto("001", "7891", "101"));
        catalogo.Adicionar(CriarProduto("002", "7892", "001"));
        var resultado = catalogo.BuscarPorIdentificador("001");
        Assert.Equal(SituacaoBuscaProduto.IdentificadorAmbiguo, resultado.Situacao);
        Assert.Null(resultado.Produto);
    }

    [Fact]
    public void Buscar_ProdutoInativo_DeveTratarComoNaoEncontrado()
    {
        var catalogo = new CatalogoProdutos();
        catalogo.Adicionar(CriarProduto("001", "7891", "101", ativo: false));
        Assert.Equal(SituacaoBuscaProduto.NaoEncontrado, catalogo.BuscarPorIdentificador("7891").Situacao);
    }

    private static Produto CriarProduto(string codigo, string ean, string plu, bool ativo = true) =>
        new(Guid.NewGuid(), codigo, ean, plu, "Produto", UnidadeMedida.Unidade, 5m, false, ativo);
}
