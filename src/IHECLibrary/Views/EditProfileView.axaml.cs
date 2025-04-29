using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using IHECLibrary.ViewModels;
using System; // Add this for EventArgs

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
        
        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            
            // Log for debugging
            if (DataContext != null)
            {
                System.Diagnostics.Debug.WriteLine($"EditProfileView DataContext set to: {DataContext.GetType().FullName}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("EditProfileView DataContext is null");
            }
        }
    }
}