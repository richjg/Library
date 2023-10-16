using System.Linq.Expressions;

// ReSharper disable CheckNamespace
namespace Library
{
    public static class LinqExtensions
    {
        private sealed class AsyncEnumerableAdaptor<T> : IAsyncEnumerable<T>, IAsyncEnumerator<T>
        {
            private IEnumerator<T>? enumerator;
            private readonly IEnumerable<T> enumerable;

            public AsyncEnumerableAdaptor(IEnumerable<T> enumerable) => this.enumerable = enumerable;
            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                enumerator = enumerable.GetEnumerator(); //ensure were on the latest enumerator.
                return this;
            }
            public T Current => enumerator!.Current;
            public ValueTask DisposeAsync() => default;
            public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(enumerator!.MoveNext());
        }

        public static IEnumerable<T> AsEnumerable<T>(this T obj)
        {
            yield return obj;
        }

        public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> enumerable)
        {
            return new AsyncEnumerableAdaptor<T>(enumerable);
        }

        public static async Task<List<T>> AsListAsync<T>(this IAsyncEnumerable<T> enumerable, CancellationToken cancellationToken = default)
        {
            var items = new List<T>();
            await foreach (var item in enumerable.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                items.Add(item);
            }

            return items;
        }

        public static IReadOnlyList<T> AsReadOnlyList<T>(this IEnumerable<T> enumerable) => enumerable.ToList();
        public static IReadOnlyList<T> AsReadOnlyList<T>(this List<T> enumerable) => enumerable;
        public async static Task<IReadOnlyList<T>> AsReadOnlyListAsync<T>(this Task<IEnumerable<T>> taskOfT) => (await taskOfT).ToList();
        public async static Task<IReadOnlyList<T>> AsReadOnlyListAsync<T>(this Task<IList<T>> taskOfT) => (await taskOfT).ToList();
        public async static Task<IReadOnlyList<T>> AsReadOnlyListAsync<T>(this Task<List<T>> taskOfT) => await taskOfT;

        public static async Task<List<TSource>> ToListAsync<TSource>(this IEnumerable<Task<TSource>> source)
        {
            var list = new List<TSource>();
            foreach (var item in source)
            {
                list.Add(await item);
            }

            return list;
        }

        public static async Task<List<T2>> SelectAsync<T1, T2>(this Task<List<T1>> listTask, Func<T1, T2> select)
        {
            return (await listTask).Select(s => select(s)).ToList();
        }

        public static async Task<List<T2>> SelectAsync<T1, T2>(this Task<List<T1>> listTask, Func<T1, Task<T2>> select)
        {
            var list = await listTask;

            var result = new List<T2>();
            foreach (var item in list)
            {
                var t2 = await select(item);
                result.Add(t2);
            }

            return result;
        }


        public static async Task<List<T2>> SelectAsync<T1, T2>(this IEnumerable<T1> list, Func<T1, Task<T2>> select)
        {
            var result = new List<T2>();
            foreach (var item in list)
            {
                var t2 = await select(item);
                result.Add(t2);
            }

            return result;
        }

        public static async Task<List<T2>> SelectAsync<T1, T2>(this IEnumerable<T1> list, Func<T1, int, Task<T2>> select)
        {
            var result = new List<T2>();
            var index = 0;
            foreach (var item in list)
            {
                var t2 = await select(item, index);
                result.Add(t2);
                index++;
            }

            return result;
        }


        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? equalityComparer = null)
        {
            HashSet<TKey> knownKeys = new HashSet<TKey>(equalityComparer);

            foreach (TSource element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<IList<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int batchSize)
        {
            if (batchSize < 1)
            {
                batchSize = 1;
            }

            List<TSource> batch = new List<TSource>();
            int count = 0;
            foreach (var item in source)
            {
                batch.Add(item);

                if (++count == batchSize)
                {
                    yield return batch;
                    count = 0;
                    batch = new List<TSource>();
                }
            }

            if (batch?.Any() == true)
            {
                yield return batch;
            }
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> childrenSelector)
        {
            if (source != null)
            {
                foreach (var item in source)
                {
                    yield return item;
                    foreach (var item2 in childrenSelector(item).Flatten(childrenSelector))
                    {
                        yield return item2;
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int IndexOf<T>(this IEnumerable<T> ts, T value, IEqualityComparer<T>? equalityComparer = null)
        {
            int index = 0;
            var comparer = equalityComparer == null ? EqualityComparer<T>.Default : equalityComparer;
            foreach (var item in ts)
            {
                if (comparer.Equals(value, item))
                {
                    return index;
                }

                index++;
            }
            return -1;
        }

        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var p1 = expr1.Parameters.First();
            var expr2BodyWithParam1 = new ParameterReplaceVisitor(expr2.Parameters.First(), p1).Visit(expr2.Body);
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, expr2BodyWithParam1), p1);
        }

        private class ParameterReplaceVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression p1;
            private readonly ParameterExpression p2;

            public ParameterReplaceVisitor(ParameterExpression p1, ParameterExpression p2)
            {
                this.p1 = p1;
                this.p2 = p2;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node == p1)
                {
                    return p2;
                }

                return base.VisitParameter(node);
            }
        }
    }
}
