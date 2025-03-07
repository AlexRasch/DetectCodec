using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectCodec
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            // Vi har även H.264 och H.265 videofiler i projektet för att testa
            // ändra "Samplevideo_h265.mp4" till "Samplevideo_h264.mp4" för att testa H.264
            string testFilePath = Path.Combine(Environment.CurrentDirectory, "Samplevideo_h265.mp4");
            args = new string[] { testFilePath };
#endif

            Console.Title = "DetectCodec";
            if(args.Length == 0)
            {
                Console.WriteLine("Usage: DetectCodec <filename>"); return;
            }

            string filename = args[0].ToLower();
            Console.WriteLine("Detecting codec for file: " + filename);

            if (!filename.EndsWith(".mp4"))
            {
                Console.WriteLine("This program only supports MP4 files.");
                return;
            }

            bool ish265 = IsH265(filename);
            if (!ish265)
                Console.WriteLine("H.265 (HEVC) not detected.");
        }


        public static bool IsH265(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fileStream))
            {
                while (fileStream.Position < fileStream.Length)
                {
                    // Läs box-storlek (4 bytes)
                    uint boxSize = SwapUInt32(reader.ReadUInt32());
                    // Läs box-typ (4 bytes)
                    string boxType = Encoding.ASCII.GetString(reader.ReadBytes(4));

                    Console.WriteLine($"Box: {boxType}, Size: {boxSize}");

                    if (boxType == "moov") // Sök vidare i moov-boxen
                        return ParseMoov(reader, boxSize);
                    else // Hoppa över boxen om den inte är moov
                        fileStream.Seek(boxSize - 8, SeekOrigin.Current);
                }
            }
            return false;
        }

        private static bool ParseMoov(BinaryReader reader, uint moovSize)
        {
            long endPosition = reader.BaseStream.Position + moovSize - 8;
            while (reader.BaseStream.Position < endPosition)
            {
                uint boxSize = SwapUInt32(reader.ReadUInt32());
                string boxType = Encoding.ASCII.GetString(reader.ReadBytes(4));

                if (boxType == "trak")
                    return ParseTrak(reader, boxSize);
                else
                    reader.BaseStream.Seek(boxSize - 8, SeekOrigin.Current);
            }
            return false;
        }

        private static bool ParseTrak(BinaryReader reader, uint trakSize)
        {
            long endPosition = reader.BaseStream.Position + trakSize - 8;
            while (reader.BaseStream.Position < endPosition)
            {
                uint boxSize = SwapUInt32(reader.ReadUInt32());
                string boxType = Encoding.ASCII.GetString(reader.ReadBytes(4));

                if (boxType == "mdia")
                    return ParseMdia(reader, boxSize);
                else
                    reader.BaseStream.Seek(boxSize - 8, SeekOrigin.Current);
            }
            return false;
        }

        private static bool ParseMdia(BinaryReader reader, uint mdiaSize)
        {
            long endPosition = reader.BaseStream.Position + mdiaSize - 8;
            while (reader.BaseStream.Position < endPosition)
            {
                uint boxSize = SwapUInt32(reader.ReadUInt32());
                string boxType = Encoding.ASCII.GetString(reader.ReadBytes(4));

                if (boxType == "minf")
                    return ParseMinf(reader, boxSize);
                else
                    reader.BaseStream.Seek(boxSize - 8, SeekOrigin.Current);
            }
            return false;
        }

        private static bool ParseMinf(BinaryReader reader, uint minfSize)
        {
            long endPosition = reader.BaseStream.Position + minfSize - 8;
            while (reader.BaseStream.Position < endPosition)
            {
                uint boxSize = SwapUInt32(reader.ReadUInt32());
                string boxType = Encoding.ASCII.GetString(reader.ReadBytes(4));

                if (boxType == "stbl")
                    return ParseStbl(reader, boxSize);
                else
                    reader.BaseStream.Seek(boxSize - 8, SeekOrigin.Current);
            }
            return false;
        }

        private static bool ParseStbl(BinaryReader reader, uint stblSize)
        {
            long endPosition = reader.BaseStream.Position + stblSize - 8;
            while (reader.BaseStream.Position < endPosition)
            {
                uint boxSize = SwapUInt32(reader.ReadUInt32());
                string boxType = Encoding.ASCII.GetString(reader.ReadBytes(4));

                if (boxType == "stsd")
                    return CheckCodec(reader, boxSize);
                else
                    reader.BaseStream.Seek(boxSize - 8, SeekOrigin.Current);
            }
            return false;
        }

        private static bool CheckCodec(BinaryReader reader, uint stsdSize)
        {
            // Hoppa över version och flags (4 bytes)
            reader.ReadBytes(4);
            // Antal entries (4 bytes)
            uint entryCount = SwapUInt32(reader.ReadUInt32());

            for (uint i = 0; i < entryCount; i++)
            {
                uint sampleSize = SwapUInt32(reader.ReadUInt32());
                string sampleType = Encoding.ASCII.GetString(reader.ReadBytes(4));

                if (sampleType == "hvc1" || sampleType == "hev1")
                {
                    Console.WriteLine("H.265 (HEVC) detected!");
                    return true;
                }
                else // Hoppa över resten av sample entry
                    reader.BaseStream.Seek(sampleSize - 8, SeekOrigin.Current);
                
            }
            return false;
        }

        // Hjälpfunktion för att hantera big-endian till little-endian
        private static uint SwapUInt32(uint value)
        {
            return ((value & 0x000000FF) << 24) |
                   ((value & 0x0000FF00) << 8) |
                   ((value & 0x00FF0000) >> 8) |
                   ((value & 0xFF000000) >> 24);
        }
    }
}
