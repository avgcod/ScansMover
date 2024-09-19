using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Scans_Mover.Components
{
    public class LoadingSpinner : TemplatedControl
    {
        public static readonly StyledProperty<bool> IsLoadingProperty =
            AvaloniaProperty.Register<LoadingSpinner, bool>(nameof(IsLoading), false);

        public bool IsLoading
        {
            get { return GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public static readonly StyledProperty<double> DiameterProperty =
            AvaloniaProperty.Register<LoadingSpinner, double>(nameof(Diameter),25.00);

        public double Diameter
        {
            get { return GetValue(DiameterProperty); }
            set { SetValue(DiameterProperty, value); }
        }

        public static readonly StyledProperty<double> ThicknessProperty =
            AvaloniaProperty.Register<LoadingSpinner, double>(nameof(Thickness),2.00);

        public double Thickness
        {
            get { return GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }

        public static readonly StyledProperty<IBrush> ColorProperty =
            AvaloniaProperty.Register<LoadingSpinner, IBrush>(nameof(Color), Brushes.Black);

        public IBrush Color
        {
            get { return GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
    }
}
