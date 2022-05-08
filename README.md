[![Build status](https://github.com/icalvo/scrap/actions/workflows/PullRequest.yml/badge.svg)](https://github.com/icalvo/scrap/actions/workflows/PullRequest.yml)
![Nuget](https://img.shields.io/nuget/v/scrap)
![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/scrap?label=nuget%20pre)

# 🐾 scrap
**scrap** is a general purpose web scraper.

A scraping process walks through the structure of a web site and downloads specific resources from it. The resources could be images, PDF documents, videos, or text elements within the HTML files.

## <a id="Installation"></a>Installation

```
> dotnet tool install --global scrap
```
After installing, invoke it a first time with `scrap config` in order to make the initial configuration.

## Uninstallation

```
> dotnet tool install --global scrap
```
After uninstalling, the global configuration file (stored under the user profile folder at `.scrap/scrap-user.json`) will NOT be deleted.

## Configuration and usage
Please head to the [wiki page](https://github.com/icalvo/scrap/wiki) to find documentaion on configuration and usage.
