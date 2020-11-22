using System;
using JosephGuadagno.Broadcasting.Domain.Interfaces;
using Microsoft.Azure.Cosmos.Table;

namespace JosephGuadagno.Broadcasting.Domain.Models
{
    public class SourceData : TableEntity, ISourceData
    {
        public SourceData() {}
        public SourceData(string sourceSystem): base(sourceSystem, Guid.NewGuid().ToString())
        {
            
        }
        
        public SourceData(string sourceSystem, string id)
        {
            if (string.IsNullOrEmpty(sourceSystem))
            {
                throw new ArgumentNullException(nameof(sourceSystem), "The source system cannot be null or empty.");
            }
            PartitionKey = sourceSystem;
            RowKey = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
        }

        public string SourceSystem => PartitionKey;
        public string Id => RowKey;
        
        /// <summary>
        /// Indicates when the item was added
        /// </summary>
        /// <remarks>The date time is in UTC</remarks>
        public DateTime AddedOn { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        // Text was commented out due to a size limitation of Azure Table Storage
        //     this will be revisited if we determine text is needed.
        //public string Text { get; set; }
        public string Url { get; set; }
        public string ShortenedUrl { get; set; }
        
        /// <summary>
        /// When the item was published at the source
        /// </summary>
        /// <remarks>
        /// The date time is in UTC. If the publication date is not available from the source, the value will be the same as the <see cref="AddedOn"/> property.
        /// </remarks>
        public DateTime PublicationDate { get; set; }
        /// <summary>
        /// When the item was updated at the source
        /// </summary>
        /// <remarks>
        /// The date time is in UTC. If the last modified date is not available from the source, the value will be the same as the <see cref="PublicationDate"/> property.
        /// </remarks>
        public DateTime? UpdatedOnDate { get; set; }
        /// <summary>
        /// Indicates when we should stop sending out social posts on this item
        /// </summary>
        /// <remarks>The date time is in UTC</remarks>
        public DateTime? EndAfter { get; set; }
    }
}