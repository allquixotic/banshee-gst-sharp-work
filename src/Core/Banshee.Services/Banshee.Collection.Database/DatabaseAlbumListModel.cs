//
// DatabaseAlbumListModel.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//   Gabriel Burt <gburt@novell.com>
//
// Copyright (C) 2007 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using Mono.Unix;

using Hyena;
using Hyena.Data.Sqlite;
using Hyena.Query;

using Banshee.Database;

namespace Banshee.Collection.Database
{
    public class DatabaseAlbumListModel : DatabaseFilterListModel<DatabaseAlbumInfo, AlbumInfo>
    {
        public DatabaseAlbumListModel (Banshee.Sources.DatabaseSource source, DatabaseTrackListModel trackModel, BansheeDbConnection connection, string uuid)
            : base (Banshee.Query.BansheeQuery.AlbumField.Name, Banshee.Query.BansheeQuery.AlbumField.Label,
                    source, trackModel, connection, DatabaseAlbumInfo.Provider, new AlbumInfo (null), uuid)
        {
            QueryFields = new QueryFieldSet (Banshee.Query.BansheeQuery.AlbumField);
            ReloadFragmentFormat = @"
                FROM CoreAlbums WHERE CoreAlbums.AlbumID IN
                        (SELECT CoreTracks.AlbumID FROM CoreTracks, CoreCache{0}
                            WHERE CoreCache.ModelID = {1} AND
                                  CoreCache.ItemId = {2} {3})
                    ORDER BY CoreAlbums.TitleSortKey, CoreAlbums.ArtistNameSortKey";
        }

        public override string FilterColumn {
            get { return "CoreTracks.AlbumID"; }
        }

        protected override string ItemToFilterValue (object item)
        {
            return (item is DatabaseAlbumInfo) ? (item as DatabaseAlbumInfo).DbId.ToString () : null;
        }

        public override void UpdateSelectAllItem (long count)
        {
            select_all_item.Title = String.Format (Catalog.GetString ("All Albums ({0})"), count);
        }
    }
}
