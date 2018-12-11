# NETworkManager.Core

This folder contains a C# project file that builds sources from the 
NETworkManager path targeting [.NET Core 3](https://blogs.msdn.microsoft.com/dotnet/2018/05/07/net-core-3-and-support-for-windows-desktop-applications/).

The resulting executable is equivalent to the one produced by building the 
main NETworkManager project except that this project will run on 
[.NET Core](https://docs.microsoft.com/dotnet/core/) instead of .NET 
Framework. This project is kept in a separate folder from the primary project 
so that mainline .NET Framework development is not affected. Although 
NETworkManager [works on .NET Core](https://youtu.be/zbsf7e9xm3Y), the framework is still in Preview and some 
important development features (such as WPF designers in Visual 
Studio) are not supported yet.

To build this project, you will need the [.NET Core 3 SDK](https://github.com/dotnet/core-sdk#installers-and-binaries) installed.

## Resources

* [Demo video](https://youtu.be/zbsf7e9xm3Y)
* [.NET Core](https://docs.microsoft.com/dotnet/core/)
* [.NET Core WPF Porting Guidance](https://github.com/dotnet/samples/tree/master/wpf)