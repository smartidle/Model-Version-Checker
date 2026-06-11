using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ModelVersionChecker.Helpers;
using ModelVersionChecker.Services;
using ModelVersionChecker.ViewModels;

namespace ModelVersionChecker.Views;

/// <summary>
/// MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        // 初始化服务
        var httpClient = HttpClientSingleton.Instance;
        var lang = new LanguageService();
        var factory = new ProviderServiceFactory(httpClient);
        var exportService = new ExportService();

        // 创建并绑定 ViewModel
        _viewModel = new MainViewModel(factory, exportService, lang);
        DataContext = _viewModel;
    }

    /// <summary>
    /// PasswordBox 密码变更时同步到 ViewModel
    /// </summary>
    private void ApiKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && sender is PasswordBox pb)
        {
            vm.ApiKey = pb.Password;
        }
    }

    /// <summary>
    /// 切换 API Key 显示/隐藏
    /// </summary>
    private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton btn)
        {
            if (btn.IsChecked == true)
            {
                ApiKeyTextBox.Text = ApiKeyPasswordBox.Password;
                ApiKeyPasswordBox.Visibility = Visibility.Collapsed;
                ApiKeyTextBox.Visibility = Visibility.Visible;
                btn.Content = "🔒";
            }
            else
            {
                ApiKeyPasswordBox.Password = ApiKeyTextBox.Text;
                ApiKeyTextBox.Visibility = Visibility.Collapsed;
                ApiKeyPasswordBox.Visibility = Visibility.Visible;
                btn.Content = "👁";
            }
        }
    }
}
