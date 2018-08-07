using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeNetDumper
{
    internal static class SignatureDecoder
    {
        private static readonly StringBuilder sb = new StringBuilder();

        public static string Decode(IReadOnlyList<UnscramblingInstruction> instructions, string input)
        {
            for (int j = 0; j < instructions.Count; j++)
            {
                var index = instructions[j].Index;
                switch (instructions[j].Name)
                {
                    case "reverse":
                        input = Reverse(input);
                        break;

                    case "slice":
                        input = Slice(input, index);
                        break;

                    case "swap":
                        input = Swap(input, index);
                        //sb.Clear();
                        break;

                    default: break;
                }
            }

            return input;
        }

        public static string Reverse(string input)
        {
            var tmp = input.ToCharArray();
            Array.Reverse(tmp);
            return new string(tmp);
        }

        public static string Slice(string input, int index)
        {
            return input.Substring(index);
        }

        public static string Swap(string input, int index)
        {
            sb.Clear();
            sb.Append(input);
            sb[0] = input[index];
            sb[index] = input[0];
            return sb.ToString();
        }
    }
}