ruler
=====
Simple on-screen pixel ruler.

[<img src="https://andrijac.visualstudio.com/_apis/public/build/definitions/e1a573e5-0959-4685-8cce-05da5e128d6a/1/badge"/>](https://andrijac.visualstudio.com/ruler/_build/index?definitionId=e1a573e5-0959-4685-8cce-05da5e128d6a)

<a href="http://www.softpedia.com/progDownload/Ruler-andrijac-Download-256095.html"><img alt="download page" src="https://raw.githubusercontent.com/andrijac/ruler/master/img/softpedia_download_large_shadow.png" /></a>
<br />

<img src="https://github.com/andrijac/ruler/raw/master/img/ruler.gif">


### Releases
https://github.com/andrijac/ruler/releases

### Legacy
Ruler is fork of small utility application originally developed by Jeff Key:

- http://ruler.codeplex.com/ :floppy_disk:
- ~~http://weblogs.asp.net/jkey/archive/2004/09/01/224433.aspx~~ :skull:
- ~~http://www.sliver.com/dotnet/ruler/~~ :skull:


### .NET version support
~~Support for .NET 2.0 is kept.~~
Last version with .NET 2.0 support is 1.6.
https://github.com/andrijac/ruler/releases/tag/1.6.0.0

Onward, Ruler will target .NET 4.6 which is deployed with Windows 10.

**<a name="newfeatures" href="https://github.com/andrijac/ruler/blob/master/NewFeatures.md">Added features in this fork.</a>**

### Contributions
If you want to contribute to project, please follow the rules:
1. **Zero-Warning policy**. There should be no C# compiler warnings in PR. :pray:
2. **Use StyleCop** Visual Studio Extension (by Chris Dahlberg) to check for StyleCop warnings https://marketplace.visualstudio.com/items?itemName=ChrisDahlberg.StyleCop. Zero-warning policy extends to StyleCop warnings. :cop: Custom StyleCop rules file is part of repository and it should be picked up by StyleCop in Visual Studio once you run it.
3. Keep in mind that **Ruler should be simple tool**. It should be disposable, easy to run, easy to close, easy to run in multiple instances. For now, I still refuse to add any kind of additional dependency to project like persistent storage for Ruler settings (which are still, at the moment, few), having a Ruler icon in system tray etc. for sake of keeping Ruler simple. Since Ruler can run multiple instances, features mentioned would add unnecessary complexity to project. :speech_balloon:
4. Use **Visual Studio ~~2017~~ 2019** with latest updates. :coffee:

### Ruler shortcuts:

- **Space** and **Double click**: will toggle direction of Ruler 
- **Arrow keys**: move Ruler (+ **Shift** for small step)
- **Ctrl** + **Arrow keys**: resize Ruler (+ **Shift** for small step)
- **Ctrl** + **S**: open resize form

### Command line parameters (optional):
In current implementation, you can either not use parameters (just start app) or use all parameters (you cannot used parameters selectively).
Parameters are intended to be used internally to duplicate Ruler by passing configuration to new instance, but you can use them to save prefered Ruler configuration too
The way parameters are passed to program and parsed might change in future.

`ruler.exe [Width:int] [Height:int] [IsVertical:bool] [Opacity:double] [ShowToolTip:bool] [IsLocked:bool] [TopMost:bool]`

Example:
`ruler.exe 100 50 false 0.6 true false true`

*Gifs are made using LICEcap http://www.cockos.com/licecap/*