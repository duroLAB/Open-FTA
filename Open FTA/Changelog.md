# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Unsaved changes: the title shows * and Open FTA asks whether to save them before closing, opening another file or starting a new tree.
- Keyboard shortcuts: Ctrl+N (new), Ctrl+O (open), Ctrl+S (save), F5 (analysis of the selected tab), F1 (help), Ctrl+plus / Ctrl+minus (zoom), Ctrl+0 (center the tree); shortcuts are shown in the tool tips.
- About window (Help → About Open FTA) with the version, author, licence and links to the source code and to the thesis.

### Changed
- Target framework .NET 10 (LTS); .NET 9 support ends on 10 November 2026.
- New look of the events with three styles in Settings (Display settings → Event style): Header (tag band coloured by event type, default), Minimal (colour stripe, tag and value as labels) and Clean (tinted tag and value rows). All event names have the same font size; it is reduced only when a name does not fit. The original look and the setting "Event pen type" were removed.
- The displayed importance measure (BIM, RAW, …) is shown inside the event: in its bottom row with a thin bar along the bottom edge (default), or as a coloured badge on the corner (Settings → Metric display).
- Gates, event symbols, connecting lines and the label of the highlighted minimal cut set drawn in the same flat style and colours as the events; light grey canvas with a dot grid. Exported images keep a white background.
- Titles of the side panels (Tree Structure, Working Directory) as light headers with grey upper-case text instead of blue bands.
- Main window: flat tool bars, flat tabs with an accent line, the tree of events and the file list in the style of Windows Explorer. The sections of the tree of events are named Main Tree Structure and Minimal Cut Sets and shown in bold. The two "Add" buttons are named "Add event" and "Add basic event".
- Center (Ctrl+0) fits the whole visible tree into the view as large as possible (collapsed branches, symbols and metric bars are taken into account) and is no longer an undo step; the minimal cut set tree uses the same fitting.
- One set of icons in all tool bars and in the context menu of the tree: outline icons of the Windows icon font (Segoe Fluent Icons / Segoe MDL2 Assets) and icons of the fault tree (add event, add basic event, database, run) drawn in the colours of the tree; sharp at every display scaling.
- Status bar shows the result of the top event, the number of events and the zoom.
- The importance measure table no longer shows the event GUID (it stays in the CSV export).
- New look of selected events (accent border, focus ring, tinted tag row) and of the event under the mouse (accent border, raised shadow). Hover and selection are not drawn into exported images. The tree is no longer repainted on every mouse move over an empty area.

### Fixed
- After saving, the file was not remembered: the title stayed "(unsaved)" and the next save offered the default name. The message box after every save was removed.
- Tags with a space or with the characters + * ( ) broke the minimal cut set analysis. The event editor rejects such tags (also empty ones), and the minimal cut set analysis, importance measures and the report list the events with such tags instead of giving wrong results.
- Names, tags, descriptions and references with the characters & < > broke the HTML report; they are now escaped.
- Opening the report or the help on a computer without the Microsoft Edge WebView2 Runtime ended with an unhandled exception (FileNotFoundException 0x80070002). A message now explains what is missing, links to the download page and offers to open the report or the help in the web browser. WebView2 keeps its data in `%LocalAppData%\OpenFTA\WebView2`, so it also works when the application is in a read-only folder.
- Minimal cut set table showed raw floating-point values (e.g. 0.030000000000000002); frequency is shown with 4 and contribution with 3 significant digits.
- Title of the report window.
- Own records in the reliability database were lost: `MainAppDB.db` next to the exe was overwritten by every build and by an update. The database is now kept in `%AppData%\OpenFTA\MainAppDB.db`; on first start it is created as a copy of the shipped database. Records added in an earlier version are in `MainAppDB.db` next to the old `Open FTA.exe` – copy that file to `%AppData%\OpenFTA\` (before the first start of the new version) to keep them.
- The reliability database window shows its title and the path of the database file.
- Adding and deleting events left the parent's list of children out of date until the tree view was refreshed; events linked only by their parent could stay in the structure after their parent was deleted. A tag given when adding an event was ignored.
- Undo and redo did not restore changes made in the event editor (name, tag, value, gate, …): the undo step was saved only after the change, also when the editor was cancelled. Undo now keeps independent copies of the events, an edit is one undo step (none when nothing changed), and opening a file clears the undo history of the previous one.
- The report and the drawing could use another event as the top event when the top event was not the first event in the file.
- Sort with a selected event drew the tree only from that event, so events outside its branch disappeared with the ALGO I and ALGO II layouts.

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
