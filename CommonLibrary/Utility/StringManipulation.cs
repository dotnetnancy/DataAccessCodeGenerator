using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary.Utility
{
    public static class StringManipulation
    {
        public static string RemoveAllSpacesFromString(string input)
        {
            return input.Trim().Replace(" ", string.Empty);
        }

    }
}
