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

        public static T RandomElement<T>(this IEnumerable<T> items)
        {
            var random = new Random();
            var collection = items as ICollection<T>;
            if (collection == null)
            {
                collection = new List<T>(items);
            }

            var randomElementIndex = random.Next(0, collection.Count);
            return items.ElementAtOrDefault(randomElementIndex);
        }
    }
}