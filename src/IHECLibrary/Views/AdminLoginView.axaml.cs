using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace IHECLibrary.Views
{
    public partial class AdminLoginView : UserControl
    {
        public AdminLoginView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
