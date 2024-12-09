// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Community.PowerToys.Run.Plugin.BrowserFavorite.Models;

namespace Community.PowerToys.Run.Plugin.BrowserFavorite.Helpers
{
    public interface IFavoriteProvider : IDisposable
    {
        FavoriteItem Root { get; }
    }
}
