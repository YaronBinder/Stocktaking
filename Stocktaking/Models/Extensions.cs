using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stocktaking.Models;

public static class Extensions
{
    /// <summary>
    /// Check if a given <see cref="string"/> is <see langword="null"/> or <see cref="string.Empty"/> or whitespace
    /// </summary>
    /// <param name="input">The specified string</param>
    /// <returns><see langword="true"/> if the given string is <see langword="null"/> or <see cref="string.Empty"/> or whitespace; otherwise <see langword="false"/></returns>
    public static bool IsNull(this string input)
        => string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input);
}
