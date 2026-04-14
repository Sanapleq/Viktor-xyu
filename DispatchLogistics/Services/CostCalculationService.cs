using System;
using System.Collections.Generic;
using DispatchLogistics.Models;

namespace DispatchLogistics.Services
{
    /// <summary>
    /// Сервис расчёта стоимости перевозки
    /// 
    /// Формула расчёта:
    /// BaseDistanceCost = DistanceKm * CostPerKm  (если CostPerKM задан)
    /// IdleCost         = IdleHours * CostPerHour (если CostPerHour задан)
    /// WeightCost       = CargoWeight * CostPerTon (если CostPerTon задан)
    /// FuelCost         = FuelSurcharge (если задан, иначе 0)
    /// ServicesCost     = сумма всех доп. услуг
    /// Subtotal         = BaseDistanceCost + IdleCost + WeightCost + FuelCost + ServicesCost
    /// Total            = Subtotal * SeasonalCoefficient
    /// </summary>
    public class CostCalculationService
    {
        /// <summary>
        /// Рассчитывает стоимость перевозки на основе тарифа, расстояния, веса и простоя
        /// </summary>
        /// <param name="tariff">Тариф для расчёта</param>
        /// <param name="distanceKm">Расстояние в км</param>
        /// <param name="cargoWeight">Масса груза (тонны), может быть null</param>
        /// <param name="idleHours">Часы простоя, может быть null</param>
        /// <param name="servicesCost">Сумма дополнительных услуг</param>
        /// <returns>Результат расчёта с полной расшифровкой</returns>
        public CalculationResult Calculate(
            TariffModel tariff,
            decimal distanceKm,
            decimal? cargoWeight,
            decimal? idleHours,
            decimal servicesCost)
        {
            CalculationResult result = new CalculationResult();

            // Стоимость за расстояние (если тариф содержит ставку за км)
            if (tariff.CostPerKm.HasValue && distanceKm > 0)
            {
                result.BaseDistanceCost = Math.Round(distanceKm * tariff.CostPerKm.Value, 2);
            }

            // Стоимость простоя (если тариф содержит ставку за час)
            if (tariff.CostPerHour.HasValue && idleHours.HasValue && idleHours.Value > 0)
            {
                result.IdleCost = Math.Round(idleHours.Value * tariff.CostPerHour.Value, 2);
            }

            // Стоимость за тоннаж (если тариф содержит ставку за тонну)
            if (tariff.CostPerTon.HasValue && cargoWeight.HasValue && cargoWeight.Value > 0)
            {
                result.WeightCost = Math.Round(cargoWeight.Value * tariff.CostPerTon.Value, 2);
            }

            // Топливный сбор
            if (tariff.FuelSurcharge.HasValue)
            {
                result.FuelCost = tariff.FuelSurcharge.Value;
            }

            // Доп. услуги
            result.ServicesCost = servicesCost;

            // Подытог
            result.Subtotal = Math.Round(
                result.BaseDistanceCost + result.IdleCost + result.WeightCost +
                result.FuelCost + result.ServicesCost, 2);

            // Сезонный коэффициент
            result.SeasonalCoefficient = tariff.SeasonalCoefficient;

            // Итого
            result.CalculatedTotal = Math.Round(
                result.Subtotal * result.SeasonalCoefficient, 2);

            return result;
        }

        /// <summary>
        /// Рассчитывает сумму дополнительных услуг
        /// </summary>
        public decimal CalculateServicesCost(List<OrderServiceModel> services)
        {
            decimal total = 0m;
            if (services == null)
                return total;

            foreach (var s in services)
            {
                total += s.Total;
            }
            return Math.Round(total, 2);
        }

        /// <summary>
        /// Создаёт читаемую строку с расшифровкой расчёта для отображения в интерфейсе
        /// </summary>
        public string GetCalculationDescription(CalculationResult result)
        {
            string text = "=== Расшифровка расчёта ===\r\n\r\n";

            if (result.BaseDistanceCost > 0)
                text += string.Format("  Стоимость за расстояние: {0:N2} руб.\r\n", result.BaseDistanceCost);

            if (result.IdleCost > 0)
                text += string.Format("  Стоимость простоя: {0:N2} руб.\r\n", result.IdleCost);

            if (result.WeightCost > 0)
                text += string.Format("  Стоимость за тоннаж: {0:N2} руб.\r\n", result.WeightCost);

            if (result.FuelCost > 0)
                text += string.Format("  Топливный сбор: {0:N2} руб.\r\n", result.FuelCost);

            if (result.ServicesCost > 0)
                text += string.Format("  Дополнительные услуги: {0:N2} руб.\r\n", result.ServicesCost);

            text += string.Format("\r\n  Подытог: {0:N2} руб.\r\n", result.Subtotal);

            if (result.SeasonalCoefficient != 1.00m)
                text += string.Format("  Сезонный коэффициент: {0:N2}\r\n", result.SeasonalCoefficient);

            text += string.Format("\r\n  ИТОГО: {0:N2} руб.", result.CalculatedTotal);

            return text;
        }
    }
}
