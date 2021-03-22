namespace RdxFileReader
{
    public class TrackPoint
    {
        /// <summary>
        ///     X coordinates (from known reference. OSGB coordinates preferred) 
        /// </summary>
        public int Xmetres { get; set; }

        /// <summary>
        ///     Y coordinates (from known reference. OSGB coordinates preferred) 
        /// </summary>
        public int Ymetres { get; set; }

        /// <summary>
        ///     Height in meters above air field elevation
        /// </summary>
        public int HeightMetres { get; set; }

        /// <summary>
        ///     Speed in metres/sec
        /// </summary>
        public short SpeedMetresPerSecond { get; set; }

        /// <summary>
        ///     Time in seconds
        /// </summary>
        public short TimeSeconds { get; set; }
    }
}