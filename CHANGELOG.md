# Changelog

All notable changes to this project are be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [3.0.0] 2023-10-05

## Added

- Dropbox support (for resource download)
- Postgresql support (for visited pages database)
- Now parsing command line with a fork of [CommandLineParser](https://github.com/icalvo/commandline)
- UNIX Syntax (`-f`, `--fullScan`)
- Leaner syntax with positional arguments for site name/root URL

## Changed

- Config key for sites JSON file is now `Scrap:Sites`, although the old `Scrap:Definitions` key is still supported.

## [2.3.0] - 2023-03-14

### Changed

- `database` verb split into `searchvisited` and `deletevisited`.
- `scrap /all` replaced by `all` verb.

### Added

- Job definitions without resource configurations for `traverse`, `markvisited` and `visited`.
- `showconfig` verb for showing all the current configuration values.
- `Scrap:BaseRootFolder` global option
### Changed

## [2.2.0] - 2023-01-27
### Added
- `database` verb for managing the visited page database.
### Fixed
- Some verb aliases and descriptions.
- Removed stack traces from error output unless `/verbose` is used.

## [2.1.4] - 2023-01-17
### Fixed
- Fresh install does not give an error before configuring.

## [2.1.3] - 2022-10-26
### Fixed
- URLs with HTML entities (e.g. `&amp;#039;`) didn't download correctly.

## [2.1.2] - 2022-05-08
### Changed
- Added README to nuget.org

## [2.1.1] - 2022-05-08
### Changed
- `Page.Contents` filters out null or empty results
- Better logging

## [2.1.0] - 2022-02-12
### Added
- Non-interactive configuration command (`scrap config /key=value`)

### Changed
- Better logging (more colorful and log to files)
- Enabled URLs with non-standard ports
- Added version to banner

### Fixed
- XPath `html:` fixed
- When finding by root URL, do not match if several definitions match.

## [2.0.0] - 2022-01-17
### Added
- `traverse` command
- `resources` command
- `version` command
- New options for `scrap` command: `disableMarkingVisited` and `disableResourceWrites`

### Changed
- Better logs
- Fancy header limited to `scrap` and `configure` commands.
### Fixed
- Removed retry on 4xx errors

### Deprecated
- Removed `whatif` flag in favor of more specific ones.

## [1.0.0] - 2022-01-13
### Added
- Recursive graph web scrapper.
