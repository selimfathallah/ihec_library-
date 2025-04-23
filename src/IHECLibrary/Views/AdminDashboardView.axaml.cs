using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace IHECLibrary.Views
{
    public partial class AdminDashboardView : UserControl
    {
        public AdminDashboardView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
