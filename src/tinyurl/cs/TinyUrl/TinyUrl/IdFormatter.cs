using Sqids;

namespace TinyUrl;

public static class IdFormatter
{
    public static SqidsEncoder<int> Encoder { get; } = new SqidsEncoder<int>(new SqidsOptions()
    {
        Alphabet = "k3G7QAe5F1CsPW92uEOyq4Bg6Sp8YzVTmnU0liDwdHXLajZrfxNhobJIRcMvKt"
    });
}