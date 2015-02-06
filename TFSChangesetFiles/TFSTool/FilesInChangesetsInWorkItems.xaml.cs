using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TFSTool
{
    /// <summary>
    /// Interaction logic for FilesInChangesetsInWorkItems.xaml
    /// </summary>
    public partial class FilesInChangesetsInWorkItems : Window
    {
        public FilesInChangesetsInWorkItems()
        {
            InitializeComponent();
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
            ContentRendered += FilesInChangesetsInWorkItems_ContentRendered;
        }

        void FilesInChangesetsInWorkItems_ContentRendered(object sender, EventArgs e)
        {
            Keyboard.Focus(WorkItemsTextBox);
            //this.Focus();
            //WorkItemsTextBox.Focus();
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void WorkItemsTextBox_OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                //WorkItemsTextBox.IsEnabled = false;
                FillDataGrid();
            }
        }

        private void FillDataGrid()
        {
            try
            {
                var workItemIds = GetWorkitemIds();
                var files = GetFilesChanged(workItemIds).Select(file => new {File = file});
                FilesGrid.ItemsSource = files;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //WorkItemsTextBox.IsEnabled = true;
                //WorkItemsTextBox.Focus();
            }
        }

        private IEnumerable<int> GetWorkitemIds()
        {
            var workItemIds = WorkItemsTextBox.Text.Split(',').Select(wi => int.Parse(wi.Trim()));
            return workItemIds;
        }

        private IEnumerable<string> GetFilesChanged(IEnumerable<int> workItems)
        {
            var files = new HashSet<string>();
            foreach (var workItemId in workItems)
            {
                var workItem = App.WorkItemStore.GetWorkItem(workItemId);
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
            return files.OrderBy(f => f);
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
                changeset = App.VersionControlStore.ArtifactProvider.GetChangeset(new Uri(link.LinkedArtifactUri));
            return changeset != null;
        }

        private static bool IsChangesetLink(ExternalLink link)
        {
            var artifact = LinkingUtilities.DecodeUri(link.LinkedArtifactUri);
            return String.Equals(artifact.ArtifactType, "Changeset", StringComparison.Ordinal);
        }
    }
}