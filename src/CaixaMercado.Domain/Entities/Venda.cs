using CaixaMercado.Domain.Enums;

namespace CaixaMercado.Domain.Entities;

public class Venda
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public long Numero { get; set; }
    public DateTime DataHora { get; set; } = DateTime.Now;
    public string Filial { get; set; } = "FILIAL 01";
    public string PDV { get; set; } = "PDV 01";
    public string Operador { get; set; } = "CARLOS EDUARDO";
    public List<ItemVenda> Itens { get; set; } = new();
    public decimal Desconto { get; set; }
    public decimal Acrescimo { get; set; }
    public StatusVenda Status { get; set; } = StatusVenda.EmAberto;

    public decimal Subtotal => Itens.Sum(i => i.Total);
    public decimal Total => Math.Max(0m, Subtotal - Desconto + Acrescimo);
    public int TotalItens => Itens.Count;
    public decimal TotalQuantidade => Itens.Sum(i => i.Quantidade);
}
