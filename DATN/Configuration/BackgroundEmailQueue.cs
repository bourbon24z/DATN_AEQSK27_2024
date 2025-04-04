using System.Threading.Channels;

namespace DATN.Configuration
{
    public interface IBackgroundEmailQueue
    {
        void EnqueueEmail(Func<Task> emailTask);
        Task<Func<Task>> DequeueAsync(CancellationToken cancellationToken);
    }

    public class BackgroundEmailQueue : IBackgroundEmailQueue
    {
        private readonly Channel<Func<Task>> _queue;

        public BackgroundEmailQueue(int capacity)
        {
            
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<Func<Task>>(options);
        }

        public void EnqueueEmail(Func<Task> emailTask)
        {
            if (emailTask == null)
                throw new ArgumentNullException(nameof(emailTask));
            _queue.Writer.TryWrite(emailTask);
        }

        public async Task<Func<Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            var emailTask = await _queue.Reader.ReadAsync(cancellationToken);
            return emailTask;
        }
    }

}
