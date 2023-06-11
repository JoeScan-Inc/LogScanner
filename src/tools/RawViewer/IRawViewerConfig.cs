using System.ComponentModel;

namespace RawViewer;

public interface IRawViewerConfig
{

    string LastFileBrowserLocation { get; set; }
    [DefaultValue(1.0)]
    double EncoderPulseInterval { get; set; }
}
