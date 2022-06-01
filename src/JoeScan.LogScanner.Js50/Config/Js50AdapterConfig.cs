using JoeScan.Pinchot;
using Newtonsoft.Json;
using NLog;
using System.Dynamic;
using System.Runtime.Serialization;

namespace JoeScan.LogScanner.Js50.Config;

public class Js50AdapterConfig 
{
    private const double mm_to_in = 25.4;

    #region Public Properties

    public double ScanRate { get; set; } 
    public DataFormat DataFormat { get; set; }
    public string Units { get; set; }
    public double EncoderPulseInterval { get; set; }
    public List<Js50HeadConfig> ScanHeads { get; set; }

    #endregion

    #region Private Fields

    bool isValid;

    #endregion

    #region Lifecycle
    
    public static Js50AdapterConfig ReadFromFile(string configFile)
    {
        using StreamReader file = File.OpenText(configFile);
        JsonSerializer serializer = new JsonSerializer();
        var res = (Js50AdapterConfig)serializer.Deserialize(file, typeof(Js50AdapterConfig))!;
        return res;
    }
    #endregion

    #region IJs50Config Implementation
    public bool IsValid => isValid;
    #endregion

    #region JSON Deserialization Callback

    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        if (Units == "Millimeters")
        {
            // adjust all linear values to be in inches, the v13 API does not 
            // handle units at all
            EncoderPulseInterval /= mm_to_in;
            foreach (var head in ScanHeads)
            {
                head.Alignment.ShiftX /= mm_to_in;
                head.Alignment.ShiftY /= mm_to_in;
                head.Window.Top /= mm_to_in;
                head.Window.Bottom /= mm_to_in;
                head.Window.Left /= mm_to_in;
                head.Window.Right /= mm_to_in;
            }
        }
        //TODO: add a bit more verification 
        isValid = true;
    }

    #endregion
}
