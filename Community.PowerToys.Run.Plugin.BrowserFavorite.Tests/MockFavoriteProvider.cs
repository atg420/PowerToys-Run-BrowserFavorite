// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Community.PowerToys.Run.Plugin.BrowserFavorite.Helpers;
using Community.PowerToys.Run.Plugin.BrowserFavorite.Models;

namespace Community.PowerToys.Run.Plugin.BrowserFavorite.Tests
{
    public class MockFavoriteProvider : IFavoriteProvider
    {
        private readonly FavoriteItem _root;

        public FavoriteItem Root => _root;

        public MockFavoriteProvider()
        {
            var coding = new FavoriteItem("Coding", null, "Coding", FavoriteType.Folder);
            coding.AddChildren(new FavoriteItem("GitHub", new Uri("https://github.com/"), "Coding/GitHub", FavoriteType.Url));
            coding.AddChildren(new FavoriteItem("Microsoft Azure", new Uri("https://portal.azure.com/"), "Coding/Microsoft Azure", FavoriteType.Url));
            coding.AddChildren(new FavoriteItem("Microsoft Developer Blogs", new Uri("https://devblogs.microsoft.com/"), "Coding/Microsoft Developer Blogs", FavoriteType.Url));

            var tools = new FavoriteItem("Tools", null, "Coding", FavoriteType.Folder);
            tools.AddChildren(new FavoriteItem("JWT", new Uri("https://jwt.io/"), "Coding/Tools/JWT", FavoriteType.Url));
            tools.AddChildren(new FavoriteItem("Pigment", new Uri("https://pigment.shapefactory.co/"), "Coding/Tools/Pigment", FavoriteType.Url));
            coding.AddChildren(tools);

            var shopping = new FavoriteItem("Shopping", null, "Shopping", FavoriteType.Folder);
            shopping.AddChildren(new FavoriteItem("Amazon", new Uri("https://www.amazon.com/"), "Shopping/Amazon", FavoriteType.Url));
            shopping.AddChildren(new FavoriteItem("eBay", new Uri("https://www.ebay.com/"), "Shopping/eBay", FavoriteType.Url));

            _root = new FavoriteItem("Favorites bar", null, string.Empty, FavoriteType.Folder);
            _root.AddChildren(new FavoriteItem("YouTube", new Uri("https://www.youtube.com/"), "YouTube", FavoriteType.Url));
            _root.AddChildren(new FavoriteItem("Spotify", new Uri("https://open.spotify.com/"), "Spotify", FavoriteType.Url));
            _root.AddChildren(new FavoriteItem("LinkedIn", new Uri("https://www.linkedin.com/"), "LinkedIn", FavoriteType.Url));
            _root.AddChildren(coding);
            _root.AddChildren(shopping);
        }

        public void Dispose()
        {
        }
    }
}
