using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Navigation;
using RadarProcessor.Domain;
using RadarProcessor.Enums;
using RadarProcessor.Extensions;

namespace RadarProcessor.Models
{
    public class RdxFileReader : ObservableBase
    {
        private readonly List<TrackDetails> tracksDetails = new List<TrackDetails>();

        private string status = string.Empty;
        private DateTime fromDateTime;
        private DateTime toDateTime;
        private string rdxFilePath;

        public DateTime FromDateTime
        {
            get => this.fromDateTime;
            set => this.SetProperty(ref this.fromDateTime, value);
        }

        public DateTime ToDateTime
        {
            get => this.toDateTime;
            set => this.SetProperty(ref this.toDateTime, value);
        }

        public string RdxFilePath
        {
            get => this.rdxFilePath;
            set => this.SetProperty(ref this.rdxFilePath, value);
        }

        public string Status
        {
            get => this.status;
            private set => this.SetProperty(ref this.status, value);
        }

        public List<string> BaaTypes { get; set; }

        public List<string> IcaoTypes { get; set; }
        
        public List<string> IataTypes { get; set; }

        public string[] AnconTypes { get; set; }

        //IAnconTypeMappingProvider

        /// <summary>
        ///     Allowed operation types (By default all)
        /// </summary>
        public OperationType[] OperationTypes { get; set; } =
            {OperationType.Overflight, OperationType.Arrival, OperationType.Departure};

        /// <summary>
        ///     This gets metadata from the RDX (i.e. number of tracks, opnums etc.)
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<int> Initialise(CancellationToken token)
        {
            try
            {
                this.FromDateTime = DateTime.MaxValue;
                this.ToDateTime = DateTime.MinValue;
                return await Task.Run(() => 
                    {
                        var count = 1;
                        tracksDetails.Clear();
                        using (var reader = new BinaryReader(new FileStream(rdxFilePath, FileMode.Open)))
                        {
                            reader.ReadBytes(64); //skip header
                            while (reader.BaseStream.Position != reader.BaseStream.Length)
                            {

                                var id = reader.BaseStream.Position;
                                var opnums = reader.ReadInt32();
                                var dateTimeChars = reader.ReadChars(20);
                                var pathName = new string(reader.ReadChars(4));
                                var flightnum = new string(reader.ReadChars(8));
                                var operation = reader.ReadChars(4)[0];
                                var runway = new string(reader.ReadChars(4));
                                var baaType = new string(reader.ReadChars(8));
                                var icaoType = new string(reader.ReadChars(4));
                                var iataType = new string(reader.ReadChars(4));
                                var typeDesc = new string(reader.ReadChars(32));
                                var engineDesc = new string(reader.ReadChars(24));
                                var port = new string(reader.ReadChars(4));
                                var pathExtension = new string(reader.ReadChars(6));
                                reader.ReadBytes(2); //Error in specs!
                                var tailNumber = new string(reader.ReadChars(16));
                                var anconType = new string(reader.ReadChars(16));
                                reader.ReadChars(92); //skip
                                var nTrackPoints = reader.ReadInt32();

                                var dateTime = dateTimeChars.ToDateTime();
                                if (dateTime.HasValue)
                                {
                                    if (dateTime < this.FromDateTime)
                                    {
                                        this.FromDateTime = dateTime.Value;
                                    }

                                    if (dateTime > this.ToDateTime)
                                    {
                                        this.ToDateTime = dateTime.Value;
                                    }
                                }

                                tracksDetails.Add(new TrackDetails
                                {
                                    Id = id,
                                    Opnum = opnums,
                                    DateTime = dateTime,
                                    FlightNum = flightnum,
                                    OperationType = operation.ToOperationType(),
                                    Runway = runway,
                                    BaaType = baaType,
                                    IcaoType = icaoType,
                                    IataType = iataType,
                                    TypeDescription = typeDesc,
                                    EngineDescription = engineDesc,
                                    Port = port,
                                    PathExtension = pathExtension,
                                    TailNumber = tailNumber,
                                    AnconType = anconType,
                                    NumberOfTrackPoints = nTrackPoints
                                });

                                count++;
                                var currentPosition = reader.BaseStream.Position;
                                //Bypass track points
                                reader.BaseStream.Position = currentPosition + nTrackPoints * 16;
                            }

                            this.Status = $"{count} tracks found.";

                            return 0;
                        }
                    }, token);
            }
            catch (Exception e)
            {
                this.Status = e.Message;
                return -1;
            }
        }

        public async Task<List<Track>> ReadAsync(CancellationToken token)
        {
            try
            {
                return await Task.Run(() =>
                {
                    var tracks = new List<Track>();
                    using (var reader = new BinaryReader(new FileStream(this.RdxFilePath, FileMode.Open)))
                    {
                        reader.ReadBytes(64); //skip header
                        while (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            token.ThrowIfCancellationRequested();

                            var opnum = reader.ReadInt32();
                            var dateTimeChars = reader.ReadChars(20);
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

                            var dateTime = dateTimeChars.ToDateTime();
                            var operationType = operation.ToOperationType();
                            if (dateTime == null || 
                                dateTime <= this.FromDateTime || dateTime >= this.ToDateTime || 
                                !this.OperationTypes.Contains(operationType))
                            {
                                var currentPosition = reader.BaseStream.Position;
                                reader.BaseStream.Position = currentPosition + nTrackPoints * 16;
                                continue;
                            }
                            
                            var track = new Track
                            {
                                AnconType = anconType,
                                BaaType = baaType,
                                DateTime = dateTimeChars,
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

                            this.Status = $"Track {opnum} ({nTrackPoints} points) {new string(dateTimeChars)} {operation}";
                            var count = 1;
                            var points = new List<TrackPoint>();
                            while (count <= nTrackPoints)
                            {
                                token.ThrowIfCancellationRequested();

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

                        this.Status = $"{tracks.Count} tracks found.";
                        return tracks;
                    }
                }
                , token);
            }
            catch (Exception e)
            {
                this.Status = e.Message;
                return null;
            }
        }

        public async Task<List<Track>> GetAsync(CancellationToken token, Func<TrackDetails, bool> predicate = null)
        {
            //TODO: validation
            using (var reader = new BinaryReader(new FileStream(this.rdxFilePath, FileMode.Open)))
            {
                var tracks = new List<Track>();
                foreach (var trackDetail in this.tracksDetails.Where(predicate ?? (t => true)))
                {
                    token.ThrowIfCancellationRequested();
                    reader.BaseStream.Position = trackDetail.Id + 256;
                    var count = 1;
                    var points = new List<TrackPoint>();
                    while (count <= trackDetail.NumberOfTrackPoints)
                    {
                        token.ThrowIfCancellationRequested();

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

                    tracks.Add(new Track
                    {
                        TrackDetails = trackDetail,
                        TrackPoints = points
                    });
                }

                return tracks;
            }
        }
    }
}