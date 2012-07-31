using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UsoundRadio.Common
{
    public static class Match
    {
        public static Match<T> Value<T>(T value)
        {
            return new Match<T>(value);
        }
    }

    public class Match<T>
    {
        private readonly T value;
        public Match(T value)
        {
            this.value = value;
        }

        public Match<T, TResult> With<TResult>(T otherValue, TResult result)
        {
            return new Match<T, TResult>(this.value).With(otherValue, result);
        }

        public Match<T, TResult> With<TResult>(Func<T, bool> predicate, TResult result)
        {
            return new Match<T, TResult>(this.value).With(predicate, result);
        }

        public Match<T, TResult> With<TResult>(Func<T, bool> predicate, Func<T, TResult> resultFetcher)
        {
            return new Match<T, TResult>(this.value).With(predicate, resultFetcher);
        }
    }

    public class Match<T, TResult>
    {
        private readonly T value;
        private readonly List<Tuple<T, TResult>> valueMatchers = new List<Tuple<T, TResult>>(2);
        private readonly List<Tuple<Func<T, bool>, TResult>> predicateMatchers = new List<Tuple<Func<T, bool>, TResult>>(2);
        private readonly List<Tuple<Func<T, bool>, Func<T, TResult>>> predicateFetchers = new List<Tuple<Func<T,bool>, Func<T,TResult>>>();
        private TResult defaultValue;

        public Match(T value)
        {
            this.value = value;
        }

        public Match<T, TResult> With(T otherValue, TResult result)
        {
            valueMatchers.Add(Tuple.Create(otherValue, result));
            return this;
        }

        public Match<T, TResult> With(Func<T, bool> predicate, TResult result)
        {
            predicateMatchers.Add(Tuple.Create(predicate, result));
            return this;
        }

        public Match<T, TResult> With(Func<T, bool> predicate, Func<T, TResult> resultFetcher)
        {
            predicateFetchers.Add(Tuple.Create(predicate, resultFetcher));
            return this;
        }

        public Match<T, TResult> DefaultTo(TResult result)
        {
            defaultValue = result;
            return this;
        }

        public static implicit operator TResult(Match<T, TResult> match)
        {
            if (match == null)
            {
                return default(TResult);
            }
            
            var equality = EqualityComparer<T>.Default;
            var matchingValue = match.valueMatchers.FirstOrDefault(v => equality.Equals(v.Item1, match.value));
            if (matchingValue != null)
            {
                return matchingValue.Item2;
            }

            var matchingPredicate = match.predicateMatchers.FirstOrDefault(p => p.Item1(match.value));
            if (matchingPredicate != null)
            {
                return matchingPredicate.Item2;
            }

            var matchingPredicateFetcher = match.predicateFetchers.FirstOrDefault(p => p.Item1(match.value));
            if (matchingPredicateFetcher != null)
            {
                return matchingPredicateFetcher.Item2.Invoke(match.value);
            }

            return match.defaultValue;
        }
    }
}