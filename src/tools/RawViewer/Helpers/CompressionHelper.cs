using System;
using System.IO.Compression;
using System.IO;

namespace RawViewer.Helpers;

public static class CompressionHelper
{
    public static bool IsCompressedFile(string fileName)
    {
        using var fs = new FileStream(fileName, FileMode.Open);

        try
        {
            // we may have a gzipped stream or just a binary dump
            var zipped = new GZipStream(fs, CompressionMode.Decompress);
            var br = new BinaryReader(zipped);
            zipped.ReadByte();
            return true;
        }
        catch (Exception e)
        {
            // if the stream is not compressed, reading from it will throw
        }

        return false;
    }
}
