using Goober.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace Goober.Core.Extensions
{
    public static class RequiredExtentions
    {
        class ExceptionParameters
        {
            public string PropertyName { get; set; }

            public string ExcMessage { get; set; }
        }

        public static void RequiredArgumentEmail<T>(this T @object, Expression<Func<string>> propertyLambda)
            where T : class
        {
            var value = (string)ExpressionsUtils.GetValue(propertyLambda);

            if (new EmailAddressAttribute().IsValid(value) == false)
                ThrowArgumentException(propertyLambda, $"Not valid email = {value}", context: @object);
        }

        public static void RequiredArgumentEnumIsDefinedValue<T, TProperty>(this T @object, Expression<Func<TProperty>> propertyLambda)
            where T : class
            where TProperty : struct, IConvertible

        {
            if (typeof(TProperty).IsEnum == false)
            {
                throw new InvalidOperationException("TProperty must be an enumerated type");
            }

            var value = ExpressionsUtils.GetValue(propertyLambda);

            var propertyType = typeof(TProperty);

            if (Enum.IsDefined(propertyType, value) == false)
            {
                ThrowArgumentException(propertyLambda, $"Not defined {propertyType.Name} value = {value}", context: @object);
            }
        }

        public static void RequiredArgumentDateIsActual<T>(this T @object, Expression<Func<DateTime>> propertyLambda, int yearDelta = 1)
            where T : class
        {
            var dateNow = DateTime.Now;

            var fromDate = dateNow.AddYears(-yearDelta);

            var toDate = dateNow.AddYears(yearDelta);

            var value = (DateTime)ExpressionsUtils.GetValue(propertyLambda);

            if (value < fromDate || value > toDate)
            {
                ThrowArgumentException(propertyLambda, $"DateTime is out of range values from = {fromDate}, to =  {toDate}", context: @object);
            }
        }

        public static void RequiredArgumnetNotNullAndNotDefaultValue<T, TProperty>(this T @object, Expression<Func<TProperty?>> propertyLambda)
            where T : class
            where TProperty : struct
        {
            RequiredArgumentNotNull(@object, propertyLambda);

            var value = ExpressionsUtils.GetValue(propertyLambda);

            if (value.Equals(default(TProperty)))
            {
                var typeParameterType = typeof(TProperty);

                ThrowArgumentException(propertyLambda: propertyLambda, message: $"{typeParameterType.Name} equals default type value = {value}", context: @object);
            }
        }

        public static void RequiredArgumentNotDefaultValue<T, TProperty>(this T @object, Expression<Func<TProperty>> propertyLambda)
            where T : class
            where TProperty : struct
        {
            var value = ExpressionsUtils.GetValue(propertyLambda);

            if (value.Equals(default(TProperty)))
            {
                var typeParameterType = typeof(TProperty);

                ThrowArgumentException(propertyLambda: propertyLambda, message: $"{typeParameterType.Name} equals default type value = {value}", context: @object);
            }
        }

        public static void RequiredArgumentListNotEmpty<T, TProperty>(this T @object, Expression<Func<IEnumerable<TProperty>>> propertyLambda)
            where T : class
        {
            RequiredArgumentNotNull(@object, propertyLambda);

            var value = ExpressionsUtils.GetValue(propertyLambda) as IEnumerable<TProperty>;
            if (value == null)
                throw new InvalidOperationException($"value must be IEnumerable<{typeof(TProperty).Name}>");

            if (value.Any() == false)
            {
                var typeParameterType = typeof(TProperty);

                ThrowArgumentNullException(propertyLambda: propertyLambda, message: $"IEnumerable<{typeParameterType.Name}> must not be empty", context: @object);
            }
        }

        public static void RequiredListNotEmpty<T, TProperty>(this T @object, Expression<Func<IEnumerable<TProperty>>> propertyLambda)
            where T : class
        {
            RequiredNotNull(@object, propertyLambda);

            var value = ExpressionsUtils.GetValue(propertyLambda) as IEnumerable<TProperty>;
            if (value == null)
                throw new InvalidOperationException($"value must be IEnumerable<{typeof(TProperty).Name}>");

            if (value.Any() == false)
            {
                var typeParameterType = typeof(TProperty);

                ThrowInvalidOperationException(propertyLambda: propertyLambda, message: $"IEnumerable<{typeParameterType.Name}> must not be empty", context: @object);
            }
        }

        public static void RequiredArgumentNotNull<T>(this T @object, string paramName, string message = null, object context = null)
            where T : class
        {
            if (@object != null)
            {
                return;
            }

            var messageParts = new List<string>
            {
                $"paramName: {paramName}"
            };

            if (string.IsNullOrEmpty(message) == false)
            {
                messageParts.Add(message);
            }

            if (context != null)
            {
                messageParts.Add($"context: {JsonConvert.SerializeObject(context)}");
            }

            var excMessage = string.Join(", ", messageParts);

            throw new ArgumentNullException(paramName: paramName, message: excMessage);
        }

        public static void RequiredArgumentNotNull<T, TProperty>(this T @object, Expression<Func<TProperty>> propertyLambda)
            where T : class
        {
            var value = ExpressionsUtils.GetValue(propertyLambda);

            if (value == null)
            {
                ThrowArgumentNullException(propertyLambda: propertyLambda, context: @object);
            }

            var typeParameterType = typeof(TProperty);

            if (typeParameterType == typeof(string)
                && string.IsNullOrEmpty((string)value))
            {
                ThrowArgumentNullException(propertyLambda: propertyLambda, context: @object);
            }
        }

        public static void RequiredNotNull<T>(this T @object, string paramName, string message = null, object context = null)
            where T : class
        {
            if (@object != null)
            {
                return;
            }

            var messageParts = new List<string>
            {
                $"paramName: {paramName}"
            };

            if (string.IsNullOrEmpty(message) == false)
            {
                messageParts.Add(message);
            }

            if (context != null)
            {
                messageParts.Add($"context: {JsonConvert.SerializeObject(context)}");
            }

            var excMessage = string.Join(", ", messageParts);

            throw new InvalidOperationException(message: excMessage);
        }

        public static void RequiredNotNull<T, TProperty>(this T @object, Expression<Func<TProperty>> propertyLambda)
            where T : class
        {
            var value = ExpressionsUtils.GetValue(propertyLambda);

            if (value == null)
            {
                ThrowInvalidOperationException(propertyLambda: propertyLambda, context: @object);
            }

            var typeParameterType = typeof(TProperty);

            if (typeParameterType == typeof(string)
                && string.IsNullOrEmpty((string)value))
            {
                ThrowInvalidOperationException(propertyLambda: propertyLambda, context: @object);
            }
        }

        #region private methods

        private static void ThrowInvalidOperationException<T>(Expression<Func<T>> propertyLambda, string message = null, object context = null)
        {
            var excParams = GenerateExceptionParameters(propertyLambda, message, context);

            throw new InvalidOperationException(message: excParams.ExcMessage);
        }

        private static void ThrowArgumentNullException<T>(Expression<Func<T>> propertyLambda, string message = null, object context = null)
        {
            var excParams = GenerateExceptionParameters(propertyLambda, message, context);

            throw new ArgumentNullException(paramName: excParams.PropertyName, message: excParams.ExcMessage);
        }

        private static void ThrowArgumentException<T>(Expression<Func<T>> propertyLambda, string message = null, object context = null)
        {
            var excParams = GenerateExceptionParameters(propertyLambda, message, context);

            throw new ArgumentException(paramName: excParams.PropertyName, message: excParams.ExcMessage);
        }

        private static ExceptionParameters GenerateExceptionParameters<T>(Expression<Func<T>> propertyLabmda, string message, object context)
        {
            var ret = new ExceptionParameters();

            var messageParts = new List<string>();

            ret.PropertyName = ExpressionsUtils.GetPropertyName<T>(propertyLabmda);

            messageParts.Add($"paramName: {ret.PropertyName}");

            if (string.IsNullOrEmpty(message) == false)
            {
                messageParts.Add(message);
            }

            if (context != null)
            {
                messageParts.Add($"context: {JsonConvert.SerializeObject(context)}");
            }

            ret.ExcMessage = string.Join(", ", messageParts);

            return ret;
        }

        #endregion
    }
}
