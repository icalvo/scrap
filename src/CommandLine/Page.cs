using System;
using HtmlAgilityPack;

namespace Scrap.CommandLine
{
    public class Page: IEquatable<Page>
    {
        public Page(Uri uri, HtmlDocument document)
        {
            Uri = uri;
            Document = document;
        }

        public Uri Uri { get; }
        public HtmlDocument Document { get; }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Page)obj);
        }

        public bool Equals(Page? other)
        {
            return other != null && Uri.AbsoluteUri.Equals(other.Uri.AbsoluteUri);
        }

        public override int GetHashCode()
        {
            return Uri.AbsoluteUri.GetHashCode();
        }
    }
}