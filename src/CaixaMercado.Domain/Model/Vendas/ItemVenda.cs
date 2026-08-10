using CaixaMercado.Domain.Model.Catalogo;

namespace CaixaMercado.Domain.Model.Vendas;

public sealed class ItemVenda
{
    private ItemVenda()
    {
        Produto = null!;
    }

    internal ItemVenda(Guid id, int sequencial, ProdutoSnapshot produto, decimal quantidade)
    {
        Id = id;
        Sequencial = sequencial;
        Produto = produto;
        ValidarQuantidade(quantidade, produto.UnidadeMedida);
        Quantidade = quantidade;
    }

    public Guid Id { get; }
    public int Sequencial { get; }
    public ProdutoSnapshot Produto { get; }
    public decimal Quantidade { get; private set; }
    public decimal Desconto { get; private set; }
    public decimal ValorBruto => Quantidade * Produto.PrecoUnitario;
    public decimal Total => ValorBruto - Desconto;

    internal void Acrescentar(decimal quantidade)
    {
        ValidarQuantidade(quantidade, Produto.UnidadeMedida);
        var novaQuantidade = Quantidade + quantidade;
        ValidarQuantidade(novaQuantidade, Produto.UnidadeMedida);
        Quantidade = novaQuantidade;
    }

    internal void AplicarDesconto(decimal valor)
    {
        if (valor < 0m || valor > ValorBruto)
            throw new ArgumentOutOfRangeException(nameof(valor), "O desconto deve estar entre zero e o valor bruto do item.");
        Desconto = valor;
    }

    private static void ValidarQuantidade(decimal quantidade, UnidadeMedida unidadeMedida)
    {
        if (quantidade <= 0m) throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade deve ser maior que zero.");
        if (decimal.Round(quantidade, 3) != quantidade)
            throw new ArgumentException("A quantidade deve ter no máximo três casas decimais.", nameof(quantidade));
        if (unidadeMedida == UnidadeMedida.Unidade && decimal.Truncate(quantidade) != quantidade)
            throw new ArgumentException("Produtos vendidos por unidade exigem quantidade inteira.", nameof(quantidade));
    }
}
