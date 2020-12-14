using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace SandBox
{
    /// <summary>
    /// MainWindowのViewModelです。
    /// </summary>
    public class MainWindowVm : IDisposable
    {
        public string Directory { get; }
        public IReactiveProperty<string> FilterName { get; }
        public IReactiveProperty<string> FilterExtension { get; }
        public IReactiveProperty<int> CollectedSize { get; }
        public IReactiveProperty<int> VirtualProxySize { get; }
        public IReactiveProperty<int> StagedSize { get; }
        public VirtualCollectionSource<FileModel> VirtualSource { get; }
        
        private IReadOnlyReactiveProperty<string> LowerFilterName { get; }
        private IReadOnlyReactiveProperty<string> LowerFilterExtension { get; }
        
        private IList<IDisposable> Disposables { get; } = new List<IDisposable>();
        
        public MainWindowVm(string directory , 
                            VirtualCollectionSource<FileModel> virtualSource)
        {
            Directory        = directory;
            VirtualSource    = virtualSource;
            FilterName       = new ReactivePropertySlim<string>().AddTo(Disposables);
            FilterExtension  = new ReactivePropertySlim<string>().AddTo(Disposables);
            CollectedSize    = new ReactivePropertySlim<int>().AddTo(Disposables);
            StagedSize       = new ReactivePropertySlim<int>().AddTo(Disposables);
            VirtualProxySize = new ReactivePropertySlim<int>().AddTo(Disposables);

            LowerFilterName = FilterName.Select(x => x?.ToLower()).ToReadOnlyReactivePropertySlim().AddTo(Disposables);
            LowerFilterExtension = FilterExtension.Select(x => x?.ToLower()).ToReadOnlyReactivePropertySlim().AddTo(Disposables);

            // 仮想Cコンテナにフィルター関数を設定します。
            virtualSource.SetFilter(　x=>　x.Filter(LowerFilterName.Value,LowerFilterExtension.Value)　);
            
            // 検索ボックスの入力からトリガしてフィルターを実行します。
            FilterName
                .Merge(FilterExtension)
                .Throttle(TimeSpan.FromMilliseconds(200), UIDispatcherScheduler.Default)
                .Subscribe(async _ => await VirtualSource.ResetCollection())
                .AddTo(Disposables);

            // 画面表示用に定期更新します
            Observable.Interval(TimeSpan.FromMilliseconds(100), UIDispatcherScheduler.Default)
                .Subscribe(_ =>
                {
                    CollectedSize.Value    = VirtualSource.SourceSize;
                    StagedSize.Value       = VirtualSource.Items.Count;
                    VirtualProxySize.Value = VirtualSource.ProxySize;
                }).AddTo(Disposables);
        }

        public void Dispose()
        {
            foreach (var disposable in Disposables)
                disposable.Dispose();
        }
    }
}