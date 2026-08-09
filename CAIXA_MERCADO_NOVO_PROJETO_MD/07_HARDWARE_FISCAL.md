# HARDWARE E FISCAL

## Scanner
Primeira abordagem: scanner USB como teclado. Criar abstração para evolução.

## Balança
Preparar `IScaleService` para conexão, leitura, desconexão e erros.

## Impressora
Preparar `IPrinterService`:
- ImprimirCupom;
- ImprimirComprovante;
- ImprimirFechamento;
- ImprimirSegundaVia.

## Gaveta
Preparar `ICashDrawerService`.

## TEF
Preparar `IPaymentTerminal`. Não criar TEF fake apresentado como real.

## PIX
Preparar fluxo:
```text
Criar cobrança → aguardar → consultar → confirmar
```

## NFC-e
Preparar `IFiscalService`:
Emitir, Consultar, Cancelar, Contingencia e Imprimir.

## Offline
Arquitetura futura:
```text
PDV → fila local → operação permitida → reconectar → sincronizar
```

Não implementar offline completo na primeira fase.

## Regra
Não inventar drivers, APIs ou integrações reais.
