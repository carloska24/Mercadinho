namespace CaixaMercado.Domain.Model.Auditoria;

public sealed class EventoAuditoria
{
    public EventoAuditoria(Guid id, string acao, Guid recursoId, Guid terminalId, Guid sessaoCaixaId,
        Guid operadorId, DateTimeOffset criadoEmUtc, string? correlationId = null)
    {
        if (id == Guid.Empty || recursoId == Guid.Empty || terminalId == Guid.Empty ||
            sessaoCaixaId == Guid.Empty || operadorId == Guid.Empty)
            throw new ArgumentException("Os identificadores da auditoria são obrigatórios.");
        if (string.IsNullOrWhiteSpace(acao) || acao.Length > 80) throw new ArgumentException("A ação é obrigatória.", nameof(acao));
        if (correlationId?.Length > 100) throw new ArgumentException("A correlação deve ter até 100 caracteres.", nameof(correlationId));
        if (criadoEmUtc.Offset != TimeSpan.Zero) throw new ArgumentException("A data deve estar em UTC.", nameof(criadoEmUtc));
        Id = id; Acao = acao.Trim(); RecursoId = recursoId; TerminalId = terminalId;
        SessaoCaixaId = sessaoCaixaId; OperadorId = operadorId; CriadoEmUtc = criadoEmUtc;
        CorrelationId = string.IsNullOrWhiteSpace(correlationId) ? null : correlationId.Trim();
    }

    public Guid Id { get; }
    public string Acao { get; }
    public Guid RecursoId { get; }
    public Guid TerminalId { get; }
    public Guid SessaoCaixaId { get; }
    public Guid OperadorId { get; }
    public DateTimeOffset CriadoEmUtc { get; }
    public string? CorrelationId { get; }
}
