using System;

namespace JosephGuadagno.Broadcasting.Domain.Interfaces
{
    /// <summary>
    /// A talk that is given at an <see cref="IEngagement"/>
    /// </summary>
    public interface ITalk
    {
        /// <summary>
        /// The name of the talk
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// A Url for the talk on the conference website
        /// </summary>
        public string UrlForConferenceTalk { get; set; }
        
        /// <summary>
        /// The Url for the talk on the talk deliverers website
        /// </summary>
        public string UrlForTalk { get; set; }
        
        /// <summary>
        /// The start date and time of the talk
        /// </summary>
        public DateTime StartDateTime { get; set; }
        
        /// <summary>
        /// The end date and time of the talk
        /// </summary>
        public DateTime EndDateTime { get; set; }
        
        /// <summary>
        /// The room/channel/url for the talk
        /// </summary>
        public string TalkLocation { get; set; }
        
        /// <summary>
        /// Comments for the talk
        /// </summary>
        public string Comments { get; set; }
    }
}