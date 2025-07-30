using System.Collections.Generic;
using NCBI.Datasets.API.Models.Common;

namespace NCBI.Datasets.API.Models.Responses
{
    /// <summary>
    /// Response model for prokaryote gene download operations
    /// Note: This service returns binary data (ZIP files), so no specific response model is needed
    /// The actual response is handled as byte[] in the service methods
    /// </summary>
    public class ProkaryoteGeneDownloadResponse
    {
        /// <summary>
        /// Binary ZIP file data
        /// </summary>
        public byte[]? Data { get; set; }

        /// <summary>
        /// Content type of the response
        /// </summary>
        public string ContentType { get; set; } = "application/zip";

        /// <summary>
        /// Filename for the download
        /// </summary>
        public string Filename { get; set; } = "ncbi_dataset.zip";
    }
}
