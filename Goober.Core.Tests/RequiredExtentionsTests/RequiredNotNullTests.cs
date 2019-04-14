using AutoFixture;
using Goober.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Goober.Core.Tests.RequiredExtentions
{
    public class RequiredArgumentNotNullTests
    {
        class TestClass
        {
            public int? IntProperty { get; set; }

            public string StringProperty { get; set; }

            public SubClass SubClassProperty { get; set; }

            public IEnumerable<int> EnumerableProperty { get; set; }
        }

        class SubClass
        {
            public int? IntProperty { get; set; }
        }

        [Fact]
        public void IntNotNull_NoExceptionThrows()
        {
            //arrange
            var sut = new Fixture().Build<TestClass>().With(x => x.IntProperty).Create();

            //act+assert
            sut.RequiredArgumentNotNull(() => sut.IntProperty);
        }

        [Fact]
        public void IntIsNull_ShouldThrowArgumentNullException()
        {
            //arrange
            var sut = new Fixture().Build<TestClass>().Without(x=>x.IntProperty).Create();

            //act+assert
            var exc = Assert.Throws<ArgumentNullException>(() => sut.RequiredArgumentNotNull(() => sut.IntProperty));
            Assert.Contains("sut.IntProperty", exc.Message);
        }

        [Fact]
        public void StringNotNull_NoExceptionThrows()
        {
            //arrange
            var sut = new Fixture().Build<TestClass>().With(x => x.StringProperty).Create();

            //act+assert
            sut.RequiredArgumentNotNull(() => sut.StringProperty);
        }

        [Fact]
        public void StringIsNull_ShouldThrowArgumentNullException()
        {
            //arrange
            var sut = new Fixture().Build<TestClass>().Without(x => x.StringProperty).Create();

            //act+assert
            var exc = Assert.Throws<ArgumentNullException>(() => sut.RequiredArgumentNotNull(() => sut.StringProperty));
            Assert.Contains("sut.StringProperty", exc.Message);
        }

        [Fact]
        public void StringIsEmpty_ShouldThrowArgumentNullException()
        {
            //arrange
            var sut = new Fixture().Build<TestClass>().With(x => x.StringProperty, string.Empty).Create();

            //act+assert
            var exc = Assert.Throws<ArgumentNullException>(() => sut.RequiredArgumentNotNull(() => sut.StringProperty));
            Assert.Contains("sut.StringProperty", exc.Message);
        }

        [Fact]
        public void SubClassIsNotNull_NoExceptionThrows()
        {
            //arrange
            var sut = new Fixture().Build<TestClass>().With(x => x.SubClassProperty, new SubClass()).Create();

            //act+assert
            sut.RequiredArgumentNotNull(() => sut.SubClassProperty);
        }

        [Fact]
        public void SubClassIsNull_ShouldThrowArgumentNullException()
        {
            //arrange
            var sut = new Fixture().Build<TestClass>().Without(x => x.SubClassProperty).Create();

            //act+assert
            var exc = Assert.Throws<ArgumentNullException>(() => sut.RequiredArgumentNotNull(() => sut.SubClassProperty));
            Assert.Contains("sut.SubClassProperty", exc.Message);
        }

        [Fact]
        public void SubClassPropertyIsNotNull_NoExceptionThrows()
        {
            //arrange
            var sut = new Fixture().Build<TestClass>().With(x => x.SubClassProperty, new SubClass { IntProperty = 5 }).Create();

            //act+assert
            sut.RequiredArgumentNotNull(() => sut.SubClassProperty.IntProperty);
        }

        [Fact]
        public void SubClassPropertyIsNull_ShouldThrowArgumentNullException()
        {
            //arrange
            var sut = new Fixture().Build<TestClass>().With(x => x.SubClassProperty, new SubClass { IntProperty = null }).Create();

            //act+assert
            var exc = Assert.Throws<ArgumentNullException>(() => sut.RequiredArgumentNotNull(() => sut.SubClassProperty.IntProperty));
            Assert.Contains("sut.SubClassProperty.IntProperty", exc.Message);
        }

        [Fact]
        public void ListPropertyIsNotNull_NoExceptionThrows()
        {
            //arrange
            var fixture = new Fixture();
            var list = fixture.CreateMany<int>();
            var sut = fixture.Build<TestClass>().With(x => x.EnumerableProperty, list).Create();

            //act+assert
            sut.RequiredArgumentNotNull(() => sut.EnumerableProperty);
        }

        [Fact]
        public void ListPropertyIsNull_ShouldThrowArgumentNullException()
        {
            //arrange
            var sut = new Fixture().Build<TestClass>().Without(x => x.EnumerableProperty).Create();

            //act+assert
            var exc = Assert.Throws<ArgumentNullException>(() => sut.RequiredArgumentNotNull(() => sut.EnumerableProperty));
            Assert.Contains("sut.EnumerableProperty", exc.Message);
        }
    }
}
