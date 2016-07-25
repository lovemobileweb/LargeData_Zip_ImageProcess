using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipTool
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                /*double seconds = 0.0d;

                Utilities.Log("\r\n[ For 1000 times ]\r\n");
                Utilities.Transparency(null, "png", true, false);
                ////////////////////////////////////////////////////////////
                Utilities.Log("===== Write to a file =====");
                Utilities.IsDontWrite2File = false;
                Utilities.Log("Lock Bits");
                BitmapWrapper.IsLockBits = true;
                Benchmark.Start();
                for (int i = 1000; i > 0; i--)
                    Utilities.Transparency(null, "png", false, false);
                Benchmark.End();
                seconds = Benchmark.GetSeconds();
                Utilities.Log("\tWith no resaturate : " + seconds + " ms");
                Benchmark.Start();
                for (int i = 1000; i > 0; i--)
                    Utilities.Transparency(null, "png", true, false);
                Benchmark.End();
                seconds = Benchmark.GetSeconds();
                Utilities.Log("\tWith resaturate : " + seconds + " ms");
                //----------------------------------------------------
                Utilities.Log("Get / Set Pixel");
                BitmapWrapper.IsLockBits = false;
                Benchmark.Start();
                for (int i = 1000; i > 0; i--)
                    Utilities.Transparency(null, "png", false, false);
                Benchmark.End();
                seconds = Benchmark.GetSeconds();
                Utilities.Log("\tWith no resaturate : " + seconds + " ms");
                Benchmark.Start();
                for (int i = 1000; i > 0; i--)
                    Utilities.Transparency(null, "png", true, false);
                Benchmark.End();
                seconds = Benchmark.GetSeconds();
                Utilities.Log("\tWith resaturate : " + seconds + " ms");
                //----------------------------------------------------
                Utilities.Log("");
                ////////////////////////////////////////////////////////////
                Utilities.Log("===== Don't write to a file =====");
                Utilities.IsDontWrite2File = true;
                Utilities.Log("Lock Bits");
                BitmapWrapper.IsLockBits = true;
                Benchmark.Start();
                for (int i = 1000; i > 0; i--)
                    Utilities.Transparency(null, "png", false, false);
                Benchmark.End();
                seconds = Benchmark.GetSeconds();
                Utilities.Log("\tWith no resaturate : " + seconds + " ms");
                Benchmark.Start();
                for (int i = 1000; i > 0; i--)
                    Utilities.Transparency(null, "png", true, false);
                Benchmark.End();
                seconds = Benchmark.GetSeconds();
                Utilities.Log("\tWith resaturate : " + seconds + " ms");
                //----------------------------------------------------
                Utilities.Log("Get / Set Pixel");
                BitmapWrapper.IsLockBits = false;
                Benchmark.Start();
                for (int i = 1000; i > 0; i--)
                    Utilities.Transparency(null, "png", false, false);
                Benchmark.End();
                seconds = Benchmark.GetSeconds();
                Utilities.Log("\tWith no resaturate : " + seconds + " ms");
                Benchmark.Start();
                for (int i = 1000; i > 0; i--)
                    Utilities.Transparency(null, "png", true, false);
                Benchmark.End();
                seconds = Benchmark.GetSeconds();
                Utilities.Log("\tWith resaturate : " + seconds + " ms");
                //----------------------------------------------------
                Utilities.Log("");
                ////////////////////////////////////////////////////////////

                Utilities.DrawTile();
                return;*/

                // parsing command line
                double seconds = 0.0d;
                int maxCmd = -1, cmd = 0, opt = -1;
                long testFileSize = 5L * 1024 * 1024 * 1024; // 5 GBytes
                string textFilePath = Environment.CurrentDirectory + "\\" + Utilities.GetStringConfig("textFileName", "test_inputfile.txt");
                string zippedFilePath = Environment.CurrentDirectory + "\\" + Utilities.GetStringConfig("zippedFileName", "test_zippedfile.zip");
                string unzippedFilePath = Environment.CurrentDirectory + "\\" + Utilities.GetStringConfig("unzippedFileName", "test_unzippedfile.txt");

                foreach (string arg in args)
                {
                    if (arg == "-0")
                        cmd = 0;
                    else if (arg == "-1")
                        cmd = 1;
                    else if (arg == "-2")
                        cmd = 2;
                    else if (arg == "-3")
                        cmd = 3;
                    else if (arg == "-4")
                        cmd = 4;
                    else if (arg == "-5")
                        cmd = 5;
                    else if (arg == "-?" || arg == "/?")
                        cmd = -1;
                    else if (arg == "-t")
                        opt = 0;
                    else if (arg == "-z")
                        opt = 1;
                    else if (arg == "-u")
                        opt = 2;
                    else if (arg == "-s")
                        opt = 3;
                    else
                    {
                        if (opt == 0)
                            textFilePath = arg.Trim(new char[] { '"' });
                        else if (opt == 1)
                            zippedFilePath = arg.Trim(new char[] { '"' });
                        else if (opt == 2)
                            unzippedFilePath = arg.Trim(new char[] { '"' });
                        else if (opt == 3)
                            testFileSize = long.Parse(arg.Trim(new char[] { '"' }));
                    }
                }

                // processing
                if (cmd == -1) // if help cmd
                {
                    Help();
                    return;
                }
                
                int chunkSize = Utilities.GetIntConfig("fileChunkSize", 1048576); // read fileChunkSize value from app.config

                if (cmd == 0) // if auto mode
                {
                    maxCmd = 4;
                    Utilities.Log("[Main] Processing with auto mode");
                    cmd++;
                }
                else // if manual mode
                    maxCmd = cmd;
                if (cmd == 1 && cmd <= maxCmd) // create a text file for test
                {
                    Utilities.Log("[Main] Creating text file " + textFilePath);

                    Benchmark.Start();
                        if (Utilities.CreateFile(textFilePath, testFileSize, chunkSize) == false)
                            return;
                    Benchmark.End();
                    seconds = Benchmark.GetSeconds();

                    Utilities.Log("[Main] Created text file : " + seconds + " sec");
                    cmd++;
                }
                if (cmd == 2 && cmd <= maxCmd) // zip to file
                {
                    Utilities.Log("[Main] Zipping file " + zippedFilePath);

                    Benchmark.Start();
                        if (ZipWrapper.Zip(zippedFilePath, textFilePath, chunkSize) == false)
                            return;
                    Benchmark.End();
                    seconds = Benchmark.GetSeconds();

                    Utilities.Log("[Main] Zipped file : " + seconds + " sec");
                    cmd++;
                }
                if (cmd == 3 && cmd <= maxCmd) // unzip to file
                {
                    Utilities.Log("[Main] Unzipping file " + zippedFilePath);

                    Benchmark.Start();
                        if (ZipWrapper.Unzip(zippedFilePath, unzippedFilePath, chunkSize) == false)
                            return;
                    Benchmark.End();
                    seconds = Benchmark.GetSeconds();

                    Utilities.Log("[Main] Unzipped file : " + seconds + " sec");
                    cmd++;
                }
                if (cmd == 4 && cmd <= maxCmd) // compare two files
                {
                    Utilities.Log("[Main] Comparing file " + zippedFilePath);

                    Benchmark.Start();
                        if (Utilities.CompareFiles(textFilePath, unzippedFilePath, chunkSize) != Utilities.ComapreResult.Equal)
                            return;
                    Benchmark.End();
                    seconds = Benchmark.GetSeconds();

                    Utilities.Log("[Main] Equal files : " + seconds + " sec");
                    cmd++;
                }
            }
            catch (Exception ex)
            {
                Utilities.Log(string.Format("Error [Main] {0}", ex.Message));
            }
        }

        /// <summary>
        /// print help message
        /// </summary>
        static public void Help()
        {
            Utilities.Log("");
            Utilities.Log("Usage: ziptool [cmd] [opt]");
            Utilities.Log("");
            Utilities.Log("Cmd:");
            Utilities.Log("\t-0 \t\tAuto step (default)");
            Utilities.Log("\t-1 \t\tCreate a text file");
            Utilities.Log("\t-2 \t\tZip");
            Utilities.Log("\t-3 \t\tUnzip");
            Utilities.Log("\t-4 \t\tCompare");
            Utilities.Log("\t-?, /? \t\tHelp");

            Utilities.Log("");
            Utilities.Log("Opt:");
            Utilities.Log("\t-s path \tSize of a text file is created");
            Utilities.Log("\t-t path \tPath a text file is created");
            Utilities.Log("\t-z path \tPath a zip file is zipped");
            Utilities.Log("\t-u path \tPath a text file is unzipped");
            Utilities.Log("");
        }
    }
}
