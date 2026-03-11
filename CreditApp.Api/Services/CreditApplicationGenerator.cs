using Bogus;
using CreditApp.Api.Models;

namespace CreditApp.Api.Services;

/// <summary>
/// Генератор кредитных заявок
/// </summary>
public class CreditApplicationGenerator
{
    private static readonly string[] _loanTypes =
        ["Потребительский", "Ипотека", "Автокредит", "Рефинансирование", "Образовательный"];

    private static readonly string[] _statuses =
        ["Новая", "В обработке", "Одобрена", "Отклонена", "Отменена"];

    private static readonly string[] _terminalStatuses = ["Одобрена", "Отклонена"];

    private static readonly Faker<CreditApplication> _faker = new Faker<CreditApplication>("ru")
        .RuleFor(x => x.Id, _ => 0)
        .RuleFor(x => x.LoanType, f => f.PickRandom(_loanTypes))
        .RuleFor(x => x.RequestedAmount, f => Math.Round(f.Random.Decimal(50_000m, 10_000_000m), 2))
        .RuleFor(x => x.TermMonths, f => f.Random.Int(6, 360))
        .RuleFor(x => x.InterestRate, f => Math.Round(f.Random.Double(15.5, 21.5), 2))
        .RuleFor(x => x.ApplicationDate, f =>
            DateOnly.FromDateTime(f.Date.Between(DateTime.Today.AddYears(-2), DateTime.Today)))
        .RuleFor(x => x.InsuranceRequired, f => f.Random.Bool())
        .RuleFor(x => x.Status, f => f.PickRandom(_statuses))
        .RuleFor(x => x.DecisionDate, (f, app) =>
            _terminalStatuses.Contains(app.Status)
                ? DateOnly.FromDateTime(f.Date.Between(app.ApplicationDate.ToDateTime(TimeOnly.MinValue), DateTime.Today))
                : null)
        .RuleFor(x => x.ApprovedAmount, (f, app) =>
            app.Status == "Одобрена"
                ? Math.Round(f.Random.Decimal(app.RequestedAmount * 0.5m, app.RequestedAmount), 2)
                : null);

    /// <summary>
    /// Генерирует кредитную заявку по указанному идентификатору
    /// </summary>
    /// <param name="id">Идентификатор заявки</param>
    /// <returns>Сгенерированная кредитная заявка</returns>
    public CreditApplication Generate(int id)
    {
        var credit = _faker.Generate();
        credit.Id = id;
        return credit;
    }
}
