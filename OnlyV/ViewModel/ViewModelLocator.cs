namespace OnlyV.ViewModel
{
    using CommonServiceLocator;
    using GalaSoft.MvvmLight.Ioc;
    using OnlyV.Services;

    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    internal class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<ScripturesViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<PreviewViewModel>();

            SimpleIoc.Default.Register<IBibleVersesService, BibleVersesService>();
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        public ScripturesViewModel Scriptures => ServiceLocator.Current.GetInstance<ScripturesViewModel>();

        public SettingsViewModel Settings => ServiceLocator.Current.GetInstance<SettingsViewModel>();

        public PreviewViewModel Preview => ServiceLocator.Current.GetInstance<PreviewViewModel>();
    }
}