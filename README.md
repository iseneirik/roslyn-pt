# Package Templates (PT) - Extension of C\#

This Repository is a fork of [dotnet/roslyn](https://github.com/dotnet/roslyn). It is my starting point for extending C\# with PT. Since Roslyn is under constant development, it is nice to have a starting point that is constant, hence my own fork.

## Plan

The plan is to implement this with a TDD (test driven development) approach. By creating tests that gradually grow in complexity that describe valid PT code, and implementing this so that the tests pass, I am able to gradually extend the C\# compiler to accept PT code. Starting off with tests defining lexing and parsing, moving on to syntax analysis and then continue to semantics and finally ending with the emit phase. 

## Building, Debugging and Testing on Windows

### Working with the code

Using the command line Roslyn can be developed using the following pattern:

1. Clone https://github.com/dotnet/roslyn
1. Run Restore.cmd 
1. Run Build.cmd
1. Run Test.cmd

### Developing with Visual Studio 2017

1. [Visual Studio 2017 Update 3](https://www.visualstudio.com/vs/)
    - Ensure C#, VB, MSBuild and Visual Studio Extensibility are included in the selected work loads
    - Ensure Visual Studio is on Version "15.3" or greater
1. [.NET Core SDK 2.0](https://www.microsoft.com/net/download/core)
1. [PowerShell 3.0 or newer](https://docs.microsoft.com/en-us/powershell/scripting/setup/installing-windows-powershell). If you are on Windows 10, you are fine; you'll only need to upgrade if you're on Windows 7. The download link is under the "upgrading existing Windows PowerShell" heading.
1. Run Restore.cmd
1. Open Roslyn.sln

If you already installed Visual Studio and need to add the necessary work loads or move to update 3
do the following:

- Open the vs_installer.  Typically located at "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vs_installer.exe"
- The Visual Studio installation will be listed under the Installed section
- Click on the hamburger menu, click Modify 
- Choose the workloads listed above and click Modify

### Running the project

To compile and run a new instance of VisualStudio with the extended compiler, do the following:

1. Open VisualStudio17
2. Open Roslyn.sln
3. Find "CompilerExtension" in the solution explorer and set it as startup project
4. Hit F5

### Running the tests

To compile the project and run tests on it, do the following:

1. Open VisualStudio17
2. Open Compilers.sln
3. Right-click the solution in the solution explorer and select "Run Unit Tests"
4. Alternatively, right-click a class and select "Run Unit Tests"

#### dotnet/roslyn

If you wish to see the original and maintained version of this README, head over to [dotnet/roslyn](https://github.com/dotnet/roslyn). Here you will find all information that is to know about Roslyn and its use.