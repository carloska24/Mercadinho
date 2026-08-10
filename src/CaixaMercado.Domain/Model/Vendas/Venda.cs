using CaixaMercado.Domain.Model.Catalogo;

namespace CaixaMercado.Domain.Model.Vendas;

public sealed class Venda
{
    private readonly List<ItemVenda> _itens = new();

    private Venda()
    {
    }

    private Venda(Guid id, Guid filialId, Guid terminalId, Guid sessaoCaixaId, Guid operadorId, DateTimeOffset criadaEmUtc)
    {
        ValidarId(id, nameof(id));
        ValidarId(filialId, nameof(filialId));
        ValidarId(terminalId, nameof(terminalId));
        ValidarId(sessaoCaixaId, nameof(sessaoCaixaId));
        ValidarId(operadorId, nameof(operadorId));
        if (criadaEmUtc.Offset != TimeSpan.Zero) throw new ArgumentException("A data de criação deve estar em UTC.", nameof(criadaEmUtc));

        Id = id;
        FilialId = filialId;
        TerminalId = terminalId;
        SessaoCaixaId = sessaoCaixaId;
        OperadorId = operadorId;
        CriadaEmUtc = criadaEmUtc;
        Status = StatusVendaOperacional.Aberta;
    }

    public Guid Id { get; }
    public long? Numero { get; private set; }
    public Guid FilialId { get; }
    public Guid TerminalId { get; }
    public Guid SessaoCaixaId { get; }
    public Guid OperadorId { get; }
    public DateTimeOffset CriadaEmUtc { get; }
    public StatusVendaOperacional Status { get; private set; }
    public long Versao { get; private set; }
    public IReadOnlyList<ItemVenda> Itens => _itens.AsReadOnly();
    public decimal ValorBruto => _itens.Sum(i => i.ValorBruto);
    public decimal Subtotal => _itens.Sum(i => i.Total);
    public decimal Desconto { get; private set; }
    public decimal Total => Subtotal - Desconto;
    public decimal QuantidadeTotal => _itens.Sum(i => i.Quantidade);

    public static Venda Abrir(Guid id, Guid filialId, Guid terminalId, Guid sessaoCaixaId,
        Guid operadorId, DateTimeOffset criadaEmUtc) =>
        new(id, filialId, terminalId, sessaoCaixaId, operadorId, criadaEmUtc);

    public void AtribuirNumero(long numero)
    {
        if (numero <= 0) throw new ArgumentOutOfRangeException(nameof(numero));
        if (Numero.HasValue) throw new InvalidOperationException("O número da venda já foi atribuído.");
        Numero = numero;
        IncrementarVersao();
    }

    public ItemVenda AdicionarItem(Produto produto, decimal quantidade)
    {
        AssegurarAberta();
        ArgumentNullException.ThrowIfNull(produto);
        if (!produto.Ativo) throw new InvalidOperationException("Produto inativo não pode ser incluído na venda.");

        var snapshot = ProdutoSnapshot.Criar(produto);
        var existente = _itens.FirstOrDefault(i => i.Produto == snapshot);
        if (existente is not null)
        {
            existente.Acrescentar(quantidade);
            IncrementarVersao();
            return existente;
        }

        var proximoSequencial = _itens.Count == 0 ? 1 : checked(_itens.Max(i => i.Sequencial) + 1);
        var item = new ItemVenda(Guid.NewGuid(), proximoSequencial, snapshot, quantidade);
        _itens.Add(item);
        IncrementarVersao();
        return item;
    }

    public void RemoverItem(Guid itemId)
    {
        AssegurarAberta();
        var item = _itens.SingleOrDefault(i => i.Id == itemId) ?? throw new InvalidOperationException("Item não encontrado na venda.");
        _itens.Remove(item);
        if (Desconto > Subtotal) Desconto = Subtotal;
        IncrementarVersao();
    }

    public void AplicarDescontoNoItem(Guid itemId, decimal valor)
    {
        AssegurarAberta();
        var item = _itens.SingleOrDefault(i => i.Id == itemId) ?? throw new InvalidOperationException("Item não encontrado na venda.");
        item.AplicarDesconto(valor);
        if (Desconto > Subtotal) Desconto = Subtotal;
        IncrementarVersao();
    }

    public void AplicarDesconto(decimal valor)
    {
        AssegurarAberta();
        if (valor < 0m || valor > Subtotal)
            throw new ArgumentOutOfRangeException(nameof(valor), "O desconto deve estar entre zero e o subtotal.");
        Desconto = valor;
        IncrementarVersao();
    }

    public void IniciarPagamento()
    {
        AssegurarAberta();
        if (_itens.Count == 0) throw new InvalidOperationException("Venda vazia não pode iniciar pagamento.");
        Status = StatusVendaOperacional.AguardandoPagamento;
        IncrementarVersao();
    }

    public void Cancelar()
    {
        if (Status is not (StatusVendaOperacional.Aberta or StatusVendaOperacional.AguardandoPagamento))
            throw new InvalidOperationException("A venda não pode ser cancelada no estado atual.");
        Status = StatusVendaOperacional.Cancelada;
        IncrementarVersao();
    }

    private void AssegurarAberta()
    {
        if (Status != StatusVendaOperacional.Aberta)
            throw new InvalidOperationException("A venda somente pode ser alterada enquanto estiver aberta.");
    }

    private void IncrementarVersao() => Versao++;

    private static void ValidarId(Guid id, string nomeParametro)
    {
        if (id == Guid.Empty) throw new ArgumentException("O identificador é obrigatório.", nomeParametro);
    }
}
