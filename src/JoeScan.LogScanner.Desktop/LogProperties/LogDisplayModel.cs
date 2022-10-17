using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using System.Collections.ObjectModel;

namespace JoeScan.LogScanner.Desktop.LogProperties
{
    public  class LogDisplayModel
    {
        private readonly LogModel model;
        public ObservableCollection<string> LogModelProperties { get; set; } = new BindableCollection<string>();

        public LogDisplayModel(LogModel model)
        {
            this.model = model;
            
        }
        
       // public Length Length => model.Length.FromLogScannerUnits(model.)) 

    }

   
}
