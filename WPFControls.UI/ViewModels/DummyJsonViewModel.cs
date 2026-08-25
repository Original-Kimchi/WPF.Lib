using System.Collections.ObjectModel;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using WPF.Lib.Api;
using WPF.Lib.Controls.Abstractions;
using WPF.Lib.Controls.Models;
using WPF.Lib.MVVM;
using WPF.Lib.MVVM.Commands;
using WPFControls.UI.Models.DummyJson;
using WPFControls.UI.Services;

namespace WPFControls.UI.ViewModels;

public sealed class DummyJsonViewModel : BaseViewModel
{
    private const int PageSize = 12;
    private readonly DummyJsonProductService _productService;
    private readonly IToastService _toastService;
    private readonly ILogger<DummyJsonViewModel> _logger;
    private string _searchText = string.Empty;
    private string _statusText = "검색어를 입력하거나 전체 상품을 불러오세요.";
    private bool _isBusy;
    private int _skip;
    private int _total;

    public DummyJsonViewModel(
        DummyJsonProductService productService,
        IToastService toastService,
        ILogger<DummyJsonViewModel> logger)
    {
        _productService = productService;
        _toastService = toastService;
        _logger = logger;

        SearchCommand = new RelayCommand(Search, _ => !IsBusy);
        PreviousPageCommand = new RelayCommand(PreviousPage, () => !IsBusy && _skip > 0);
        NextPageCommand = new RelayCommand(NextPage, () => !IsBusy && _skip + Products.Count < _total);
    }

    public ObservableCollection<DummyJsonProduct> Products { get; } = [];

    public IToastService ToastService => _toastService;

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                NotifyCommandStates();
            }
        }
    }

    public RelayCommand SearchCommand { get; }

    public RelayCommand PreviousPageCommand { get; }

    public RelayCommand NextPageCommand { get; }

    private void Search(object? parameter)
    {
        if (parameter is string query)
        {
            SearchText = query;
        }

        _skip = 0;
        LoadProducts();
    }

    private void PreviousPage()
    {
        _skip = Math.Max(0, _skip - PageSize);
        LoadProducts();
    }

    private void NextPage()
    {
        _skip += PageSize;
        LoadProducts();
    }

    private async void LoadProducts()
    {
        IsBusy = true;
        StatusText = "DummyJSON에서 상품을 불러오는 중입니다.";

        try
        {
            var page = await _productService.SearchAsync(SearchText, _skip, PageSize);

            Products.Clear();
            foreach (var product in page.Products)
            {
                Products.Add(product);
            }

            _total = page.Total;
            StatusText = Products.Count == 0
                ? "검색 결과가 없습니다."
                : $"{_skip + 1:N0}–{_skip + Products.Count:N0} / 총 {_total:N0}개";
        }
        catch (ApiException exception)
        {
            _logger.LogError(exception, "DummyJSON API가 오류를 반환했습니다. StatusCode: {StatusCode}", exception.StatusCode);
            StatusText = $"API 오류가 발생했습니다. HTTP {(int)exception.StatusCode}";
            _toastService.Show("상품 정보를 불러오지 못했습니다.", ToastType.Error);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "DummyJSON API에 연결하지 못했습니다.");
            StatusText = "네트워크 연결을 확인해 주세요.";
            _toastService.Show("DummyJSON에 연결하지 못했습니다.", ToastType.Error);
        }
        catch (TaskCanceledException exception)
        {
            _logger.LogWarning(exception, "DummyJSON API 요청 시간이 초과되었습니다.");
            StatusText = "요청 시간이 초과되었습니다.";
            _toastService.Show("API 요청 시간이 초과되었습니다.", ToastType.Warning);
        }
        finally
        {
            IsBusy = false;
            NotifyCommandStates();
        }
    }

    private void NotifyCommandStates()
    {
        SearchCommand.NotifyCanExecuteChanged();
        PreviousPageCommand.NotifyCanExecuteChanged();
        NextPageCommand.NotifyCanExecuteChanged();
    }
}
