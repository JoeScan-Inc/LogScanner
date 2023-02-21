// Copyright(c) JoeScan Inc. All Rights Reserved.
//
// Licensed under the BSD 3 Clause License. See LICENSE.txt in the project
// root for license information.

using System.Reflection;

[assembly: AssemblyProduct("JoeScan.LogScanner")]
[assembly: AssemblyTitle("JoeScan.LogScanner")]
[assembly: AssemblyCompany("JoeScan Inc.")]
[assembly: AssemblyCopyright("Copyright © JoeScan 2023")]
[assembly: AssemblyTrademark("JoeScan")]

[assembly: AssemblyDescription("LogScanner is an application for creating accurate measurements of roundwood (logs), using the JS-20/JS-25/JS-50 series of laser scanners manufactured by JoeScan Inc. ")]
[assembly: AssemblyMetadata("RepositoryUrl", "https://github.com/JoeScan-Inc/LogScanner")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
