using CaixaMercado.Domain.Entities;
using CaixaMercado.Domain.Enums;

namespace CaixaMercado.Application.Interfaces;

public interface IVendaService
{
    Venda ObterVendaAtual();
    Caixa ObterCaixaAtual();
    IReadOnlyList<Produto> ObterCatalogoProdutos();
    IReadOnlyList<Produto> PesquisarProdutos(string termo);
    Produto? BuscarProdutoPorEanOuCodigo(string eanOuCodigo);
    ItemVenda? AdicionarItem(string eanOuCodigo, decimal quantidade = 1m, decimal descontoItem = 0m);
    bool RemoverItem(int sequencial);
    bool AplicarDescontoItem(int sequencial, decimal valorDesconto);
    bool AplicarDescontoVenda(decimal valorDesconto);
    bool AplicarDescontoPercentualVenda(decimal percentual);
    bool FinalizarVenda(TipoPagamento tipoPagamento, decimal valorPago, out decimal troco, out string mensagemErro);
    void NovaVenda();
    bool CancelarVenda();
}
