using System.ComponentModel;
using UnitsNet;
using UnitsNet.Units;

#pragma warning disable CS0618

namespace JoeScan.LogScanner.Core.Models;

[AttributeUsage(AttributeTargets.Property)]
public class UnitAttribute : Attribute
{
    public QuantityType SourceUnitType { get; set; }

    public UnitAttribute(QuantityType sourceUnitType)
    {
        SourceUnitType = sourceUnitType;
    }
}

public class LogData
{
    public LengthUnit SourceLengthUnit { get; set; } = LengthUnit.Millimeter;
    public VolumeUnit SourceVolumeUnit { get; set; } = VolumeUnit.CubicMillimeter;
    public AngleUnit SourceAngleUnit { get; set; } = AngleUnit.Radian;

    [Unit(QuantityType.Length)]
    public double Length { get;  set; }

    /// <summary>
    /// File version number. 
    /// </summary>
    [Unit(QuantityType.Undefined)]
    public int FileVersionRev { get; private set; }

    /// <summary>
    /// Number assigned to the Log
    /// </summary>
    [Unit(QuantityType.Undefined)]
    public int LogNumber { get; private set; }

    /// <summary>
    /// Date and Time when the log was originally scanned
    /// </summary>
    [Unit(QuantityType.Undefined)]
    public DateTime ScannedDateTime { get; private set; }


    /// <summary>
    /// Estimated volume of wood in the log in mm^3
    /// </summary>
    [Unit(QuantityType.Volume)]
    public double Volume { get; private set; }

    /// <summary>
    /// Estimate of the bark volume based on the BarkAllowance, in mm^3
    /// </summary>
    [Unit(QuantityType.Volume)]
    public double BarkVolume { get; private set; }

    /// <summary>
    /// Small End Diameter in mm
    /// </summary>
    [Unit(QuantityType.Length)]
    public double Sed { get; private set; }
    [Unit(QuantityType.Length)]
    public double SedX { get; private set; }
    [Unit(QuantityType.Length)]
    public double SedY { get; private set; }

    /// <summary>
    /// Large End Diameter in mm
    /// </summary>
    [Unit(QuantityType.Length)]
    public double Led { get; private set; }
    [Unit(QuantityType.Length)]
    public double LedX { get; private set; }
    [Unit(QuantityType.Length)]
    public double LedY { get; private set; }

    /// <summary>
    /// The largest diameter found on the log
    /// </summary>
    [Unit(QuantityType.Length)]
    public double MaxDiameter { get; private set; }

    /// <summary>
    /// The position along the length of the log where the max was found
    /// </summary>
    [Unit(QuantityType.Length)]
    public double MaxDiameterZ { get; private set; }

    /// <summary>
    /// The smallest diameter found on the log
    /// </summary>
    [Unit(QuantityType.Length)]
    public double MinDiameter { get; private set; }

    /// <summary>
    /// The position along the length of the log where the minimum diameter was found
    /// </summary>
    [Unit(QuantityType.Length)]
    public double MinDiameterZ { get; private set; }

    /// <summary>
    /// For a straight line from the begining center to end center, sweep is the largest deviation in mm
    /// </summary>
    [Unit(QuantityType.Length)]
    public double Sweep { get; private set; }
    [Unit(QuantityType.Angle)]
    public double SweepAngle { get; private set; }

    /// <summary>
    /// The max deviation 180 degrees from SweepAngle
    /// </summary>
    [Unit(QuantityType.Length)]
    public double CompoundSweepS { get; private set; }

    /// <summary>
    /// max deviation at 90/270 degrees from SweepAngle
    /// </summary>
    [Unit(QuantityType.Length)]
    public double CompoundSweep90 { get; private set; }

    /// <summary>
    /// Taper is (LED-SED)/Length in mm/mm
    /// </summary>
    [Unit(QuantityType.Length)]
    public double Taper { get; private set; }

    /// <summary>
    /// TaperX is (LEDX-SEDX)/Length in mm/mm
    /// </summary>
    [Unit(QuantityType.Length)]
    public double TaperX { get; private set; }

    /// <summary>
    /// TaperY is (LEDY-SEDY)/Length in mm/mm
    /// </summary>        
    [Unit(QuantityType.Length)]
    public double TaperY { get; private set; }


    //        /// <summary>
    //        /// True if the log came in large end (butt) first.
    //        /// </summary>
    //        public bool ButtFirst { get; private set; }
    //
    //        /// <summary>
    //        /// True if the log have Butt.
    //        /// </summary>
    //        public bool FlaredButt { get; private set; }

    /// <summary>
    /// The path and file where additional data (raw scans, etc) for the log is stored.
    /// </summary>
    public string DataFile { get; set; }

    /// <summary>
    ///  The file where the data is stored as comma separated values (file name only)
    /// </summary>
    public string CsvFile { get; set; }

    /// <summary>
    /// The diameter of the maximum inscribing cylinder around the 
    /// centerline.
    /// </summary>
    [Unit(QuantityType.Length)]
    [DisplayName("Max. Inscr. Cyl. Ø")]
    public double MaxInscribingCylinderDiameter { get; private set; }

    /// <summary>
    /// The diameter of the minimum containing cylinder around on the 
    /// centerline.
    /// </summary>
    [Unit(QuantityType.Length)]
    public double MinContainingCylinderDiameter { get; private set; }

    /// <summary>
    /// ratio of absolute sweep value and small end diameter 
    /// </summary>        
    [Unit(QuantityType.Length)]
    public double SweepPercent { get; private set; }

    /// <summary>
    /// The usable volume as computed from the maximum inscribing cylinder and the length
    /// </summary>
    [Unit(QuantityType.Volume)]
    public double UsableVolume
    {
        get
        {
            return ((Math.PI * (MaxInscribingCylinderDiameter) * (MaxInscribingCylinderDiameter)) / 4.0) * Length;
        }
    }


}
