using JoeScan.LogScanner.Core.Models;
using RawViewer.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace RawViewer.Helpers;

public static class DataReader
{
    public static Task<List<RawProfile>> ReadFromFileAsync(string fileName)
    {
        return Task.Run(() =>
        {
            var l = new List<RawProfile>();
            var sw = Stopwatch.StartNew();
            int idx = 0;
            BinaryReader? br = null;
            if (CompressionHelper.IsCompressedFile(fileName))
            {
                var fs = new FileStream(fileName, FileMode.Open);
                var zipped = new GZipStream(fs, CompressionMode.Decompress);
                br = new BinaryReader(zipped);
            }
            else
            {
                var fs = new FileStream(fileName, FileMode.Open);
                br = new BinaryReader(fs);
            }

            try
            {
                while (true)
                {
                    var p = ProfileReaderWriter.Read(br);

                    var r = new RawProfile(p) { Index = idx++ };
                    l.Add(r);
                }
            }
            catch (EndOfStreamException)
            {
                //ignore, eof
            }
           
            finally
            {
                sw.Stop();
            }

            br.Dispose();
           
            return l;
        });
    }
}
