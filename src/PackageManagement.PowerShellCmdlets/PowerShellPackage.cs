﻿using NuGet.Client.VisualStudio;
using NuGet.Packaging;
using NuGet.ProjectManagement;
using NuGet.Versioning;
using System.Collections.Generic;
using System.Linq;

namespace NuGet.PackageManagement.PowerShellCmdlets
{
    /// <summary>
    /// Represent the view of packages installed to project(s)
    /// </summary>
    internal class PowerShellInstalledPackage
    {
        public string Id { get; set; }

        public List<NuGetVersion> Version { get; set; }

        public string ProjectName { get; set; }

        /// <summary>
        /// Get the view of installed packages. Use for Get-Package command. 
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="versionType"></param>
        /// <returns></returns>
        internal static List<PowerShellInstalledPackage> GetPowerShellPackageView(Dictionary<NuGetProject, IEnumerable<PackageReference>> dictionary)
        {
            List<PowerShellInstalledPackage> views = new List<PowerShellInstalledPackage>();
            foreach (KeyValuePair<NuGetProject, IEnumerable<PackageReference>> entry in dictionary)
            {
                foreach (PackageReference package in entry.Value)
                {
                    PowerShellInstalledPackage view = new PowerShellInstalledPackage();
                    view.Id = package.PackageIdentity.Id;
                    view.Version = new List<NuGetVersion>() { package.PackageIdentity.Version };
                    view.ProjectName = entry.Key.GetMetadata<string>(NuGetProjectMetadataKeys.Name);
                    views.Add(view);
                }
            }
            return views;
        }
    }

    /// <summary>
    /// Represent packages found from the remote package source
    /// </summary>
    internal class PowerShellRemotePackage
    {
        public string Id { get; set; }

        public List<NuGetVersion> Version { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Get the view of PowerShellPackage. Used for Get-Package -ListAvailable command. 
        /// </summary>
        /// <param name="metadata">list of PSSearchMetadata</param>
        /// <param name="versionType"></param>
        /// <returns></returns>
        internal static List<PowerShellRemotePackage> GetPowerShellPackageView(IEnumerable<PSSearchMetadata> metadata, VersionType versionType)
        {
            List<PowerShellRemotePackage> view = new List<PowerShellRemotePackage>();
            foreach (PSSearchMetadata data in metadata)
            {
                PowerShellRemotePackage package = new PowerShellRemotePackage();
                package.Id = data.Identity.Id;
                package.Description = data.Summary;

                switch (versionType)
                {
                    case VersionType.all:
                        {
                            package.Version = data.Versions.OrderByDescending(v => v).ToList();
                        }
                        break;
                    case VersionType.latest:
                        {
                            NuGetVersion nVersion = data.Version;
                            if (nVersion == null)
                            {
                                nVersion = data.Versions.OrderByDescending(v => v).FirstOrDefault();
                            }
                            package.Version = new List<NuGetVersion>() { nVersion };
                        }
                        break;
                }

                view.Add(package);
            }
            return view;
        }
    }

    /// <summary>
    /// Represent package updates found from the remote package source
    /// </summary>
    internal class PowerShellUpdatePackage
    {
        public string Id { get; set; }

        public List<NuGetVersion> Version { get; set; }

        public string Description { get; set; }

        public string ProjectName { get; set; }

        /// <summary>
        /// Get the view of PowerShellPackage. Used for Get-Package -Updates command. 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="version"></param>
        /// <param name="versionType"></param>
        /// <returns></returns>
        internal static PowerShellUpdatePackage GetPowerShellPackageUpdateView(PSSearchMetadata data, NuGetVersion version, VersionType versionType, NuGetProject project)
        {
            PowerShellUpdatePackage package = new PowerShellUpdatePackage();
            package.Id = data.Identity.Id;
            package.Description = data.Summary;
            package.Version = new List<NuGetVersion>();
            package.ProjectName = project.GetMetadata<string>(NuGetProjectMetadataKeys.Name);
            switch (versionType)
            {
                case VersionType.updates:
                    {
                        package.Version = data.Versions.Where(p => p > version).OrderByDescending(v => v).ToList();
                    }
                    break;
                case VersionType.latest:
                    {
                        NuGetVersion nVersion = data.Versions.Where(p => p > version).OrderByDescending(v => v).FirstOrDefault();
                        if (nVersion != null)
                        {
                            package.Version.Add(nVersion);
                        }
                    }
                    break;
            }

            return package;
        }
    }

    /// <summary>
    /// Enum for types of version to output, which can be all versions, latest version or update versions.
    /// </summary>
    public enum VersionType
    {
        all,
        latest,
        updates
    }
}
