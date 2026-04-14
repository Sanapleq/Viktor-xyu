using System;

namespace DispatchLogistics.Models
{
    /// <summary>
    /// Модель для результата расчёта стоимости перевозки
    /// </summary>
    public class CalculationResult
    {
        public decimal BaseDistanceCost { get; set; }
        public decimal IdleCost { get; set; }
        public decimal WeightCost { get; set; }
        public decimal FuelCost { get; set; }
        public decimal ServicesCost { get; set; }
        public decimal Subtotal { get; set; }
        public decimal SeasonalCoefficient { get; set; }
        public decimal CalculatedTotal { get; set; }

        /// <summary>
        /// Формирует читаемую расшифровку расчёта
        /// </summary>
        public string GetBreakdownText()
        {
            string text = "=== Расчёт стоимости ===\r\n";
            text += string.Format("Расстояние: {0} км x тариф = {1:N2} руб.\r\n",
                BaseDistanceCost > 0 ? "" : "0", BaseDistanceCost);
            text += string.Format("Простой: {0} руб.\r\n", IdleCost);
            text += string.Format("За тоннаж: {0:N2} руб.\r\n", WeightCost);
            text += string.Format("Топливный сбор: {0:N2} руб.\r\n", FuelCost);
            text += string.Format("Доп. услуги: {0:N2} руб.\r\n", ServicesCost);
            text += string.Format("Подытог: {0:N2} руб.\r\n", Subtotal);
            text += string.Format("Сезонный коэф.: {0:N2}\r\n", SeasonalCoefficient);
            text += string.Format("ИТОГО: {0:N2} руб.", CalculatedTotal);
            return text;
        }
    }
}
