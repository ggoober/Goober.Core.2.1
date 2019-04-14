using AutoFixture;
using System;
using System.Collections.Generic;
using Goober.Core.Extensions;
using Xunit;

namespace Goober.Core.Tests.RequestArgumentExtensionsTests
{
    public class RequiredArgumentNotEmptyTests
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
            var exc = Assert.Throws<ArgumentNullException>(() => sut.RequiredArgumentNotEmpty(() => sut.EnumerableProperty));
            Assert.Contains("sut.EnumerableProperty", exc.Message);
        }
    }
}
