using System;
using System.Collections.Generic;
using System.Text;

namespace ID3ParseConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    Console.Write("File to parse (empty to quit): ");
                    string strFile = Console.ReadLine();
                    if (strFile.Trim().Length == 0)
                        break;

                    ID3ParseLib.MP3File file = new ID3ParseLib.MP3File(strFile);
                    Console.WriteLine("ID3v1 Tag Information");
                    Console.WriteLine("---------------------");
                    Console.WriteLine(file.ID3v1.ToString());

                    Console.WriteLine();
                    Console.WriteLine("ID3v2 Tag Information");
                    Console.WriteLine("---------------------");
                    foreach (ID3ParseLib.Frame f in file.ID3v2.Frames)
                    {
                        if (f.Name == "Year")
                            Console.WriteLine(string.Format("{0}\t{1}\t{2} ({3} years old)", f.ID.ToString(), f.Name, f.Value, ((DateTime.Now - new DateTime(int.Parse(f.Value), 1, 1)).TotalDays / 365.0f).ToString("F1")));
                        else
                            Console.WriteLine(string.Format("{0}\t{1}\t{2}", f.ID.ToString(), f.Name, f.Value));
                    }
                    Console.WriteLine("Found " + file.ID3v2.Frames.Length + " frames.");
                    Console.WriteLine();
                }
                catch
                {
                    Console.WriteLine("That file doesn't exist, please try again (press any key)...");
                }
            }
        }
    }
}
