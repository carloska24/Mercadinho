using CaixaMercado.Domain.Model.Catalogo;
using CaixaMercado.Domain.Model.Vendas;

namespace CaixaMercado.Application.Operacional.Contratos;

public enum CodigoOperacao
{
    Sucesso = 0,
    RequisicaoInvalida = 1,
    ProdutoNaoEncontrado = 2,
    IdentificadorProdutoAmbiguo = 3,
    VendaNaoEncontrada = 4,
    ConflitoVersao = 5,
    ChaveIdempotenciaReutilizada = 6,
    RegraNegocioViolada = 7,
    ConflitoIdempotencia = 8
}

public sealed record ResultadoOperacao<T>(CodigoOperacao Codigo, T? Dados, string? Mensagem = null)
{
    public bool Sucesso => Codigo == CodigoOperacao.Sucesso;

    public static ResultadoOperacao<T> Ok(T dados) => new(CodigoOperacao.Sucesso, dados);

    public static ResultadoOperacao<T> Falha(CodigoOperacao codigo, string mensagem) =>
        new(codigo, default, mensagem);
}

public sealed record ProdutoDto(
    Guid Id,
    string CodigoInterno,
    string? Ean,
    string? Plu,
    string Descricao,
    UnidadeMedida UnidadeMedida,
    decimal PrecoVenda,
    bool ProdutoPesavel);

public sealed record ItemVendaDto(
    Guid Id,
    int Sequencial,
    Guid ProdutoId,
    string CodigoInterno,
    string Descricao,
    UnidadeMedida UnidadeMedida,
    decimal PrecoUnitario,
    decimal Quantidade,
    decimal Desconto,
    decimal Total);

public sealed record VendaDto(
    Guid Id,
    long? Numero,
    Guid FilialId,
    Guid TerminalId,
    Guid SessaoCaixaId,
    Guid OperadorId,
    DateTimeOffset CriadaEmUtc,
    StatusVendaOperacional Status,
    long Versao,
    decimal QuantidadeTotal,
    decimal Subtotal,
    decimal Desconto,
    decimal Total,
    IReadOnlyList<ItemVendaDto> Itens);

public sealed record PesquisarProdutosQuery(string? Termo, int Limite = 50);

public sealed record ResolverProdutoQuery(string Identificador);

public sealed record CriarVendaCommand(
    Guid VendaId,
    Guid FilialId,
    Guid TerminalId,
    Guid SessaoCaixaId,
    Guid OperadorId,
    string ChaveIdempotencia);

public sealed record AdicionarItemVendaCommand(
    Guid VendaId,
    Guid TerminalId,
    string IdentificadorProduto,
    decimal Quantidade,
    long VersaoEsperada,
    string ChaveIdempotencia);
