using System.Collections.ObjectModel;
using System.Net.Http;
using PokemonDex.Core.Models;
using PokemonDex.Core.Services;
using PokemonDex.Models;
using WPF.Lib.MVVM;
using WPF.Lib.MVVM.Commands;

namespace PokemonDex.ViewModels;

public sealed class MainViewModel : BaseViewModel
{
    private const int PageSize = 20;
    private readonly IPokemonService _pokemonService;
    private string _searchText = string.Empty;
    private string _statusText = "포켓몬을 불러올 준비가 되었습니다.";
    private bool _isBusy;
    private int _offset;
    private int _total;
    private TypeFilterOption _selectedType;
    private GenerationFilterOption _selectedGeneration;

    public MainViewModel(IPokemonService pokemonService)
    {
        _pokemonService = pokemonService;
        Types = CreateTypeOptions();
        Generations = CreateGenerationOptions();
        _selectedType = Types[0];
        _selectedGeneration = Generations[0];
        SearchCommand = new RelayCommand(ApplyFilters, () => !IsBusy);
        ResetCommand = new RelayCommand(ResetFilters, () => !IsBusy);
        PreviousPageCommand = new RelayCommand(PreviousPage, () => !IsBusy && _offset > 0);
        NextPageCommand = new RelayCommand(NextPage, () => !IsBusy && _offset + Pokemon.Count < _total);
    }

    public ObservableCollection<PokemonCard> Pokemon { get; } = [];
    public IReadOnlyList<TypeFilterOption> Types { get; }
    public IReadOnlyList<GenerationFilterOption> Generations { get; }
    public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }
    public TypeFilterOption SelectedType { get => _selectedType; set => SetProperty(ref _selectedType, value); }
    public GenerationFilterOption SelectedGeneration { get => _selectedGeneration; set => SetProperty(ref _selectedGeneration, value); }
    public string StatusText { get => _statusText; private set => SetProperty(ref _statusText, value); }
    public bool IsBusy
    {
        get => _isBusy;
        private set { if (SetProperty(ref _isBusy, value)) NotifyCommandStates(); }
    }

    public RelayCommand SearchCommand { get; }
    public RelayCommand ResetCommand { get; }
    public RelayCommand PreviousPageCommand { get; }
    public RelayCommand NextPageCommand { get; }
    public Task InitializeAsync() => LoadPageAsync();

    private async void ApplyFilters()
    {
        _offset = 0;
        await LoadPageAsync();
    }

    private async void ResetFilters()
    {
        SearchText = string.Empty;
        SelectedType = Types[0];
        SelectedGeneration = Generations[0];
        _offset = 0;
        await LoadPageAsync();
    }

    private async void PreviousPage() { _offset = Math.Max(0, _offset - PageSize); await LoadPageAsync(); }
    private async void NextPage() { _offset += PageSize; await LoadPageAsync(); }

    private Task LoadPageAsync() => ExecuteAsync(async () =>
    {
        StatusText = "선택한 조건으로 포켓몬을 검색하는 중입니다.";
        var criteria = new PokemonSearchCriteria(SearchText.Trim(), SelectedType.ApiName, SelectedGeneration.Id);
        var page = await _pokemonService.SearchAsync(criteria, _offset, PageSize);
        Pokemon.Clear();
        foreach (var item in page.Items) Pokemon.Add(item);
        _total = page.Total;
        StatusText = page.Items.Count == 0
            ? "검색 결과가 없습니다. 이름은 영문 또는 도감 번호로 입력해 주세요."
            : $"{page.Offset + 1:N0}–{page.Offset + page.Items.Count:N0} / 총 {page.Total:N0}마리";
    });

    private async Task ExecuteAsync(Func<Task> action)
    {
        if (IsBusy) return;
        IsBusy = true;
        try { await action(); }
        catch (HttpRequestException) { StatusText = "PokéAPI에 연결하지 못했습니다. 네트워크 상태를 확인해 주세요."; }
        catch (TaskCanceledException) { StatusText = "요청 시간이 초과되었습니다. 잠시 후 다시 시도해 주세요."; }
        finally { IsBusy = false; NotifyCommandStates(); }
    }

    private void NotifyCommandStates()
    {
        SearchCommand.NotifyCanExecuteChanged();
        ResetCommand.NotifyCanExecuteChanged();
        PreviousPageCommand.NotifyCanExecuteChanged();
        NextPageCommand.NotifyCanExecuteChanged();
    }

    private static IReadOnlyList<TypeFilterOption> CreateTypeOptions() =>
    [
        new("전체 타입", null), new("노말", "normal"), new("불꽃", "fire"), new("물", "water"),
        new("전기", "electric"), new("풀", "grass"), new("얼음", "ice"), new("격투", "fighting"),
        new("독", "poison"), new("땅", "ground"), new("비행", "flying"), new("에스퍼", "psychic"),
        new("벌레", "bug"), new("바위", "rock"), new("고스트", "ghost"), new("드래곤", "dragon"),
        new("악", "dark"), new("강철", "steel"), new("페어리", "fairy")
    ];

    private static IReadOnlyList<GenerationFilterOption> CreateGenerationOptions() =>
    [
        new("전체 세대", null), new("1세대", 1), new("2세대", 2), new("3세대", 3),
        new("4세대", 4), new("5세대", 5), new("6세대", 6), new("7세대", 7),
        new("8세대", 8), new("9세대", 9)
    ];
}
