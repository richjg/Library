using System.Linq.Expressions;
using NUnit.Framework;

namespace LibraryTests
{
    public class LinqExtensionsTests
    {
        [Test]
        public void IndexOf_ReturnsMinus1_WhenNoElements()
        {
            IEnumerable<string> items = new List<string>();
            //act
            var result = items.IndexOf("test");
            //asert
            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public void IndexOf_ReturnsMinus1_WhenElementNotFound()
        {
            IEnumerable<string> items = new List<string> { "item1", "item2", "item3" };
            //act
            var result = items.IndexOf("test");
            //asert
            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public void IndexOf_ReturnsMinus1_WhenElementNotFoundByCase()
        {
            IEnumerable<string> items = new List<string> { "item1", "item2", "item3" };
            //act
            var result = items.IndexOf("ITEM2");
            //asert
            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public void IndexOf_ReturnsIndexOfElements_WhenElementEquals()
        {
            IEnumerable<string> items = new List<string> { "item1", "item2", "item3" };
            //act
            var result = items.IndexOf("item2");
            //asert
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void IndexOf_ReturnsIndexOfElements_WhenElementEqualsUsingComparer()
        {
            IEnumerable<string> items = new List<string> { "item1", "item2", "item3" };
            //act
            var result = items.IndexOf("ITEM2", StringComparer.OrdinalIgnoreCase);
            //asert
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void IndexOf_ReturnsIndexOfElement_WhenItemsAreTypeAnonymousAndFound()
        {
            var items = new[] { new { id = 1 }, new { id = 2 } }.Select(a => a);
            //act
            var result = items.IndexOf(new { id = 2 });

            //asert
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void IndexOf_ReturnsMinus1_WhenItemsAreTypeAnonymousAndNotFound()
        {
            var items = new[] { new { id = 1 }, new { id = 2 } }.Select(a => a);
            //act
            var result = items.IndexOf(new { id = 3 });

            //asert
            Assert.That(result, Is.EqualTo(-1));
        }


        [Test]
        public void IndexOf_ReturnsIndexOfElement_WhenItemsAreTypeInt()
        {
            var items = new[] { 1, 2 }.Select(a => a);
            //act
            var result = items.IndexOf(2);

            //asert
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void IndexOf_ReturnsMinus1_WhenItemsAreTypeIntAndNotFound()
        {
            var items = new[] { 1, 2 }.Select(a => a);
            //act
            var result = items.IndexOf(3);

            //asert
            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public void IndexOf_ReturnsIndexOfElement_WhenItemsAreTypeObjectInstance()
        {
            var item1 = new TestFindByObjectInstance();
            var item2 = new TestFindByObjectInstance();
            var items = new[] { item1, item2 }.Select(a => a);
            //act
            var result = items.IndexOf(item2);

            //asert
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void IndexOf_ReturnsMinus1_WhenItemsAreTypeObjectInstanceAndNotFound()
        {
            var item1 = new TestFindByObjectInstance();
            var item2 = new TestFindByObjectInstance();
            var items = new[] { item1, item2 }.Select(a => a);
            //act
            var result = items.IndexOf(new TestFindByObjectInstance());

            //asert
            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public void IndexOf_ReturnsIndexOfElement_WhenItemsAreTypeObjectEquatable()
        {
            var item1 = new TestFindByObjectComparere { Value = "1" };
            var item2 = new TestFindByObjectComparere { Value = "2" };
            var items = new[] { item1, item2 }.Select(a => a);
            //act
            var result = items.IndexOf(new TestFindByObjectComparere { Value = "2" });

            //asert
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void IndexOf_ReturnsMinus1_WhenItemsAreTypeObjectEquatableAndNotFound()
        {
            var item1 = new TestFindByObjectComparere { Value = "1" };
            var item2 = new TestFindByObjectComparere { Value = "2" };
            var items = new[] { item1, item2 }.Select(a => a);
            //act
            var result = items.IndexOf(new TestFindByObjectComparere { Value = "3" });

            //asert
            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public void AndAlso_ReplacesThePredicateParameter()
        {
            var x1 = Create<int>(x => x == 1);

            //Act
            var x2 = x1.AndAlso(c => c == 2);

            var b1 = x2.Body as BinaryExpression;
            var b1Right = b1!.Right as BinaryExpression;
            var b1RightParam = b1Right!.Left as ParameterExpression;

            Assert.That(b1RightParam!.Name, Is.EqualTo("x"));

            static Expression<Func<T, bool>> Create<T>(Expression<Func<T, bool>> expression) => expression;
        }

        [Test]
        public void AndAlso_ReturnsFalse_WhenSingleAndCombitionIsFalse()
        {
            var x1 = Create<int>(x => x > 1);
            var x2 = x1.AndAlso(c => c < 9);
            //Act

            var result = x2.Compile()(9);

            Assert.That(result, Is.EqualTo(false));

            static Expression<Func<T, bool>> Create<T>(Expression<Func<T, bool>> expression) => expression;
        }

        [Test]
        public void AndAlso_ReturnsTrue_WhenSingleAndCombitionIsTrue()
        {
            var x1 = Create<int>(x => x > 1);
            var x2 = x1.AndAlso(c => c < 9);
            //Act

            var result = x2.Compile()(8);

            Assert.That(result, Is.EqualTo(true));

            static Expression<Func<T, bool>> Create<T>(Expression<Func<T, bool>> expression) => expression;
        }

        [Test]
        public void AndAlso_ReturnsFalse_WhenTheresMultirAndAlsoAndCombitionIsFalse()
        {
            var expr = Create<int>(a => a > 1).AndAlso(b => b > 2).AndAlso(b => b > 3).AndAlso(c => c > 4);
            //Act

            var result = expr.Compile()(2);

            Assert.That(result, Is.EqualTo(false));

            static Expression<Func<T, bool>> Create<T>(Expression<Func<T, bool>> expression) => expression;
        }

        [Test]
        public void AndAlso_ReturnsTrue_WhenTheresMultirAndAlsoAndCombitionIsTrue()
        {
            var expr = Create<int>(a => a > 1).AndAlso(b => b > 2).AndAlso(b => b > 3).AndAlso(c => c > 4);
            //Act

            var result = expr.Compile()(5);

            Assert.That(result, Is.EqualTo(true));

            static Expression<Func<T, bool>> Create<T>(Expression<Func<T, bool>> expression) => expression;
        }

        [Test]
        public async Task ToAsyncEnumerable_ReturnsAsyncEnumerable_WhenEnumerableTIsAEnumerableOfT()
        {
            IEnumerable<int> GetItems()
            {
                yield return 1;
                yield return 2;
            }

            //Act
            var items = GetItems().ToAsyncEnumerable();
            List<int> result = new List<int>();
            await foreach (var item in items)
            {
                result.Add(item);
            }


            //Assert
            Assert.That(result, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public async Task ToAsyncEnumerable_ReturnsAsyncEnumerable_WhenEnumerableTIsAListOfT()
        {
            IEnumerable<int> GetItems()
            {
                yield return 1;
                yield return 2;
            }

            //Act
            var items = GetItems().ToList().ToAsyncEnumerable();
            List<int> result = new List<int>();
            await foreach (var item in items)
            {
                result.Add(item);
            }

            //Assert
            Assert.That(result, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public async Task ToAsyncEnumerable_CanEnumerateMultipleTimes_WhenEnumerableType()
        {
            IEnumerable<int> GetItems()
            {
                yield return 1;
                yield return 2;
            }

            //Act
            var items = GetItems().ToAsyncEnumerable();
            List<int> result = new List<int>();
            await foreach (var item in items)
            {
                result.Add(item);
            }

            await foreach (var item in items)
            {
                result.Add(item);
            }

            //Assert
            Assert.That(result, Is.EqualTo(new[] { 1, 2, 1, 2 }));
        }

        [Test]
        public async Task ToAsyncEnumerable_CanEnumerateMultipleTimes_WhenEnumerableIsListType()
        {
            IEnumerable<int> GetItems()
            {
                yield return 1;
                yield return 2;
            }

            //Act
            var items = GetItems().ToList().ToAsyncEnumerable();
            List<int> result = new List<int>();
            await foreach (var item in items)
            {
                result.Add(item);
            }

            await foreach (var item in items)
            {
                result.Add(item);
            }

            //Assert
            Assert.That(result, Is.EqualTo(new[] { 1, 2, 1, 2 }));
        }

        [Test]
        public async Task AsListAsync_ReturnsList_WhenIAsyncEnumerableIsEmpty()
        {
            async IAsyncEnumerable<int> GetItems()
            {
                await Task.Yield();
                yield break;
            }

            //Act
            var result = await GetItems().AsListAsync();

            //Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task AsListAsync_ReturnsList_WhenIAsyncEnumerableIsNotEmpty()
        {
            async IAsyncEnumerable<int> GetItems()
            {
                await Task.Yield();
                yield return 1;
                yield return 2;
            }

            //Act
            var result = await GetItems().AsListAsync();

            //Assert
            Assert.That(result, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public async Task AsListAsync_ReturnsList_WhenIAsyncEnumerableInOrder()
        {
            async IAsyncEnumerable<int> GetItems()
            {
                await Task.Yield();
                yield return await Task.Run(async () =>
                {
                    await Task.Delay(50);
                    return 1;
                });
                yield return await Task.Run(() =>
                {
                    return 2;
                });
            }

            //Act
            var result = await GetItems().AsListAsync();

            //Assert
            Assert.That(result, Is.EqualTo(new[] { 1, 2 }));
        }

        public class TestFindByObjectInstance
        {

        }

        public class TestFindByObjectComparere : IEquatable<TestFindByObjectComparere>
        {
            public string Value { get; set; } = string.Empty;

            public bool Equals(TestFindByObjectComparere? other)
            {
                if (other == null)
                {
                    return false;
                }

                return Value == other.Value;
            }
        }
    }
}
