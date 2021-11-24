# scrap
**scrap** is a general purpose web scraper.

A scraping process walks through the structure of a web site and downloads specific resources from it. The resources could be images, PDF documents, videos, or text elements within the HTML files.

## Root URL (`rootUrl`)
A scraping process starts with some URL.

```
"rootUrl": "https://example.com"
```

## Adjacency specification (`adjacencyXPath`)
In general terms, the structure of a web site is represented by a directed graph, in which the nodes represent pages and the edges represent a link from node A to node B. We don't want to blindly follow all links in a web page because that could potentially end up walking through most of the Internet. Therefore we need to define exactly which links do we want to follow. This is known as the **adjacency function**.

In **scrap**, the adjacency function is defined with an **XPath expression**. For example, let's say that our target web site has only internal links. Then we can simply use the XPath `//a/@href`. Since the `href` attribute is the most usual, we can omit it from the XPath, e.g. `//a` would be a valid XPath.

```
 "adjacencyXPath": "(//*[@id='comicnav']//a[contains(text(), 'Previous')])[1]"
```
## Resource specification
Some of the nodes we walk through will have links to the content we want, or the content itself. We need to say whether we want to download links or get text pieces from the pages:

```
 "resourceType": "downloadLink"
 "resourceType": "text"
```
The default value for `resourceType` is `downloadLink`, so you can omit this configuration for link downloads.

To specify the pieces of the page that will be used as links or text, the method is the same as before: we use an **XPath expression**. For example, let's say we are downloading all the images:

```
 "resourceXPath": "//img/@src"
```

## Resource repository
The resource repository is where the resources are stored. Currently there is only one type of repository, `filesystem`, that must be specified:

```
"resourceRepository": {
  "type": "filesystem"
}
```


## Local file path specification
When we have resource links, we download them and store them locally. Therefore we need to define a local path for each of the resources.

First of all we declare a root folder:
```
"resourceRepository": {
  "type": "filesystem",
  "rootFolder": "C:\Downloads"
}
```

Then we will specify a number of path fragments. Each fragment can return a single string or an array of strings which will be path-concatenated.

Each fragment is specified with a full C# expression. This expression will have at its disposal several context and global variables, and also a number of standard namespaces in order to be able to use standard functions, and some extension methods that come with this project. Let's first see an example:

```csharp
resourceUrl.CleanSegments()[^1]
```

The segments of an URL without slashes can be obtained by scrap's extension method `CleanSegments()`. So we are combining the destination root folder (as provided in the command line arguments) with some segments of the page URL, and then with a file name that is composed by the last segment of the page URL plus the extension of the resource URL.

The variables and methods available are:

| Name | Type | Description |
|---|---|---|
| `page` | `Page` | Information about the current page. |
| `page.Uri` | `Page` | Page URL. |
| `pageIndex` | `int` | Index of the page in the graph traversal. |
| `resourceUrl` | `Uri` | URL of the downloaded resource. |
| `resourceIndex` | `int` | Index of the resource in the page (zero-based). |
| `uri.CleanSegments()` | `string` list | Gets the clean segments of an Uri |
| `page.Content(xpath)` | `string` | Gets a content (attribute, text or HTML) |
| `page.Link(xpath)` | `Uri` | Gets a link. |
| `page.LinkedDoc(xpath)` | `Page` | Gets another page linked from the current one. |

