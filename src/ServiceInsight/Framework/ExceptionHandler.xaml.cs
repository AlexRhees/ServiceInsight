﻿namespace Particular.ServiceInsight.Desktop.Framework
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Anotar.Serilog;
    using ExtensionMethods;
    using Mindscape.Raygun4Net;

    public partial class ExceptionHandler
    {
        static RaygunClient client;

        static ExceptionHandler()
        {
            var assemblyInfo = typeof(App).Assembly.GetAttribute<AssemblyInformationalVersionAttribute>();
            client = new RaygunClient("uX5c/PiCVqF31xlEm3jShA==")
            {
                ApplicationVersion = assemblyInfo != null ? assemblyInfo.InformationalVersion : "Unknown Version"
            };
        }

        public static void Attach()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Dispatcher.CurrentDispatcher.UnhandledException += CurrentDispatcher_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
                HandleException(ex);
        }

        static void CurrentDispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception);
            e.Handled = true;
        }

        static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            HandleException(e.Exception);
        }

        public static Action<Exception> HandleException = ex =>
        {
            if (ex == null)
                return;

            var rootError = ex.GetBaseException();

            LogException(rootError);
            ShowExceptionDialog(rootError);
        };

        static void LogException(Exception ex)
        {
            if (ex == null)
                return;

            var baseError = ex.GetBaseException();

            LogTo.Error(ex, "An unhandled exception occurred. Error message is {Message}.", baseError.Message);
        }

        static void ShowExceptionDialog(Exception ex)
        {
            var dialog = new ExceptionHandler
            {
                ErrorDetails =
                {
                    Text = ex.ToString()
                },
                Exception = ex
            };
            if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsLoaded)
            {
                dialog.Owner = Application.Current.MainWindow;
            }
            dialog.ShowDialog();
        }

        public ExceptionHandler()
        {
            InitializeComponent();
        }

        Exception Exception { get; set; }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        void ExitClick(object sender, RoutedEventArgs e)
        {
            Environment.Exit(1);
        }

        void CopyClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ErrorDetails.Text);
        }

        void ReportClick(object sender, RoutedEventArgs e)
        {
            client.SendInBackground(Exception);
            ((Button)sender).IsEnabled = false;
        }
    }
}