using Xamarin.Forms;

namespace SlidingPuzzle
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new SlidingPuzzlePage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
