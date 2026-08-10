using CaixaMercado.Application.Operacional.Contratos;
using CaixaMercado.Domain.Model.Catalogo;
using CaixaMercado.Domain.Model.Vendas;

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
