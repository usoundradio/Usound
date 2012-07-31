using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UsoundRadio.Common
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> sequence, int chunkSize)
        {
            var lastChunkSize = -1;
            var skip = 0;
            while (lastChunkSize != 0)
            {
                var chunk = sequence.Skip(skip).Take(chunkSize).ToArray();
                yield return chunk;
                lastChunkSize = chunk.Length;
            }
        }
    }
}