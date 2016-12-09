namespace Unosquare.Swan
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    partial class Terminal
    {
        static public void Debug(string text)
        {
            $" {DateTime.Now:HH:mm:ss} DBG >> {text}".WriteLine(ConsoleColor.Gray);
        }

        static public void Trace(string text)
        {
            $" {DateTime.Now:HH:mm:ss} TRC >> {text}".WriteLine(ConsoleColor.DarkGray);
        }

        static public void Warn(string text)
        {
            $" {DateTime.Now:HH:mm:ss} WRN >> {text}".WriteLine(ConsoleColor.Yellow);
        }

        static public void Info(string text)
        {
            $" {DateTime.Now:HH:mm:ss} INF >> {text}".WriteLine(ConsoleColor.Cyan);
        }

        static public void Error(string text)
        {
            $" {DateTime.Now:HH:mm:ss} ERR >> {text}".WriteLine(ConsoleColor.Red);
        }
    }
}
