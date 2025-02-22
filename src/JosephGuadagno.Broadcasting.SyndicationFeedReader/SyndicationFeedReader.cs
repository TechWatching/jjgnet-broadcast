﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using JosephGuadagno.Broadcasting.Domain;
using JosephGuadagno.Broadcasting.Domain.Models;
using JosephGuadagno.Broadcasting.SyndicationFeedReader.Interfaces;
using Microsoft.Extensions.Logging;

namespace JosephGuadagno.Broadcasting.SyndicationFeedReader
{
    public class SyndicationFeedReader: ISyndicationFeedReader
    {
        private readonly ISyndicationFeedReaderSettings _syndicationFeedReaderSettings;
        private readonly ILogger _logger;
        
        public SyndicationFeedReader(ISyndicationFeedReaderSettings syndicationFeedReaderSettings, ILogger<SyndicationFeedReader> logger)
        {
            if (syndicationFeedReaderSettings == null)
            {
                throw new ArgumentNullException(nameof(syndicationFeedReaderSettings), "The SyndicationFeedReaderSettings cannot be null");
            }
            
            if (string.IsNullOrEmpty(syndicationFeedReaderSettings.FeedUrl))
            {
                throw new ArgumentNullException(nameof(syndicationFeedReaderSettings.FeedUrl), "The FeedUrl of the SyndicationFeedReaderSettings is required");
            }

            _syndicationFeedReaderSettings = syndicationFeedReaderSettings;
            _logger = logger;
        }

        public List<SourceData> Get(DateTime sinceWhen)
        {
            var currentTime = DateTime.UtcNow;
            var feedItems = new List<SourceData>();

            _logger.LogDebug("Checking syndication feed '{_syndicationFeedReaderSettings.FeedUrl}' for new posts since '{sinceWhen:u}'",
                _syndicationFeedReaderSettings.FeedUrl, sinceWhen);

            List<SyndicationItem> items = null;

            try
            {
                using var reader = XmlReader.Create(_syndicationFeedReaderSettings.FeedUrl);
                var feed = SyndicationFeed.Load(reader);

                items = feed.Items.Where(i => (i.PublishDate > sinceWhen) || (i.LastUpdatedTime > sinceWhen))
                    .ToList();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error parsing the syndication feed for: {_syndicationFeedReaderSettings.FeedUrl}",
                    _syndicationFeedReaderSettings.FeedUrl);
                throw;
            }
            
            _logger.LogDebug($"Found {items.Count} posts");

            foreach (var syndicationItem in items)
            {
                feedItems.Add(new SourceData(SourceSystems.SyndicationFeed)
                {
                    Author = syndicationItem.Authors.FirstOrDefault()?.Name,
                    PublicationDate = syndicationItem.PublishDate.UtcDateTime, 
                    UpdatedOnDate = syndicationItem.LastUpdatedTime.UtcDateTime,
                    //Text = ((TextSyndicationContent) syndicationItem.Content).Text,
                    Title =  syndicationItem.Title.Text,
                    Url = syndicationItem.Id,
                    EndAfter = null,
                    AddedOn = currentTime,
                    Tags = syndicationItem.Categories is null ? null : string.Join(",", syndicationItem.Categories.Select(c => c.Name))
                });
            }
            return feedItems;
        }

        public async Task<List<SourceData>> GetAsync(DateTime sinceWhen)
        {
            return await Task.Run(() => Get(sinceWhen));
        }

        public List<SyndicationItem> GetSyndicationItems(DateTime sinceWhen, List<string> excludeCategories)
        {
            _logger.LogDebug($"Checking syndication feed '{_syndicationFeedReaderSettings.FeedUrl}' for posts since '{sinceWhen:u}'",
                _syndicationFeedReaderSettings, sinceWhen);

            List<SyndicationItem> items = new List<SyndicationItem>();

            try
            {
                using var reader = XmlReader.Create(_syndicationFeedReaderSettings.FeedUrl);
                var feed = SyndicationFeed.Load(reader);

                var recentItems = feed.Items.Where(i => (i.PublishDate > sinceWhen) || (i.LastUpdatedTime > sinceWhen));
                items.AddRange(from item in recentItems
                    let found = item.Categories.Any(itemCategory =>
                        excludeCategories.Contains(itemCategory.Name.ToLower().Trim()))
                    where !found
                    select item);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error parsing the syndication feed for: {_syndicationFeedReaderSettings.FeedUrl}",
                    _syndicationFeedReaderSettings);
                throw;
            }
            
            _logger.LogDebug($"Found {items.Count} posts");

            return items;
        }

        public SyndicationItem GetRandomSyndicationItem(DateTime sinceWhen, List<string> excludeCategories)
        {
            _logger.LogDebug(
                $"Getting a random syndication item from feed '{_syndicationFeedReaderSettings.FeedUrl}' for posts since '{sinceWhen:u}",
                _syndicationFeedReaderSettings, sinceWhen);

            var syndicationItems = GetSyndicationItems(sinceWhen, excludeCategories);
            
            // Pick a Random one
            var randomPost = syndicationItems
                .OrderBy(p => Guid.NewGuid())
                .FirstOrDefault();

            if (randomPost == null)
            {
                _logger.LogWarning(
                    $"Could not get a random posts from feed '{_syndicationFeedReaderSettings.FeedUrl}' for posts since '{sinceWhen:u}",
                    _syndicationFeedReaderSettings, sinceWhen);
                Console.WriteLine("Could not get a post. Exiting");
                return null;
            }

            return randomPost;
        }
    }
}