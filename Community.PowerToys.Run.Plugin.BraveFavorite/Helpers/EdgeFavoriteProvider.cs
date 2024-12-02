// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Community.PowerToys.Run.Plugin.BraveFavorite.Helpers;

public class EdgeFavoriteProvider : ChromiumFavoriteProvider
{
    private static readonly string Path =
        Environment.ExpandEnvironmentVariables(
            @"%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Bookmarks");

    public EdgeFavoriteProvider()
        : base(Path)
    {
    }
}
