﻿using System.Globalization;

namespace IronAHK.Scripting
{
    partial class Parser
    {
        #region Identifiers

        public static bool IsIdentifier(char symbol)
        {
            if (symbol == Concatenate)
                return false;
            return char.IsLetterOrDigit(symbol) || VarExt.IndexOf(symbol) != -1;
        }

        bool IsIdentifier(string token)
        {
            return IsIdentifier(token, false);
        }

        bool IsIdentifier(string token, bool dynamic)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            if (token.Length == 1)
            {
                switch (token[0])
                {
                    case TernaryA:
                        return false;
                }
            }

            foreach (char sym in token)
            {
                if (!IsIdentifier(sym))
                {
                    if (dynamic && sym == Resolve)
                        continue;
                    return false;
                }
            }

            return true;
        }

        bool IsDynamicReference(string code)
        {
            bool d = false;

            for (int i = 0; i < code.Length; i++)
            {
                char sym = code[i];

                if (sym == Resolve)
                {
                    if (d)
                        if (code[i - 1] == Resolve)
                            return false;
                    d = !d;
                }
                else if (!IsIdentifier(sym))
                    return false;
            }

            return code.Length != 0;
        }

        bool IsVariable(string code)
        {
            return IsIdentifier(code, true) && !IsKeyword(code);
        }

        #endregion

        #region Primatives

        bool IsPrimativeObject(string code, out object result)
        {
            result = null;

            if (string.IsNullOrEmpty(code))
                return true;

            // Mono incorrectly determines "." as a numeric value
            if (code.Length == 1 && code[0] == Concatenate)
                return false;

            string codeTrim = code.Trim(Spaces);
            var info = CultureInfo.CreateSpecificCulture("en-GB");

            decimal d;
            if (decimal.TryParse(codeTrim, NumberStyles.Any, info, out d))
            {
                result = d;
                return true;
            }

            int i;
            const string hex = "0x";
            int z = codeTrim.IndexOf(hex);
            bool negative = false;
            if (z == 1 && codeTrim[0] == Minus)
            {
                negative = true;
                codeTrim = codeTrim.Substring(1);
            }
            if ((z == 0 || negative) && int.TryParse(codeTrim.Replace(hex, string.Empty), NumberStyles.HexNumber, info, out i))
            {
                result = (decimal)(negative ? -i : i);
                return true;
            }

            result = null;
            return false;
        }

        bool IsPrimativeObject(string code)
        {
            object result;
            return IsPrimativeObject(code, out result);
        }

        #endregion

        #region Expressions

        bool IsExpressionParameter(string code)
        {
            code = code.TrimStart(Spaces);
            int z = code.IndexOf(Resolve);
            return z == 0 && (code.Length == 1 || IsSpace(code[1]));
        }

        bool IsExpression(string code)
        {
            char sym = code[0];

            if (sym == ParenOpen)
                return true;

            if (!IsIdentifier(sym))
                return true;

            string word = code.Split(Spaces, 2)[0];
            switch (word.ToLowerInvariant())
            {
                case AndTxt:
                case OrTxt:
                case NotTxt:
                    return true;
            }

            foreach (char token in code)
            {
                if (token == ParenOpen)
                    return true;
                else if (!IsIdentifier(token) && token != Resolve)
                    return false;
            }

            return false;
        }

        #endregion

        #region Misc

        bool IsKeyword(string code)
        {
            switch (code.ToLowerInvariant())
            {
                case NotTxt:
                case OrTxt:
                case AndTxt:
                    return true;

                default:
                    return false;
            }
        }

        bool IsRemap(string code)
        {
            code = code.Trim(Spaces);

            if (code.Length == 0)
                return false;

            if (IsSpace(code[0]))
                return false;

            for (int i = 1; i < code.Length; i++)
            {
                if (IsCommentAt(code, i))
                    return true;
                else if (!IsSpace(code[i]))
                    return false;
            }

            return true;
        }

        #endregion
    }
}
