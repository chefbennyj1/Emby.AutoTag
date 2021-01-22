using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Tasks;

namespace ItemTagEditor
{
    public class AutoTagScheduledTask : IScheduledTask, IConfigurableScheduledTask
    {
        private ILibraryManager LibraryManager { get; set; }
        private IUserManager UserManager { get; set; }
        private ILogger Log { get; }
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
                Log.Info(rule.Profile.Type);
                Log.Info(rule.Profile.Container);
                Log.Info(rule.Profile.AudioCodec);

                // ReSharper disable once ComplexConditionExpression
                var itemQuery = LibraryManager.QueryItems(new InternalItemsQuery()
                {
                    Recursive        = true,
                    IncludeItemTypes = new[] { rule.Profile.Type },
                    User             = UserManager.Users.FirstOrDefault(user => user.Policy.IsAdministrator),
                    AudioCodecs = new []{ rule.Profile.AudioCodec ?? "" },
                    Containers = new []{ rule.Profile.Container ?? "" },
                    OfficialRatings = new []{ rule.Profile.Rating ?? "" }
                });

                Log.Info($"Query has {itemQuery.TotalRecordCount} items.");

                Parallel.ForEach(itemQuery.Items,new ParallelOptions() { MaxDegreeOfParallelism = 4}, (item, state) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        state.Break();
                    }

                    foreach (var stream in item.GetMediaStreams())
                    {
                        if (stream.Width != null)
                        {
                            if (stream.Width == Convert.ToInt32(rule.Profile.Resolution.Replace("p", string.Empty)))
                            {

                            }
                        }
                    }
                    

                    
                    Log.Info($"{item.Name}\n{item.Container}\n{item.OfficialRating}\n{}");
                    

                    
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
