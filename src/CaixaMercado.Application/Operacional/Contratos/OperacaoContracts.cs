using CaixaMercado.Domain.Model.Catalogo;
using CaixaMercado.Domain.Model.Vendas;
using CaixaMercado.Domain.Model.Caixas;

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
    ConflitoIdempotencia = 8,
    SessaoCaixaNaoEncontrada = 9,
    SessaoCaixaJaAberta = 10,
    SessaoCaixaFechada = 11,
    EstoqueInsuficiente = 12
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

public sealed record SessaoCaixaDto(Guid Id, Guid FilialId, Guid TerminalId, Guid OperadorAberturaId,
    decimal ValorAbertura, DateTimeOffset AbertaEmUtc, StatusSessaoCaixa Status,
    Guid? OperadorFechamentoId, decimal? ValorEsperadoFechamento, decimal? ValorContadoFechamento,
    decimal? DiferencaFechamento, DateTimeOffset? FechadaEmUtc, long Versao);

public sealed record AbrirSessaoCaixaCommand(Guid SessaoCaixaId, Guid FilialId, Guid TerminalId,
    Guid OperadorId, decimal ValorAbertura, string ChaveIdempotencia);

public sealed record FecharSessaoCaixaCommand(Guid SessaoCaixaId, Guid TerminalId, Guid OperadorId,
    decimal ValorContado, long VersaoEsperada, string ChaveIdempotencia);

public sealed record PagamentoCommand(Guid PagamentoId, FormaPagamentoOperacional Forma,
    decimal ValorAplicado, StatusPagamentoOperacional Status, decimal? ValorRecebidoDinheiro = null,
    string? ReferenciaExterna = null);

public sealed record FinalizarVendaCommand(Guid VendaId, Guid TerminalId, Guid OperadorId,
    long VersaoEsperada, IReadOnlyList<PagamentoCommand> Pagamentos, string ChaveIdempotencia,
    Guid? AutorizacaoId = null, string? CorrelationId = null);

public sealed record FinalizacaoVendaDto(Guid VendaId, long? Numero, long Versao,
    StatusVendaOperacional Status, decimal Total, decimal Troco,
    int QuantidadePagamentos, int QuantidadeMovimentosEstoque);
