[![Build status](https://github.com/icalvo/scrap/actions/workflows/ci-build.yml/badge.svg)](https://github.com/icalvo/scrap/actions/workflows/ci-build.yml)
![Nuget](https://img.shields.io/nuget/v/scrap)
![Nuget (prerelease)](https://img.shields.io/nuget/vpre/scrap)

# 🐾 scrap
**scrap** is a general purpose web scraper.

A scraping process walks through the structure of a web site and downloads specific resources from it. The resources could be images, PDF documents, videos, or text elements within the HTML files.

## <a id="Installation"></a>Installation

```
> dotnet tool install --global scrap
```
After installing, invoke it a first time with `scrap` in order to make the initial configuration.

## Uninstallation

```
> dotnet tool install --global scrap
```
After uninstalling, the global configuration file (stored under the user profile folder at `.scrap/scrap-user.json`) will NOT be deleted.

## Configuration
Scrap job definitions are defined in a global JSON file that you specify when you install the tool (see [Installation](#Installation)).

Inside you will find an empty JSON array (`[]`) in which you can add several objects, each one representing a definition of a scrap job. Each definition has a number of properties which are explained below.

When you call the tool you can specify what definition to use. Tou can add additional options to complete or modify the definition you chose. Finally, you can also choose to launch all definitions that have enough information to be run.

### Name (`name`)
**REQUIRED**. The definition must have a name.

```
"name": "example"
```

You can start a job using its name:

```
> scrap -name=example 
```

### Root URL (`rootUrl`)
A scraping process starts with some URL.

```
"rootUrl": "https://example.com"
```
If the root URL is not given here, the definition is partial and therefore you will need to provide it in the command line:

```
> scrap -name=defWithoutRoot -rootUrl=http://example.com/item/4033
```

### Adjacency specification (`adjacencyXPath`)
In general terms, the structure of a web site is represented by a directed graph, in which the nodes represent pages and the edges represent a link from node A to node B. We don't want to blindly follow all links in a web page because that could potentially end up walking through most of the Internet. Therefore we need to define exactly which links do we want to follow. This is known as the **adjacency function**.

In **scrap**, the adjacency function is defined with an **XPath expression**. For example, let's say that our target web site has only internal links. Then we can simply use the XPath `//a/@href`. Since the `href` attribute is the most usual, we can omit it from the XPath, e.g. `//a` would be a valid XPath.

```
 "adjacencyXPath": "(//*[@id='comicnav']//a[contains(text(), 'Previous')])[1]"
```

This property can be undefined. This will mean that `scrap` will not navigate anywhere and will only process the resources on the root URL.

### Resource specification (`resourceXPath`)
**REQUIRED**. Some of the nodes we walk through will have links to the content we want, or the content itself.

To specify the pieces of the page that will be used as links or text, the method is the same as before: we use an **XPath expression**. For example, let's say we are downloading all the images:

```
 "resourceXPath": "//img/@src"
```

### Resource repository (`resourceRepository`, `type`)
**REQUIRED**. The resource repository is where the resources are stored. Currently there is only one type of repository, `filesystem`, that must be specified:

```
"resourceRepository": {
  "type": "filesystem"
}
```

### Filesystem resource repository

#### Root folder (`rootFolder`)
**REQUIRED**. The base folder in which the files will be saved.

#### Local file path specification (`pathFragments`)
When we have resource links, we download them and store them in a folder. Therefore we need to define a local path for each of the resources.

This is done by specifying a number of **path fragments**. Each fragment can return a **single string** or an **array of strings** which will then be path-concatenated. If some of these strings is null or empty, it will be discarded.

Each fragment is specified with a full C# expression. This expression will have at its disposal several context and global variables, and also a number of standard namespaces in order to be able to use standard functions, and some extension methods that come with `scrap`. Let's first see an example:

```csharp
resourceUrl.CleanSegments()[^1]
```

The segments of an URL without slashes can be obtained by `scrap`'s extension method `CleanSegments()`. So we are combining the destination root folder (as provided in the command line arguments) with some segments of the page URL, and then with a file name that is composed by the last segment of the page URL plus the extension of the resource URL.

The variables and methods available are:

| Name | Type          | Description                                     |
|---|---------------|-------------------------------------------------|
| `page` | `Page`        | Information about the current page.             |
| `page.Uri` | `Page`        | Page URL.                                       |
| `pageIndex` | `int`         | Index of the page in the graph traversal.       |
| `resourceUrl` | `Uri`         | URL of the downloaded resource.                 |
| `resourceIndex` | `int`         | Index of the resource in the page (zero-based). |
| `uri.CleanSegments()` | `string[]`    | Gets the clean segments of an URL               |
| `page.Content(xpath)` | `string`      | Gets a content (attribute, text or HTML)        |
| `page.Link(xpath)` | `Uri`         | Gets a link.                                    |
| `page.LinkedDoc(xpath)` | `Task<Page?>` | Gets another page linked from the current one.  |

`LinkedDoc` does not always request the page to the server. `scrap` keeps a cache of the already visited pages, so if we are linking to one of those (which is a usual thing to do), you will not have to wait for the request.

A full example of `pathFragments`:

```json
      {
          "type": "filesystem",
          "rootFolder": "C:\\Users\\ignacio\\",
          "pathFragments": [
            "(await page.LinkedDoc(\"//*[contains(@class, 'back-to-gallery')]//a\")).Content(\"//a[contains(@href, '/gallery/artist')]/text()\")",
            "(await page.LinkedDoc(\"//*[contains(@class, 'back-to-gallery')]//a\")).Content(\"//h1/text()\") ?? \"\"",
            "page.Uri.CleanSegments()[^1] + resourceUrl.Extension()"
          ]
}
```
This spec has three fragments. All of them return a single string, therefore the path will have three elements (`folder1/folder2/file`). The first two navigate back to a gallery page to pick up different elements. The third one gets the last segment of the current page URL and then appends the resource file extension to it.