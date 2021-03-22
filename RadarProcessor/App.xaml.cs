using System.Windows;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using RadarProcessor.Services;
using RadarProcessor.ViewModels;

namespace RadarProcessor
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IDialogService, DialogService>();
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<StatusViewModel>();
        }
    }
}