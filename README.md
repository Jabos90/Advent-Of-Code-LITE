# Advent Of Code LITE
 A slice of my C# platform for automating away the busywork from Advent of Code

## Prerequisites
 * Visual Studio ([2022](https://visualstudio.microsoft.com/vs/))
 * [.NET8](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks?cid=getdotnetsdk)

## How to use
 Included in the repository are my solutions to the first 3 days of Advent of Code 2015.<br/>
 Hopefully, these examples are sufficient to explain the basic usage. If not, I've gone into more detail below:

### Project
 There are no particular requirements on the project itself, as long as all projects use the same .NET version.
 
 However, [DayBase](https://github.com/Jabos90/Advent-Of-Code-LITE/blob/main/Common/DayBase.cs#L225) assumes the project name ends with (at least) 2 numbers that it can use to determine which year is running.<br/>
 Make the necessary changes there if you would like to use a different naming scheme.

### Program.cs
 Program.cs is the default entry point for a project, but it can be named whatever you want it to be.<br/>
 The main logic happens in the [Launcher](https://github.com/Jabos90/Advent-Of-Code-LITE/blob/main/Common/Launcher.cs#L16), so all the entry point needs to do is [call it](https://github.com/Jabos90/Advent-Of-Code-LITE/blob/main/AdventOfCode2015/Program.cs#L1).

 Calling the `Run()` method will attempt to run the task of whatever day the computer is set to (up to 25).<br/>
 To run a specific day, simply supply it as the parameter to the same method.

### Day
 The class containing the logic to solve the task(s) of a given day.
 
 They *must* be named `Day##` because that is the name that [the launcher will look for](https://github.com/Jabos90/Advent-Of-Code-LITE/blob/main/Common/Launcher.cs#L27) when creating the instance.<br/>
 Also, [DayBase](https://github.com/Jabos90/Advent-Of-Code-LITE/blob/main/Common/DayBase.cs#L231) assumes the class name ends with 2 numbers that it can use to determine which day is running.<br/>
 Make the necessary changes there if you would like to use a different naming scheme.

 It is also required that the class inherits from DayBase, which will handle [setup and teardown](https://github.com/Jabos90/Advent-Of-Code-LITE/blob/main/Common/DayBase.cs#L26), and provides convenient methods for [handling the answer](https://github.com/Jabos90/Advent-Of-Code-LITE/blob/main/Common/DayBase.cs#L92).
 Visual Studio will demand that you implement the abstract method [Run(string[] input)](https://github.com/Jabos90/Advent-Of-Code-LITE/blob/main/AdventOfCode2015/Day01.cs#L7) which is where you put all the necessary code.

### PrintResult
 [PrintResult](https://github.com/Jabos90/Advent-Of-Code-LITE/blob/main/Common/DayBase.cs#L50) will allow you to print the answer to the console with (optional) flavor text [before](https://github.com/Jabos90/Advent-Of-Code-LITE/blob/main/Common/DayBase.cs#L60) and/or [after](https://github.com/Jabos90/Advent-Of-Code-LITE/blob/main/Common/DayBase.cs#L70) it.<br/>
 It will also save the answer in the correct location for automatic submission once execution has finished (presuming a SessionID has been provided).

 By default, the assumption is that the first call to this method is the answer to part 1, and the next is for part 2.<br/>
 However, each overload has an optional parameter where you can specify which part the answer is for.

 If you do not want the extra functionality, you can also save your answer directly to Results[0] (part 1) or Results[1] (part 2).
