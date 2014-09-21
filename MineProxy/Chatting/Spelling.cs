using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace MineProxy.Chatting
{
    public class Spelling
    {
        public static Dictionary<string,string> Corrections = new Dictionary<string, string>();

        public static string SpellFormat(string message)
        {
            string[] parts = Regex.Split(message, @"([-<> .,;: !\?])");
            
            StringBuilder sb = new StringBuilder();
            
            //Format each word
            for (int n = 0; n < parts.Length; n++)
            {
                string w = parts [n];

                string word = w.Trim(new char[]{'*','_','/','-'});
                string key = word.ToLowerInvariant();
                if (Corrections.ContainsKey(key))
                {
                    w = w.Replace(word, Corrections [key]);
                }

                while (w.Length > 2)
                {
                    if (w.StartsWith("*") && w.EndsWith("*"))
                    {
                        w = Chat.Bold + w.Substring(1, w.Length - 2) + Chat.Reset;
                        continue;
                    }
                    if (w.StartsWith("_") && w.EndsWith("_"))
                    {
                        w = Chat.Underline + w.Substring(1, w.Length - 2) + Chat.Reset;
                        continue;
                    }
                    if (w.StartsWith("/") && w.EndsWith("/"))
                    {
                        w = Chat.Italic + w.Substring(1, w.Length - 2) + Chat.Reset;
                        continue;
                    }
                    if (w.StartsWith("-") && w.EndsWith("-"))
                    {
                        w = Chat.Strike + w.Substring(1, w.Length - 2) + Chat.Reset;
                        continue;
                    }
                    break;
                }
                sb.Append(w);
            }
            return sb.ToString();
        }
        
    }
}

