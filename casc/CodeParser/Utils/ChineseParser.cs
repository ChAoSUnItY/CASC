using System.Collections.Generic;
using System;

namespace CASC.CodeParser.Utils
{
    static class ChineseParser
    {
        private static readonly Dictionary<char, int> zh2digit_table = new Dictionary<char, int>
        {
            {'零', 0}, {'一', 1}, {'\u4e8c', 2}, {'三', 3}, {'四', 4}, {'五', 5}, {'六', 6}, {'七', 7}, {'八', 8}, {'九', 9}, {'十', 10},
            {'o', 0}, {'壹', 1}, {'貳', 2}, {'參', 3}, {'叁', 3}, {'肆', 4}, {'伍', 5}, {'陸', 6}, {'陆', 6}, {'柒', 7}, {'捌', 8}, {'玖', 9}, {'拾', 10},
            {'百', 100}, {'千', 1000}, {'萬', 10000}, {'億', 100000000}, {'佰', 100}, {'仟', 1000}
        };

        public static bool isDigit(char character)
        {
            if (char.IsDigit(character))
                return true;

            return zh2digit_table.ContainsKey(character);
        }

        public static bool tryParseDigits(string str, out int value)
        {
            if (int.TryParse(str, out value))
                return true;

            var result = ParseDigitsFromChinese(str);
            value = result.value;

            return result.pass;
        }

        public static (bool pass, int value) ParseDigitsFromChinese(string str)
        {
            var digitNum = 0;
            var result = 0;
            var tmp = 0;
            var billion = 0;

            while (digitNum < str.Length)
            {
                var tmpZH = str[digitNum];
                if (!zh2digit_table.ContainsKey(tmpZH))
                    throw new Exception($"ERROR: String \"{str}\" is not valid chinese numeral.");

                var tmpNum = zh2digit_table[tmpZH];

                if (tmpNum == 100000000)
                {
                    result += tmp;
                    result *= (int)tmpNum;
                    billion *= 100000000;
                    billion += result;
                    result = 0;
                    tmp = 0;
                }
                else if (tmpNum == 10000)
                {
                    result += tmp;
                    result *= (int)tmpNum;
                    tmp = 0;
                }
                else if (tmpNum >= 10)
                {
                    if (tmp == 0)
                        tmp = 1;

                    result += (int)tmpNum * tmp;
                    tmp = 0;
                }
                else
                {
                    tmp *= 10;
                    tmp += tmpNum;
                }


                digitNum += 1;
            }

            result += tmp;
            result += billion;

            return (true, result);
        }
    }
}