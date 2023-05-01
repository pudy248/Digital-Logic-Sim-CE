﻿using System;
using System.Text.RegularExpressions;

namespace Plugin.VitoBarra.TextVerifier
{
    public static class TextVerifier
    {
        public static string ExtractFloat(string s)
        {
            return Regex.Match(s, @"\d+([\.,]\d+)?").Value;
        }
        
        public static string ExtractInt(string s)
        {
            return Regex.Match(s, @"\d+?").Value;
        }
        
    }
}