namespace DesignReview.Application.Options
{
    /// <summary>
    /// Options for storing uploaded document files.
    /// </summary>
    public class FileStorageOptions
    {
        public const string SectionName = "FileStorage";

        /// <summary>
        /// Base directory path for uploaded files (e.g. full path to "uploads" folder).
        /// </summary>
        public string BasePath { get; set; } = "Uploads";

        /// <summary>
        /// Maximum allowed file size in bytes for PDF uploads (e.g. 52428800 = 50 MB). Zero or null = no limit (subject to server limits).
        /// </summary>
        public long? MaxFileSizeBytes { get; set; }
    }
}
