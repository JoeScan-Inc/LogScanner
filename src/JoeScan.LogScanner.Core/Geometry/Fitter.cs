using JoeScan.LogScanner.Core.Models;
using LpSolveDotNet;
using NLog;
using System.Diagnostics;

namespace JoeScan.LogScanner.Core.Geometry;

public class Fitter
{
    public bool IsInitialized { get; }

    public event EventHandler FitterMessage;

    public Fitter(string lpSolverPath = ".")
    {
        try
        {
            IsInitialized = LpSolve.Init(); // initialize the lp_solve library
            var version = LpSolve.LpSolveVersion;
            OnFitterMessage(LogLevel.Info, 
                $"Initialized CylinderFitter with LPSolve {version.Major}.{version.Minor}.{version.Revision}.{version.Build}.");
        }
        catch (Exception e)
        {
            OnFitterMessage(LogLevel.Error, "Failed to initialize CylinderFitterPlugin.");
            IsInitialized = false;
        }
    }

    public FitterSolution? RunFit(LogModel model)
    {
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
            foreach (var section in model.Sections)
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
                solver.set_bounds(i, -10000.0, 10000.0);
            }

            // now let lpsolve calculate a solution
            var res = solver.solve();
            if (res != LpSolveDotNet.lpsolve_return.OPTIMAL)
            {
                OnFitterMessage(LogLevel.Error, "solve() failed, no optimal solution found");
                return null;
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
            OnFitterMessage(LogLevel.Info, $"Cylinder model solved for log # {model.LogNumber} in {sw.ElapsedMilliseconds} ms.");
            OnFitterMessage(LogLevel.Trace,
                $"Radius: {sol.Radius}, XOffset: {sol.XOffset}, YOffset: {sol.YOffset}, XSlope: {sol.XSlope}, YSlope: {sol.YSlope}");
            return sol;
        }
        catch (Exception e)
        {
            OnFitterMessage(LogLevel.Error, $"Failed to solve cylinder model for log {model.LogNumber}.");
        }

        return null;
    }

    private void MessageCallback(IntPtr lp, IntPtr userhandle, string Buf)
    {
        OnFitterMessage(LogLevel.Trace, StripLastNewline(Buf));
    }

    #region Helpers

    /// <summary>
    /// Strips the last newline from a string. Needed because the output from
    /// LPSolve contains a trailing newline.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private static string StripLastNewline(string s)
    {
        if (s.EndsWith("\n"))
        {
            return s.Substring(0, s.Length - 1);
        }

        return s;
    }

    #endregion

    private void OnFitterMessage(LogLevel level, string message)
    {
        FitterMessage?.Invoke(this, new FitterMessageEventArgs(level,message));
    }
}

public class FitterMessageEventArgs : EventArgs
{
    public FitterMessageEventArgs(NLog.LogLevel level, string message)
    {
        Level = level;
        Message = message;
    }

    public LogLevel Level { get; }
    public string Message { get; }
}
