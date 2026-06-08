namespace inzynierka.IO.Pipeline;

/// <summary>
/// Synchroniczna projekcja jednej linii UTF-8 na rekord wyjściowy.
/// Łączy parsowanie + filtr (Where) + mapowanie (Select) w jedną operację
/// wykonywaną na workerze. Zwraca <c>false</c>, gdy rekord ma zostać odrzucony.
/// </summary>
internal delegate bool RecordProjection<TOut>(ReadOnlyMemory<byte> utf8Line, out TOut value);
