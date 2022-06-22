using JoeScan.LogScanner.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoeScan.LogScanner.Core.Models
{
    public static class ProfileReaderWriter
    {
         const int FileMagic = 0x010203;

        public static void Write(this Profile p, BinaryWriter bw)
        {
            bw.Write(FileMagic);
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
                // to save disk space, save as 1/100 of an inch 
                // this lets us map an area of -327" to 327" (-8323mm to 8323mm)
                // which is sufficient in most cases. Our accuracy is 
                // limited to 1/100 of an inch, or 0.25 mm - good enough for debugging etc
                bw.Write((short)(t.X*100.0));
                bw.Write((short)(t.Y*100.0));
                // scale brightness to 8 bit
                bw.Write((byte)t.B);
            }
        }

        public static Profile? Read(BinaryReader br)
        {
            if (br.ReadInt32() != FileMagic)
            {
                return null;
            }
            var p = new Profile();
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
                var b = (double) br.ReadByte();
                p.Data[i] = new Point2D(x, y, b);
            }
            return p;
        }
         
    }
}
