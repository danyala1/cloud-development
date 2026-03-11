namespace CreditApp.Api.Models;

/// <summary>
/// Кредитная заявка
/// </summary>
public class CreditApplication
{
    /// <summary>
    /// Идентификатор в системе
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Тип кредита (например, "Потребительский", "Ипотека", "Автокредит")
    /// </summary>
    public required string LoanType { get; set; }

    /// <summary>
    /// Запрашиваемая сумма, округлённая до двух знаков после запятой
    /// </summary>
    public decimal RequestedAmount { get; set; }

    /// <summary>
    /// Срок кредита в месяцах
    /// </summary>
    public int TermMonths { get; set; }

    /// <summary>
    /// Процентная ставка (не менее ставки ЦБ РФ), округлённая до двух знаков после запятой
    /// </summary>
    public double InterestRate { get; set; }

    /// <summary>
    /// Дата подачи заявки (не более двух лет назад от текущей даты)
    /// </summary>
    public DateOnly ApplicationDate { get; set; }

    /// <summary>
    /// Необходимость страховки
    /// </summary>
    public bool InsuranceRequired { get; set; }

    /// <summary>
    /// Статус заявки (например, "Новая", "В обработке", "Одобрена", "Отклонена")
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// Дата решения. Заполняется только для терминальных статусов ("Одобрена", "Отклонена")
    /// </summary>
    public DateOnly? DecisionDate { get; set; }

    /// <summary>
    /// Одобренная сумма. Заполняется только при статусе "Одобрена", не превышает <see cref="RequestedAmount"/>
    /// </summary>
    public decimal? ApprovedAmount { get; set; }
}
