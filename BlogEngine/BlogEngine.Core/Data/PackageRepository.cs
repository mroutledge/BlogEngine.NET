using BlogEngine.Core.Data.Contracts;
using BlogEngine.Core.Data.Models;
using BlogEngine.Core.Data.Services;
using BlogEngine.Core.Packaging;
using BlogEngine.Core.Web.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Caching;

namespace BlogEngine.Core.Data
{
    /// <summary>
    /// Package repository
    /// </summary>
    public class PackageRepository : IPackageRepository
    {
        /// <summary>
        /// Find packages
        /// </summary>
        /// <param name="take">Items to take</param>
        /// <param name="skip">Items to skip</param>
        /// <param name="filter">Filter expression</param>
        /// <param name="order">Sort order</param>
        /// <returns>List of packages</returns>
        public IEnumerable<Package> Find(int take = 10, int skip = 0, string filter = "", string order = "")
        {
            if (string.IsNullOrEmpty(order)) order = "LastUpdated desc";

            var packages = new List<Package>();
            var pkgsToLoad = new List<Package>();

            if (filter.ToLower() == "extensions")
            {
                if (!Security.IsAuthorizedTo(BlogEngine.Core.Rights.ManageExtensions))
                    throw new System.UnauthorizedAccessException();
                pkgsToLoad = Packaging.FileSystem.LoadExtensions();
            }
            else if (filter.ToLower() == "themes")
            {
                if (!Security.IsAuthorizedTo(BlogEngine.Core.Rights.ManageThemes))
                    throw new System.UnauthorizedAccessException();
                pkgsToLoad = Packaging.FileSystem.LoadThemes();
            }
            else if (filter.ToLower() == "widgets")
            {
                if (!Security.IsAuthorizedTo(BlogEngine.Core.Rights.ManageWidgets))
                    throw new System.UnauthorizedAccessException();
                pkgsToLoad = Packaging.FileSystem.LoadWidgets();
            }
            else
            {
                if (!Security.IsAuthorizedTo(BlogEngine.Core.Rights.ManagePackages))
                    throw new System.UnauthorizedAccessException();
                pkgsToLoad = CachedPackages;
            }

            if (take == 0) take = pkgsToLoad.Count();

            return pkgsToLoad.AsQueryable().OrderBy(order).Skip(skip).Take(take);
        }

        /// <summary>
        /// Package by ID
        /// </summary>
        /// <param name="id">Package ID</param>
        /// <returns>Package</returns>
        public Package FindById(string id)
        {
            if (!Security.IsAuthorizedTo(BlogEngine.Core.Rights.ManagePackages))
                throw new System.UnauthorizedAccessException();

            return CachedPackages.FirstOrDefault(pkg => pkg.Id == id);
        }

