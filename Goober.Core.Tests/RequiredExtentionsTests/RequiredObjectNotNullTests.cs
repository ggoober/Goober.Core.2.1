using System;
using Goober.Core.Extensions;
using Xunit;

namespace Goober.Core.Tests.RequiredExtentions
{
    public class RequiredArgumentObjectNotNullTests
    {
        class TestClass
        {
            public int IntProperty { get; set; }

            public int? NullableIntProperty { get; set; }
        }

        [Fact]
        public void ObjectIsNull_ShouldThrowArgumentNullException()
        {
            //arrange
            TestClass sut = null;

            //act+assert
            var exc = Assert.Throws<ArgumentNullException>(() => sut.RequiredArgumentNotNull("sut"));
            Assert.Contains("sut", exc.Message);
        }

        [Fact]
        public void ObjectIsNotNull_NoExceptionThrows()
        {
            //arrange
            var sut = new TestClass();

            //act+assert
            sut.RequiredArgumentNotNull("sut");
        }
    }
}
