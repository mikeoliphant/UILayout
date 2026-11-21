# UILayout

UILayout is a simple, peformant, layout-based, cross-platform user interface library for .NET languages.

# Platforms Supported

UILayout currently supports SkiaSharp and MonoGame as underlying rendering systems. As such, it has the ability to target the same platforms they do - so most platforms are potentially supported.

Tested platforms:
- Windows x64
- Linux x64

# Architecture

- **Cross-platform:** one code base on Windows and Linux, with SkiaSharp or MonoGame underneath.
- **High performance:** a retained element tree, laid out when it changes, drawn in batches.
- **Hardware acceleration:** GPU rendering with the MonoGame backend.
- **Low GC pressure:** steady-state frames don't allocate, so the garbage collector stays quiet (important for audio and
  game apps).
- **High DPI support:** layout in logical units, scaled to the display; images carry a density, so high-resolution
  images draw pixel for pixel at a matching scale.
- **Themeable:** images are white with alpha, so they can be tinted or swapped, and the input-style controls take their
  colors from a theme.
- **Embeddable:** can run inside another application's window (for example, an audio plugin editor in a DAW), or as an
  app of its own.
- **Touch and mouse:** input is handled as touches, with mouse and keyboard support on top.

# Functionality

<img width="800" height="447" alt="UILayoutTest-dx" src="https://github.com/user-attachments/assets/f83a3bea-ff5e-41bc-93cf-3792250fe8a1" />

UILayout provides the following:

- Layout structure (dock, horizontal/vertical stacks, etc.)
- UI controls (buttons, text blocks, sliders, menus, dialogs, scrollbars, tab panels, swipe lists, etc.)
- Input-style controls (framed buttons, drop-downs, check lists, sections) following the theme
- Image rendering (images, ninepatch images, rotation, tinting, batching, image atlases, image densities)
- Clipping to element bounds
- Font rendering (skia text rendering, bitmap fonts)
- Mouse/Keyboard/Touch input handling, hover, drag and drop
- File dialog support

# Getting Started

Have a look at the included test UI for examples of how to use it:

https://github.com/mikeoliphant/UILayout/blob/master/UILayout.Test/LayoutTest.cs

# Current Status

UILayout is currently under active development and everything is very much subject to change.

That said, it already provides considerable functionality and is being used in several large projects.

Projects using UILayout include:

- [ChartPlayer](https://github.com/mikeoliphant/ChartPlayer): Visual player for song charts synchronized to music recordings.
- [ChartConverter](https://github.com/mikeoliphant/ChartConverter): Convert Rocksmith PSARC and Rock Band files to OpenSongChart format.
- [StompboxUI](https://github.com/mikeoliphant/StompboxUI): Remote GUI and VST3 plugin for Stompbox guitar simulation.
- [FaustVst](https://github.com/mikeoliphant/FaustVst): Load/edit Faust dsp effects in a VST.

