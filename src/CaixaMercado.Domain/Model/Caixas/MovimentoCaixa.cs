using CaixaMercado.Domain.Model.Vendas;

namespace CaixaMercado.Domain.Model.Caixas;

public sealed class MovimentoCaixa
{
    public MovimentoCaixa(Guid id, Guid sessaoCaixaId, Guid vendaId, Guid pagamentoId,
        FormaPagamentoOperacional forma, decimal valorLiquido, decimal valorRecebido,
        decimal troco, DateTimeOffset criadoEmUtc)
    {
        if (id == Guid.Empty || sessaoCaixaId == Guid.Empty || vendaId == Guid.Empty || pagamentoId == Guid.Empty)
            throw new ArgumentException("Os identificadores do movimento de caixa são obrigatórios.");
        if (valorLiquido <= 0 || valorRecebido < valorLiquido || troco != valorRecebido - valorLiquido)
            throw new ArgumentException("Os valores do movimento de caixa são inconsistentes.");
        if (criadoEmUtc.Offset != TimeSpan.Zero) throw new ArgumentException("A data deve estar em UTC.", nameof(criadoEmUtc));
        Id = id; SessaoCaixaId = sessaoCaixaId; VendaId = vendaId; PagamentoId = pagamentoId;
        Forma = forma; ValorLiquido = valorLiquido; ValorRecebido = valorRecebido; Troco = troco; CriadoEmUtc = criadoEmUtc;
    }

    public Guid Id { get; }
    public Guid SessaoCaixaId { get; }
    public Guid VendaId { get; }
    public Guid PagamentoId { get; }
    public FormaPagamentoOperacional Forma { get; }
    public decimal ValorLiquido { get; }
    public decimal ValorRecebido { get; }
    public decimal Troco { get; }
    public DateTimeOffset CriadoEmUtc { get; }
}
