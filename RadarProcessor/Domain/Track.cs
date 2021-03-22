using System.Collections.Generic;

namespace RadarProcessor.Domain
{
    public class Track
    {
        public int Opnum { get; set; }

        /// <summary>
        ///     Date and time of 1st track point
        /// </summary>
        public char[] DateTime { get; set; }

        /// <summary>
        ///     Parhname (inferred SID)
        /// </summary>
        public char[] Pathname { get; set; }

        public char[] Flightnum { get; set; }

        /// <summary>
        ///     Operation D/A/O
        /// </summary>
        public char Operation { get; set; }

        public char[] Runway { get; set; }

        public char[] BaaType { get; set; }

        public char[] IcaoType { get; set; }

        public char[] IataType { get; set; }

        public char[] TypeDescription { get; set; }

        public char[] EngineDescription { get; set; }

        /// <summary>
        ///     Port (destination/origin)s
        /// </summary>
        public char[] Port { get; set; }

        public char[] PathExtension { get; set; }

        public char[] TailNumber { get; set; }

        public char[] AnconType { get; set; }

        public TrackDetails TrackDetails { get; set; }

        public List<TrackPoint> TrackPoints { get; set; }
    }
}