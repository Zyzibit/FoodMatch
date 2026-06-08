namespace FastPipe.Pipeline;

/// <summary>
/// Co robić, gdy parsowanie pojedynczego rekordu rzuci wyjątek.
/// </summary>
public enum ErrorPolicy
{
    /// <summary>Pomiń uszkodzony rekord i zlicz go w raporcie. Domyślne dla importów masowych.</summary>
    Skip = 0,

    /// <summary>Przerwij cały potok pierwszym napotkanym błędem.</summary>
    Throw = 1
}
