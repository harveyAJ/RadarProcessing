using CommonServiceLocator;

namespace RadarProcessor.ViewModels
{
    /// <summary>
    ///     This class contains static references to all the view models in the
    ///     application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        public StatusViewModel Status => ServiceLocator.Current.GetInstance<StatusViewModel>();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}