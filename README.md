ruler
=====
Simple on-screen pixel ruler.

<br />
<a href="http://www.softpedia.com/progDownload/Ruler-andrijac-Download-256095.html"><img alt="download page" src="https://raw.githubusercontent.com/andrijac/ruler/master/img/softpedia_download_large_shadow.png" /></a>
<br />
<br />
<img src="https://github.com/andrijac/ruler/raw/master/img/ruler.gif">

**Releases**
https://github.com/andrijac/ruler/releases

### Legacy
Ruler is fork of small utility application originally developed by Jeff Key:

- http://ruler.codeplex.com/ :floppy_disk:
- ~~http://weblogs.asp.net/jkey/archive/2004/09/01/224433.aspx~~ :skull:
- ~~http://www.sliver.com/dotnet/ruler/~~ :skull:

Support for .NET 2.0 is kept. :thumbsup:

**<a name="newfeatures" href="https://github.com/andrijac/ruler/blob/master/NewFeatures.md">Added features in this fork.</a>**

### Ruler shortcuts:

- **Space** and **Double click**: will toggle direction of Ruler 
- **Arrow keys**: move Ruler
- **Ctrl** + **Shift** + **Arrow keys**: resize Ruler

### Contributions
If want to contribute to project, please follow the rules:
1. **Zero-Warning policy**. There should be no C# compiler warnings in PR. :pray:
2. **Use StyleCop**. Use StyleCop Visual Studio Extension (by Chris Dahlberg) to check for StyleCop warnings https://marketplace.visualstudio.com/items?itemName=ChrisDahlberg.StyleCop. Zero-warning policy extends to StyleCop warnings. :cop:
3. Keep in mind that Ruler should be simple tool. It should be disposable, easy to run, easy to close, easy to run in multiple instances. For now, I still refuse to add any kind of additional dependency to project like persistent storage for Ruler settings (which are still, at the moment, few), having a Ruler icon in system tray etc. for sake of keeping Ruler simple. Since Ruler can run multiple instances, features mentioned would add unnecessary complexity to project. :speech_balloon:

### Command line parameters (optional):
In current implementation, you can either not use parameters (just start app) or use all parameters (you cannot used parameters selectively).
Parameters are intended to be used internally to duplicate Ruler by passing configuration to new instance, but you can use them to save prefered Ruler configuration too
The way parameters are passed to program and parsed might change in future.

`ruler.exe [Width:int] [Height:int] [IsVertical:bool] [Opacity:double] [ShowToolTip:bool] [IsLocked:bool] [TopMost:bool]`

Example:
`ruler.exe 100 50 false 0.6 true false true`

*Gifs are made using LICEcap http://www.cockos.com/licecap/*