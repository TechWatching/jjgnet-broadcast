using System.Text.Json;
using System.Threading.Tasks;
using JosephGuadagno.Broadcasting.Data;
using JosephGuadagno.Broadcasting.Domain;
using JosephGuadagno.Broadcasting.Domain.Models;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace JosephGuadagno.Broadcasting.Functions.Twitter
{
    public class ProcessNewSourceData
    {
        private readonly SourceDataRepository _sourceDataRepository;

        public ProcessNewSourceData(SourceDataRepository sourceDataRepository)
        {
            _sourceDataRepository = sourceDataRepository;
        }
        
        // Debug Locally: https://docs.microsoft.com/en-us/azure/azure-functions/functions-debug-event-grid-trigger-local
        // Sample Code: https://github.com/Azure-Samples/event-grid-dotnet-publish-consume-events
        [FunctionName("twitter_process_new_source_data")]
        public async Task RunAsync(
            [EventGridTrigger()] EventGridEvent eventGridEvent,
            [Queue(Constants.Queues.TwitterTweetsToSend)] ICollector<string> outboundMessages,
            ILogger log
        )
        {
            // Get the Source Data identifier for the event
            
            var tableEvent = JsonSerializer.Deserialize<TableEvent>(eventGridEvent.Data.ToString());
            if (tableEvent == null)
            {
                log.LogInformation($"Failed to parse the TableEvent data for event '{eventGridEvent.Id}'");
                return;
            }

            // Create the scheduled tweets for it
            log.LogInformation($"Looking for source with fields '{tableEvent.PartitionKey}' and '{tableEvent.RowKey}'");
            var sourceData = await _sourceDataRepository.GetAsync(tableEvent.PartitionKey, tableEvent.RowKey);
            log.LogInformation($"Record for '{tableEvent.PartitionKey}', '{tableEvent.RowKey}' was {sourceData!=null}.");
            var tweet = ComposeTweet(sourceData);
            if (!string.IsNullOrEmpty(tweet))
            {
                outboundMessages.Add(tweet);
            }
            
            // Done
            // log?
        }
        
        private string ComposeTweet(SourceData item)
        {
            if (item == null)
            {
                return null;
            }

            const int maxTweetLenght = 240;
            
            // Build Tweet
            var tweetStart = "New Blog Post: ";
            var url = item.ShortenedUrl ?? item.Url;
            var postTitle = item.Title;
        
            if (tweetStart.Length + url.Length + postTitle.Length + 3 >= maxTweetLenght)
            {
                var newLength = maxTweetLenght - tweetStart.Length - url.Length - 1;
                postTitle = postTitle.Substring(0, newLength - 4) + "...";
            }
            
            var tweet = $"{tweetStart} {postTitle} {url}";
        
            return tweet;
        }
    }
}