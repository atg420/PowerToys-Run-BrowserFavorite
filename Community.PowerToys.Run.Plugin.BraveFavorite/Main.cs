// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Community.PowerToys.Run.Plugin.BraveFavorite.Helpers;
using Community.PowerToys.Run.Plugin.BraveFavorite.Models;
using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;
using Wox.Infrastructure;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.BraveFavorite
{
    public class Main : IPlugin, ISettingProvider, IContextMenu
    {
        public static string PluginID => "D73A7EF0633F4C82A14454FFD848F444";

        private const string SearchTree = nameof(SearchTree);
        private const string SearchBaseUrl = nameof(SearchBaseUrl);
        private const string BrowserSource = nameof(BrowserSource);
        private const bool SearchTreeDefault = false;
        private const bool SearchBaseUrlDefault = false;
        private const int BrowserSourceDefault = 0;
        private readonly IFavoriteQuery _favoriteQuery;
        private IFavoriteProvider _favoriteProvider;
        private PluginInitContext? _context;
        private bool _searchTree;
        private bool _searchBaseUrl;
        private int _browserSource;

        public string Name => "Brave Favorite";

        public string Description => "Open Browser favorites.";

        public IEnumerable<PluginAdditionalOption> AdditionalOptions => new List<PluginAdditionalOption>
        {
            new()
            {
                Key = SearchTree,
                Value = SearchTreeDefault,
                DisplayLabel = "Search as tree",
                DisplayDescription = "Navigate the original directory tree when searching.",
            },
            new()
            {
                Key = SearchBaseUrl,
                Value = SearchBaseUrlDefault,
                DisplayLabel = "Base URL Search",
                DisplayDescription =
                    "If enabled, bookmarks without titles can be found by searching their base URL. For example, 'https://www.google.com/' can be found by searching 'google'. If disabled, bookmarks without titles cannot be searched.",
            },
            new()
            {
                Key = BrowserSource,
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Combobox,
                DisplayLabel = "Browser Source",
                DisplayDescription =
                    "Select the browser bookmark source",
                ComboBoxItems = Enum.GetValues(typeof(BrowserSource)).Cast<int>().Select(v => new KeyValuePair<string, string>(((BrowserSource)v).ToString(), v + string.Empty)).ToList(),
                ComboBoxValue = BrowserSourceDefault,
            },
        };

        public Main()
        {
            _favoriteProvider = new BraveFavoriteProvider();
            _favoriteQuery = new FavoriteQuery();

            UpdateBrowserSource((BrowserSource)_browserSource);
        }

        public void Init(PluginInitContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _context.API.ThemeChanged += OnThemeChanged;
            UpdateIconsPath(_context.API.GetCurrentTheme());
        }

        public List<Result> Query(Query query)
        {
            var search = query.Search.Replace('\\', '/').Split('/');

            Log.Info($"Search {_searchTree} {_searchBaseUrl} {_browserSource}", typeof(Main));

            if (_searchTree)
            {
                return _favoriteQuery
                    .Search(_favoriteProvider.Root, search, 0)
                    .OrderBy(f => f.Type)
                    .ThenBy(f => f.Name)
                    .Select(f => f.CreateResult())
                    .ToList();
            }
            else
            {
                var results = new List<Result>();
                foreach (var favorite in _favoriteQuery.GetAll(_favoriteProvider.Root))
                {
                    var name = favorite.Name;

                    if (_searchBaseUrl && string.IsNullOrWhiteSpace(name))
                    {
                        name = favorite.BaseUrl;
                    }

                    var score = StringMatcher.FuzzySearch(query.Search, name);
                    if (string.IsNullOrWhiteSpace(query.Search) || score.Score > 0)
                    {
                        var result = favorite.CreateResult();
                        result.Score = score.Score;
                        result.TitleHighlightData = score.MatchData;
                        results.Add(result);
                    }
                }

                return results.OrderBy(r => r.Title).ToList();
            }
        }

        public Control CreateSettingPanel()
        {
            throw new NotImplementedException();
        }

        public void UpdateSettings(PowerLauncherPluginSettings settings)
        {
            Log.Info($"Update Settings", typeof(Main));
            if (settings != null && settings.AdditionalOptions != null)
            {
                Log.Info($"Costume Settings", typeof(Main));
                _searchTree = settings.AdditionalOptions.FirstOrDefault(x => x.Key == SearchTree)?.Value ??
                              SearchTreeDefault;
                _searchBaseUrl = settings.AdditionalOptions.FirstOrDefault(x => x.Key == SearchBaseUrl)?.Value ??
                                 SearchBaseUrlDefault;
                _browserSource =
                    settings.AdditionalOptions.FirstOrDefault(x => x.Key == BrowserSource)?.ComboBoxValue ??
                    BrowserSourceDefault;
            }
            else
            {
                Log.Info($"Default Settings", typeof(Main));
                _searchTree = SearchTreeDefault;
                _searchBaseUrl = SearchBaseUrlDefault;
                _browserSource = BrowserSourceDefault;
            }

            UpdateBrowserSource((BrowserSource)_browserSource);
        }

        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (selectedResult.ContextData is not FavoriteItem favorite)
            {
                return new List<ContextMenuResult>();
            }

            return favorite.CreateContextMenuResult();
        }

        private void OnThemeChanged(Theme currentTheme, Theme newTheme)
        {
            UpdateIconsPath(newTheme);
        }

        private static void UpdateIconsPath(Theme theme)
        {
            FavoriteItem.SetIcons(theme);
        }

        private void UpdateBrowserSource(BrowserSource browserSource)
        {
            _favoriteProvider.Dispose();

            switch (browserSource)
            {
                case BraveFavorite.BrowserSource.Brave:
                    _favoriteProvider = new BraveFavoriteProvider();
                    break;
                case BraveFavorite.BrowserSource.Chrome:
                    _favoriteProvider = new ChromeFavoriteProvider();
                    break;
                case BraveFavorite.BrowserSource.Edge:
                    _favoriteProvider = new EdgeFavoriteProvider();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(browserSource), browserSource, null);
            }
        }
    }
}
