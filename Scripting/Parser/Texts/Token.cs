﻿using System;

namespace IronAHK.Scripting
{
    partial class Parser
    {
        enum Token { Unknown, Assign, Command, Label, Hotkey, Flow, Expression, Directive }

        Token GetToken(string code)
        {
            code = code.TrimStart(Spaces);

            if (code.Length == 0)
                return Token.Unknown;

            if (code[0] == Directive)
                return Token.Directive;
            if (IsFlowOperator(code))
                return Token.Flow;
            else if (IsLabel(code))
                return Token.Label;
            else if (IsHotkeyLabel(code) || IsHotstringLabel(code))
                return Token.Hotkey;
            else if (IsAssignment(code))
                return Token.Assign;
            else if (IsCommand(code))
                return Token.Command;
            else
                return Token.Expression;
        }

        bool IsSpace(char sym)
        {
            return Array.IndexOf<char>(Spaces, sym) != -1;
        }

        bool IsSpace(string code)
        {
            foreach (char sym in code)
                if (!IsSpace(sym))
                    return false;
            return true;
        }

        bool IsFlowOperator(string code)
        {
            char[] delimiters = new char[Spaces.Length + 1];
            delimiters[0] = Multicast;
            Spaces.CopyTo(delimiters, 1);
            string word = code.Split(delimiters, 2)[0].ToLowerInvariant();

            switch (word)
            {
                case FlowBreak:
                case FlowContinue:
                case FlowElse:
                case FlowGosub:
                case FlowGoto:
                case FlowIf:
                case FlowLoop:
                case FlowReturn:
                case FlowWhile:
                case FunctionLocal:
                case FunctionGlobal:
                case FunctionStatic:
                    return true;
            }

            return false;
        }

        bool IsLabel(string code)
        {
            for (int i = 0; i < code.Length; i++)
            {
                char sym = code[i];
                if (!IsIdentifier(sym))
                {
                    if (sym == HotkeyBound)
                    {
                        if (i == 0)
                            return false;
                        else if (i == code.Length - 1)
                            return true;
                        else
                        {
                            string sub = StripCommentSingle(code.Substring(i));
                            return sub.Length == 0 || IsSpace(sub);
                        }
                    }
                    else
                        return false;
                }
            }

            return false;
        }

        bool IsHotkeyLabel(string code)
        {
            // TODO: hotkey tokens

            if (!code.Contains(HotkeySignal))
                return false;

            return true;
        }

        bool IsHotstringLabel(string code)
        {
            if (!code.Contains(HotkeySignal))
                return false;

            // TODO: hotstring tokens

            return code.Length > 0 && code[0] == HotkeyBound;
        }

        bool IsAssignment(string code)
        {
            int i = 0;

            while (i < code.Length && (IsIdentifier(code[i]) || code[i] == Resolve)) i++;

            if (i == 0 || i == code.Length)
                return false;

            while (IsSpace(code[i])) i++;

            return i < code.Length && code[i] == Equal;
        }

        bool IsCommand(string code)
        {
            int i = 0;

            while (i < code.Length && IsIdentifier(code[i])) i++;

            if (i == 0 || i == code.Length)
                return false;
            else if (code[i] == Multicast)
                return true;
            else if (IsSpace(code[i]))
            {
                i++;
                while (i < code.Length && IsSpace(code[i])) i++;

                int n = i + 1;
                if (n < code.Length && code[n] == Equal)
                    return false;

                return true;
            }
            else
                return false;
        }
    }
}