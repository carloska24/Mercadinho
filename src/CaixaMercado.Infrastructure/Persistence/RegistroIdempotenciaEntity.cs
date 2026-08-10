using CaixaMercado.Application.Operacional.Contratos;
using CaixaMercado.Application.Operacional.Portas;

namespace CaixaMercado.Infrastructure.Persistence;

internal sealed class RegistroIdempotenciaEntity
{
    private RegistroIdempotenciaEntity()
    {
        Operacao = null!;
        Chave = null!;
        HashRequisicao = null!;
    }

    private RegistroIdempotenciaEntity(RegistroIdempotencia registro)
    {
        Id = Guid.NewGuid();
        Operacao = registro.Operacao;
        TerminalId = registro.TerminalId;
        Chave = registro.Chave;
        HashRequisicao = registro.HashRequisicao;
        CodigoResultado = registro.CodigoResultado;
        Mensagem = registro.Mensagem;
        RecursoJson = registro.RecursoJson;
        CriadoEmUtc = registro.CriadoEmUtc;
    }

    public Guid Id { get; private set; }
    public string Operacao { get; private set; }
    public Guid TerminalId { get; private set; }
    public string Chave { get; private set; }
    public string HashRequisicao { get; private set; }
    public CodigoOperacao CodigoResultado { get; private set; }
    public string? Mensagem { get; private set; }
    public string? RecursoJson { get; private set; }
    public DateTimeOffset CriadoEmUtc { get; private set; }

    public static RegistroIdempotenciaEntity Criar(RegistroIdempotencia registro) => new(registro);

    public RegistroIdempotencia ParaRegistro() => new(
        Operacao,
        TerminalId,
        Chave,
        HashRequisicao,
        CodigoResultado,
        Mensagem,
        RecursoJson,
        CriadoEmUtc);
}
