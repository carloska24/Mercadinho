namespace CaixaMercado.Domain.Model.Vendas;

public enum StatusVendaOperacional
{
    Aberta = 1,
    AguardandoPagamento = 2,
    Paga = 3,
    FiscalPendente = 4,
    Finalizada = 5,
    Cancelada = 6,
    Falhou = 7,
    Revisao = 8
}
