using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using ViennaDotNet.Common.Utils;

namespace ViennaDotNet.Launcher.Views;

internal sealed class TabbedListView<T> : View
{
    private readonly IDataProvider<T> _data;

    private readonly ListView _listView;
    private readonly SpinnerView _spinner;
    private readonly Button _firstPageButton;
    private readonly Button _previousPageButton;
    private readonly Label _pageLabel;
    private readonly Button _nextPageButton;
    private readonly Button _lastPageButton;
    private readonly Label _itemsLabel;

    private readonly ObservableCollection<T> _collection;

    private readonly ReaderWriterLockSlim _collectionLock = new(LockRecursionPolicy.SupportsRecursion);
    private readonly Lock _itemsSkippedWriteLock = new Lock();
    private CancellationTokenSource? _pageLoadTokenSource;

    private int _pageSize = -1;
    private int _itemsSkipped;

    private bool _reachedEndOfData;
    private int _loadingCounter;

    public TabbedListView(IDataProvider<T> data)
    {
        _data = data;

        _collection = [];

        _listView = new ListView()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
            AllowsMarking = true,
            CanFocus = true,
        };
        _listView.SetSource(_collection);
        _listView.DrawComplete += (s, e) =>
        {
            // TODO: not this
            var source = _listView.Source;

            _collectionLock.EnterReadLock();

            try
            {
                for (int i = 0; i < _collection.Count; i++)
                {
                    if (source.IsMarked(i))
                    {
                        source.SetMark(i, false);

                        OnItemSelected?.Invoke(_collection[i], _itemsSkipped + i);

                        break;
                    }
                }
            }
            finally
            {
                _collectionLock.ExitReadLock();
            }
        };

        _spinner = new SpinnerView()
        {
            Visible = false,

            AutoSpin = true,
            Style = new SpinnerStyle.SimpleDotsScrolling(),
            X = 0,
            Y = Pos.AnchorEnd() - 1,
        };

        _firstPageButton = new Button()
        {
            Text = "<<",
            X = 0,
            Y = Pos.AnchorEnd() + 1,
        };
        _firstPageButton.Accepting += (s, e) =>
        {
            e.Handled = true;

            ReachedEndOfData = false;

            lock (_itemsSkippedWriteLock)
            {
                _itemsSkipped = 0;
            }

            ReloadPage();
        };

        _previousPageButton = new Button()
        {
            Text = "<",
            X = Pos.Right(_firstPageButton),
            Y = Pos.Y(_firstPageButton),
        };
        _previousPageButton.Accepting += (s, e) =>
        {
            e.Handled = true;

            ReachedEndOfData = false;

            ChangePage(-_pageSize);
        };

        _pageLabel = new Label()
        {
            Text = "1/?",
            X = Pos.Right(_previousPageButton),
            Y = Pos.Y(_previousPageButton),
        };

        _nextPageButton = new Button()
        {
            Text = ">",
            X = Pos.Right(_pageLabel),
            Y = Pos.Y(_pageLabel),
        };
        _nextPageButton.Accepting += (s, e) =>
        {
            e.Handled = true;

            ChangePage(_pageSize);
        };

        _lastPageButton = new Button()
        {
            Text = ">>",
            X = Pos.Right(_nextPageButton),
            Y = Pos.Y(_nextPageButton),
        };
        _lastPageButton.Accepting += (s, e) =>
        {
            e.Handled = true;

            SkipToLastPage();
        };

        _itemsLabel = new Label()
        {
            Text = "1-?/?",
            X = Pos.Right(_lastPageButton),
            Y = Pos.Y(_lastPageButton),
        };

        Add(_listView, _spinner, _firstPageButton, _previousPageButton, _pageLabel, _nextPageButton, _lastPageButton, _itemsLabel);

