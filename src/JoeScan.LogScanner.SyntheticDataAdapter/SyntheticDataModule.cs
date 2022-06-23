using Autofac;
using JoeScan.LogScanner.Core.Interfaces;

namespace JoeScan.LogScanner.SyntheticDataAdapter;
public class SyntheticDataModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SyntheticDataAdapter>().As<IScannerAdapter>();
       
    }
}
