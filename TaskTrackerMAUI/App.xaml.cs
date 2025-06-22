using TaskTrackerMAUI.Services;
using TaskTrackerMAUI.Views;
using Plugin.LocalNotification;
using System.Diagnostics;
using System;

namespace TaskTrackerMAUI;

public partial class App : Application
{
    public App(IThemeService themeService)
    {
        InitializeComponent();
        MainPage = new AppShell();

        LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;
        LocalNotificationCenter.Current.NotificationReceived += OnNotificationReceived;
    }

    private void OnNotificationReceived(Plugin.LocalNotification.EventArgs.NotificationEventArgs e)
    {
        Debug.WriteLine($"[DEBUG] Notification Received: {e.Request.NotificationId}, Title: {e.Request.Title}, ReturningData: {e.Request.ReturningData}");
    }

    private async void OnNotificationActionTapped(Plugin.LocalNotification.EventArgs.NotificationActionEventArgs e)
    {
        Debug.WriteLine($"[DEBUG] Notification Action Tapped: {e.Request.NotificationId}, ActionId: {e.ActionId}, ReturningData: {e.Request.ReturningData}");
        if (e.Request.ReturningData != null && int.TryParse(e.Request.ReturningData, out int taskId))
        {
            if (taskId > 0 && MainPage is Shell shell)
            {
                await shell.Dispatcher.DispatchAsync(async () =>
                {
                    try { await Shell.Current.GoToAsync($"{nameof(TaskDetailPage)}?taskId={taskId}"); Debug.WriteLine($"[DEBUG] Navigating to TaskDetailPage for Task ID: {taskId}"); }
                    catch (Exception ex) { Debug.WriteLine($"[ERROR] Failed to navigate from notification tap: {ex.Message}"); }
                });
            }
            else { Debug.WriteLine("[ERROR] MainPage is not Shell or TaskID is invalid, cannot navigate from notification."); }
        }
    }

    protected override async void OnStart()
    {
        base.OnStart();
        bool permissionGranted = await LocalNotificationCenter.Current.RequestNotificationPermission();
        Debug.WriteLine($"[DEBUG] Notification permission status on start: {permissionGranted}");

        if (!permissionGranted)
        {
            Debug.WriteLine("[WARNING] Notification permission was not granted on start.");
        }
    }
}