using CaixaMercado.Application.Interfaces;
using CaixaMercado.Domain.Entities;
using CaixaMercado.Domain.Enums;

namespace CaixaMercado.Application.Services;

public class VendaService : IVendaService
{
    private readonly List<Produto> _produtos;
    private Venda _vendaAtual;
    private Caixa _caixaAtual;
    private long _numeroVendaSequencial = 1001;

    public VendaService()
    {
        _produtos = InicializarCatalogoMock();
        _caixaAtual = new Caixa();
        _vendaAtual = CriarNovaVendaInstancia();
    }

    public Venda ObterVendaAtual() => _vendaAtual;
    public Caixa ObterCaixaAtual() => _caixaAtual;
    public IReadOnlyList<Produto> ObterCatalogoProdutos() => _produtos.AsReadOnly();

    public Produto? BuscarProdutoPorEanOuCodigo(string eanOuCodigo)
    {
        if (string.IsNullOrWhiteSpace(eanOuCodigo)) return null;

        var termo = eanOuCodigo.Trim().ToLowerInvariant();
        return _produtos.FirstOrDefault(p =>
            p.Ativo && (
                p.EAN.Equals(termo, StringComparison.OrdinalIgnoreCase) ||
                p.Codigo.Equals(termo, StringComparison.OrdinalIgnoreCase) ||
                p.PLU.Equals(termo, StringComparison.OrdinalIgnoreCase)
            )
        );
    }

    public IReadOnlyList<Produto> PesquisarProdutos(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return _produtos.Where(p => p.Ativo).ToList().AsReadOnly();

        var query = termo.Trim().ToLowerInvariant();
        return _produtos.Where(p =>
            p.Ativo && (
                p.Descricao.ToLowerInvariant().Contains(query) ||
                p.Codigo.ToLowerInvariant().Contains(query) ||
                p.EAN.ToLowerInvariant().Contains(query) ||
                p.PLU.ToLowerInvariant().Contains(query) ||
                p.Categoria.ToLowerInvariant().Contains(query)
            )
        ).ToList().AsReadOnly();
    }

    public ItemVenda? AdicionarItem(string eanOuCodigo, decimal quantidade = 1m, decimal descontoItem = 0m)
    {
        if (_vendaAtual.Status != StatusVenda.EmAberto)
        {
            throw new InvalidOperationException("Não é possível adicionar itens a uma venda que não está em aberto.");
        }

        var produto = BuscarProdutoPorEanOuCodigo(eanOuCodigo);
        if (produto == null)
        {
            return null;
        }

        if (quantidade <= 0) quantidade = 1m;

        // CONSOLIDAÇÃO DE PRODUTO REPETIDO (PDV_MELHORIAS_LAYOUT_E_FLUXO.md - Seção 6)
        var itemExistente = _vendaAtual.Itens.FirstOrDefault(i => i.ProdutoId == produto.Id);
        if (itemExistente != null)
        {
            itemExistente.Quantidade += quantidade;
            itemExistente.Desconto += descontoItem;
            return itemExistente;
        }

        var sequencial = _vendaAtual.Itens.Count + 1;
        var item = new ItemVenda
        {
            Sequencial = sequencial,
            ProdutoId = produto.Id,
            CodigoProduto = produto.Codigo,
            EAN = produto.EAN,
            DescricaoProduto = produto.Descricao,
            Quantidade = quantidade,
            Unidade = produto.Unidade,
            PrecoUnitario = produto.PrecoVenda,
            Desconto = descontoItem
        };

        _vendaAtual.Itens.Add(item);
        ReordenarSequencial();
        return item;
    }

    public bool RemoverItem(int sequencial)
    {
        var item = _vendaAtual.Itens.FirstOrDefault(i => i.Sequencial == sequencial);
        if (item == null) return false;

        _vendaAtual.Itens.Remove(item);
        ReordenarSequencial();
        return true;
    }

    public bool AplicarDescontoItem(int sequencial, decimal valorDesconto)
    {
        var item = _vendaAtual.Itens.FirstOrDefault(i => i.Sequencial == sequencial);
        if (item == null) return false;

        item.Desconto = Math.Max(0m, valorDesconto);
        return true;
    }

    public bool AplicarDescontoVenda(decimal valorDesconto)
    {
        if (valorDesconto < 0) return false;
        _vendaAtual.Desconto = Math.Min(_vendaAtual.Subtotal, valorDesconto);
        return true;
    }

    public bool AplicarDescontoPercentualVenda(decimal percentual)
    {
        if (percentual < 0 || percentual > 100) return false;
        var valorCalculado = Math.Round(_vendaAtual.Subtotal * (percentual / 100m), 2);
        return AplicarDescontoVenda(valorCalculado);
    }

