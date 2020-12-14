using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Windows;

namespace SandBox
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static string[] _commandLineArgs;
        [STAThread]
        public static void Main(string[] args)
        {
            _commandLineArgs = args;
            var app = new App();
            app.Run();
        }

        protected override async  void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // C:ブラウジングするディレクトリをコマンドラインから取得します。
            var targetDirectory = _commandLineArgs.Any() ? Path.GetFullPath(_commandLineArgs[0]) : "C:\\";
            
            // データソースの作成-------------------------------------------------------
            var dataSource = new DataSource<FileModel>();

            // データの列挙に利用する IEnumeratorです。
            var dataSourceEnumerator = FileSystemEnumerator
                .EnumerateFiles(targetDirectory)
                .Select(x => new FileModel(x));

            // データソースの非同期読み取りを fire and forgetで開始します。
            _ = dataSource.StartRead(dataSourceEnumerator);

            // 仮想コンテナを作成--------------------------------------------------------
            var defaultCapacityCount = 50;// 仮想コンテナを50にします。

            var virtualSource = new VirtualCollectionSource<FileModel,FileViewModel>(
                dataSource.Loaded,
                x=>new FileViewModel(x), 
                defaultCapacityCount,
                ImmediateScheduler.Instance);
            
            // データ50個読み込まれるまで待ちます。
            dataSource.BlockUntilRead(defaultCapacityCount);
            await virtualSource.ResetCollection();

            // ウィンドウの作成----------------------------------------------------------
            var vm = new MainWindowVm(targetDirectory, virtualSource);
            var window = new MainWindow();
            window.DataContext = vm;
            window.Closed += (_, _1) =>
            {
                vm.Dispose();
                virtualSource.Dispose();
                dataSource.Dispose();
            };
            window.Show();
        }
    }
}