namespace RediveExtract
{
    public record Config
    {
        /// <summary>
        /// Real version number. It's may internally called "resource version". Different versions have different
        /// version format, and updating of version may not continuous.
        /// </summary>
        public int TruthVersion { get; set; } = 14016;
        
        /// <summary>
        /// Operating system. Commonly "Android" or "iOS".
        /// </summary>
        public string OS { get; set; } = "Android";
        
        /// <summary>
        /// Locale of the game. However, in So-net version, it's still "Jpn". Why? 
        /// </summary>
        public string Locale { get; set; } = "Jpn";
        
        /// <summary>
        /// Version number.
        /// </summary>
        public int[] Version { get; set; } = { 2, 3, 0 };
    }
}
