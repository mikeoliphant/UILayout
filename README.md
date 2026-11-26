# UILayout

UILayout is a simple, performant, layout-based, cross-platform user interface library for .NET languages. It began as the UI framework for [my games](https://www.facebook.com/nostaticsoftware/) back in the day - starting on XBOX Indie Games, and moving on from there to XBOX, Nintendo, PlayStation and Steam and mobile platforms. It has become the UI framework for most of my cross-platform app development. 

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
- **Themeable:** every control takes its colors, fonts and images from one theme (`Layout.Theme`). Images are white
  with alpha, so they can be tinted or swapped.
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

# Projects using UILayout

Projects using UILayout include:

- [AudioPlugSharp](https://github.com/mikeoliphant/AudioPlugSharp): Audio plugin framework (VST3, CLAP) for C# .NET
- [ChartPlayer](https://github.com/mikeoliphant/ChartPlayer): Visual player for song charts synchronized to music recordings.
- [Stompbox](https://github.com/mikeoliphant/Stompbox): Guitar amplification and effects pedalboard simulation.
- [FaustVst](https://github.com/mikeoliphant/FaustVst): Load/edit Faust dsp effects in a VST.

