namespace VetMed.App.Services;

public static class PetVisuals
{
    public static string Emoji(string? species)
    {
        var s = (species ?? string.Empty).ToLowerInvariant();
        return s switch
        {
            _ when s.Contains("pies") || s.Contains("pso")               => "🐶",
            _ when s.Contains("kot")                                      => "🐱",
            _ when s.Contains("król") || s.Contains("krol")              => "🐰",
            _ when s.Contains("ptak") || s.Contains("papug") || s.Contains("kanar") => "🐦",
            _ when s.Contains("chom") || s.Contains("mysz") || s.Contains("szczur") => "🐹",
            _ when s.Contains("świn") || s.Contains("swin")              => "🐹",
            _ when s.Contains("fretk")                                    => "🦦",
            _ when s.Contains("żół") || s.Contains("zol") || s.Contains("gad") || s.Contains("wąż") || s.Contains("waz") => "🦎",
            _ when s.Contains("ryb")                                      => "🐠",
            _ when s.Contains("koń") || s.Contains("kon")               => "🐴",
            _                                                            => "🐾"
        };
    }
}
