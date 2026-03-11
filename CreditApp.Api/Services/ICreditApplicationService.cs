using CreditApp.Api.Models;

namespace CreditApp.Api.Services;

/// <summary>
/// Сервис получения кредитных заявок с кэшированием
/// </summary>
public interface ICreditApplicationService
{
    /// <summary>
    /// Получает кредитную заявку по идентификатору из кэша или генерирует новую
    /// </summary>
    /// <param name="id">Идентификатор заявки</param>
    /// <returns>Кредитная заявка</returns>
    public Task<CreditApplication> GetOrGenerate(int id);
}
