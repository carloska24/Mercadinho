using CaixaMercado.Domain.Enums;

namespace CaixaMercado.Domain.Entities;

public class Caixa
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Filial { get; set; } = "FILIAL 01";
    public string PDV { get; set; } = "PDV 01";
    public string Operador { get; set; } = "CARLOS EDUARDO";
    public DateTime DataAbertura { get; set; } = DateTime.Now;
    public decimal ValorInicial { get; set; } = 150.00m;
    public DateTime? DataFechamento { get; set; }
    public decimal? ValorFinal { get; set; }
    public StatusCaixa Status { get; set; } = StatusCaixa.Aberto;
}
