namespace CaixaMercado.Domain.Model.Caixas;

public enum StatusSessaoCaixa { Aberta = 1, Fechada = 2 }

public sealed class SessaoCaixa
{
    private SessaoCaixa() { }

    private SessaoCaixa(Guid id, Guid filialId, Guid terminalId, Guid operadorAberturaId,
        decimal valorAbertura, DateTimeOffset abertaEmUtc)
    {
        ValidarId(id, nameof(id)); ValidarId(filialId, nameof(filialId));
        ValidarId(terminalId, nameof(terminalId)); ValidarId(operadorAberturaId, nameof(operadorAberturaId));
        ValidarDinheiro(valorAbertura, nameof(valorAbertura)); ValidarUtc(abertaEmUtc, nameof(abertaEmUtc));
        Id = id; FilialId = filialId; TerminalId = terminalId; OperadorAberturaId = operadorAberturaId;
        ValorAbertura = valorAbertura; AbertaEmUtc = abertaEmUtc; Status = StatusSessaoCaixa.Aberta;
    }

    public Guid Id { get; }
    public Guid FilialId { get; }
    public Guid TerminalId { get; }
    public Guid OperadorAberturaId { get; }
    public decimal ValorAbertura { get; }
    public DateTimeOffset AbertaEmUtc { get; }
    public StatusSessaoCaixa Status { get; private set; }
    public Guid? OperadorFechamentoId { get; private set; }
    public decimal? ValorEsperadoFechamento { get; private set; }
    public decimal? ValorContadoFechamento { get; private set; }
    public decimal? DiferencaFechamento => ValorContadoFechamento - ValorEsperadoFechamento;
    public DateTimeOffset? FechadaEmUtc { get; private set; }
    public long Versao { get; private set; }

    public static SessaoCaixa Abrir(Guid id, Guid filialId, Guid terminalId, Guid operadorId,
        decimal valorAbertura, DateTimeOffset abertaEmUtc) =>
        new(id, filialId, terminalId, operadorId, valorAbertura, abertaEmUtc);

    public void Fechar(Guid operadorId, decimal valorEsperado, decimal valorContado, DateTimeOffset fechadaEmUtc)
    {
        if (Status != StatusSessaoCaixa.Aberta) throw new InvalidOperationException("A sessão de caixa já está fechada.");
        ValidarId(operadorId, nameof(operadorId)); ValidarDinheiro(valorEsperado, nameof(valorEsperado));
        ValidarDinheiro(valorContado, nameof(valorContado)); ValidarUtc(fechadaEmUtc, nameof(fechadaEmUtc));
        if (fechadaEmUtc < AbertaEmUtc) throw new ArgumentException("O fechamento não pode anteceder a abertura.", nameof(fechadaEmUtc));
        OperadorFechamentoId = operadorId; ValorEsperadoFechamento = valorEsperado;
        ValorContadoFechamento = valorContado; FechadaEmUtc = fechadaEmUtc;
        Status = StatusSessaoCaixa.Fechada; Versao++;
    }

    public void RegistrarVenda()
    {
        if (Status != StatusSessaoCaixa.Aberta)
            throw new InvalidOperationException("Não é possível registrar venda em uma sessão fechada.");
        Versao++;
    }

    private static void ValidarId(Guid id, string nome) { if (id == Guid.Empty) throw new ArgumentException("O identificador é obrigatório.", nome); }
    private static void ValidarDinheiro(decimal valor, string nome)
    { if (valor < 0 || decimal.Round(valor, 2) != valor) throw new ArgumentOutOfRangeException(nome, "O valor deve ser não negativo e ter até duas casas decimais."); }
    private static void ValidarUtc(DateTimeOffset valor, string nome)
    { if (valor.Offset != TimeSpan.Zero) throw new ArgumentException("A data deve estar em UTC.", nome); }
}
