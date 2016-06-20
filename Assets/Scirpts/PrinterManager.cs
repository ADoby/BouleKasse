using POSPrinter;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class PrinterManager : MonoBehaviour
{
    public static char ESC = (char)27;
    public static char CR = (char)13;
    public static string LF = Convert.ToString(Convert.ToChar(10));
    public static string Cut = ESC + "m";
    public static string PrintAndFeedPaper = ESC + "J";
    public static string LFAfter = LF + LF + LF + LF + LF;

    public void PrintSomething(string text)
    {
        text = text.Replace("#FEED", LFAfter);
        text = text.Replace("#CUT", Cut);

        Printer print = new Printer();
        print.PrintReceipt(text);
    }
}