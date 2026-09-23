using System.Threading;

namespace Neurix.DAL.Data
{
    public sealed class CmsContentRevision
    {
        private long _value;

        public long Current => Interlocked.Read(ref _value);

        public void Advance() => Interlocked.Increment(ref _value);
    }
}
