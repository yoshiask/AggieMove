using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AggieMove.Controls
{
    [TemplatePart(Name = nameof(PrimaryContentControl))]
    [TemplatePart(Name = nameof(SecondaryContentControl))]
    public sealed partial class DualPanelPage : Control
    {
        private DependencyObject PrimaryContentControl = null;
        private DependencyObject SecondaryContentControl = null;

        public DualPanelPage()
        {
            this.DefaultStyleKey = typeof(DualPanelPage);
            SizeChanged += DualPanelPage_SizeChanged;
        }

        public object PrimaryContent
        {
            get => GetValue(PrimaryContentProperty);
            set => SetValue(PrimaryContentProperty, value);
        }
        public static readonly DependencyProperty PrimaryContentProperty = DependencyProperty.Register(
            nameof(PrimaryContent), typeof(object), typeof(DualPanelPage), new PropertyMetadata(null));

        public object SecondaryContent
        {
            get => GetValue(SecondaryContentProperty);
            set => SetValue(SecondaryContentProperty, value);
        }
        public static readonly DependencyProperty SecondaryContentProperty = DependencyProperty.Register(
            nameof(SecondaryContent), typeof(object), typeof(DualPanelPage), new PropertyMetadata(null));

        public DataTemplate PrimaryContentTemplate
        {
            get => (DataTemplate)GetValue(PrimaryContentTemplateProperty);
            set => SetValue(PrimaryContentTemplateProperty, value);
        }
        public static readonly DependencyProperty PrimaryContentTemplateProperty = DependencyProperty.Register(
            nameof(PrimaryContentTemplate), typeof(DataTemplate), typeof(DualPanelPage), new PropertyMetadata(null));

        public DataTemplate SecondaryContentTemplate
        {
            get => (DataTemplate)GetValue(SecondaryContentTemplateProperty);
            set => SetValue(SecondaryContentTemplateProperty, value);
        }
        public static readonly DependencyProperty SecondaryContentTemplateProperty = DependencyProperty.Register(
            nameof(SecondaryContentTemplate), typeof(DataTemplate), typeof(DualPanelPage), new PropertyMetadata(null));

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PrimaryContentControl = GetTemplateChild(nameof(PrimaryContentControl));
            SecondaryContentControl = GetTemplateChild(nameof(SecondaryContentControl));
        }

        private void DualPanelPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            string state = e.NewSize.Width > e.NewSize.Height ? "Wide" : "Tall";
            VisualStateManager.GoToState(this, state, true);
        }
    }
}
