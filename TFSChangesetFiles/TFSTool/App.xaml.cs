using System;
using System.Configuration;
using System.Windows;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TFSTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string TfsServerUrlSetting = "TFSServerUrl";
        private static readonly string TfsServerUrl = ConfigurationManager.AppSettings[TfsServerUrlSetting];

        internal static void Initialize()
        {
            GetTfsTeamProjects();
            VersionControlStore = TfsTeamProjects.GetService<VersionControlServer>();
            WorkItemStore = TfsTeamProjects.GetService<WorkItemStore>();
        }

        private static TfsTeamProjectCollection GetTfsTeamProjects()
        {
            TfsTeamProjects = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(TfsServerUrl));
            TfsTeamProjects.Connect(ConnectOptions.None);
            return TfsTeamProjects;
        }

        public static VersionControlServer VersionControlStore;
        public static WorkItemStore WorkItemStore;
        public static TfsTeamProjectCollection TfsTeamProjects;
    }
}
