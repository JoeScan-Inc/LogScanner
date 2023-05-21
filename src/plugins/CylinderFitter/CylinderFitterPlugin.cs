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

    private void MessageCallback(IntPtr lp, IntPtr userhandle, string Buf)
    {
        OnPluginMessage(new PluginMessageEventArgs(LogLevel.Trace, StripLastNewline(Buf)));
    }

    

    public void Consume(LogModelResult logModel)
    {
        if (!isInitialized || !logModel.IsValidModel)
        {
            return;
        }
        var sw = Stopwatch.StartNew();
        int ncol = 2 + 3;
        using var solver = LpSolve.make_lp(0, ncol); // not using rows yet
        solver.put_logfunc(MessageCallback, IntPtr.Zero);
        solver.set_verbose(lpsolve_verbosity.NORMAL);
        try
        {
            solver.set_col_name(1, "OffsetX");
            solver.set_col_name(2, "OffsetY");
            solver.set_col_name(3, "SlopeX");
            solver.set_col_name(4, "SlopeY");
            solver.set_col_name(5, "Radius");
            var row = new double[ncol + 1];
            solver.set_add_rowmode(true);
            // build up solver matrix
            foreach (var section in logModel.LogModel!.Sections)
            {
                foreach (var p in section.ModeledProfile)
                {
                    var pt = new Point2D((float)(p.X - section.CentroidX), (float)(p.Y - section.CentroidY), 0);
                    row[1] = pt.X;
                    row[2] = pt.Y;
                    row[3] = -pt.X * section.SectionCenter;
                    row[4] = -pt.Y * section.SectionCenter;
                    row[5] = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);

                    if (!solver.add_constraint(row, LpSolveDotNet.lpsolve_constr_types.LE,
                            pt.X * pt.X + pt.Y * pt.Y + section.CentroidX * pt.X + section.CentroidY * pt.Y))
                    {
                        throw new Exception("add_constraint() failed");
                    }
                }
            }

            solver.set_add_rowmode(false);
            solver.set_obj_fn(new[] { 0, 0, 0, 0, 0, 1.0 });
            solver.set_maxim();

            for (int i = 1; i < ncol; i++)
            {
                solver.set_bounds(i, -100000.0, 100000.0);
            }

            //set an abort function that is periodically checked 
            // LPSolveDotNet.put_abortfunc(solver, CancellationCallback, 0);
            // now let lpsolve calculate a solution
            var res = solver.solve();
            if (res != LpSolveDotNet.lpsolve_return.OPTIMAL)
            {
                throw new Exception("solve() failed");
            }

            // a solution is calculated, now lets get some results
            var results = new double[ncol];
            
            solver.get_variables(results);
            var sol = new FitterSolution()
            {
                Radius = results[4],
                XOffset = -results[0],
                YOffset = -results[1],
                XSlope = -results[2],
                YSlope = -results[3]
            };
            sw.Stop();
            OnPluginMessage(new PluginMessageEventArgs(LogLevel.Info,
                $"Cylinder model solved for log # {logModel.LogNumber} in {sw.ElapsedMilliseconds} ms."));


            OnPluginMessage(new PluginMessageEventArgs(LogLevel.Trace, $"Radius: { sol.Radius }, XOffset: { sol.XOffset}, YOffset: { sol.YOffset}, XSlope: { sol.XSlope}, YSlope: { sol.YSlope}"));
            
        }
        catch (Exception e)
        {
            OnPluginMessage(new PluginMessageEventArgs(LogLevel.Error,
                $"Failed to solve cylinder model for log {logModel.LogNumber}."));
        }
    }

    #endregion

    #region Lifecycle

    public CylinderFitterPlugin()
    {
        isInitialized = false;
    }

    #endregion

    #region Event Invocation

    protected virtual void OnPluginMessage(PluginMessageEventArgs e)
    {
        PluginMessage?.Invoke(this, e);
    }

    #endregion

    #region Helpers

    private static string StripLastNewline(string s)
    {
        if (s.EndsWith("\n"))
        {
            return s.Substring(0, s.Length - 1);
        }
        return s;
    }
    #endregion
}
