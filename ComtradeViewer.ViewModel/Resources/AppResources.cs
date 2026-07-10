using System.Globalization;
using System.Resources;
using System.Threading;

namespace ComtradeViewer.ViewModel.Resources
{
    public static class AppResources
    {
        private static readonly ResourceManager ResourceManager =
            new ResourceManager("ComtradeViewer.ViewModel.Resources.Strings", typeof(AppResources).Assembly);

        public static CultureInfo CurrentCulture { get; private set; } = CultureInfo.InvariantCulture;

        public static string Get(string name)
        {
            return ResourceManager.GetString(name, CurrentCulture) ?? name;
        }

        public static void SetCulture(CultureInfo culture)
        {
            CurrentCulture = culture ?? CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;
        }
    }
}
