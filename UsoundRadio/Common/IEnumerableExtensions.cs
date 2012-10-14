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
            var skip = 0;
            var chunk = sequence.Skip(skip).Take(chunkSize);
            var enumerator = chunk.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return chunk;
                skip += chunkSize;
                enumerator = sequence.Skip(skip).Take(chunkSize).GetEnumerator();
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