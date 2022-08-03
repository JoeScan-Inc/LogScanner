using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.LogReview.Interfaces;
using JoeScan.LogScanner.LogReview.Models;
using JoeScan.LogScanner.LogReview.Navigator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JoeScan.LogScanner.LogReview.SectionTable;

public class SectionTableViewModel : Screen
{
    public ILogModelObservable Model { get; }

    public SectionTableViewModel(ILogModelObservable model)
    {
        Model = model;
    }


   

   
}
