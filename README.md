ruler
=====

Fork of small utility application for Windows called Ruler.

<img src="https://github.com/andrijac/ruler/raw/master/img/ruler.gif">

Download working version:
https://github.com/andrijac/ruler/tree/master/Bin

Original project was developed by Jeff Key:

- http://ruler.codeplex.com/ :floppy_disk:
- ~~http://weblogs.asp.net/jkey/archive/2004/09/01/224433.aspx~~ :skull:
- ~~http://www.sliver.com/dotnet/ruler/~~ :skull:

I wanted to add some features and to have it hosted on GitHub.

Support for .NET 2.0 is kept. :thumbsup:

<a name="newfeatures" href="https://github.com/andrijac/ruler/blob/master/NewFeatures.md">New features</a>

### Ruler shortcuts:

- **Space** and **Double click**: will toggle direction of ruler. 
- **Arrow keys**: move ruler

### Command line parameters (optional):
In current implementation, you can either not use parameters (just start app) or use all parameters (you cannot used parameters selectively)
Parameters are intended to be used internally to duplicate ruler, but you can use them to save settings too.
The way parameters are passed to program and parsed might change in future.

`ruler [Width:int] [Height:int] [IsVertical:bool] [Opacity:double] [ShowToolTip:bool] [IsLocked:bool] [TopMost:bool]`

Example:
`ruler 100 50 false 0.6 true false true`

*Gifs are made using LICEcap http://www.cockos.com/licecap/*