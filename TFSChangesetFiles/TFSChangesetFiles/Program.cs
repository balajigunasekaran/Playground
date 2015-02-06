using System.IO;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TFSChangesetFiles
{
    static class Program
    {
        private const string TfsServerUrlSetting = "TFSServerUrl";
        private static readonly VersionControlServer VersionControlStore;
        private static readonly WorkItemStore WorkItemStore;
        private static readonly string TfsServerUrl = ConfigurationManager.AppSettings[TfsServerUrlSetting];
        private static readonly int[] WorkItemNumbers = new int[] {97413, 96701};

        static Program()
        {
            var tfsService = GetTfsService();
            VersionControlStore = tfsService.GetService<VersionControlServer>();
            WorkItemStore = tfsService.GetService<WorkItemStore>();
        }

        private static TfsTeamProjectCollection GetTfsService()
        {
            var projects = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(TfsServerUrl));
            projects.Connect(ConnectOptions.None);
            return projects;
        }

        static void Main(string[] args)
        {
            var files = new HashSet<string>();
            foreach (var workItemId in WorkItemNumbers)
            {
                var workItem = WorkItemStore.GetWorkItem(workItemId);
                var changesets = GetChangesetsForWorkItem(workItem);
                foreach (var changeset in changesets)
                {
                    foreach (var change in changeset.Changes)
                    {
                        var filePath = change.Item.ServerItem;
                        if (!files.Contains(filePath))
                            files.Add(filePath);
                    }
                }
            }
            var orderedFiles = files.OrderBy(f => f);
        }

        private static IEnumerable<Changeset> GetChangesetsForWorkItem(WorkItem workItem)
        {
            var changesets = new List<Changeset>();
            foreach (var link in workItem.Links.OfType<ExternalLink>())
            {
                var changeset = default(Changeset);
                if (TryParseChangesetLink(link, out changeset))
                    changesets.Add(changeset);
            }
            return changesets;
        }

        private static bool TryParseChangesetLink(ExternalLink link, out Changeset changeset)
        {
            changeset = null;
            if (IsChangesetLink(link))
                changeset = VersionControlStore.ArtifactProvider.GetChangeset(new Uri(link.LinkedArtifactUri));
            return changeset != null;
        }

        private static bool IsChangesetLink(ExternalLink link)
        {
            var artifact = LinkingUtilities.DecodeUri(link.LinkedArtifactUri);
            return String.Equals(artifact.ArtifactType, "Changeset", StringComparison.Ordinal);
        }
    }
}
