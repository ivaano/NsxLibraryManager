using System.Globalization;
using System.Text.RegularExpressions;

namespace NsxLibraryManager.Utils;

public static class LanguageChecker
{
    public static bool IsNonEnglish(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        //if (AsciiCheckRegex().IsMatch(text))
        if (Regex.IsMatch(text, @"[^\x00-\x7F]")) 
        {
            return true;
        }

        foreach (var c in text)
        {
            var category = char.GetUnicodeCategory(c);
            if (category is UnicodeCategory.OtherLetter or 
                UnicodeCategory.OtherNumber or 
                UnicodeCategory.OtherPunctuation or 
                UnicodeCategory.OtherSymbol or 
                UnicodeCategory.ModifierLetter or 
                UnicodeCategory.ModifierSymbol or 
                UnicodeCategory.NonSpacingMark or 
                UnicodeCategory.SpacingCombiningMark)
            {
                return true; 
            }

            // Check for specific ranges of non-English characters.
            if (c is >= '\u3040' and <= '\u30FF' || // Japanese/Chinese/Korean
                c is >= '\uAC00' and <= '\uD7A3') // Korean Hangul
            {
                return true;
            }
        }

        return false;
    }
/*
    [GeneratedRegex(@"[^\x00-\x7F]")]
    private static partial Regex AsciiCheckRegex();
    */
}