using CaixaMercado.Application.Operacional.Contratos;
using CaixaMercado.Domain.Model.Catalogo;
using CaixaMercado.Domain.Model.Vendas;
using CaixaMercado.Domain.Model.Caixas;
using CaixaMercado.Domain.Model.Estoque;
using CaixaMercado.Domain.Model.Auditoria;

namespace CaixaMercado.Application.Operacional.Portas;

public interface IProdutoRepository
{
    Task<IReadOnlyList<Produto>> PesquisarAsync(string? termo, int limite, CancellationToken cancellationToken);
    Task<ResultadoBuscaProduto> ResolverPorIdentificadorAsync(string identificador, CancellationToken cancellationToken);
}

public interface IVendaRepository
{
    Task<Venda?> ObterAsync(Guid vendaId, CancellationToken cancellationToken);
    Task AdicionarAsync(Venda venda, CancellationToken cancellationToken);

    /// <summary>
    /// Prepara a atualização usando a versão original informada. A implementação deve
    /// garantir concorrência otimista no commit da unidade de trabalho.
    /// </summary>
    Task AtualizarAsync(Venda venda, long versaoEsperada, CancellationToken cancellationToken);
}

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken);
    Task DescartarAlteracoesAsync(CancellationToken cancellationToken);
}

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public interface ISessaoCaixaRepository
{
    Task<SessaoCaixa?> ObterAsync(Guid sessaoId, CancellationToken cancellationToken);
    Task<SessaoCaixa?> ObterAbertaPorTerminalAsync(Guid terminalId, CancellationToken cancellationToken);
    Task AdicionarAsync(SessaoCaixa sessao, CancellationToken cancellationToken);
    Task AtualizarAsync(SessaoCaixa sessao, long versaoEsperada, CancellationToken cancellationToken);
}

public interface IPagamentoVendaRepository
{
    Task AdicionarAsync(IReadOnlyCollection<PagamentoVenda> pagamentos, CancellationToken cancellationToken);
}

public interface IEstoqueRepository
{
    /// <summary>Agenda uma baixa condicional. Deve retornar false sem confirmar saldo negativo.</summary>
    Task<bool> TentarBaixarAsync(MovimentoEstoque movimento, CancellationToken cancellationToken);
}

public interface IMovimentoCaixaRepository
{
    Task AdicionarAsync(IReadOnlyCollection<MovimentoCaixa> movimentos, CancellationToken cancellationToken);
    Task<decimal> ObterRecebimentoLiquidoDinheiroAsync(Guid sessaoCaixaId, CancellationToken cancellationToken);
}

public interface IAuditoriaRepository
{
    Task AdicionarAsync(EventoAuditoria evento, CancellationToken cancellationToken);
}

public sealed record RegistroIdempotencia(
    string Operacao,
    Guid TerminalId,
    string Chave,
    string HashRequisicao,
    CodigoOperacao CodigoResultado,
    string? Mensagem,
    string? RecursoJson,
    DateTimeOffset CriadoEmUtc);

public interface IIdempotencyStore
{
    Task<RegistroIdempotencia?> ObterAsync(
        Guid terminalId,
        string chave,
        CancellationToken cancellationToken);

    Task AdicionarAsync(RegistroIdempotencia registro, CancellationToken cancellationToken);
}

public sealed class ConflitoConcorrenciaException : Exception
{
    public ConflitoConcorrenciaException(string? mensagem = null, Exception? innerException = null)
        : base(mensagem ?? "O recurso foi alterado por outra operação.", innerException)
    {
    }
}

public sealed class ConflitoIdempotenciaException : Exception
{
    public ConflitoIdempotenciaException(string? mensagem = null, Exception? innerException = null)
        : base(mensagem ?? "A chave de idempotência foi registrada simultaneamente.", innerException)
    {
    }
}

public sealed class ConflitoSessaoCaixaException : Exception
{
    public ConflitoSessaoCaixaException(string? mensagem = null, Exception? innerException = null)
        : base(mensagem ?? "Já existe uma sessão aberta para o terminal.", innerException) { }
}
