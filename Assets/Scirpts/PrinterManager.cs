using System.Diagnostics;
using UnityEngine;

public class PrinterManager : MonoBehaviour
{
    public string FilePath = "Print.txt";

    public void PrintOrder(AppData.Order order)
    {
        string text;
        text = "ESC@ESC!0Herzlich Willkommen\n";
        text += "ESCa1";
        foreach (var item in order.Data)
        {
            for (int i = 0; i < item.Value; i++)
            {
                text += "\n";
                text += "ESC!1Boulefreunde Gernsbach\n";
                text += "Boulefest 2016\n";
                text += "ESC!0\n";
                text += string.Format("ESCm{0}\n", item.Key);
                text += "LF";
            }
        }
        text += "LFLFLF\n";
        text += "ESCm";
        Print(text);
    }

    public void Print(string text)
    {
        if (Application.isEditor)
        {
            return;
        }
        if (!System.IO.File.Exists("POSPrinter.exe"))
            return;
        System.IO.File.WriteAllText(FilePath, text);
        Process process = new Process();
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.FileName = "POSPrinter.exe";
        process.Start();
    }
}