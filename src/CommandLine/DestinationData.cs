using System;
using System.Collections.Generic;

namespace Scrap.CommandLine
{
    public record DestinationData
    {
        public DestinationData(string destinationFolder, string[] pageSegments, string[] resourceSegments, string resourceExtension)
        {
            DestinationFolder = destinationFolder;
            PageSegments = pageSegments;
            ResourceSegments = resourceSegments;
            ResourceExtension = resourceExtension;
        }

        public string DestinationFolder { get; }
        public string[] PageSegments { get; }
        public string[] ResourceSegments { get; }
        public string ResourceExtension { get; }

        public IEnumerable<string> Parse(IEnumerable<string> pattern)
        {
            foreach (var part in pattern)
            {
                var split = part.Split("[");
                var field = split[0];
                switch (field)
                {
                    case "DestinationFolder":
                        yield return DestinationFolder;
                        break;
                    case "PageSegments":
                    {
                        if (split.Length == 1)
                        {
                            foreach (var segment in PageSegments)
                            {
                                yield return segment;
                            }                        
                        }

                        var rangeSpec = split[1].TrimEnd(']');
                        foreach (var segment in ParseRange(rangeSpec, PageSegments))
                        {
                            yield return segment;
                        }                             
                        break;
                    }
                    case "ResourceSegments":
                    {
                        if (split.Length == 1)
                        {
                            foreach (var segment in ResourceSegments)
                            {
                                yield return segment;
                            }                        
                        }

                        var rangeSpec = split[1].TrimEnd(']');
                        foreach (var segment in ParseRange(rangeSpec, ResourceSegments))
                        {
                            yield return segment;
                        }                             

                        break;
                    }
                    case "ResourceExtension":
                        yield return ResourceExtension;
                        break;
                    default:
                        throw new Exception();
                }
                {
                
                }
            }
        }

        private static IEnumerable<string> ParseRange(string rangeSpec, string[] segments)
        {
            var rangeParts = rangeSpec.Split("..");
            var rangeStart = ParseIndex(rangeParts[0]);
            Index rangeEnd;
            if (rangeParts.Length == 1)
            {
                yield return segments[rangeStart];
                rangeEnd = rangeStart;
            }
            else
            {
                rangeEnd = ParseIndex(rangeParts[1]);
            }

            foreach (var segment in segments[new Range(rangeStart, rangeEnd)])
            {
                yield return segment;
            }
        }

        private static Index ParseIndex(string indexSpec)
        {
            return 
                indexSpec.StartsWith("^")
                    ? new Index(int.Parse(indexSpec[1..]), fromEnd: true)
                    : new Index(int.Parse(indexSpec), fromEnd: false);
        }

        public void Deconstruct(out string DestinationFolder, out string[] PageSegments, out string[] ResourceSegments, out string ResourceExtension)
        {
            DestinationFolder = this.DestinationFolder;
            PageSegments = this.PageSegments;
            ResourceSegments = this.ResourceSegments;
            ResourceExtension = this.ResourceExtension;
        }
    }
}