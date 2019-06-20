using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Goober.RabbitMq.Abstractions
{
    /// <summary>
    /// Предоставляет абстракцию Подписчика в модели подписчик/издатель (Producer/Consumer)
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения, на которое подписан данный объект</typeparam>
    public interface IMessageHandler<TMessage> where TMessage : class
    {
        /// <summary>
        /// Метод будет вызван при появлении нового сообщения в очереди
        /// </summary>
        /// <param name="event">Сообщение</param>
        /// <returns></returns>
        Task ProcessAsync(TMessage message);
    }
}