        /// <summary>
        /// Update package metadata
        /// </summary>
        /// <param name="item">Package object</param>
        /// <returns>True if success</returns>
        public bool Update(Package item)
        {
            if (!Security.IsAdministrator)
                throw new System.UnauthorizedAccessException();

            if (item == null)
                return false;

            switch (item.PackageType)
            {
                case "Extension":
                    if (!string.IsNullOrEmpty(item.Id))
                    {
                        var ext = ExtensionManager.GetExtension(item.Id);

                        if (ext == null)
                        {
                            // handle when extension and package ID different
                            var map = Packaging.FileSystem.ExtansionMap();
                            foreach (var m in map)
                            {
                                if (m.Value.ToString() == item.Id)
                                {
                                    ext = ExtensionManager.GetExtension(m.Key);
                                    ExtensionManager.ChangeStatus(m.Key, item.Enabled);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            ExtensionManager.ChangeStatus(item.Id, item.Enabled);
                        }

                        ext.Priority = item.Priority;
                        ExtensionManager.SaveToStorage(ext);
                        Blog.CurrentInstance.Cache.Remove(Constants.CacheKey);
                    }
                    break;
                case "Theme":
                    break;
                case "Widget":
                    break;
                default:
                    break;
            }
            return true;
        }

        /// <summary>
        /// Install package
        /// </summary>
        /// <param name="id">Package id</param>
        /// <returns>True if success</returns>
        public bool Install(string id)
        {
            if (!Security.IsAdministrator || !Blog.CurrentInstance.IsPrimary)
                throw new System.UnauthorizedAccessException();

            return Installer.InstallPackage(id);
        }

        /// <summary>
        /// Uninstall package
        /// </summary>
        /// <param name="id">Package id</param>
        /// <returns>True if success</returns>
        public bool Uninstall(string id)
        {
            if (!Security.IsAdministrator || !Blog.CurrentInstance.IsPrimary)
                throw new System.UnauthorizedAccessException();

            DeleteThemeCustomFields(id);

            return Installer.UninstallPackage(id);
        }

        #region Private methods

        static List<Package> CachedPackages
        {
            get
            {
                // uncomment this line to disable gallery caching for debugging
                // Blog.CurrentInstance.Cache.Remove(Constants.CacheKey);

                if (Blog.CurrentInstance.Cache[Constants.CacheKey] == null)
                {
                    Blog.CurrentInstance.Cache.Add(
                        Constants.CacheKey,
                        LoadPackages(),
                        null,
                        Cache.NoAbsoluteExpiration,
                        new TimeSpan(0, 15, 0),
                        CacheItemPriority.Low,
                        null);
                }
                return (List<Package>)Blog.CurrentInstance.Cache[Constants.CacheKey];
            }
        }

        static List<Package> LoadPackages()
        {
            var packages = new List<Package>();
            Gallery.Load(packages);
            foreach (var p in packages)
            {
                p.LocalVersion = BlogEngine.Core.Packaging.FileSystem.GetInstalledVersion(p.Id);
            }
            return packages;
        }

        static void LoadExtensions(List<Package> packages)
        {
            var extensions = ExtensionManager.Extensions.Where(x => x.Key != "MetaExtension").ToList();

            foreach (KeyValuePair<string, ManagedExtension> ext in extensions)
            {
                var x = ExtensionManager.GetExtension(ext.Key);

                var adminPage = string.IsNullOrEmpty(x.AdminPage) ?
                string.Format(Utils.RelativeWebRoot + "admin/Extensions/Settings.aspx?ext={0}&enb={1}", x.Name, x.Enabled) :
                string.Format(x.AdminPage, x.Name, x.Enabled);

                // If extension name in gallery differ from package ID
                // they will show as 2 extensions in the list.
                // To avoid, we can add mapping to /app_data/extensionmap.txt
                // in format "ExtensionId=PackageName" to map exension id to package id
                var map = Packaging.FileSystem.ExtansionMap();
                var extId = map.ContainsKey(x.Name) ? map[x.Name] : x.Name;
                var existingPackage = packages.Where(p => p.Id == extId).FirstOrDefault();

                if (existingPackage == null)
                {
                    var p = new Package
                    {
                        Id = x.Name,
                        PackageType = "Extension",
                        Title = x.Name,
                        Description = x.Description,
                        LocalVersion = x.Version,
                        Authors = x.Author,
                        IconUrl = string.Format("{0}pics/ext.png", Utils.ApplicationRelativeWebRoot),
                        Enabled = x.Enabled,
                        Priority = x.Priority,
                        SettingsUrl = x.Settings.Count > 0 ? adminPage : ""
                    };
                    packages.Add(p);
                }
                else
                {
                    existingPackage.LocalVersion = x.Version;
                    existingPackage.Enabled = x.Enabled;
                    existingPackage.SettingsUrl = x.AdminPage;
                }
            }
        }

        static void Trace(string msg, List<Package> packages)
        {
            string s = "{0}|{1}|{2}|{3}|{4}|{5}";
            foreach (var p in packages)
            {
                System.Diagnostics.Debug.WriteLine(string.Format(s, msg, p.PackageType, p.Id, p.LocalVersion, p.OnlineVersion));
            }
        }

        static void DeleteThemeCustomFields(string theme)
        {
            var p = CachedPackages.FirstOrDefault(pkg => pkg.Id == theme);
            if (p != null && p.PackageType != null && p.PackageType.ToLower() == "theme")
            {
                var fields = BlogEngine.Core.Providers.BlogService.FillCustomFields();

                foreach (var f in fields)
                {
                    if (f.BlogId == Blog.CurrentInstance.Id && f.CustomType.ToLower() == "theme" && f.ObjectId == theme)
                    {
                        BlogEngine.Core.Providers.BlogService.DeleteCustomField(f);
                    }
                }
                CustomFieldsParser.ClearCache();
            }
        }

        #endregion
    }
}
