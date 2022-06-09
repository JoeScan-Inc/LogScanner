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

    public RawLog(int logNumber, IEnumerable<Profile> profiles)
    {
        LogNumber = logNumber;
        profileData = profiles.OrderBy(q => q.EncoderValues[0]).ToList();
        TimeScanned = DateTime.Now;
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
