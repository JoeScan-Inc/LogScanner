// Copyright(c) JoeScan Inc. All Rights Reserved.
//
// Licensed under the BSD 3 Clause License. See LICENSE.txt in the project
// root for license information.


using System.Text.Json;
using JoeScan.Pinchot;

// ReSharper disable MemberCanBePrivate.Global

namespace JoeScan.LogScanner.Core.Adapters.JS50
{
    public static class ScanSystemExtensions
    {
        #region Public Methods

        /// <summary>
        /// Creates a ScanSystem from a file. The file must be valid JSON, with
        /// the exception that comments are allowed. 
        /// Currently, the only supported version is 1.0.
        /// For options, see <see cref="ScanSystemCreationOptions"/>
        /// </summary>
        /// <param name="fileName">JSON formatted file to parse.</param>
        /// <param name="options">parsing options</param>
        /// <returns>populated and configured ScanSystem, ready to connect</returns>
        /// <exception cref="ArgumentException">file is null or empty</exception>
        /// <exception cref="JsonException">malformed JSON</exception>
        /// <exception cref="ApplicationException">formal error in ScanSystem Specification</exception>
        public static ScanSystem CreateFromFile(string fileName, ScanSystemCreationOptions? options = null)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(nameof(fileName));
            }

            var jsonString = File.ReadAllText(fileName);

