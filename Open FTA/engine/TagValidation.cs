/// <summary>
/// Rules for event tags. The minimal cut set analysis builds a text expression from the tags of the
/// basic events ("(A + B) * C"), so a tag must be one word and must not contain the characters of that
/// expression. The same tag in several places means the same (repeated) basic event.
/// </summary>
public static class TagValidation
{
    /// <summary>Characters used as operators and brackets in the minimal cut set expression.</summary>
    public const string ReservedCharacters = "+*()";

    /// <summary>Why the tag cannot be used, or null when it is valid.</summary>
    public static string? GetError(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return "TAG must not be empty.";
        if (tag.Any(char.IsWhiteSpace))
            return "TAG must be a single word; no spaces allowed.";
        if (tag.Any(c => ReservedCharacters.Contains(c)))
            return "TAG must not contain the characters + * ( ).";
        return null;
    }

    /// <summary>Events whose tag is used in the minimal cut set expression (events without children) and is not valid.</summary>
    public static List<FTAitem> FindInvalidBasicEventTags(IEnumerable<FTAitem> events) =>
        events.Where(e => e.Children.Count == 0 && GetError(e.Tag) != null).ToList();
}
