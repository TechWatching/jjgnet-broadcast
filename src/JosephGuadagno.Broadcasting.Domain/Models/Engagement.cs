using System;
using System.Collections.Generic;
using JosephGuadagno.Broadcasting.Domain.Interfaces;
using Microsoft.Azure.Cosmos.Table;

namespace JosephGuadagno.Broadcasting.Domain.Models
{
    /// <summary>
    /// A speaking engagement
    /// </summary>
    /// <remarks>This can be a conference or webinar or event that holds one or more <see cref="Talk"/></remarks>s
    public class Engagement: TableEntity, IEngagement
    {
        public Engagement()
        {
            Talks = new List<Talk>();
            PartitionKey = "Engagement";
            RowKey = Guid.NewGuid().ToString();
        }

        public Engagement(string id)
        {
            Talks = new List<Talk>();
            PartitionKey = "Engagement";
            RowKey = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
        }
        
        /// <summary>
        /// The Id of the item
        /// </summary>
        public string Id => RowKey;

        /// <summary>
        /// The name of the engagement
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The Url for the engagement
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// The date and time the engagement starts
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// The date and time the engagement ends
        /// </summary>
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// Comments for the engagement
        /// </summary>
        /// <remarks>Could be a discount code for the engagement</remarks>
        public string Comments { get; set; }

        /// <summary>
        /// A list of all of the talks that are being delivered
        /// </summary>
        public List<Talk> Talks { get; set; }
    }
}