using Avalonia;
using Avalonia.Controls;

namespace Scans_Mover.Components
{
    public partial class LocationsTab : UserControl
    {
        public static readonly StyledProperty<string> MainFolderProperty =
        AvaloniaProperty.Register<LocationsTab, string>(nameof(MainFolder), defaultValue: string.Empty);

        public static readonly StyledProperty<string> DeliveriesFolderProperty =
        AvaloniaProperty.Register<LocationsTab, string>(nameof(DeliveriesFolder), defaultValue: string.Empty);

        public static readonly StyledProperty<string> RMAsFolderProperty =
        AvaloniaProperty.Register<LocationsTab, string>(nameof(RMAsFolder), defaultValue: string.Empty);

        public static readonly StyledProperty<string> ShippingLogsFolderProperty =
        AvaloniaProperty.Register<LocationsTab, string>(nameof(ShippingLogsFolderProperty), defaultValue: string.Empty);

        public static readonly StyledProperty<string> ServiceFolderProperty =
        AvaloniaProperty.Register<LocationsTab, string>(nameof(ServiceFolder), defaultValue: string.Empty);

        public LocationsTab()
        {
            InitializeComponent();
        }

        public string MainFolder
        {
            get => GetValue(MainFolderProperty);
            set => SetValue(MainFolderProperty, value);
        }

        public string DeliveriesFolder
        {
            get => GetValue(DeliveriesFolderProperty);
            set => SetValue(DeliveriesFolderProperty, value);
        }

        public string RMAsFolder
        {
            get => GetValue(RMAsFolderProperty);
            set => SetValue(RMAsFolderProperty, value);
        }

        public string ShippingLogsFolder
        {
            get => GetValue(ShippingLogsFolderProperty);
            set => SetValue(ShippingLogsFolderProperty, value);
        }

        public string ServiceFolder
        {
            get => GetValue(ServiceFolderProperty);
            set => SetValue(ServiceFolderProperty, value);
        }
    }
}
