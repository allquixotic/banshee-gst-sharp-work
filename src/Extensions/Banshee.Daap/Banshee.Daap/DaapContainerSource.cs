//
// DaapContainerSource.cs
//
// Authors:
//   Alexander Hixon <hixon.alexander@mediati.org>
//
// Copyright (C) 2008 Alexander Hixon
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
using Mono.Unix;

using Banshee.Base;
using Banshee.Collection;
using Banshee.Sources;
using Banshee.ServiceStack;

namespace Banshee.Daap
{
    public class DaapContainerSource : Source
    {
        public DaapContainerSource () :  base (Catalog.GetString ("Shared Music"), Catalog.GetString ("Shared Music"), 300)
        {
            Properties.SetString ("Icon.Name", "applications-internet");
        }
        
        public override bool CanRename {
            get { return false; }
        }
        
        public override bool? AutoExpand {
            get { return true; }
        }
        
        public override bool CanActivate {
            get { return false; }
        }
        
        /*private int count;
        public override int Count {
            get { return count; }
        }
        
        public override void AddChildSource (Source child)
        {
            count++;
            base.AddChildSource (child);
        }
        
        public override void RemoveChildSource (Source child)
        {
            count--;
            base.RemoveChildSource (child);
        }*/
        
        protected override string TypeUniqueId {
            get { return "daap-container"; }
        }
    }
}