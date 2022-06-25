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

public static class RawLogWriter
{
    private const int magic = 0x01;
    public static void Write(this RawLog r, BinaryWriter bw)        
    {
        bw.Write(magic); 
        bw.Write(r.LogNumber); 
        bw.Write(r.Id.ToByteArray()); //16 bytes
        bw.Write(r.ProfileData.Count);
        foreach (var profile in r.ProfileData)
        {
            profile.Write(bw);
        }
        bw.Flush();
    }
}
