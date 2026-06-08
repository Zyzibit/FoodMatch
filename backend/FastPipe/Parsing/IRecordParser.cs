namespace FastPipe.Parsing;

public interface IRecordParser<T>
{
    /// <summary>
    /// Próbuje sparsować linię. Zwraca <c>false</c>, gdy rekord należy odrzucić
    /// (np. pusty/niepełny). Wyjątek oznacza błąd parsowania i jest obsługiwany
    /// zgodnie z polityką błędów potoku.
    /// </summary>
    bool TryParse(ReadOnlySpan<byte> utf8Line, out T value);
}
