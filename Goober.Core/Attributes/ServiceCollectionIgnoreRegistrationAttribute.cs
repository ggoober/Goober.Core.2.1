using System;

namespace Goober.Core.Attributes
{
    /// <summary>
    /// Добавление этого атрибута принудительно исключает регистрацию элемента в контейнере.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class ServiceCollectionIgnoreRegistrationAttribute : Attribute
    {
    }
}
