using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Objects;
using System.Linq.Expressions;
using System.Diagnostics.Contracts;
using System.Collections.Concurrent;
using UsoundRadio.Models;

namespace UsoundRadio.Common
{
    public static class Extensions
    {
        private readonly static Random random = new Random();

        public static IEnumerable<T> RandomOrder<T>(this IEnumerable<T> elements)
        {
            var list = elements.ToList();
            while (list.Any())
            {
                var randomIndex = random.Next(0, list.Count);
                yield return list[randomIndex];
                list.RemoveAt(randomIndex);
            }
        }

        public static double Range(this Random random, double max)
        {
            return random.NextDouble() * max;
        }

        public static bool Contains(this string text, string element)
        {
            return Contains(text, element, StringComparison.OrdinalIgnoreCase);
        }

        public static bool Contains(this string text, string element, StringComparison comparison)
        {
            return text.IndexOf(element, comparison) != -1;
        }

        public static bool ContainsAny(this string text, IEnumerable<string> any, StringComparison comparison)
        {
            return any.Any(s => text.IndexOf(s, comparison) != -1);
        }

        public static bool ContainsArtist(this string text, string artist)
        {
            Contract.Requires(text != null);
            if (text.StartsWith(artist, StringComparison.OrdinalIgnoreCase))
            {
                return text.StartsWith(artist + " - ", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public static bool ContainsAlbum(this string text, string album)
        {
            Contract.Requires(text != null);
            if (text.Contains(album, StringComparison.OrdinalIgnoreCase))
            {
                return text.Contains(" - " + album + " - ", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public static double MinMax(this double value, double min, double max)
        {
            var valMinned = Math.Max(value, min);
            var valMinnedAndMaxed = Math.Min(valMinned, max);
            return valMinnedAndMaxed;
        }

        /// <summary>
        /// Big hack to make Linq to Entities find an element in a list.
        /// The following does not work: context.Foos.Where(f => listOfBars.Contains(f.Id))
        /// This method is a work-around hack.
        /// </summary>
        /// <remarks>http://stackoverflow.com/questions/374267/contains-workaround-using-linq-to-entities</remarks>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="query"></param>
        /// <param name="selector"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> WhereIn<TEntity, TValue>(this ObjectQuery<TEntity> query, Expression<Func<TEntity, TValue>> selector, IEnumerable<TValue> collection)
        {
            if (selector == null) throw new ArgumentNullException("selector");
            if (collection == null) throw new ArgumentNullException("collection");
            ParameterExpression p = selector.Parameters.Single();

            if (!collection.Any()) return query;

            IEnumerable<Expression> equals = collection.Select(value =>
               (Expression)Expression.Equal(selector.Body,
                    Expression.Constant(value, typeof(TValue))));

            Expression body = equals.Aggregate((accumulate, equal) =>
                Expression.Or(accumulate, equal));

            return query.Where(Expression.Lambda<Func<TEntity, bool>>(body, p));
        }

        public static bool ToBool(this SongLike likeStatus)
        {
            return likeStatus == SongLike.Like ? true : false;
        }

        public static bool IsShabbat(this DateTime dateTime)
        {
            var isFridayPast5 = dateTime.DayOfWeek == DayOfWeek.Friday && dateTime.Hour >= 17;
            var isSaturdayBefore7 = dateTime.DayOfWeek == DayOfWeek.Saturday && dateTime.Hour <= 19;
            return isFridayPast5 || isSaturdayBefore7;
        }

        public static SongLike ToSongLike(this bool value)
        {
            return value ? SongLike.Like : SongLike.Dislike;
        }

        /// <summary>
        /// Converts a Like database object into a SongLike enum.
        /// This is an extension method; if the Like object is null,
        /// this will return SongLike.None. 
        /// </summary>
        /// <param name="like"></param>
        /// <returns>
        /// If the Like object is null, it returns SongLike.None.
        /// Otherwise, it returns the Like.LikeStatus converted to a SongLike enum.
        /// </returns>
        public static SongLike ToSongLikeEnum(this Like like)
        {
            return Match.Value(like)
                .With(default(Like), SongLike.None)
                .With(l => l != null, l => l.LikeStatus);
        }
    }
}