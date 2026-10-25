# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.0] - 2026-10-25

### Added
- Minimal Cut Set Tree tab: drawing of the minimal cut sets (top event → cut sets → events) with pan, zoom, centring and image export.
- ALGO III tree layout: compact tidy-tree algorithm (Reingold–Tilford / Walker), fast also for large trees.
- Option to open Explorer with the exported image or the created archive selected.
- Hints in empty result tables (minimal cut sets, importance measures).
- Automated tests (xUnit) for calculations, minimal cut sets, importance measures, tree layout, export and reports.

### Changed
- Target framework .NET 9.
- Settings are saved to `%AppData%\OpenFTA\AppSettings.json` and kept after restart.
- Importance measures are computed from the top event result in its own quantity (frequency or probability). BIM, CIM and RAW are not defined for basic events given as a frequency (f, λ) and are shown as n/a.
- Reliability (R) values are treated as probabilities in gates.
- Labels of the importance measure bars show the value of the measure.
- In the minimal cut set tree, cut sets with one event are drawn without a gate.
- Saved `.fta` files contain the real application version; a warning is shown when a file from a newer major version is opened. Files from earlier versions open as before.

### Fixed
- Exact probability calculation used a wrong exposure time when the unit of an event differed from the base time unit, or when the base time unit was not a year; results could be wrong by orders of magnitude. **Results of the exact calculation mode change.**
- Importance measures (BIM, CIM, RAW, RRW, FV) for events given as a frequency or reliability, for repeated events (same tag), and the state of the tree after the calculation. **Values of importance measures change.**
- Minimal cut sets: a basic event that is part of several cut sets; highlighting of a minimal cut set stopped working after a report was created.
- Settings were never saved.
- Number input: crash in the event editor on systems with a decimal comma; values in the reliability database could be stored as 0 or 1. A decimal comma and a decimal point are now both accepted.
- Image export cut off the last events, used a fixed extra space and changed the view when the dialog was cancelled.
- Results of a previously opened file remained in the result tables and in the tree view.
- The help window opened with an empty page.
- README: link to the changelog.

## [1.0.1] - 2026-03-25

### Added
Added option to hide frequency for individual events
Added contribution value of individual MCS to reports and analysis
Added method to open a specific directory in Windows Explorer
Embedded sorting script directly in the report, including support for numbers with units

### Changed / Improved
Optimized text wrapping and font sizing across individual events

### Fixed
Fixed event text rendering to respect Graphics world transform
Fixed automatic working directory assignment

## [1.0.0] - 2025-11-23

### Added
- Redesign of the visual tree view.
- TreeView support for hiding/showing tree sections.
- Splash screen on application start.
- Functionality for highlighting minimal cut sets.
- Support for exporting basic data.
- Support for text wrapping in events.

### Changed / Improved
- Refactor of event gate rendering: removed bitmaps and replaced with engine drawing.
- Improvement of Main Form UI.
- Computation engine improvements.
- Modification of application settings engine.
- Improved user experience for drag and drop.
- Implemented new reliability metrics in CompEngine and drawing engine.

### Fixed
- Bug with working directory.
- Issue with the Y-coordinate of a newly created event.
