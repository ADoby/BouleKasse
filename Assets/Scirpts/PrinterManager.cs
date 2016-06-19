using Microsoft.PointOfService;
using System;
using UnityEngine;

public class PrinterManager : MonoBehaviour
{
    public static char ESC = (char)27;
    public static char CR = (char)13;
    public static string LF = Convert.ToString(Convert.ToChar(10));
    public static string Cut = ESC + "m";
    public static string PrintAndFeedPaper = ESC + "J";
    public static string LFAfter = LF + LF + LF + LF + LF;

    public void Print(string text)
    {
        text = text.Replace("#FEED", LFAfter);
        text = text.Replace("#CUT", Cut);

        Debug.Log("Try Print:" + text);

        PosPrinter printer = GetReceiptPrinter();

        try
        {
            ConnectToPrinter(printer);
            printer.PrintImmediate(PrinterStation.Receipt, text);
        }
        finally
        {
            DisconnectFromPrinter(printer);
        }
    }

    private void DisconnectFromPrinter(PosPrinter printer)
    {
        printer.Release();
        printer.Close();
    }

    private void ConnectToPrinter(PosPrinter printer)
    {
        try
        {
            printer.Open();
            printer.Claim(10000);
            printer.DeviceEnabled = true;
        }
        catch (Exception ex)
        {
            throw new Exception("Exception", ex);
        }
    }

    private static PosPrinter GetReceiptPrinter()
    {
        try
        {
            PosExplorer posExplorer = new PosExplorer();
            DeviceInfo receiptPrinterDevice = posExplorer.GetDevice("PosPrinter", "PosPrinter");
            return (PosPrinter)posExplorer.CreateInstance(receiptPrinterDevice);
        }
        catch (Exception ex)
        {
            throw new Exception("Exception", ex);
        }
    }

    private void PrintReceiptFooter(PosPrinter printer, int subTotal, double tax, double discount, string footerText)
    {
        string offSetString = new string(' ', printer.RecLineChars / 2);

        PrintTextLine(printer, new string('-', (printer.RecLineChars / 3) * 2));
        PrintTextLine(printer, offSetString + String.Format("SUB-TOTAL     {0}", subTotal.ToString("#0.00")));
        PrintTextLine(printer, offSetString + String.Format("TAX           {0}", tax.ToString("#0.00")));
        PrintTextLine(printer, offSetString + String.Format("DISCOUNT      {0}", discount.ToString("#0.00")));
        PrintTextLine(printer, offSetString + new string('-', (printer.RecLineChars / 3)));
        PrintTextLine(printer, offSetString + String.Format("TOTAL         {0}", (subTotal - (tax + discount)).ToString("#0.00")));
        PrintTextLine(printer, offSetString + new string('-', (printer.RecLineChars / 3)));
        PrintTextLine(printer, String.Empty);

        //Embed 'center' alignment tag on front of string below to have it printed in the center of the receipt.
        //PrintTextLine(printer, System.Text.ASCIIEncoding.ASCII.GetString(New Byte() {27, CByte("|"), CByte("c"), CByte("A")}) & footerText)

        //Added in these blank lines because RecLinesToCut seems to be wrong on my printer and
        //these extra blank lines ensure the cut is after the footer ends.
        PrintTextLine(printer, String.Empty);
        PrintTextLine(printer, String.Empty);
        PrintTextLine(printer, String.Empty);
        PrintTextLine(printer, String.Empty);
        PrintTextLine(printer, String.Empty);

        printer.CutPaper(70);
        //Print 'advance and cut' escape command.
        //PrintTextLine(printer, System.Text.ASCIIEncoding.ASCII.GetString(New Byte() {27, CByte("|"), CByte("1"), CByte("0"), CByte("0"), CByte("P"),CByte("f"), CByte("P")}))
    }

    private void PrintLineItem(PosPrinter printer, string itemCode, int quantity, double unitPrice)
    {
        PrintText(printer, TruncateAt(itemCode.PadRight(9), 9));
        PrintText(printer, TruncateAt(quantity.ToString("#0.00").PadLeft(9), 9));
        PrintText(printer, TruncateAt(unitPrice.ToString("#0.00").PadLeft(10), 10));
        PrintTextLine(printer, TruncateAt((quantity * unitPrice).ToString("#0.00").PadLeft(10), 10));
    }

    private void PrintReceiptHeader(PosPrinter printer, string companyName, string addressLine1, string addressLine2, string taxNumber, DateTime dateTime, string cashierName)
    {
        PrintTextLine(printer, companyName);
        PrintTextLine(printer, addressLine1);
        PrintTextLine(printer, addressLine2);
        PrintTextLine(printer, taxNumber);
        PrintTextLine(printer, new string('-', printer.RecLineChars / 2));
        PrintTextLine(printer, String.Format("DATE : {0}", dateTime.ToShortDateString()));
        PrintTextLine(printer, String.Format("CASHIER : {0}", cashierName));
        PrintTextLine(printer, String.Empty);
        PrintText(printer, "item      ");
        PrintText(printer, "qty       ");
        PrintText(printer, "Unit Price ");
        PrintTextLine(printer, "Total      ");
        PrintTextLine(printer, new string('=', printer.RecLineChars));
        PrintTextLine(printer, String.Empty);
    }

    private void PrintText(PosPrinter printer, string text)
    {
        if (text.Length <= printer.RecLineChars)
        {
            printer.PrintNormal(PrinterStation.Receipt, text);
            //Print text
        }
        else if (text.Length > printer.RecLineChars)
        {
            printer.PrintNormal(PrinterStation.Receipt, TruncateAt(text, printer.RecLineChars));
        }
        //Print exactly as many characters as the printer allows, truncating the rest.
    }

    private void PrintTextLine(PosPrinter printer, string text)
    {
        if (text.Length < printer.RecLineChars)
        {
            printer.PrintNormal(PrinterStation.Receipt, text + Environment.NewLine);
            //Print text, then a new line character.
        }
        else if (text.Length > printer.RecLineChars)
        {
            printer.PrintNormal(PrinterStation.Receipt, TruncateAt(text, printer.RecLineChars));
            //Print exactly as many characters as the printer allows, truncating the rest, no new line character (printer will probably auto-feed for us)
        }
        else if (text.Length == printer.RecLineChars)
        {
            printer.PrintNormal(PrinterStation.Receipt, text + Environment.NewLine);
        }
        //Print text, no new line character, printer will probably auto-feed for us.
    }

    private string TruncateAt(string text, int maxWidth)
    {
        string retVal = text;
        if (text.Length > maxWidth)
        {
            retVal = text.Substring(0, maxWidth);
        }

        return retVal;
    }
}