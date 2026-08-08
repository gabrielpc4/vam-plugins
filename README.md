# Virt-A-Mate Scene Control Suite

This repository contains a collection of scripts and small tools for **Virt-A-Mate (VaM)**. It is designed to make scene-based VR sessions easier to control, more predictable when loading, and more comfortable to navigate while keeping each feature independently useful.

## What it provides

- A bootstrap entry point that loads the session plugin bundle onto `CoreControl`.
- Scene-load settling that waits for VaM loading state, loading UI, geometry, and icons to finish before releasing playback.
- Temporary playback and audio control during scene transitions, with safeguards against repeated scene-load work.
- VR palm HUD controls, including scene navigation and hand-menu actions.
- Passenger possession and improved point-of-view behavior, including hand-only possession, rig snapshots, head handling, and cleanup when leaving or changing scenes.
- Camera helpers for monitor mode, scene-camera restoration, field-of-view preferences, and reflection-related behavior.
- Interaction features for hands, clothing, head proximity, soft-physics preferences, E-Motion paths, Spankings, and multi-atom pivot handles.
- Animation checks that help identify clips that unexpectedly loop.
- VaM-focused utilities for plugin injection, file handling, person/atom lookup, and same-folder loading checks.
- Offline Python tools for scene-camera patching and scene-menu labels, thumbnails, text, and browser URL setup.

## Repository layout

```text
bootstrap/          Session bootstrap entry point
session-plugins/    Main session plugin list and scene/session runtime
features/           Optional runtime features grouped by purpose
tools/              Offline scene-camera and scene-menu utilities
util/               Shared VaM helpers
docs/               Design notes and implementation history
```

The repository root is intended to map to VaM's `Custom/Scripts` directory. The folder structure inside this repository is therefore part of the expected runtime paths.

## Installation

1. Back up the VaM installation you want to modify.
2. Copy the contents of this repository into VaM's `Custom/Scripts` directory, preserving the folder structure.
3. In VaM, load the bootstrap `.cs` or `.cslist` entry from `bootstrap/`.
4. Enable the individual feature `.cslist` files only when you want those features active.

The session bundle is assembled through the `.cslist` files in `session-plugins/`. If you use only individual scripts, check their feature documentation first because some features expect the shared session runtime or utility classes to be available.

## Design notes

The scripts target the C# environment provided by VaM, including its older C# language constraints. Runtime code favors cached references, event-driven work, debounced scene transitions, and small scans instead of repeated whole-scene work every frame.

The `FEATURE.md` files beside each area contain more detailed behavior notes and expected integration points. The Python files in `tools/` are maintenance utilities for editing scene files outside VaM; review their paths and backups before running them.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE).
