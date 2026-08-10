namespace CaixaMercado.Domain.Model.Estoque;

public enum TipoMovimentoEstoque { SaidaVenda = 1, EntradaCancelamento = 2, AjusteEntrada = 3, AjusteSaida = 4 }

public sealed class MovimentoEstoque
{
    private MovimentoEstoque(Guid id, Guid produtoId, Guid? vendaId, Guid? itemVendaId,
        TipoMovimentoEstoque tipo, decimal quantidade, DateTimeOffset criadoEmUtc)
    {
        if (id == Guid.Empty || produtoId == Guid.Empty) throw new ArgumentException("Os identificadores do movimento são obrigatórios.");
        if (!Enum.IsDefined(tipo)) throw new ArgumentOutOfRangeException(nameof(tipo));
        if (quantidade <= 0 || decimal.Round(quantidade, 3) != quantidade)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade deve ser positiva e ter até três casas decimais.");
        if (criadoEmUtc.Offset != TimeSpan.Zero) throw new ArgumentException("A data deve estar em UTC.", nameof(criadoEmUtc));
        Id = id; ProdutoId = produtoId; VendaId = vendaId; ItemVendaId = itemVendaId;
        Tipo = tipo; Quantidade = quantidade; CriadoEmUtc = criadoEmUtc;
    }

    public Guid Id { get; }
    public Guid ProdutoId { get; }
    public Guid? VendaId { get; }
    public Guid? ItemVendaId { get; }
    public TipoMovimentoEstoque Tipo { get; }
    public decimal Quantidade { get; }
    public DateTimeOffset CriadoEmUtc { get; }

    public static MovimentoEstoque SaidaPorVenda(Guid id, Guid produtoId, Guid vendaId, Guid itemVendaId,
        decimal quantidade, DateTimeOffset criadoEmUtc)
    {
        if (vendaId == Guid.Empty || itemVendaId == Guid.Empty) throw new ArgumentException("Venda e item são obrigatórios.");
        return new(id, produtoId, vendaId, itemVendaId, TipoMovimentoEstoque.SaidaVenda, quantidade, criadoEmUtc);
    }
}
