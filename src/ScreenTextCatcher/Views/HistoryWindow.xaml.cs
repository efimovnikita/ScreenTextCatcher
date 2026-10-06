using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ScreenTextCatcher.Core.Models;
using ScreenTextCatcher.Core.Services;

namespace ScreenTextCatcher.Views;

public partial class HistoryWindow : Window
{
    private readonly IHistoryService _historyService;
    private List<HistoryItem> _allItems = new();

    public HistoryWindow(IHistoryService historyService)
    {
        InitializeComponent();

        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        LoadItems();
    }

    private void LoadItems()
    {
        _allItems = _historyService.GetRecent(100).ToList();
        FilterItems();
    }

    private void FilterItems()
    {
        var query = TxtSearch.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrEmpty(query))
        {
            LstHistory.ItemsSource = _allItems;
        }
        else
        {
            LstHistory.ItemsSource = _allItems
                .Where(x => x.Text.ToLowerInvariant().Contains(query))
                .ToList();
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        FilterItems();
    }

    private void OnListDoubleClick(object sender, MouseButtonEventArgs e)
    {
        CopySelected();
    }

    private void OnCopySelectedClick(object sender, RoutedEventArgs e)
    {
        CopySelected();
    }

    private void CopySelected()
    {
        if (LstHistory.SelectedItem is HistoryItem item)
        {
            System.Windows.Clipboard.SetText(item.Text);
            System.Windows.MessageBox.Show(
                LocalizationManager.GetString("Loc_HistoryCopied"),
                LocalizationManager.GetString("Loc_HistoryTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    private void OnClearHistoryClick(object sender, RoutedEventArgs e)
    {
        var result = System.Windows.MessageBox.Show(
            LocalizationManager.GetString("Loc_ClearHistoryConfirm"),
            LocalizationManager.GetString("Loc_ClearHistoryTitle"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _historyService.Clear();
            LoadItems();
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
