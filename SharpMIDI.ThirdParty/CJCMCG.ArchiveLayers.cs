/*
This script segment is made by hccdy, found in midi-counter-gen
https://github.com/hccdy/midi-counter-gen/blob/master/Program.cs
*/

using System.Diagnostics;
using System.IO.Compression;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Tar;
using SharpCompress.Archives.GZip;
using SharpCompress.Compressors.Xz;

namespace SharpMIDI
{
    class CJCMCG
    {
        static string filein = "";
        public static bool CanDec(string s)
        {
            return s.EndsWith(".mid") || s.EndsWith(".xz") || s.EndsWith(".zip") || s.EndsWith(".7z") || s.EndsWith(".rar") || s.EndsWith(".tar") || s.EndsWith(".gz");
        }
        public static Stream AddXZLayer(Stream input)
        {
            try
            {
                Process xz = new Process();
                xz.StartInfo = new ProcessStartInfo("xz", "-dc --threads=0")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false
                };
                xz.Start();
                Task.Run(() =>
                {
                    input.CopyTo(xz.StandardInput.BaseStream);
                    xz.StandardInput.Close();
                });
                return xz.StandardOutput.BaseStream;
            }
            catch (Exception)
            {
                return new XZStream(input);
            }
        }
        public static Stream AddZipLayer(Stream input)
        {
            var zip = new ZipArchive(input, ZipArchiveMode.Read);
            foreach (var entry in zip.Entries)
            {
                if (CanDec(entry.Name))
                {
                    filein = entry.Name;
                    return entry.Open();
                }
            }
            MessageBox.Show("No MIDI file found in the ZIP file.");
            throw new Exception();
        }
        public static Stream AddRarLayer(Stream input)
        {
            var zip = RarArchive.Open(input);
            foreach (var entry in zip.Entries)
            {
                if (CanDec(entry.Key))
                {
                    filein = entry.Key;
                    return entry.OpenEntryStream();
                }
            }
            MessageBox.Show("No MIDI file found in the RAR file.");
            throw new Exception();
        }
        public static Stream Add7zLayer(Stream input)
        {
            var zip = SevenZipArchive.Open(input);
            foreach (var entry in zip.Entries)
            {
                if (CanDec(entry.Key))
                {
                    filein = entry.Key;
                    return entry.OpenEntryStream();
                }
            }
            MessageBox.Show("No MIDI file found in the 7z file.");
            throw new Exception();
        }
        public static Stream AddTarLayer(Stream input)
        {
            var zip = TarArchive.Open(input);
            foreach (var entry in zip.Entries)
            {
                if (CanDec(entry.Key))
                {
                    filein = entry.Key;
                    return entry.OpenEntryStream();
                }
            }
            MessageBox.Show("No MIDI file found in the TAR file.");
            throw new Exception();
        }
        public static Stream AddGZLayer(Stream input)
        {
            var zip = GZipArchive.Open(input);
            foreach (var entry in zip.Entries)
            {
                if (CanDec(entry.Key))
                {
                    filein = entry.Key;
                    return entry.OpenEntryStream();
                }
            }
            MessageBox.Show("No MIDI file found in the GZ file.");
            throw new Exception();
        }
        public static (bool,Stream) ArchiveStreamPassthrough(string path, Stream currentRead)
        {
            filein = path;
            bool mod = false;
            while (!filein.EndsWith(".mid"))
            {
                if (filein.EndsWith(".xz"))
                {
                    mod = true;
                    currentRead = AddXZLayer(currentRead);
                    filein = filein.Substring(0, filein.Length - 3);
                }
                else if (filein.EndsWith(".zip"))
                {
                    mod = true;
                    currentRead = AddZipLayer(currentRead);
                }
                else if (filein.EndsWith(".rar"))
                {
                    mod = true;
                    currentRead = AddRarLayer(currentRead);
                }
                else if (filein.EndsWith(".7z"))
                {
                    mod = true;
                    currentRead = Add7zLayer(currentRead);
                }
                else if (filein.EndsWith(".tar"))
                {
                    mod = true;
                    currentRead = AddTarLayer(currentRead);
                }
                else if (filein.EndsWith(".gz"))
                {
                    mod = true;
                    currentRead = AddGZLayer(currentRead);
                } else
                {
                    //MessageBox.Show("Infinite loop on archive layers, if you are loading an XZ archive check so the file name is correct.\n\nPress OK to continue.");
                    break;
                }
            }
            return (mod,currentRead);
        }
    }
}