        OnPageSizeChanged();
    }

    public event Action<T, int>? OnItemSelected;

    private bool ReachedEndOfData
    {
        get => _reachedEndOfData;
        set
        {
            _nextPageButton.Enabled = !value;
            _lastPageButton.Enabled = !value;
            _reachedEndOfData = value;
        }
    }

    private int LoadingCounter
    {
        get => _loadingCounter;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);

            _loadingCounter = value;

            //_spinner.Visible = value > 0; // for some reason if the spinner is enables, the app really lags
        }
    }

    protected override void OnViewportChanged(DrawEventArgs e)
    {
        base.OnViewportChanged(e);

        OnPageSizeChanged();
    }

    private void ChangePage(int move)
    {
        CancelPageReload();

        lock (_itemsSkippedWriteLock)
        {
            int lastSkipped = _itemsSkipped;
            _itemsSkipped = int.Max(RoundToPage(_itemsSkipped + move), 0);

            if (_itemsSkipped == lastSkipped)
            {
                return;
            }
        }

        ReloadPage();
    }

    private void OnPageSizeChanged()
    {
        int newPageSize = Viewport.Height - 1;

        if (newPageSize == _pageSize)
        {
            return;
        }

        // TODO: read lock?
        if (newPageSize <= 0)
        {
            _collectionLock.EnterWriteLock();
            _collection.Clear();
            _collectionLock.ExitWriteLock();

            _pageSize = 0;

            UpdatePageLabel();
            UpdateItemsLabel(0, _data.Count);
        }
        //else if (newPageSize < _pageSize && newPageSize >= _collection.Count && _collection.Count > 0)
        //{
        //    _pageSize = newPageSize;

        //    UpdatePageLabel();
        //    UpdateItemsLabel(_collection.Count, _data.Count);
        //}
        //else if (newPageSize < _pageSize)
        //{
        //    _collectionLock.EnterWriteLock();
        //    int removeCount = _collection.Count - newPageSize;

        //    for (int i = 0; i < removeCount; i++)
        //    {
        //        _collection.RemoveAt(_collection.Count - 1);
        //    }

        //    _collectionLock.ExitWriteLock();

        //    _pageSize = newPageSize;

        //    UpdatePageLabel();
        //    UpdateItemsLabel(_collection.Count, _data.Count);
        //}
        else
        {
            _pageSize = newPageSize;
            lock (_itemsSkippedWriteLock)
            {
                // "align" to a page
                _itemsSkipped = RoundToPage(_itemsSkipped);
            }

            ReloadPage();
        }
    }

    private void ReloadPage()
    {
        CancelPageReload();

        ReloadPageAsync(_pageLoadTokenSource.Token)
            .Forget();
    }

    private void SkipToLastPage()
    {
        if (ReachedEndOfData)
        {
            return;
        }

        ReachedEndOfData = true;

        if (_data.Count is { } itemCount)
        {
            CancelPageReload();

            lock (_itemsSkippedWriteLock)
            {
                _itemsSkipped = RoundToPage(itemCount - 1);
            }

            ReloadPage();
        }
        else
        {
            CancelPageReload();

            SkipToLastPageAsync(_pageLoadTokenSource.Token)
                .Forget();
        }
    }

    [MemberNotNull(nameof(_pageLoadTokenSource))]
    private void CancelPageReload()
    {
        _pageLoadTokenSource?.Cancel();
        _pageLoadTokenSource = new CancellationTokenSource();
    }

    private async Task ReloadPageAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        _collectionLock.EnterWriteLock();
        LoadingCounter++;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            int? itemCount = _data.Count;

            UpdatePageLabel();
            UpdateItemsLabel(_collection.Count, itemCount);

            _collection.Clear();

            cancellationToken.ThrowIfCancellationRequested();

            bool wasSkippedNegative = false;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await foreach (var item in _data.GetDataAsync(_itemsSkipped, _pageSize, cancellationToken))
                {
                    if (itemCount is not null && itemCount.Value < _itemsSkipped + _pageSize)
                    {
                        ReachedEndOfData = true;
                    }

                    _collection.Add(item);
                    UpdateItemsLabel(_collection.Count, itemCount);
                }

                if (_collection.Count > 0)
                {
                    if (_collection.Count < _pageSize)
                    {
                        ReachedEndOfData = true;
                    }

                    break;
                }

                ReachedEndOfData = true;

                cancellationToken.ThrowIfCancellationRequested();
                lock (_itemsSkippedWriteLock)
                {
                    _itemsSkipped -= _pageSize;
                }

                if (_itemsSkipped < 0)
                {
                    _itemsSkipped = 0;
                    if (wasSkippedNegative)
                    {
                        break;
                    }

                    wasSkippedNegative = true;
                }

                UpdatePageLabel();
            }

            UpdatePageLabel();
            UpdateItemsLabel(_collection.Count, _data.Count);
        }
        finally
        {
            LoadingCounter--;
            _collectionLock.ExitWriteLock();
        }
    }

    private async Task SkipToLastPageAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        _collectionLock.EnterWriteLock();
        LoadingCounter++;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            UpdatePageLabel();
            UpdateItemsLabel(0, null);

            _collection.Clear();

            cancellationToken.ThrowIfCancellationRequested();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!await _data.GetDataAsync(_itemsSkipped, _pageSize, cancellationToken).AnyAsync(cancellationToken))
                {
                    ReloadPage();
                    break;
                }

                lock (_itemsSkippedWriteLock)
                {
                    _itemsSkipped += _pageSize;
                }

                UpdatePageLabel();
                UpdateItemsLabel(0, null);
            }
        }
        finally
        {
            LoadingCounter--;
            _collectionLock.ExitWriteLock();
        }
    }

    private void UpdatePageLabel(int? itemCount = null)
    {
        itemCount ??= _data.Count;

        int currentPage = (_itemsSkipped / _pageSize) + 1;

        if (itemCount is { })
        {
            int lastPageNumber = (itemCount.Value / _pageSize) + 1;
            _pageLabel.Text = $"{currentPage.ToString().PadLeft(GetDigitCount(lastPageNumber), '0')}/{lastPageNumber}";
        }
        else
        {
            _pageLabel.Text = $"{currentPage}/?";
        }
    }

    private void UpdateItemsLabel(int shownItemCount, int? totalItemCount)
    {
        int lastShownItemNumber = _itemsSkipped + shownItemCount;

        int itemDigitCount = GetDigitCount(totalItemCount is null ? lastShownItemNumber : int.Max(lastShownItemNumber, totalItemCount.Value));
        _itemsLabel.Text = $"{(_itemsSkipped + 1).ToString().PadLeft(itemDigitCount, '0')}-{lastShownItemNumber.ToString().PadLeft(itemDigitCount, '0')}/{totalItemCount?.ToString() ?? "?"}";
    }

    private int RoundToPage(int itemIndex)
        => (itemIndex / _pageSize) * _pageSize;

    private static int GetDigitCount(int number)
    {
        number = Math.Abs(number);
        return (number == 0) ? 1 : 1 + (int)Math.Floor(Math.Log10(number));
    }
}
