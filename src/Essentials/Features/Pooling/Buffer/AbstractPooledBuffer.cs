using System;

namespace NanoTweenRootNamespace.Pooling.Buffer
{
    /// <summary>
    /// An abstract class for creating custom buffers to use the <c>Builder</c> pattern.
    /// </summary>
    /// <example>
    /// public class SomeBuilderBuffer : AbstractPooledBuffer<![CDATA[<SomeBuilderBuffer>]]> {}
    /// </example>
    /// <remarks>
    /// Static object pooling is used, so, each buffer of type <c>T</c> must be unique.
    /// </remarks>
    public abstract class AbstractPooledBuffer<T> where T : AbstractPooledBuffer<T>, new()
    {
        protected static T PoolRoot = new();
        
        protected T Next;
        
        /// <summary>
        /// Buffer version. 
        /// </summary>
        public ushort Revision;

        /// <summary>
        /// Takes a buffer from the pool of available buffers.
        /// </summary>
        public static T GetPooled()
        {
            T result;
            if (PoolRoot == null)
            {
                result = new T();
            }
            else
            {
                result = PoolRoot;
                PoolRoot = PoolRoot.Next;
                result.Next = null;
            }
            
            result.Reset();
            result.Validate(result, "The buffer has been received more than once or is in an incorrect state.");

            return result;
        }
        
        /// <summary>
        /// Returns the buffer to the pool and increments <see cref="Revision"/>.
        /// </summary>
        public static void Release(T buffer)
        {
            buffer.Revision++;
            buffer.Reset();
            buffer.Validate(buffer, "Attempting to return a buffer in an invalid state to the pool.");
            
            if (buffer.Revision != ushort.MaxValue)
            {
                buffer.Next = PoolRoot;
                PoolRoot = buffer;
            }
        }
        
        protected void Validate(T buffer, string errorMessage)
        {
            if (buffer.Next != null)
            {
                throw new InvalidOperationException(errorMessage);
            }
        }
        
        /// <summary>
        /// Reset all buffer values.
        /// </summary>
        protected abstract void Reset();
    }
}
