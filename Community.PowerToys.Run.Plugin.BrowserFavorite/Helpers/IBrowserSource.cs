// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Community.PowerToys.Run.Plugin.BrowserFavorite.Helpers;

public interface IBrowserSource
{
    public string DefaultExecutablePath { get; }

    public IFavoriteProvider FavoriteProvider { get; }

    public string BrowserExecutable { get; set; }

    public void Open(string url, bool privateMode = false);

    public void Open(Uri url, bool privateMode = false)
    {
        Open(url.ToString(), privateMode);
    }

    public void Open(string[] urls, bool privateMode = false)
    {
        foreach (var url in urls)
        {
            Open(url, privateMode);
        }
    }

    public void Dispose()
    {
        FavoriteProvider.Dispose();
    }
}
