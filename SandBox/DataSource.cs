using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SandBox
{
    public class DataSource<T> : IDisposable
    {
        public IEnumerable<T> Loaded => _bag;
        private readonly ConcurrentBag<T> _bag = new ConcurrentBag<T>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private bool _started = false;
        private bool _isLoaded = false;

        /// <summary>
        /// データの非同期読み取りを開始します。
        /// enumeratorはワーカースレッドから列挙されます。
        /// </summary>
        /// <returns></returns>
        public async Task StartRead(IEnumerable<T> enumerator)
        {
            if(_started)
                return;
            _started = true;

            var cancelToken = _cancellationTokenSource.Token;
            await Task.Run(() =>
            {
                foreach (var data in enumerator)
                {
                    if (cancelToken.IsCancellationRequested)
                        break;
                    _bag.Add(data);
                }
            },cancelToken);
            _isLoaded = true;
        }

        // 初回のn個のデータ読み込まれるまで実行スレッドをブロックします。
        public void BlockUntilRead(int n)
        {
            while (_bag.Count < n && _isLoaded is false)
            {
                Task.Delay(1).Wait();
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}