using Goober.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Goober.Core.Extensions
{
    public static class RequestArgumentExtensions
    {
        public static void RequiredArgumentNotEmpty<T, TProperty>(this T @object, Expression<Func<IEnumerable<TProperty>>> propertyLambda)
            where T : class
        {
            RequiredArgumentNotNull(@object, propertyLambda);

            var value = ExpressionsUtils.GetValue(propertyLambda) as IEnumerable<TProperty>;
            if (value == null)
                throw new InvalidOperationException("value as IEnumerable<TProperty>");

            if (value.Any() == false)
            {
                var typeParameterType = typeof(TProperty);

                ThrowArgumentNullException(property: propertyLambda, message: $"IEnumerable<{typeParameterType.FullName}> must not be empty", context: @object);
            }
        }

        public static void RequiredArgumentNotNull<T, TProperty>(this T @object, Expression<Func<TProperty>> propertyLambda) 
            where T: class
        {
            var value = ExpressionsUtils.GetValue(propertyLambda);

            if (value == null)
            {
                ThrowArgumentNullException(property: propertyLambda, context: @object);
            }

            var typeParameterType = typeof(TProperty);

            if (typeParameterType == typeof(string)
                && string.IsNullOrEmpty((string)value))
            {
                ThrowArgumentNullException(property: propertyLambda, context: @object);
            }
        }
        
        private static void ThrowArgumentNullException<T>(Expression<Func<T>> property, string message = null, object context = null)
        {
            var propertyName = ExpressionsUtils.GetPropertyName<T>(property);

            var messageParts = new List<string>();

            messageParts.Add($"paramName: {propertyName}");

            if (string.IsNullOrEmpty(message) == false)
            {
                messageParts.Add(message);
            }

            if (context != null)
            {
                messageParts.Add($"context: {JsonConvert.SerializeObject(context)}");
            }

            var excMessage = string.Join(", ", messageParts);

            throw new ArgumentNullException(paramName: propertyName, message: excMessage);
        }
    }
}
