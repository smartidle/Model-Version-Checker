using System.Configuration;
using System.Data;
using System.Windows;

namespace ModelVersionChecker;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 全局异常捕获，防止静默崩溃
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            MessageBox.Show($"发生未处理的异常:\n\n{ex?.Message}\n\n{ex?.StackTrace}",
                "Model Version Checker - 错误", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        DispatcherUnhandledException += (s, args) =>
        {
            MessageBox.Show($"发生未处理的UI异常:\n\n{args.Exception.Message}\n\n{args.Exception.StackTrace}",
                "Model Version Checker - 错误", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (s, args) =>
        {
            MessageBox.Show($"发生未处理的任务异常:\n\n{args.Exception?.InnerException?.Message}\n\n{args.Exception?.InnerException?.StackTrace}",
                "Model Version Checker - 错误", MessageBoxButton.OK, MessageBoxImage.Error);
            args.SetObserved();
        };
    }
}
