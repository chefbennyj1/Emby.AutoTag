using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Tasks;

namespace ItemTagEditor
{
    public class AutoTagScheduledTask : IScheduledTask, IConfigurableScheduledTask
    {
        private ILibraryManager LibraryManager         { get; set; }
        private IUserManager UserManager               { get; set; }
        private ILogger Log                            { get; }
        private IMediaSourceManager MediaSourceManager { get; }

        public AutoTagScheduledTask(ILibraryManager libraryManager, IUserManager userManager, ILogManager logManager, IMediaSourceManager mediaSourceManager)
        {
            LibraryManager = libraryManager;
            UserManager    = userManager;
            MediaSourceManager = mediaSourceManager;
            Log = logManager.GetLogger(Plugin.Instance.Name);
        }

        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            var config = Plugin.Instance.Configuration;
            var rules = config.Rules;
            
            foreach (var rule in rules)
            {
                
                var internalItemQuery                                                                 = new InternalItemsQuery();
                if (!string.IsNullOrEmpty(rule.Profile.AudioCodec)) internalItemQuery.AudioCodecs     = new[] { rule.Profile.AudioCodec };
                if (!string.IsNullOrEmpty(rule.Profile.Container))  internalItemQuery.Containers      = new[] { rule.Profile.Container };
                if (!string.IsNullOrEmpty(rule.Profile.Rating))     internalItemQuery.OfficialRatings = new[] { rule.Profile.Rating };
                if (!string.IsNullOrEmpty(rule.Profile.VideoCodec)) internalItemQuery.VideoCodecs     = new[] { rule.Profile.VideoCodec };
                if (rule.Profile.Year > 0)                          internalItemQuery.Years           = new[] { rule.Profile.Year };
                internalItemQuery.IncludeItemTypes                                                    = new[] { rule.Profile.Type };
                internalItemQuery.Recursive                                                           = true;
                
                var itemQuery = LibraryManager.GetItemsResult(internalItemQuery);

                Log.Info($"Query has {itemQuery.TotalRecordCount} items.");

                Parallel.ForEach(itemQuery.Items,new ParallelOptions() { MaxDegreeOfParallelism = 4}, (item, state) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        state.Break();
                    }

                    if (string.IsNullOrEmpty(rule.Profile.Resolution)) return;
                    var videoStreams = item.GetMediaStreams();
                    foreach (var stream in videoStreams)
                    {
                        if (string.IsNullOrEmpty(stream.DisplayTitle)) continue;
                        if (stream.DisplayTitle.ToLowerInvariant().Contains(rule.Profile.Resolution))
                        {
                            Log.Info($"{item.Name} tags to add {rule.Tags.ToArray()}");
                            
                            var tags = item.Tags.ToList();
                            
                            if (rule.OverwriteTags)
                            {
                                tags = rule.Tags;
                            }
                            else
                            {
                                foreach (var tag in tags)
                                {
                                    //If we already have a tag on the baseItem that matches what we are trying to add
                                    if (rule.Tags.Exists(t => t == tag)) 
                                    {
                                        rule.Tags.RemoveAll(t => t == tag); //Remove the existing tag from the list we are adding.
                                    }
                                }
                                tags.AddRange(rule.Tags);
                            }

                            item.Tags = tags.ToArray();
                            LibraryManager.UpdateItem(item, item.Parent, ItemUpdateType.MetadataEdit);
                        }

                    }

                });

            }
            progress.Report(100.0);
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return new[]
            {
                new TaskTriggerInfo
                {
                    Type          = TaskTriggerInfo.TriggerInterval,
                    IntervalTicks = TimeSpan.FromHours(24).Ticks
                }
            };
        }

        public string Name        => "Item Auto Tagging";
        public string Key         => "Auto Item Tag";
        public string Description => "Automatically tag library items";
        public string Category    => "Library";
        public bool IsHidden      => false;
        public bool IsEnabled     => true;
        public bool IsLogged      => true;
    }
}