    public bool FinalizarVenda(TipoPagamento tipoPagamento, decimal valorPago, out decimal troco, out string mensagemErro)
    {
        troco = 0m;
        mensagemErro = string.Empty;

        if (_vendaAtual.Itens.Count == 0)
        {
            mensagemErro = "A venda não contém itens.";
            return false;
        }

        var total = _vendaAtual.Total;
        if (tipoPagamento == TipoPagamento.Dinheiro)
        {
            if (valorPago < total)
            {
                mensagemErro = $"Valor pago (R$ {valorPago:N2}) é menor que o total da venda (R$ {total:N2}).";
                return false;
            }
            troco = valorPago - total;
        }

        _vendaAtual.Status = StatusVenda.Finalizada;
        return true;
    }

    public void NovaVenda()
    {
        _vendaAtual = CriarNovaVendaInstancia();
    }

    public bool CancelarVenda()
    {
        if (_vendaAtual.Status == StatusVenda.Finalizada) return false;

        _vendaAtual.Status = StatusVenda.Cancelada;
        _vendaAtual.Itens.Clear();
        return true;
    }

    private void ReordenarSequencial()
    {
        for (int i = 0; i < _vendaAtual.Itens.Count; i++)
        {
            _vendaAtual.Itens[i].Sequencial = i + 1;
        }
    }

    private Venda CriarNovaVendaInstancia()
    {
        return new Venda
        {
            Numero = _numeroVendaSequencial++,
            DataHora = DateTime.Now,
            Filial = _caixaAtual.Filial,
            PDV = _caixaAtual.PDV,
            Operador = _caixaAtual.Operador,
            Status = StatusVenda.EmAberto
        };
    }

    private static List<Produto> InicializarCatalogoMock()
    {
        return new List<Produto>
        {
            new() { Codigo = "001", EAN = "7891234567890", PLU = "101", Descricao = "ARROZ TIPO 1 TIO JOÃO 5KG", Categoria = "Mercearia", Unidade = "UN", PrecoCusto = 18.50m, PrecoVenda = 24.90m, Estoque = 120m },
            new() { Codigo = "002", EAN = "7891234567891", PLU = "102", Descricao = "FEIJÃO CARIOCA CAMIL 1KG", Categoria = "Mercearia", Unidade = "UN", PrecoCusto = 5.20m, PrecoVenda = 7.80m, Estoque = 200m },
            new() { Codigo = "003", EAN = "7891234567892", PLU = "103", Descricao = "LEITE INTEGRAL NINHO 1L", Categoria = "Laticínios", Unidade = "UN", PrecoCusto = 3.90m, PrecoVenda = 5.49m, Estoque = 300m },
            new() { Codigo = "004", EAN = "7891234567893", PLU = "104", Descricao = "CAFÉ TORRADO E MOÍDO PILÃO 500G", Categoria = "Mercearia", Unidade = "UN", PrecoCusto = 12.00m, PrecoVenda = 16.90m, Estoque = 85m },
            new() { Codigo = "005", EAN = "7891234567894", PLU = "105", Descricao = "AÇÚCAR REFINADO UNIÃO 1KG", Categoria = "Mercearia", Unidade = "UN", PrecoCusto = 3.20m, PrecoVenda = 4.59m, Estoque = 150m },
            new() { Codigo = "006", EAN = "7891234567895", PLU = "106", Descricao = "ÓLEO DE SOJA LIZA 900ML", Categoria = "Mercearia", Unidade = "UN", PrecoCusto = 5.80m, PrecoVenda = 7.49m, Estoque = 180m },
            new() { Codigo = "007", EAN = "7891234567896", PLU = "107", Descricao = "SABÃO EM PÓ OMO MULTIAÇÃO 800G", Categoria = "Limpeza", Unidade = "UN", PrecoCusto = 11.50m, PrecoVenda = 15.90m, Estoque = 90m },
            new() { Codigo = "008", EAN = "7891234567897", PLU = "108", Descricao = "REFRIGERANTE GUARANÁ ANTARCTICA 2L", Categoria = "Bebidas", Unidade = "UN", PrecoCusto = 6.00m, PrecoVenda = 8.99m, Estoque = 250m },
            new() { Codigo = "009", EAN = "2000000000099", PLU = "001", Descricao = "BANANA PRATA (PESÁVEL)", Categoria = "Hortifruti", Unidade = "KG", PrecoCusto = 3.50m, PrecoVenda = 6.99m, Estoque = 45m, ProdutoPesavel = true },
            new() { Codigo = "010", EAN = "2000000000100", PLU = "002", Descricao = "MAÇÃ FUJI (PESÁVEL)", Categoria = "Hortifruti", Unidade = "KG", PrecoCusto = 5.00m, PrecoVenda = 9.90m, Estoque = 30m, ProdutoPesavel = true }
        };
    }
}
