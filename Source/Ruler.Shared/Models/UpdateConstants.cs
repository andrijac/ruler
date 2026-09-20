using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
{
    public static class UpdateConstants
    {
        public const string ZipFileName = "ruler.zip";
        public const string ManifestFileName = "manifest.json";
        public const string ManifestSigFileName = "manifest.json.sig";
        public const string ExeFileName = "Ruler.Wpf.exe";
        public const string GitHubRepoOwner = "YourGitHubUsername"; // Replace with your GitHub username
        public const string GitHubRepoName = "YourRepositoryName"; // Replace with your repository name
        public const string GitHubApiUrl = "https://api.github.com/repos/{0}/{1}/releases/latest"; // Format with owner and repo name
        public const string UpdaterFileName = "Ruler.Updater.exe";
    }
}
