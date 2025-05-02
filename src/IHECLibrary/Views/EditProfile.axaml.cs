using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace IHECLibrary.Views
{
    public partial class EditProfileView : UserControl
    {
        public EditProfileView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}