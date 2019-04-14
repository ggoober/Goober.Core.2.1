using AutoFixture;
using System;
using System.Collections.Generic;
using Goober.Core.Extensions;
using Xunit;

namespace Goober.Core.Tests.RequiredExtentions
{
    public class RequiredArgumentListNotEmptyTests
    {
        class TestClass
        {
            public IEnumerable<int> EnumerableProperty { get; set; }
        }

        [Fact]
        public void ListPropertyIsEmpty_ShouldThrowArgumentNullException()
        {
            //arrange
            var sut = new Fixture().Build<TestClass>().With(x => x.EnumerableProperty, new List<int>()).Create();

            //act+assert
            var exc = Assert.Throws<ArgumentNullException>(() => sut.RequiredArgumentListNotEmpty(() => sut.EnumerableProperty));
            Assert.Contains("sut.EnumerableProperty", exc.Message);
        }

        [Fact]
        public void ListPropertyIsNotEmpty_NoExceptionThrows()
        {
            //arrange
            var fixture = new Fixture();
            var list = fixture.CreateMany<int>();
            var sut = fixture.Build<TestClass>().With(x => x.EnumerableProperty, list).Create();

            //act+assert
            sut.RequiredArgumentListNotEmpty(() => sut.EnumerableProperty);
        }
    }
}
