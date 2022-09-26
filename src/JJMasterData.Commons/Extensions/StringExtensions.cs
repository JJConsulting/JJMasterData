using JJMasterData.Commons.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;

namespace JJMasterData.Commons.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Remove acentos da string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string RemoveAccents(this string text)
        {
            StringBuilder sbReturn = new StringBuilder();
            var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            return sbReturn.ToString();
        }

        public static string FirstCharToUpperComplete(this string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                string valueFormat = string.Empty;
                input.Split(' ').ToList().ForEach(x => valueFormat += x.FirstCharToUpper());
 
                return valueFormat;
            }
               
            return input;
        }

    }
}
