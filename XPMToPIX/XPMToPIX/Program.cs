using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XPMToPIX
{
  class Program
  {
    static void Main(string[] args)
    {
      bool invert = false;
      // Set folder (application path or first argument)
      string folder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
      if (args.Length > 0)
      {
        for (int i = 0; i < args.Length; i++)
        {
          if (args[i].ToLower() == "--inv" || args[i].ToLower() == "--invert" || args[i].ToLower() == "-inv" || args[i].ToLower() == "-invert")
            invert = true;
          else if (Directory.Exists(args[i]))
            folder = args[i];
          else
          {
            Console.WriteLine("Path does not exist!");
            Environment.Exit(0);
          }
        }
      }

      Console.WriteLine("-----------------------------------------------------------");
      Console.WriteLine("--  XPM To PIX Converter v0.3 by MickGyver @ DaemonBite  --");
      Console.WriteLine("-----------------------------------------------------------");
      Console.WriteLine("Path: " + folder);
      Console.WriteLine("Colors: " + (invert ? "Inverted" : "Normal"));
      Console.WriteLine("Converting all XPM images to PIX...");

      // Convert all files in folder
      string [] files = Directory.GetFiles(folder, "*.xpm");
      List<string> pixLines = new List<string>();
      foreach (string file in files)
      {
        Console.Write("Converting: \"" + Path.GetFileName(file) + "\"...");

        pixLines.Clear();

        string xpm = File.ReadAllText(file);
        string pix = xpm.Replace("\r\n", "\n");
        string [] pixLinesTemp = pix.Split('\n');
        foreach(string line in pixLinesTemp)
        {
          if (!line.Trim().StartsWith("/*"))
            pixLines.Add(line);
        }
        bool error = false;

        try
        {
          // Find last line to use (the one with };)
          int lineMax = pixLines.Count - 1;
          for (int i = lineMax; i >= 0; i--)
          {
            if (pixLines[i].Contains("};"))
            {
              if (pixLines[i].Trim().Length == 2) // "Empty" ending line
                lineMax = i - 1;
              else
                lineMax = i;
              break;
            }
          }

          // Find symbols for colors
          string[] symbols = { ".", " " };  
          for (int i = 1; i <= 3; i++)
          {
            string str = pixLines[i].Trim();
            str = str.Trim('"');
            if (str.ToLower().Contains("#000000") || str.ToLower().Contains("black"))
              symbols[invert ? 1 : 0] = str.Substring(0, 1);
            if (str.ToLower().Contains("#ffffff") || str.ToLower().Contains("white"))
              symbols[invert ? 0 : 1] = str.Substring(0, 1);
          }

          // Replace }; with ) of last line
          string tmp = pixLines[lineMax].Trim();
          if (tmp.EndsWith("};"))
            pixLines[lineMax] = tmp.Substring(0, tmp.Length - 2);
          pixLines[lineMax] += ")";

          // Write "header"
          pix = "#!/bin/bash\nlogo=(";

          // Write all lines
          for (int i = 4; i <= lineMax; i++) // Starting at line 4, hardcoded, could be made smarter
            pix += pixLines[i].Replace(symbols[0], "0").Replace(symbols[1], "1") + "\n";

          // Save the PIX to the same folder with the same name (with .pix extension)
          File.WriteAllText(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)) + ".pix", pix);
        }
        catch (Exception)
        {
          Console.WriteLine(" failed!");
          error = true;
        }
        if(!error)
          Console.WriteLine(" done!");
      }

      Console.WriteLine("-----------------------------------------------------------");

    }
  }
}
