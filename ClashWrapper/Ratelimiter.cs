using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClashWrapper
{
    internal class Ratelimiter
    {
        private readonly int _requests;
        private readonly long _time;
        private readonly SemaphoreSlim _semaphore;

        private double _startTime;
        private int _count;

        public Ratelimiter(int requests, long time)
        {
            _requests = requests;
            _time = time;
            _semaphore = new SemaphoreSlim(1);
        }

        public async Task WaitAsync()
        {
            await _semaphore.WaitAsync();

            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (now - _startTime > _time)
            {
                _startTime = now;
            }

            if (_count == _requests)
            {
                var delay = 100 + now - _startTime > _time ? 0 : _startTime + _time - now;
                await Task.Delay(TimeSpan.FromMilliseconds(delay));

                _count = 0;
            }

            _count++;
            _semaphore.Release();
        }
    }
}
