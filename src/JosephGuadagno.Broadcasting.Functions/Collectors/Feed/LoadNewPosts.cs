using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JosephGuadagno.Broadcasting.Data.Repositories;
using JosephGuadagno.Broadcasting.Domain;
using JosephGuadagno.Broadcasting.Domain.Interfaces;
using JosephGuadagno.Broadcasting.Domain.Models;
using JosephGuadagno.Broadcasting.SyndicationFeedReader.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace JosephGuadagno.Broadcasting.Functions.Collectors.Feed
{
    public class CheckFeedForUpdates
    {
        private readonly ISyndicationFeedReader _syndicationFeedReader;
        private readonly ISettings _settings;
        private readonly ConfigurationRepository _configurationRepository;
        private readonly SourceDataRepository _sourceDataRepository;
        private readonly IUrlShortener _urlShortener;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<CheckFeedForUpdates> _logger;

        public CheckFeedForUpdates(ISyndicationFeedReader syndicationFeedReader,
            ISettings settings,
            ConfigurationRepository configurationRepository,
            SourceDataRepository sourceDataRepository,
            IUrlShortener urlShortener,
            IEventPublisher eventPublisher, ILogger<CheckFeedForUpdates> logger)
        {
            _syndicationFeedReader = syndicationFeedReader;
            _settings = settings;
            _configurationRepository = configurationRepository;
            _sourceDataRepository = sourceDataRepository;
            _urlShortener = urlShortener;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }
        
        [FunctionName("collectors_feed_check_for_updates")]
        public async Task RunAsync(
            [TimerTrigger("0 */2 * * * *")] TimerInfo myTimer)
        {
            var startedAt = DateTime.UtcNow;
            _logger.LogDebug($"{Constants.ConfigurationFunctionNames.CollectorsFeedLoadNewPosts} Collector started at: {startedAt}");

            var configuration = await _configurationRepository.GetAsync(Constants.Tables.Configuration,
                                    Constants.ConfigurationFunctionNames.CollectorsFeedLoadNewPosts
                                ) ??
                                new CollectorConfiguration(Constants.ConfigurationFunctionNames
                                        .CollectorsFeedLoadNewPosts)
                                    {LastCheckedFeed = startedAt, LastItemAddedOrUpdated = DateTime.MinValue};
            
            // Check for new items
            _logger.LogDebug($"Checking the syndication feed for posts since '{configuration.LastItemAddedOrUpdated}'");
            var newItems = await _syndicationFeedReader.GetAsync(configuration.LastItemAddedOrUpdated);
            
            // If there is nothing new, save the last checked value and exit
            if (newItems == null || newItems.Count == 0)
            {
                configuration.LastCheckedFeed = startedAt;
                await _configurationRepository.SaveAsync(configuration);
                _logger.LogDebug($"No new or updated posts found in the syndication feed.");
                return;
            }
            
            // Save the new items to SourceDataRepository
            // TODO: Handle duplicate posts?
            // GitHub Issue #5
            var savedCount = 0;
            var eventsToPublish = new List<SourceData>();
            foreach (var item in newItems)
            {
                // shorten the url
                item.ShortenedUrl = await _urlShortener.GetShortenedUrlAsync(item.Url, _settings.BitlyShortenedDomain);
                
                // attempt to save the item
                try
                {
                    var wasSaved = await _sourceDataRepository.SaveAsync(item);
                    if (wasSaved)
                    {
                        eventsToPublish.Add(item);
                        savedCount++;
                    }
                    else
                    {
                        _logger.LogError($"Failed to save the blog post of Id: '{item.Id}' Url:'{item.Url}'");
                    }
                    
                }
                catch (Exception e)
                {
                    _logger.LogError($"Was not able to save post with the id of '{item.Id}'. Exception: {e.Message}");
                }
            }
            
            // Publish the events
            
            var eventsPublished = await _eventPublisher.PublishEventsAsync(_settings.TopicNewSourceDataEndpoint, _settings.TopicNewSourceDataKey,
                Constants.ConfigurationFunctionNames.CollectorsFeedLoadNewPosts, eventsToPublish);
            if (!eventsPublished)
            {
                _logger.LogError($"Failed to publish the events.");
            }
            
            // Save the last checked value
            configuration.LastCheckedFeed = startedAt;
            var latestAdded = newItems.Max(item => item.PublicationDate);
            var latestUpdated = newItems.Max(item => item.UpdatedOnDate);
            configuration.LastItemAddedOrUpdated = latestUpdated > latestAdded ? latestUpdated.Value : latestAdded;

            await _configurationRepository.SaveAsync(configuration);
            
            // Return
            var doneMessage = $"Loaded {savedCount} of {newItems.Count} post(s).";
            _logger.LogDebug(doneMessage);
        }
    }
}