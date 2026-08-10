namespace CaixaMercado.Domain.Model.Vendas;

public enum FormaPagamentoOperacional { Dinheiro = 1, Pix = 2, CartaoDebito = 3, CartaoCredito = 4, Voucher = 5 }
public enum StatusPagamentoOperacional { Pendente = 1, Aprovado = 2, Recusado = 3, ResultadoDesconhecido = 4, Revertido = 5 }

public sealed class PagamentoVenda
{
    public PagamentoVenda(Guid id, Guid vendaId, Guid sessaoCaixaId, FormaPagamentoOperacional forma,
        decimal valorAplicado, StatusPagamentoOperacional status, DateTimeOffset registradoEmUtc,
        decimal? valorRecebidoDinheiro = null, string? referenciaExterna = null)
    {
        if (id == Guid.Empty || vendaId == Guid.Empty || sessaoCaixaId == Guid.Empty)
            throw new ArgumentException("Os identificadores do pagamento são obrigatórios.");
        if (!Enum.IsDefined(forma)) throw new ArgumentOutOfRangeException(nameof(forma));
        if (!Enum.IsDefined(status)) throw new ArgumentOutOfRangeException(nameof(status));
        if (valorAplicado <= 0 || decimal.Round(valorAplicado, 2) != valorAplicado)
            throw new ArgumentOutOfRangeException(nameof(valorAplicado), "O valor aplicado deve ser positivo e ter até duas casas decimais.");
        if (registradoEmUtc.Offset != TimeSpan.Zero) throw new ArgumentException("A data deve estar em UTC.", nameof(registradoEmUtc));
        if (referenciaExterna?.Length > 100) throw new ArgumentException("A referência externa deve ter até 100 caracteres.", nameof(referenciaExterna));
        if (forma == FormaPagamentoOperacional.Dinheiro)
        {
            var recebido = valorRecebidoDinheiro ?? valorAplicado;
            if (recebido < valorAplicado || decimal.Round(recebido, 2) != recebido)
                throw new ArgumentOutOfRangeException(nameof(valorRecebidoDinheiro), "O valor recebido em dinheiro não pode ser menor que o aplicado.");
            ValorRecebidoDinheiro = recebido;
        }
        else
        {
            if (valorRecebidoDinheiro is not null) throw new ArgumentException("Valor recebido é exclusivo de dinheiro.", nameof(valorRecebidoDinheiro));
            if (status == StatusPagamentoOperacional.Aprovado && string.IsNullOrWhiteSpace(referenciaExterna))
                throw new ArgumentException("Pagamento eletrônico aprovado exige referência externa.", nameof(referenciaExterna));
        }
        Id = id; VendaId = vendaId; SessaoCaixaId = sessaoCaixaId; Forma = forma;
        ValorAplicado = valorAplicado; Status = status; RegistradoEmUtc = registradoEmUtc;
        ReferenciaExterna = string.IsNullOrWhiteSpace(referenciaExterna) ? null : referenciaExterna.Trim();
    }

    public Guid Id { get; }
    public Guid VendaId { get; }
    public Guid SessaoCaixaId { get; }
    public FormaPagamentoOperacional Forma { get; }
    public decimal ValorAplicado { get; }
    public StatusPagamentoOperacional Status { get; }
    public decimal? ValorRecebidoDinheiro { get; }
    public decimal Troco => Forma == FormaPagamentoOperacional.Dinheiro ? ValorRecebidoDinheiro!.Value - ValorAplicado : 0m;
    public DateTimeOffset RegistradoEmUtc { get; }
    public string? ReferenciaExterna { get; }
}
