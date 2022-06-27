using JoeScan.LogScanner.Core.Geometry;

namespace JoeScan.LogScanner.Core.Models
{
    public static class ProfileReaderWriter
    {
         const int FileMagicV1 = 0x010203;
         const int FileMagicV2 = 0x010204;

         private const int LatestVersion = FileMagicV2;

        public static void Write(this Profile p, BinaryWriter bw)
        {
            bw.Write(LatestVersion);
            bw.Write((byte)p.Units);
            bw.Write((int)p.ScanningFlags);
            bw.Write(p.LaserIndex);
            bw.Write(p.LaserOnTimeUs);
            bw.Write(p.EncoderValues.Keys.Count);
            for (uint i = 0; i < p.EncoderValues.Keys.Count; i++)
            {
                bw.Write(p.EncoderValues[i]);
            }
            bw.Write(p.SequenceNumber);
            bw.Write(p.TimeStampNs);
            bw.Write(p.ScanHeadId);
            bw.Write(p.Camera);
            bw.Write((int)p.Inputs);
            bw.Write(p.Data.Length);
            foreach (var t in p.Data)
            {
                // DEPRECATED: up to v1 for profiles in inches: to save disk space, save as 1/100 of an inch 
                // this lets us map an area of -327" to 327" 
                // which is sufficient in most cases. Our accuracy is 
                // limited to 1/100 of an inch, or 0.25 mm - good enough for debugging etc

                // NEW: as of v2 we use floats. They use 4 bytes and work for both mm and inches
                bw.Write((float)t.X);
                bw.Write((float)t.Y);

                // scale brightness to 8 bit
                bw.Write((byte)t.B);
            }
        }

        public static Profile? Read(BinaryReader br)
        {
            var version = br.ReadInt32();
            switch (version)
            {
                case FileMagicV1:
                    return Read_v1(br);
                case FileMagicV2:
                    return Read_v2(br);
                default:
                    return null;
            }
        }

        public static Profile? Read_v1(BinaryReader br)
        {
            var p = new Profile();
            // this version assumes all is in inches
            p.ScanningFlags = (ScanFlags)br.ReadInt32();
            p.LaserIndex = br.ReadUInt32();
            p.LaserOnTimeUs = br.ReadUInt16();
            var numEncoderVals = br.ReadInt32();
            for (uint i = 0; i < numEncoderVals; i++)
            {
                p.EncoderValues[i] = br.ReadInt64();
            }
            p.SequenceNumber = br.ReadUInt32();
            p.TimeStampNs = br.ReadUInt64();
            p.ScanHeadId = br.ReadUInt32();
            p.Camera = br.ReadUInt32();
            p.Inputs = (InputFlags)br.ReadInt32();
            var numPts = br.ReadInt32();
            p.Data = new Point2D[numPts];
            for (int i = 0; i < numPts; i++)
            {
                // see scaling remarks above
                var x = br.ReadInt16() / 100.0;
                var y = br.ReadInt16() / 100.0;
                var b = (double)br.ReadByte();
                p.Data[i] = new Point2D(x, y, b);
            }
            return p;
        }

        public static Profile? Read_v2(BinaryReader br)
        {
          // ReSharper disable once UseObjectOrCollectionInitializer
            var p = new Profile(){Units = (UnitSystem)br.ReadByte() };
            p.ScanningFlags = (ScanFlags)br.ReadInt32();
            p.LaserIndex = br.ReadUInt32();
            p.LaserOnTimeUs = br.ReadUInt16();
            var numEncoderVals = br.ReadInt32();
            for (uint i = 0; i < numEncoderVals; i++)
            {
                p.EncoderValues[i] = br.ReadInt64();
            }
            p.SequenceNumber = br.ReadUInt32();
            p.TimeStampNs = br.ReadUInt64();
            p.ScanHeadId = br.ReadUInt32();
            p.Camera = br.ReadUInt32();
            p.Inputs = (InputFlags)br.ReadInt32();
            var numPts = br.ReadInt32();
            p.Data = new Point2D[numPts];
            for (int i = 0; i < numPts; i++)
            {
                // see scaling remarks above
                var x = br.ReadSingle();
                var y = br.ReadSingle();
                var b = (double)br.ReadByte();
                p.Data[i] = new Point2D(x, y, b);
            }
            return p;
        }
    }
}
