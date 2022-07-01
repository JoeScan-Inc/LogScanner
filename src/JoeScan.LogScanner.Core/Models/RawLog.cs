using System.IO.Compression;

namespace JoeScan.LogScanner.Core.Models;

public class RawLog
{

    /// <summary>
    /// We keep profiles in a list instead of a dictionary like in the
    /// old LogScanner, as we don't have a Z value to order them. The engine will
    /// have to be smart about ordering them by encoder value.
    /// </summary>
    private readonly List<Profile> profileData;
    public IReadOnlyList<Profile> ProfileData => profileData;
    public int LogNumber { get; }
    public DateTime TimeScanned { get; set; }

    public Guid Id { get; init; }
    public RawLog(int logNumber, IEnumerable<Profile> profiles)
    {
        LogNumber = logNumber;
        profileData = profiles.OrderBy(q => q.EncoderValues[0]).ToList();
        TimeScanned = DateTime.Now;
        Id = Guid.NewGuid();
    }

    
}

public static class RawLogReaderWriter
{
    private const int currentVersion = 0x01;
    public static string DefaultExtension => "loga";

    public static void Write(this RawLog r, BinaryWriter bw)        
    {
        bw.Write(currentVersion); // 32 bit int
        bw.Write(r.LogNumber); // 32 bit int
        bw.Write(r.Id.ToByteArray()); // 16 byte array
        bw.Write(r.TimeScanned.ToBinary()); // 64 bits encoding datetime and ticks
        bw.Write(r.ProfileData.Count); //32 bit int

        foreach (var profile in r.ProfileData)
        {
            profile.Write(bw);
        }
        bw.Flush();
    }

    public static RawLog Read(BinaryReader br)
    {
        var version = br.ReadInt32();
        if (version != currentVersion)
        {
            throw new Exception($"Version mismatch: File is version {version}.");
        }
        var number = br.ReadInt32();
        var guid = new Guid(br.ReadBytes(16));
        var datetime = DateTime.FromBinary(br.ReadInt64());
        var count = br.ReadInt32();
        var l = new List<Profile>();
        for (int i = 0; i < count; i++)
        {
            var p = ProfileReaderWriter.Read(br);
            if (p != null)
            {
                l.Add(p);
            }
            else
            {
                throw new Exception("Failed to read profile from stream.");
            }
        }
        return new RawLog(number, l) { Id = guid, TimeScanned = datetime };
    }

    public static RawLog Read(string fileName)
    {
        using var fs = new FileStream(fileName, FileMode.Open);
        using var gzip = new GZipStream(fs, CompressionMode.Decompress);
        using var reader = new BinaryReader(gzip);
        return Read(reader);
    }
    // TODO: benchmark and potentially use InsertionSort 

    // private void InsertionSort(int[] arr)
    // {
    //     int j, temp;
    //     for (int i = 1; i <= arr.Length - 1; i++)
    //     {
    //         temp = arr[i];
    //         j = i - 1;
    //         while (j >= 0 && arr[j] > temp)
    //         {
    //             arr[j + 1] = arr[j];
    //             j--;
    //         }
    //         arr[j + 1] = temp;
    //     }
    // }
}
