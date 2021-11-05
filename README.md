# scrap
**scrap** is a general purpose web scraper.

A scraping process walks through the structure of a web site and downloads specific resources from it. The resources could be images, PDF documents, videos, or even the HTML files themselves.

## Root URL
A scraping process starts with some URL. You must specify it in the command line with:

```
-rootUrl=https://example.com
```

## Adjacency specification
In general terms, the structure of a web site is represented by a directed graph, in which the nodes represent pages and the edges represent a link from node A to node B. We don't want to blindly follow all links in a web page because that could potentially end up walking through most of the Internet. Therefore we need to define exactly which links do we want to follow. This is known as the **adjacency function**.

In **scrap**, the adjacency function is defined with an XPath expression and an HTML attribute name (usually "href"). For example, let's say that our target web site has only internal links. Then we can simply use the XPath "//a" and the attribute "href". This would go in the command line as:

```
-adjacencyXPath=//a -adjacencyAttribute=href
```

## Resource specification
Some of the nodes we walk through will have links to the resources we want. We need therefore to specify which of the links of a page are resource links. The method is the same as before: we use an XPath expression and an attribute name. For example, let's say we are downloading all the images:

```
-resourceXPath=//img -resourceAttribute=src
```

## Local file path specification
When we have resource links, we download them and store them locally. Therefore we need to define a local path for each of the resources.

To do so we will use a full C# expression. This expression will have at its disposal several
context and global variables, and also a number of standard namespaces in order to be able to
use standard functions, and some extension methods that come with this project. Let's first see an example:

```csharp
destinationRootFolder.C(pageUrl.CleanSegments()[4..^1]).C(pageUrl.CleanSegments()[^1] + resourceUrl.Extension()).ToPath()
```

This combines a number of pieces using `Path.Combine`, which is a function to join together pieces of a path with the folder separator (`\` in Windows and `/` in Unix-related OSs). We also use the `C()` extension methods that concatenate strings and arrays of strings (no matter what). So for example:

```csharp
"ab".C("cd") == [ "ab", "cd" ]
["ab", "cd"].C("ef") == [ "ab", "cd", "ef" ]
"ab".C(["cd", "ef"]) == [ "ab", "cd", "ef" ]
["ab", "cd"].C(["ef", "gh"]) == [ "ab", "cd", "ef", "gh" ]
```

The segments without slashes can be obtained by scrap's extension method `CleanSegments()`. So we are combining the destination root folder (as provided in the command line arguments) with some segments of the page URL, and then with a file name that is composed by the last segment of the page URL plus the extension of the resource URL.

The variables are:

| Name | Type | Description |
|---|---|---|
| destinationRootFolder | string | Destination root folder as specified by -destinationRootFolder command line argument |
| page | Page | Information about the current page. |
| resourceUrl | Uri | URL of the downloaded resource. |
| pageIndex | int | Index of the resource in the page (zero-based). |

Methods and extension methods:

```
string.C(string): string list
stringList.C(string): string list
string.C(stringList): string list
stringList.C(stringList): string list
doc.CleanSegments(): string list
page.Text(xpath): string
page.Attribute(xpath, attributeName): string
page.Link(xpath): string
page.LinkedDoc(xpath): Page
```

## Job definitions
You can store a scrap job definition in a LiteDB database and then use its name and a root URL to start a job.

```
scrap.exe add -name=mycrawl -adjacencyXPath=...

scrap.exe db -name=mycrawl -rootUrl=http://domain.com

```


