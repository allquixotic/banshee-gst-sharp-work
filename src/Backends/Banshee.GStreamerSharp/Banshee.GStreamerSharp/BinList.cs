//
// BinList.cs
// 
// Author:
//   Sean McNamara <smcnam@gmail.com>
// 
// Copyright (c) 2010 Sean McNamara
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using Gst;
using Gst.Base;
using Gst.BasePlugins;

namespace Banshee.GStreamerSharp
{


    public interface IPadHack
    {
        void AddPadHack (Pad p);
        bool RemovePadHack (Pad p);
    }

    internal class MyBinType : Bin, IPadHack
    {

        internal MyBinType () : base()
        {
        }

        internal MyBinType (string name) : base(name)
        {
        }

        public void AddPadHack (Pad p)
        {
            AddPad (p);
        }

        public bool RemovePadHack (Pad p)
        {
            return RemovePad (p);
        }
    }

    public class BinList<T> : IList<T> where T : Element
    {
        private readonly MyBinType bin;
        private readonly List<T> children = new List<T> ();
        private readonly GhostPad src = new GhostPad ("src", PadDirection.Src);
        private readonly GhostPad sink = new GhostPad ("sink", PadDirection.Sink);

        public BinList () : this(null)
        {
        }

        public BinList (string name)
        {
            bin = new MyBinType (name);
            bin.AddPadHack (src);
            bin.AddPadHack (sink);
        }

        public void LnSrc (Element dest)
        {
            Hyena.Log.Warning ("Setting source ghost pad target of " + bin.Name + " to " + dest.Name);
            src.SetTarget (dest.GetStaticPad ("src"));
        }

        public void LnSink (Element dest)
        {
            Hyena.Log.Warning ("Setting sink ghost pad target of " + bin.Name + " to " + dest.Name);
            sink.SetTarget (dest.GetStaticPad ("sink"));
        }

        public Bin GetBin ()
        {
            return (Bin)bin;
        }

        public Pad GetSourceGhostPad ()
        {
            return src;
        }

        public Pad GetSinkGhostPad ()
        {
            return sink;
        }


        public int IndexOf (T item)
        {
            return children.IndexOf (item);
        }

        public void Insert (int index, T item)
        {
            Element curr, next;
            
            if (index < 0 || index > children.Count)
                throw new IndexOutOfRangeException ("BinList.Insert: Index " + index + " out of range");
            
            if (index == children.Count) {
                Add (item);
                return;
            }
            
            if (index == 0) {
                curr = null;
                next = children[0];
            } else {
                curr = children[index - 1];
                next = children[index];
            }
            
            bin.Add (item);
            
            if (curr == null) {
                //Insert at the head of the list.
                Element.Link (item, next);
                
                LnSink (item);
            } else {
                Element.Unlink (curr, next);
                Element.Link (curr, item, next);
            }
            
            children.Insert (index, item);
        }

        public void RemoveAt (int index)
        {
            
            if (index < 0 || index >= children.Count)
                throw new IndexOutOfRangeException ("BinList.Insert: Index " + index + " out of range");
            
            Element before, toRemove = children[index], after;
            
            if (index == 0) {
                before = null;
            } else {
                before = children[index - 1];
            }
            
            if (index == children.Count - 1) {
                after = null;
            } else {
                after = children[children.Count - 1];
            }
            
            if (before == null && after == null) {
                LnSink (null);
                LnSrc (null);
            } else if (before == null && after != null) {
                //Removing the first element but there are others.
                Element.Unlink (toRemove, after);
                LnSink (after);
                //`after' is the first element now.
            } else if (before != null && after == null) {
                //Removing the last element but there are ones before it.
                Element.Unlink (before, toRemove);
                LnSrc (before);
                //`before' is the last element now.
            } else {
                //Removing an inner element, no need to adjust ghost pads at all.
                Element.Unlink (before, toRemove);
                Element.Unlink (toRemove, after);
                Element.Link (before, after);
            }
            
            bin.Remove (toRemove);
            children.RemoveAt (index);
        }

        public T this[int index] {
            get { return children[index]; }
            set {
                if (index < 0 || index >= children.Count) {
                    throw new IndexOutOfRangeException ("BinList.[]: Index out of range " + index);
                } else {
                    this.RemoveAt (index);
                    this.Insert (index, value);
                }
            }
        }

        //Adds an element to the end of the graph.
        public void Add (T item)
        {
            if (item != null) {
                if (!bin.Add (item)) {
                    Hyena.Log.Warning ("Warning: Not added to " + bin.Name + " : " + item.Name);
                }
                if (children.Count > 0) {
                    Element prevTail = children[children.Count - 1];
                    Pad prevTailSrc = prevTail.GetStaticPad ("src");
                    if (prevTailSrc.IsLinked && !prevTailSrc.Unlink (prevTailSrc.Peer)) {
                        Hyena.Log.Warning ("Warning: Couldn't unlink from " + bin.Name + "'s ghost pad: " + prevTail.Name);
                    }
                    if (!Element.Link (prevTail, item)) {
                        Hyena.Log.Warning ("Warning: Couldn't link: " + prevTail.Name + " and " + item.Name);
                    }
                } else {
                    LnSink (item);
                }
                
                LnSrc (item);
                children.Add (item);
            }
        }

        public void Clear ()
        {
            int cc = children.Count;
            for (int i = 0; i < cc; i++) {
                RemoveAt (0);
            }
        }

        public bool Contains (T item)
        {
            return children.Contains (item);
        }

        public void CopyTo (T[] array, int arrayIndex)
        {
            throw new NotImplementedException ();
        }

        public bool Remove (T item)
        {
            if (item != null) {
                RemoveAt (IndexOf (item));
                return true;
            } else {
                return false;
            }
        }

        public IEnumerator<T> GetEnumerator ()
        {
            return (IEnumerator<T>)bin.ElementsSorted.GetEnumerator ();
        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return GetEnumerator ();
        }

        public int Count {
            get { return children.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }
    }
}

