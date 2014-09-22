using System;
using System.Collections.Generic;
using System.Linq;
using BlogEngine.Core.GalleryServer;
using BlogEngine.Core.Data.Models;
using NuGet;
using System.Globalization;

namespace BlogEngine.Core.Packaging
{
    /// <summary>
    /// Online gallery
    /// </summary>
    public static class Gallery
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packages"></param>
        public static void Load(List<Package> packages)
        {
            try
            {
                if (BlogSettings.Instance.GalleryFeedUrl == "http://dnbegallery.org/feed/FeedService.svc")
                {
                    foreach (var pkg in GetAllPublishedPackages().ToList())
                    {
                        //System.Diagnostics.Debug.WriteLine(string.Format("{0}|{1}|{2}|{3}", pkg.Id, pkg.Version, pkg.IsLatestVersion, pkg.IconUrl));

                        var jp = new Package
                        {
                            Id = string.IsNullOrEmpty(pkg.Id) ? pkg.Title : pkg.Id,
                            PackageType = pkg.PackageType,
                            Authors = string.IsNullOrEmpty(pkg.Authors) ? "unknown" : pkg.Authors,
                            Description = pkg.Description.Length > 140 ? string.Format("{0}...", pkg.Description.Substring(0, 140)) : pkg.Description,
                            DownloadCount = pkg.DownloadCount,
                            LastUpdated = pkg.LastUpdated == DateTime.MinValue ? pkg.Published.ToString("yyyy-MM-dd HH:mm") : pkg.LastUpdated.ToString("yyyy-MM-dd HH:mm"), // format for sort order to work with strings
                            Title = pkg.Title,
                            OnlineVersion = pkg.Version,
                            Website = pkg.ProjectUrl,
                            Tags = pkg.Tags,
                            IconUrl = pkg.IconUrl
                        };

                        // for themes or widgets, get screenshot instead of icon
                        // also get screenshot if icon is missing for package
                        if (pkg.Screenshots != null && pkg.Screenshots.Count > 0)
                        {
                            if ((pkg.PackageType == Constants.Theme || pkg.PackageType == Constants.Widget) || string.IsNullOrEmpty(pkg.IconUrl))
                            {
                                jp.IconUrl = pkg.Screenshots[0].ScreenshotUri;
                            }
                        }

                        // if both icon and screenshot missing, get default image for package type
                        if (string.IsNullOrEmpty(jp.IconUrl))
                            jp.IconUrl = DefaultThumbnail(pkg.PackageType);

                        if (!string.IsNullOrEmpty(jp.IconUrl) && !jp.IconUrl.StartsWith("http:"))
                            jp.IconUrl = Constants.GalleryUrl + jp.IconUrl;

                        if (!string.IsNullOrWhiteSpace(pkg.GalleryDetailsUrl))
                            jp.PackageUrl = PackageUrl(pkg.PackageType, pkg.Id);

                        //System.Diagnostics.Debug.WriteLine(string.Format("{0}|{1}|{2}|{3}", jp.Id, jp.OnlineVersion, jp.PackageType, jp.IconUrl));

                        packages.Add(jp);
                    }
                }
                else
                {
                    var packs = GetNugetPackages().ToList();
                    foreach (var pkg in packs)
                    {
                        if (pkg.IsLatestVersion)
                        {
                            var jp = new Package
                            {
                                Id = pkg.Id,
                                Authors = pkg.Authors == null ? "unknown" : string.Join(", ", pkg.Authors),
                                Description = pkg.Description.Length > 140 ? string.Format("{0}...", pkg.Description.Substring(0, 140)) : pkg.Description,
                                DownloadCount = pkg.DownloadCount,
                                LastUpdated = pkg.Published != null ? pkg.Published.Value.ToString("yyyy-MM-dd HH:mm") : "", // format for sort order to work with strings
                                Title = pkg.Title,
                                OnlineVersion = pkg.Version.ToString(),
                                Website = pkg.ProjectUrl == null ? null : pkg.ProjectUrl.ToString(),
                                Tags = pkg.Tags,
                                IconUrl = pkg.IconUrl == null ? null : pkg.IconUrl.ToString()
                            };

                            if (!string.IsNullOrEmpty(jp.IconUrl) && !jp.IconUrl.StartsWith("http:"))
                                jp.IconUrl = Constants.GalleryUrl + jp.IconUrl;

                            if (string.IsNullOrEmpty(jp.IconUrl))
                                jp.IconUrl = DefaultThumbnail("");

                            packages.Add(jp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.Log("BlogEngine.Core.Packaging.Load", ex);
            }   
        }

        /// <summary>
        /// Convert version from string to int for comparison
        /// </summary>
        /// <param name="version">string version</param>
        /// <returns>int version</returns>
        public static int ConvertVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
                return 0;

            int numVersion;
            Int32.TryParse(version.Replace(".", ""), out numVersion);
            return numVersion;
        }

        /// <summary>
        /// Package URL
        /// </summary>
        /// <param name="pkgType">Package Type</param>
        /// <param name="pkgId">Package ID</param>
        /// <returns></returns>
        public static string PackageUrl(string pkgType, string pkgId)
        {
            switch (pkgType)
            {
                case "Theme":
                    return string.Format("{0}/List/Themes/{1}", Constants.GalleryAppUrl, pkgId);
                case "Extension":
                    return string.Format("{0}/List/Extensions/{1}", Constants.GalleryAppUrl, pkgId);
                case "Widget":
                    return string.Format("{0}/List/Widgets/{1}", Constants.GalleryAppUrl, pkgId);
            }
            return string.Empty;
        }

        #region Private methods

        static IEnumerable<PublishedPackage> GetAllPublishedPackages()
        {
            var packagingSource = new PackagingSource { FeedUrl = BlogSettings.Instance.GalleryFeedUrl };
            var allPacks = new List<PublishedPackage>();

            // gallery has a limit 100 records per call
            // keep calling till any records returned
            int cnt;
            var skip = 0;
            do
            {
                var s = skip;
                var galleryFeedContext = new GalleryFeedContext(new Uri(BlogSettings.Instance.GalleryFeedUrl)) { IgnoreMissingProperties = true };

                // dnbegallery.org overrides feed with additional values in "screenshots" section
                var pkgs = (new[] { packagingSource }).SelectMany(source => {
                    return galleryFeedContext.Packages.Expand("Screenshots").OrderBy(p => p.Id).Where(p => p.IsLatestVersion).Skip(s).Take(100);
                });

                cnt = pkgs.Count();
                skip = skip + 100;
                allPacks.AddRange(pkgs);
            } while (cnt > 0);

            return allPacks;
        }

        static IEnumerable<IPackage> GetNugetPackages()
        {
            var rep = PackageRepositoryFactory.Default.CreateRepository(BlogSettings.Instance.GalleryFeedUrl);
            return rep.GetPackages();
        }

        static string DefaultThumbnail(string packageType)
        {
            switch (packageType)
            {
                case "Theme":
                    return string.Format("{0}pics/Theme.png", Utils.ApplicationRelativeWebRoot);
                case "Extension":
                    return string.Format("{0}pics/ext.png", Utils.ApplicationRelativeWebRoot);
                case "Widget":
                    return string.Format("{0}pics/Widget.png", Utils.ApplicationRelativeWebRoot);
            }
            return string.Format("{0}pics/pkg.png", Utils.ApplicationRelativeWebRoot);
        }

        #endregion
    }
}
