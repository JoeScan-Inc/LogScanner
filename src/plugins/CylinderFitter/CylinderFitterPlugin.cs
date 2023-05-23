using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using LpSolveDotNet;
using MathNet.Numerics.LinearAlgebra.Factorization;
using NLog;
using System;
using System.Diagnostics;

namespace CylinderFitter;

public class CylinderFitterPlugin : ILogModelConsumerPlugin
{
    private bool isInitialized;
    private Fitter? fitter;

    #region IPlugin implementation

    public string Name => "CylinderFitter";
    public uint VersionMajor => 1;
    public uint VersionMinor => 0;
    public uint VersionPatch => 0;
    public Guid Id => Guid.Parse("{98D11596-4977-437E-A93A-9F33D98813B2}");

    public event EventHandler<PluginMessageEventArgs>? PluginMessage;

    #endregion

    #region IDisposable implementation

    public void Dispose()
    {
    }

    #endregion

    #region ILogModelConsumerPlugin

    public void Initialize()
    {
        try
        {
            isInitialized = LpSolve.Init(); // initialize the lp_solve library
            var version = LpSolve.LpSolveVersion;
            fitter = new Fitter();
            fitter.FitterMessage += (sender, args) =>
            {
                if (args is FitterMessageEventArgs fma)
                {
                    OnPluginMessage(new PluginMessageEventArgs(fma.Level, fma.Message));
                }
            };
            OnPluginMessage(new PluginMessageEventArgs(LogLevel.Info,
                $"Initialized CylinderFitterPlugin with LPSolve {version.Major}.{version.Minor}.{version.Revision}.{version.Build}."));
        }
        catch (Exception e)
        {
            OnPluginMessage(new PluginMessageEventArgs(LogLevel.Error, "Failed to initialize CylinderFitterPlugin."));
            isInitialized = false;
        }
    }

    public bool IsInitialized => isInitialized;

    public void Cleanup()
    {
    }

    public void Consume(LogModelResult res)
    {
        if (!isInitialized || !res.IsValidModel)
        {
            return;
        }

        var solution = fitter!.RunFit(res.LogModel!);
    }

    #endregion

    #region Lifecycle

    public CylinderFitterPlugin()
    {
        isInitialized = false;
    }

    #endregion

    #region Event Invocation

    private void OnPluginMessage(PluginMessageEventArgs e)
    {
        PluginMessage?.Invoke(this, e);
    }

    #endregion
}
