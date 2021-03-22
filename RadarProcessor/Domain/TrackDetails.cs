using System;
using RadarProcessor.Enums;

namespace RadarProcessor.Domain
{
    public class TrackDetails
    {
        public long Id { get; set; }

        public int Opnum { get; set; }

        public DateTime? DateTime { get; set; }

        public string FlightNum { get; set; }

        public OperationType OperationType { get; set; }

        public string Runway { get; set; }

        public string BaaType { get; set; }

        public string IcaoType { get; set; }

        public string IataType { get; set; }

        public string TypeDescription { get; set; }

        public string EngineDescription { get; set; }

        public string Port { get; set; }

        public string PathExtension { get; set; }

        public string TailNumber { get; set; }

        public string AnconType { get; set; }

        public int NumberOfTrackPoints { get; set; }
    }
}