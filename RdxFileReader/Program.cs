using System;
using System.Collections.Generic;
using System.IO;

namespace RdxFileReader
{
    class Program
    {
        static void Main(string[] args)
        {
            var tracks = new List<Track>();
            using (var reader = new BinaryReader(new FileStream(@"C:\Ancon3Data\RDX\MAN17_Summer24hr.rdx", FileMode.Open)))
            {
                reader.ReadBytes(64); //skip header
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    var opnum = reader.ReadInt32();
                    var dateTime = reader.ReadChars(20);
                    var pathName = reader.ReadChars(4);
                    var flightnum = reader.ReadChars(8);
                    var operation = reader.ReadChars(4)[0];
                    var runway = reader.ReadChars(4);
                    var baaType = reader.ReadChars(8);
                    var icaoType = reader.ReadChars(4);
                    var iataType = reader.ReadChars(4);
                    var typeDesc = reader.ReadChars(32);
                    var engineDesc = reader.ReadChars(24);
                    var port = reader.ReadChars(4);
                    var pathExtension = reader.ReadChars(6);
                    reader.ReadBytes(2); //Error in specs!
                    var tailNumber = reader.ReadChars(16);
                    var anconType = reader.ReadChars(16);
                    reader.ReadChars(92); //skip
                    var nTrackPoints = reader.ReadInt32();

                    var track = new Track
                    {
                        AnconType = anconType,
                        BaaType = baaType,
                        DateTime = dateTime,
                        EngineDescription = engineDesc,
                        Flightnum = flightnum,
                        IataType = iataType,
                        IcaoType = icaoType,
                        Operation = operation,
                        Opnum = opnum,
                        PathExtension = pathExtension,
                        Port = port,
                        Runway = runway,
                        TailNumber = tailNumber,
                        TypeDescription = typeDesc
                    };

                    Console.WriteLine($"Track {opnum} ({nTrackPoints} points) {new string(dateTime)} {operation}");
                    var count = 1;
                    var points = new List<TrackPoint>();
                    while (count <= nTrackPoints)
                    {
                        points.Add(new TrackPoint
                        {
                            Xmetres = reader.ReadInt32(),
                            Ymetres = reader.ReadInt32(),
                            HeightMetres = reader.ReadInt32(),
                            SpeedMetresPerSecond = reader.ReadInt16(),
                            TimeSeconds = reader.ReadInt16()
                        });
                        count++;
                    }

                    track.TrackPoints = points;
                    tracks.Add(track);
                }

                Console.WriteLine($"{tracks.Count} tracks found.");
            }
        }
    }
}