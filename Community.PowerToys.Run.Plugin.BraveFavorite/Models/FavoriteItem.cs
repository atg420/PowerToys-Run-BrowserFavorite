// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ManagedCommon;
using Wox.Infrastructure;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.BraveFavorite.Models
{
    public class FavoriteItem
    {
        private static string? _folderIcoPath;
        private static string? _urlIcoPath;
        private readonly List<FavoriteItem> _childrens = new();
        private static readonly string[] ExludeSubDomains = { "www" };

        public string Name { get; }

        public Uri? Url { get; }

        public string BaseUrl
        {
            get
            {
                if (Url is null)
                {
                    return string.Empty;
                }

                var splitHostName = Url.Host.Split('.');

                // if subdomains are available
                if (splitHostName.Length > 2)
                {
                    return ExludeSubDomains.Contains(splitHostName[^3])
                        ? splitHostName[^2]
                        : string.Join(" ", splitHostName.Take(splitHostName.Length - 1));
                }

                return splitHostName[^2];
            }
        }

        public string Path { get; }

        public FavoriteType Type { get; }

        public ReadOnlyCollection<FavoriteItem> Childrens => _childrens.AsReadOnly();

        public FavoriteItem()
        {
            Name = string.Empty;
            Path = string.Empty;
            Type = FavoriteType.Folder;
        }

        public FavoriteItem(string name, Uri? url, string path, FavoriteType type)
        {
            Name = name;
            Url = url;
            Path = string.IsNullOrEmpty(path) ? $"/{(string.IsNullOrEmpty(name) ? BaseUrl : name)}" : path;
            Type = type;
        }

        public void AddChildren(FavoriteItem item)
        {
            _childrens.Add(item);
        }

        public Result CreateResult(IPublicAPI? api, string actionKeyword)
        {
            return Type switch
            {
                FavoriteType.Folder => new Result
                {
                    Title = Name,
                    SubTitle = $"Folder: {Path}",
                    IcoPath = _folderIcoPath,
                    QueryTextDisplay = Path,
                    ContextData = this,
                    Action = _ =>
                    {
                        var newQuery = string.IsNullOrWhiteSpace(actionKeyword)
                            ? $"{Path}/"
                            : $"{actionKeyword} {Path}/";
                        api?.ChangeQuery(newQuery, true);
                        return false;
                    },
                },
                FavoriteType.Url => new Result
                {
                    Title = string.IsNullOrEmpty(Name) ? BaseUrl : Name,
                    SubTitle = $"Favorite: {Path}",
                    IcoPath = _urlIcoPath,
                    QueryTextDisplay = Path,
                    Action = _ =>
                    {
                        Helper.OpenInShell($"{Url}");
                        return true;
                    },
                    ToolTipData = new ToolTipData(string.IsNullOrEmpty(Name) ? BaseUrl : Name, Url?.ToString()),
                    ContextData = this,
                },
                _ => throw new ArgumentException(),
            };
        }

        public List<ContextMenuResult> CreateContextMenuResult()
        {
            if (Type == FavoriteType.Url)
            {
                return new List<ContextMenuResult>
                {
                    new()
                    {
                        Title = "Copy URL (Ctrl+C)",
                        Glyph = "\xE8C8",
                        FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                        AcceleratorKey = Key.C,
                        AcceleratorModifiers = ModifierKeys.Control,
                        Action = _ =>
                        {
                            try
                            {
                                Clipboard.SetText(Url?.ToString() ?? string.Empty);
                            }
                            catch (Exception ex)
                            {
                                Log.Exception("Failed to copy URL to clipboard", ex, typeof(FavoriteItem));
                            }

                            return true;
                        },
                    },
                    new()
                    {
                        Title = "Open InPrivate (Ctrl+P)",
                        Glyph = "\xE727",
                        FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                        AcceleratorKey = Key.P,
                        AcceleratorModifiers = ModifierKeys.Control,
                        Action = _ =>
                        {
                            Helper.OpenInShell(@"shell:AppsFolder\Brave", $"-incognito  {Url}");
                            return true;
                        },
                    },
                };
            }

            return new List<ContextMenuResult>();
        }

        public static void SetIcons(Theme theme)
        {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                _folderIcoPath = "Images/Folder.light.png";
                _urlIcoPath = "Images/Url.light.png";
            }
            else
            {
                _folderIcoPath = "Images/Folder.dark.png";
                _urlIcoPath = "Images/Url.dark.png";
            }
        }
    }
}
