using JoeScan.LogScanner.Core.Geometry;

namespace JoeScan.LogScanner.Core.Models
{
    public static class ProfileReaderWriter
    {
        const int FileMagicV1 = 0x010203;
        const int FileMagicV2 = 0x010204;
        const int FileMagicV3 = 0x010205;

        private const int LatestVersion = FileMagicV3;

        public static void Write(this Profile p, BinaryWriter bw)
        {
            bw.Write(LatestVersion);
            bw.Write((byte)p.Units);
            bw.Write((int)p.ScanningFlags);
            bw.Write(p.LaserIndex);
            bw.Write(p.LaserOnTimeUs);
            bw.Write(p.Encoder);
            bw.Write(p.SequenceNumber);
            bw.Write(p.TimeStampNs);
            bw.Write(p.ScanHeadId);
            bw.Write(p.CameraIndex);
            bw.Write((int)p.Inputs);
            bw.Write(p.NumPoints);
            foreach (var pt in p.GetAllPoints())
            {
                // NEW: as of v2 we use floats. They use 4 bytes and work for both mm and inches
                bw.Write(pt.X);
                bw.Write(pt.Y);
                bw.Write(pt.B);
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
                case FileMagicV3:
                    return Read_v3(br);
                default:
                    return null;
            }
        }

        public static Profile? Read_v1(BinaryReader br)
        {
            // this version assumes all is in inches
            var scanningFlags = (ScanFlags)br.ReadInt32();
            var laserIndex = br.ReadUInt32();
            var laserOnTimeUs = br.ReadUInt16();
            var numEncoderVals = br.ReadInt32();
            long encoderVal = 0;
            if (numEncoderVals > 0)
            {
                encoderVal = br.ReadInt64();
            }

            var sequenceNumber = br.ReadUInt32();
            var timeStampNs = br.ReadUInt64();
            var scanHeadId = br.ReadUInt32();
            var cameraIndex = br.ReadUInt32();
            var inputs = (InputFlags)br.ReadInt32();
            var numPts = br.ReadInt32();
            var data = new Point2D[numPts];
            for (int i = 0; i < numPts; i++)
            {
                // see scaling remarks above
                var x = br.ReadInt16() / 100.0;
                var y = br.ReadInt16() / 100.0;
                var b = br.ReadByte();
                data[i] = new Point2D((float)x, (float)y, b);
            }

            return Profile.Build(UnitSystem.Inches, scanHeadId, laserIndex, cameraIndex, laserOnTimeUs,
                encoderVal, sequenceNumber, timeStampNs, scanningFlags, inputs, data);
        }

        public static Profile? Read_v2(BinaryReader br)
        {
            var units = (UnitSystem)br.ReadByte();
            var scanningFlags = (ScanFlags)br.ReadInt32();
            var laserIndex = br.ReadUInt32();
            var laserOnTimeUs = br.ReadUInt16();
            var numEncoderVals = br.ReadInt32();
            long encoderVal = 0;
            if (numEncoderVals > 0)
            {
                encoderVal = br.ReadInt64();
            }

            var sequenceNumber = br.ReadUInt32();
            var timeStampNs = br.ReadUInt64();
            var scanHeadId = br.ReadUInt32();
            var cameraIndex = br.ReadUInt32();
            var inputs = (InputFlags)br.ReadInt32();
            var numPts = br.ReadInt32();
            var data = new Point2D[numPts];
            for (int i = 0; i < numPts; i++)
            {
                // see scaling remarks above
                var x = br.ReadSingle();
                var y = br.ReadSingle();
                var b = br.ReadByte();
                data[i] = new Point2D(x, y, b);
            }

            return Profile.Build(units, scanHeadId, laserIndex, cameraIndex, laserOnTimeUs,
                encoderVal, sequenceNumber, timeStampNs, scanningFlags, inputs, data);
        }

        public static Profile? Read_v3(BinaryReader br)
        {
            // only difference to v2 is that we have one encoder value, defaulting to zero
            var units = (UnitSystem)br.ReadByte();
            var scanningFlags = (ScanFlags)br.ReadInt32();
            var laserIndex = br.ReadUInt32();
            var laserOnTimeUs = br.ReadUInt16();
            var encoderVal = br.ReadInt64();

            var sequenceNumber = br.ReadUInt32();
            var timeStampNs = br.ReadUInt64();
            var scanHeadId = br.ReadUInt32();
            var cameraIndex = br.ReadUInt32();
            var inputs = (InputFlags)br.ReadInt32();
            var numPts = br.ReadInt32();
            var data = new Point2D[numPts];
            for (int i = 0; i < numPts; i++)
            {
                // see scaling remarks above
                var x = br.ReadSingle();
                var y = br.ReadSingle();
                var b = br.ReadByte();
                data[i] = new Point2D(x, y, b);
            }

            return Profile.Build(units, scanHeadId, laserIndex, cameraIndex, laserOnTimeUs,
                encoderVal, sequenceNumber, timeStampNs, scanningFlags, inputs, data);
        }
    }
}
