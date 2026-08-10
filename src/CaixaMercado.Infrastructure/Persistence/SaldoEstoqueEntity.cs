namespace CaixaMercado.Infrastructure.Persistence;

internal sealed class SaldoEstoqueEntity
{
    private SaldoEstoqueEntity()
    {
    }

    public Guid ProdutoId { get; private set; }
    public decimal Quantidade { get; private set; }
    public long Versao { get; private set; }

    public bool TentarBaixar(decimal quantidade)
    {
        if (quantidade <= 0m || quantidade > Quantidade)
            return false;

        Quantidade -= quantidade;
        Versao++;
        return true;
    }
}
