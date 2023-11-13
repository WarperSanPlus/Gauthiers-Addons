namespace CharUtils;

/// <summary>
/// Class used to identify <see cref="char"/>
/// </summary>
public static class CharIdentification
{
    /// <summary>
    /// All vowels in lower case
    /// </summary>
    public const string Vowels = "aeiouy";

    /// <returns>Is the given letter a vowel ?</returns>
    public static bool IsVowel(this char c) => char.IsLetter(c) && Vowels.Contains(char.ToLower(c).ToString());

    /// <returns>Is the given letter a consonant ?</returns>
    public static bool IsConsonant(this char letter) => !IsVowel(letter);
}
