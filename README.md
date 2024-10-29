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
 
 However, `DayBase.GetDate` assumes the project name ends with (at least) 2 numbers that it can use to determine which year is running.<br/>
 Make the necessary changes there if you would like to use a different naming scheme.

### Program.cs (The Entry Point)
 Program.cs is the default entry point for a project, but it can be named whatever you want it to be.<br/>
 The main logic happens in `Common.Launcher`, so all the entry point needs to do is call it.

 Calling the `Run()` method will attempt to run the task of whatever day the computer is set to (up to 25).<br/>
 To run a specific day, simply supply it as the parameter to the same method.

### Day
 The class containing the logic to solve the task(s) of a given day.
 
 They *must* be named `Day##` because that is the name that the launcher will look for when creating the instance.<br/>
 Also, DayBase assumes the class name ends with 2 numbers that it can use to determine which day is running.<br/>
 Make the necessary changes there if you would like to use a different naming scheme.

 It is also required that the class inherits from DayBase, which will handle setup and teardown, and provides convenient methods for handling the answer.<br/>
 Visual Studio will prompt you to implement the abstract method `Run(string[] input)` which is where you put all the necessary code.

### PrintResult
 This DayBase method will allow you to print the answer to the console with (optional) flavor text before and/or after it.<br/>
 It will also save the answer in the correct location for automatic submission once execution has finished (presuming a SessionID has been provided).

 By default, the assumption is that the first call to this method is the answer to part 1, and the next is for part 2.<br/>
 However, each overload has an optional parameter where you can specify which part the answer is for.

 If you do not want the extra functionality, you can also save your answer directly to Results[0] (part 1) or Results[1] (part 2).