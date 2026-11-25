# Changelog

All notable changes to the UI Automation (UIA) Driver Server are listed here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/). Each version heading matches a GitHub release tag
without its `v` prefix.

## Unreleased

### Fixed

- A mouse input built for an element id now targets that element's top-left clickable point. The check was
  inverted: a known element got the cursor position, and an unknown one threw `KeyNotFoundException`. An unknown
  element now falls back to the cursor position.

## 2026.11.25.44

### Fixed

- A W3C `pointerDown` or `pointerUp` action with button 1 presses and releases the middle button. It pressed the
  left button before. Button 2 is still right, and every other button is still left.

### Changed

- The element mouse-move route documents that it uses the request's offsets as they are sent.
- The endpoint reference describes the pointer button mapping, and the README describes the build instead of
  linking to a guide outside the repository.

## 2026.11.25.43

### Fixed

- `MiddleCenter`, and any alignment the driver does not recognize, applies `OffsetX` and `OffsetY` like every other
  alignment. Clicks that name only an alignment still land exactly on the center.

## 2026.11.17.42

### Added

- OCR support in the Emgu builds.

## 2026.11.08.41

### Changed

- Restructured the README and expanded the documentation.

## 2026.10.23.40

### Changed

- Refined element text resolution and its documentation.

## 2026.10.23.39

### Added

- Scan-code mapping for the Windows keys, F11 and F12, with tests.