            return CreateFromString(jsonString, options);
        }

        /// <summary>
        /// Creates a ScanSystem from a string. The string must be valid JSON, with
        /// the exception that comments are allowed.
        /// Currently, the only supported version is 1.0.
        /// For options, see <see cref="ScanSystemCreationOptions"/>
        /// </summary>
        /// <param name="jsonString">JSON formatted string to parse</param>
        /// <param name="options">parsing options</param>
        /// <returns>populated and configured ScanSystem, ready to connect</returns>
        /// <exception cref="ArgumentException">file is null or empty</exception>
        /// <exception cref="JsonException">malformed JSON</exception>
        /// <exception cref="ApplicationException">formal error in ScanSystem Specification</exception>
        public static ScanSystem CreateFromString(string jsonString, ScanSystemCreationOptions? options = null)
        {
            if (String.IsNullOrEmpty(jsonString))
            {
                throw new ArgumentException(nameof(jsonString));
            }

            options ??= new ScanSystemCreationOptions();
            // we use the JsonDocument route to build a ScanSystem piece by piece
            // first, build the DOM
            using JsonDocument document = JsonDocument.Parse(jsonString,
                new JsonDocumentOptions()
                {
                    CommentHandling = options.CommentHandling, AllowTrailingCommas = options.AllowTrailingCommas
                });

            return CreateFromJsonElement(document.RootElement, options);
        }

        public static ScanSystem CreateFromJsonElement(JsonElement root, ScanSystemCreationOptions? options = null)
        {
            options ??= new ScanSystemCreationOptions();
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException("Expected a JSON object");
            }

            // as a workaround for missing version elements in the current JsSetup, we'll ignore the version 
            // if the IgnoreVersion flag is set
            if (!options.IgnoreVersion)
            {
                // the version and units properties are required, everything else is optional
                // check version
                if (!root.TryGetProperty("Version", out JsonElement versionElement))
                {
                    throw new JsonException("Expected a Version property");
                }

                if (!Version.TryParse(versionElement.GetString(), out Version? version))
                {
                    throw new JsonException("Expected a Version property of type string");
                }

                if (version.Major != 1 || version.Minor != 0)
                {
                    throw new JsonException($"Expected version 1.0, found {version!.ToString()}");
                }
            }

            return ParseRootObject(root);
        }

        #endregion

        #region Internal Methods

        internal static ScanSystem ParseRootObject(JsonElement root)
        {
            // we already checked version, next is units
            // this will throw if the property is not present or not one of the valid enum types
            // at this point, we haven't created a ScanSystem so no cleanup needed
            var units = (ScanSystemUnits)Enum.Parse(typeof(ScanSystemUnits), root.GetProperty("Units").GetString()!);
            // next is ScanHeads, which is optional. If it's not present, we'll just return an empty ScanSystem with the units set
            // further down when we iterate  through the ScanHeads
            List<ScanHeadEntry>? heads = null;
            if (root.TryGetProperty("ScanHeads", out JsonElement _))
            {
                heads = ParseScanHeadsElement(root.GetProperty("ScanHeads"));
            }

            // phase table entries, also optional
            List<PhaseTableEntry>? phaseTableEntries = null;
            if (root.TryGetProperty("PhaseTableEntries", out JsonElement phaseTableEntriesElement))
            {
                phaseTableEntries = ParsePhaseTableEntriesElement(phaseTableEntriesElement);
            }
            // the JsSetup element is ignored for a read-only parse
            // vendor element is ignored as well

            return BuildPinchotSystem(units, heads, phaseTableEntries);
        }

        private static ScanSystem BuildPinchotSystem(ScanSystemUnits units,
            List<ScanHeadEntry>? heads, IReadOnlyCollection<PhaseTableEntry>? phaseTableEntries)
        {
            // need to define a delegate to be used later in the phase table setting based on the product type
            Action<ScanSystem, PhaseTableEntry>? phaseTableEntrySetter = null;
            var system = new ScanSystem(units);
            try
            {
                if (heads != null)
                {
                    foreach (var scanHeadEntry in heads)
                    {
                        // set up delegates to set alignment and window, we do this so we don't have to 
                        // switch every time for camera vs laser based units
                        Action<ScanHead, CameraLaserPair, Alignment> alignmentSetter = (_, _, _) => { };
                        Action<ScanHead, CameraLaserPair, ScanWindow> windowSetter = (_, _, _) => { };
                        Action<ScanHead, CameraLaserPair, ExclusionMask> exclusionMaskSetter = (_, _, _) => { };
                        switch (scanHeadEntry.ProductType)
                        {
                            case ProductType.JS50MX:
                            case ProductType.JS50WX:
                            case ProductType.JS50WSC:
                                alignmentSetter = (h, p, a) => h.SetAlignment(p.Camera, a.RollDeg, a.ShiftX, a.ShiftY);
                                windowSetter = (h, p, w) => h.SetWindow(p.Camera, w);
                                exclusionMaskSetter = (h, p, m) => h.SetExclusionMask(p.Camera, m);
                                phaseTableEntrySetter = (s, p) => s.AddPhaseElement(p.Id, p.Camera);
                                break;
                            case ProductType.JS50X6B20:
                            case ProductType.JS50X6B30:
                            case ProductType.JS50Z820:
                            case ProductType.JS50Z830:
                                alignmentSetter = (h, p, a) => h.SetAlignment(p.Laser, a.RollDeg, a.ShiftX, a.ShiftY);
                                windowSetter = (h, p, w) => h.SetWindow(p.Laser, w);
                                exclusionMaskSetter = (h, p, m) => h.SetExclusionMask(p.Laser, m);
                                phaseTableEntrySetter = (s, p) => s.AddPhaseElement(p.Id, p.Laser);
                                break;
                        }

                        // this will fail if head is offline
                        var scanHead = system.CreateScanHead(scanHeadEntry.SerialNumber, scanHeadEntry.Id);

                        // if orientation is overridden, set it
                        if (scanHeadEntry.Orientation.HasValue)
                        {
                            scanHead.Orientation = scanHeadEntry.Orientation.Value;
                        }

                        // exposure, only one per head
                        if (scanHeadEntry.Exposure != null)
                        {
                            scanHead.Configure(scanHeadEntry.Exposure);
                        }

                        // for each alignment, find the camera/laser pair and set the alignment
                        if (scanHeadEntry.Alignments != null)
                        {
                            foreach (var pair in scanHeadEntry.Alignments.Keys)
                            {
                                alignmentSetter(scanHead, pair, scanHeadEntry.Alignments[pair]);
                            }
                        }

                        // windows
                        if (scanHeadEntry.Windows != null)
                        {
                            foreach (var pair in scanHeadEntry.Windows.Keys)
                            {
                                windowSetter(scanHead, pair, scanHeadEntry.Windows[pair]);
                            }
                        }

                        // exclusion masks
                        if (scanHeadEntry.ExclusionMasks != null)
                        {
                            foreach (var pair in scanHeadEntry.ExclusionMasks.Keys)
                            {
                                exclusionMaskSetter(scanHead, pair,
                                    RenderMask(scanHead, scanHeadEntry.ExclusionMasks[pair]));
                            }
                        }
                        // brightness correction not supported yet
                    }
                }

                // phase table entries
                if (phaseTableEntries != null && phaseTableEntrySetter != null)
                {
                    // get all phases in the phaseTableEntries list
                    var definedPhases = phaseTableEntries.Select(p => p.Phase).Distinct().OrderBy(q => q).ToList();
                    // get a list of all phaseTableEntries that have a given phase
                    for (int i = 0; i < definedPhases.Count; i++)
                    {
                        // check that we don't have any gaps in the phase table
                        if (definedPhases[i] != i)
                        {
                            throw new ApplicationException($"Expected phase {i}, found {definedPhases[i]}");
                        }

                        var phaseTableEntriesForPhase = phaseTableEntries.Where(p => p.Phase == i).ToList();
                        system.AddPhase();
                        foreach (var phaseTableEntry in phaseTableEntriesForPhase)
                        {
                            // depending on the product type, the phaseTableEntrySetter will set the phase for the camera or laser
                            // we technically only allow heads of the same product type 
                            phaseTableEntrySetter(system, phaseTableEntry);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // clean up any resources the scan system created
                system.Dispose();
                throw;
            }

            return system;
        }

        private static ExclusionMask RenderMask(ScanHead head, List<MaskRect> maskRects)
        {
            var exclusionMask = head.CreateExclusionMask();
            foreach (var maskRect in maskRects)
            {
                // check that top, left, top+height, and left+ width are within the bounds of the exclusion mask
                if (maskRect.Left < 0 || maskRect.Top < 0 || maskRect.Left + maskRect.Width > exclusionMask.Width ||
                    maskRect.Top + maskRect.Height > exclusionMask.Height)
                {
                    throw new ApplicationException("Exclusion mask rectangle is out of bounds");
                }

                // we don't check for overlapping of rectangles, we just set the pixels again
                for (int y = maskRect.Top; y < (maskRect.Top + maskRect.Height); y++)
                {
                    var stride = exclusionMask.Width;
                    exclusionMask.SetPixels((int)(y * stride + maskRect.Left),
                        (int)(y * stride + maskRect.Left + maskRect.Width));
                }
            }

            return exclusionMask;
        }

        private static List<ScanHeadEntry> ParseScanHeadsElement(JsonElement scanHeadsElement)
        {
            var scanHeads = new List<ScanHeadEntry>();

            foreach (var scanHeadObject in scanHeadsElement.EnumerateArray())
            {
                // required properties are Id, ProductType, and SerialNumber
                var id = scanHeadObject.GetProperty("Id").GetUInt32();
                var serialNumber = scanHeadObject.GetProperty("Serial").GetUInt32();
                var head = new ScanHeadEntry(id, serialNumber)
                {
                    ProductType = (ProductType)Enum.Parse(typeof(ProductType),
                        scanHeadObject.GetProperty("ProductType").GetString()!)
                };
                if (scanHeadObject.TryGetProperty("Orientation", out JsonElement orientationElement))
                {
                    var orientationString = orientationElement.GetString();
                    if (String.IsNullOrEmpty(orientationString))
                    {
                        throw new JsonException("Expected a non-empty Orientation property");
                    }

                    head.Orientation = (ScanHeadOrientation)Enum.Parse(typeof(ScanHeadOrientation), orientationString);
                }

                // get alignments
                if (scanHeadObject.TryGetProperty("Alignments", out JsonElement alignmentsElement))
                {
                    head.Alignments = ParseAlignmentElements(alignmentsElement);
                }

                // get exposure
                if (scanHeadObject.TryGetProperty("Exposure", out JsonElement exposureElement))
                {
                    head.Exposure = ParseExposureElement(exposureElement);
                }

                // get windows
                if (scanHeadObject.TryGetProperty("Windows", out JsonElement windowsElement))
                {
                    head.Windows = ParseWindowsElement(windowsElement);
                }

                // get exclusion masks
                if (scanHeadObject.TryGetProperty("ExclusionMasks", out JsonElement exclusionMasksElement))
                {
                    head.ExclusionMasks = ParseExclusionMasksElement(exclusionMasksElement);
                }

                // get brightness correction / not supported yet

                scanHeads.Add(head);
            }

            return scanHeads;
        }

        internal static Dictionary<CameraLaserPair, List<MaskRect>> ParseExclusionMasksElement(
            JsonElement exclusionMasksElement)
        {
            var masks = new Dictionary<CameraLaserPair, List<MaskRect>>();
            foreach (var exclusionMaskElement in exclusionMasksElement.EnumerateArray())
            {
                // required properties are Camera, Laser, and ExcludedRegions
                // Enabled is ignored, will be removed from JsSetup
                var camera = (Camera)Enum.Parse(typeof(Camera),
                    exclusionMaskElement.GetProperty("Camera").GetString()!);
                var laser = (Laser)Enum.Parse(typeof(Laser), exclusionMaskElement.GetProperty("Laser").GetString()!);
                if (!exclusionMaskElement.TryGetProperty("ExcludedRegions", out var rectsElement))
                {
                    throw new JsonException("Expected an ExcludedRegions property");
                }

                var rects = new List<MaskRect>();
                foreach (var rectElement in rectsElement.EnumerateArray())
                {
                    var left = rectElement.GetProperty("Left").GetInt32();
                    var top = rectElement.GetProperty("Top").GetInt32();
                    var width = rectElement.GetProperty("Width").GetInt32();
                    var height = rectElement.GetProperty("Height").GetInt32();
                    rects.Add(new MaskRect(left, top, width, height));
                }

                if (rects.Count > 0)
                {
                    masks.Add(new CameraLaserPair(camera, laser), rects);
                }
            }

            return masks;
        }

        internal static List<PhaseTableEntry> ParsePhaseTableEntriesElement(JsonElement phaseTableEntriesElement)
        {
            var phaseTableEntries = new List<PhaseTableEntry>();
            foreach (var phaseTableEntryElement in phaseTableEntriesElement.EnumerateArray())
            {
                // required properties are Camera, Laser, Id, and Phase
                var camera = (Camera)Enum.Parse(typeof(Camera),
                    phaseTableEntryElement.GetProperty("Camera").GetString()!);
                var laser = (Laser)Enum.Parse(typeof(Laser), phaseTableEntryElement.GetProperty("Laser").GetString()!);
                var id = phaseTableEntryElement.GetProperty("Id").GetUInt32();
                var phase = phaseTableEntryElement.GetProperty("Phase").GetInt32();
                phaseTableEntries.Add(new PhaseTableEntry(id, camera, laser, phase));
            }

            return phaseTableEntries;
        }

        internal static Dictionary<CameraLaserPair, ScanWindow> ParseWindowsElement(JsonElement windowsElement)
        {
            var windows = new Dictionary<CameraLaserPair, ScanWindow>();
            foreach (var windowElement in windowsElement.EnumerateArray())
            {
                // required properties are Camera, Laser, and Vertices
                // Enabled is ignored, will be removed from JsSetup
                var camera = (Camera)Enum.Parse(typeof(Camera), windowElement.GetProperty("Camera").GetString()!);
                var laser = (Laser)Enum.Parse(typeof(Laser), windowElement.GetProperty("Laser").GetString()!);
                if (!windowElement.TryGetProperty("Vertices", out var verticesElement))
                {
                    throw new JsonException("Expected a Vertices property");
                }

                var vertices = new List<Point2D>();
                foreach (var vertexElement in verticesElement.EnumerateArray())
                {
                    var x = vertexElement.GetProperty("X").GetDouble();
                    var y = vertexElement.GetProperty("Y").GetDouble();
                    vertices.Add(new Point2D(x, y));
                }

                if (vertices.Count > 0)
                {
                    // we require at least 3 vertices
                    if (vertices.Count < 3)
                    {
                        throw new JsonException("Expected 3 or more vertices");
                    }

                    var window = ScanWindow.CreateScanWindowPolygonal(vertices);
                    windows.Add(new CameraLaserPair(camera, laser), window);
                }
            }

            return windows;
        }

        internal static ScanHeadConfiguration ParseExposureElement(JsonElement exposureElement)
        {
            // required is that MinLaserOnTimeUs, DefaultLaserOnTimeUs, and MaxLaserOnTimeUs are present
            // everything else is optional
            var minLaserOnTimeUs = exposureElement.GetProperty("MinLaserOnTimeUs").GetUInt32();
            var defLaserOnTimeUs = exposureElement.GetProperty("DefLaserOnTimeUs").GetUInt32();
            var maxLaserOnTimeUs = exposureElement.GetProperty("MaxLaserOnTimeUs").GetUInt32();

            var exposureConfig = new ScanHeadConfiguration();
            exposureConfig.SetLaserOnTime(minLaserOnTimeUs, defLaserOnTimeUs, maxLaserOnTimeUs);

            uint? minExposureTimeUs = null;
            uint? defExposureTimeUs = null;
            uint? maxExposureTimeUs = null;

            if (exposureElement.TryGetProperty("MinExposureTimeUs", out var minExposureTimeUsElement))
            {
                minExposureTimeUs = minExposureTimeUsElement.GetUInt32();
            }

            if (exposureElement.TryGetProperty("DefExposureTimeUs", out var defExposureTimeUsElement))
            {
                defExposureTimeUs = defExposureTimeUsElement.GetUInt32();
            }

            if (exposureElement.TryGetProperty("MaxExposureTimeUs", out var maxExposureTimeUsElement))
            {
                maxExposureTimeUs = maxExposureTimeUsElement.GetUInt32();
            }

            // all optional, so we only set if we have all three
            if (minExposureTimeUs.HasValue && defExposureTimeUs.HasValue && maxExposureTimeUs.HasValue)
            {
                exposureConfig.SetCameraExposureTime(minExposureTimeUs.Value, defExposureTimeUs.Value,
                    maxExposureTimeUs.Value);
            }

            // LaserDetectionThreshold
            if (exposureElement.TryGetProperty("LaserDetectionThreshold", out var laserDetectionThresholdElement))
            {
                var laserDetectionThreshold = laserDetectionThresholdElement.GetUInt32();
                exposureConfig.LaserDetectionThreshold = laserDetectionThreshold;
            }

            // SaturationThreshold
            if (exposureElement.TryGetProperty("SaturationThreshold", out var saturationThresholdElement))
            {
                var saturationThreshold = saturationThresholdElement.GetUInt32();
                exposureConfig.SaturationThreshold = saturationThreshold;
            }

            // SaturationPercentage
            if (exposureElement.TryGetProperty("SaturationPercentage", out var saturationPercentageElement))
            {
                var saturationPercentage = saturationPercentageElement.GetUInt32();
                exposureConfig.SaturationPercentage = saturationPercentage;
            }

            return exposureConfig;
        }

        internal static Dictionary<CameraLaserPair, Alignment> ParseAlignmentElements(JsonElement alignmentsElement)
        {
            var alignments = new Dictionary<CameraLaserPair, Alignment>();
            foreach (var alignmentElement in alignmentsElement.EnumerateArray())
            {
                // required properties are Camera, Laser, RollDeg, ShiftX, and ShiftY
                var camera = (Camera)Enum.Parse(typeof(Camera), alignmentElement.GetProperty("Camera").GetString()!);
                var laser = (Laser)Enum.Parse(typeof(Laser), alignmentElement.GetProperty("Laser").GetString()!);
                var roll = alignmentElement.GetProperty("RollDeg").GetDouble();
                var shiftX = alignmentElement.GetProperty("ShiftX").GetDouble();
                var shiftY = alignmentElement.GetProperty("ShiftY").GetDouble();
                var cameraLaserPair = new CameraLaserPair(camera, laser);
                var alignment = new Alignment(roll, shiftX, shiftY);
                alignments.Add(cameraLaserPair, alignment);
            }

            return alignments;
        }

        #endregion

        #region Helper Classes

        private class ScanHeadEntry : Tuple<uint, uint>
        {
            internal uint Id => Item1;
            internal uint SerialNumber => Item2;
            public ProductType ProductType { get; init; }
            public ScanHeadOrientation? Orientation { get; set; }
            public Dictionary<CameraLaserPair, Alignment>? Alignments { get; set; }
            public ScanHeadConfiguration? Exposure { get; set; }
            public Dictionary<CameraLaserPair, ScanWindow>? Windows { get; set; }
            public Dictionary<CameraLaserPair, List<MaskRect>>? ExclusionMasks { get; set; }

            internal ScanHeadEntry(uint id, uint serialNumber)
                : base(id, serialNumber)
            {
            }
        }

        internal class CameraLaserPair : Tuple<Camera, Laser>
        {
            internal Camera Camera => Item1;
            internal Laser Laser => Item2;

            internal CameraLaserPair(Camera camera, Laser laser)
                : base(camera, laser)
            {
            }
        }

        internal class Alignment : Tuple<double, double, double>
        {
            internal double RollDeg => Item1;
            internal double ShiftX => Item2;
            internal double ShiftY => Item3;

            internal Alignment(double rollDeg, double shiftX, double shiftY)
                : base(rollDeg, shiftX, shiftY)
            {
            }
        }

        internal class PhaseTableEntry
        {
            internal uint Id { get; }
            internal Camera Camera { get; }
            internal Laser Laser { get; }
            internal int Phase { get; }

            internal PhaseTableEntry(uint id, Camera camera, Laser laser, int phase)
            {
                Id = id;
                Camera = camera;
                Laser = laser;
                Phase = phase;
            }
        }

        internal class MaskRect
        {
            public int Left { get; }
            public int Top { get; }
            public int Width { get; }
            public int Height { get; }

            public MaskRect(int left, int top, int width, int height)
            {
                Left = left;
                Top = top;
                Width = width;
                Height = height;
            }
        }

        #endregion
    }

    #region ScanSystemCreationOptions

    /// <summary>
    /// Options for creating a ScanSystem from a file or string.
    /// </summary>
    public class ScanSystemCreationOptions
    {
        /// <summary>
        /// This is a workaround for the fact that the current JsSetup does not include a Version element.
        /// Will be deprecated when the Version element is added to JsSetup.
        /// </summary>
        public bool IgnoreVersion { get; init; }

        /// <summary>
        /// Comment handling options for the JSON parser. Default is Skip.
        /// see https://docs.microsoft.com/en-us/dotnet/api/system.text.json.jsoncommenthandling?view=net-6.0
        /// </summary>
        public JsonCommentHandling CommentHandling { get; init; } = JsonCommentHandling.Skip;

        /// <summary>
        /// Relax the JSON parser to allow trailing commas. Default is false.
        /// see https://docs.microsoft.com/en-us/dotnet/api/system.text.json.jsondocumentoptions.allowtrailingcommas?view=net-6.0
        /// </summary>
        public bool AllowTrailingCommas { get; init; }
    }
}

#endregion
