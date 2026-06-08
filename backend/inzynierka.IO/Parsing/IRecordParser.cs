namespace inzynierka.IO.Parsing;

/// <summary>
/// Parsuje pojedynczy rekord (jedną linię UTF-8) na obiekt domenowy
/// bezpośrednio z bajtów — bez materializowania pośredniego <see cref="string"/>.
///
/// Implementacja musi być bezstanowa i thread-safe: silnik potoku wywołuje
/// <see cref="TryParse"/> równolegle z wielu workerów.
/// </summary>
/// <typeparam name="T">Typ docelowy rekordu.</typeparam>
public interface IRecordParser<T>
{
    /// <summary>
    /// Próbuje sparsować linię. Zwraca <c>false</c>, gdy rekord należy odrzucić
    /// (np. pusty/niepełny). Wyjątek oznacza błąd parsowania i jest obsługiwany
    /// zgodnie z polityką błędów potoku.
    /// </summary>
    bool TryParse(ReadOnlySpan<byte> utf8Line, out T value);
}
