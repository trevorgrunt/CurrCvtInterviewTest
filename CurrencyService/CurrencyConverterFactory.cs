using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyService
{
    public class CurrencyConverterFactory
    {
        List<Currency> _CurrencyCodes;
        List<CurrencyRate> _CurrencyRates;

        public CurrencyConverterFactory(IEnumerable<Currency> currency, IEnumerable<CurrencyRate> currencyRates)
        {
            _CurrencyCodes = currency.ToList();
            _CurrencyRates = currencyRates.ToList();

            foreach (var r in _CurrencyRates)
            {
                r.From = _CurrencyCodes.First(c => c.AlphabeticCode == r.FromAlfa3);
                r.To = _CurrencyCodes.First(c => c.AlphabeticCode == r.ToAlfa3);
            }
        }

        public CurrencyConverter GetConverter(Currency from, Currency to)
        {
            // Если валюты идентичны, возвращается объект CurrencyConverter с коэффициентом 1.0
            if (from.Equals(to))
            {
                return new CurrencyConverter(from, to, value => 1.0m);
            }
            var rates = _CurrencyRates;

            // Ищем коэффициент конвертации для пары валют from-to
            var rate = rates.FirstOrDefault(r => r.From.Equals(from) && r.To.Equals(to));

            // Если нашли коэффициент, создаем CurrencyConverter с функцией конвертации, использующей данный коэффициент
            if (rate != null)
            {
                return new CurrencyConverter(from, to, value => Math.Round(value * rate.Rate, 2));
            }
            else
            {
                // Если не нашли коэффициент, ищем обратный коэффициент to-from
                var reverseRate = rates.FirstOrDefault(r => r.From.Equals(to) && r.To.Equals(from));

                // Если нашли обратный коэффициент, создаем CurrencyConverter с функцией конвертации, использующей его
                if (reverseRate != null)
                {
                    return new CurrencyConverter(from, to, value => Math.Round(value / reverseRate.Rate, 2));
                }
            }
            // Если не нашли ни коэффициента, ни обратного коэффициента, ищем коэффициенты для всех валют, участвующих в конвертации
            var currencies = rates.SelectMany(r => new[] { r.From, r.To }).Distinct().ToList();
            foreach (var currency in currencies)
            {
                var rate1 = rates.FirstOrDefault(r => r.From.Equals(from) && r.To.Equals(currency));
                var rate2 = rates.FirstOrDefault(r => r.From.Equals(to) && r.To.Equals(currency));

                // Если нашли два коэффициента, создаем CurrencyConverter с функцией конвертации, использующей их
                if (rate1 != null && rate2 != null)
                {
                    return new CurrencyConverter(from, to, value => Math.Round(value * rate1.Rate / rate2.Rate, 2));
                }
            }
            // Если не нашли ни одного коэффициента, возвращаем CurrencyConverter с функцией, возвращающей 0.0
            return new CurrencyConverter(from, to, value => 0.0m);
        }
    }
}
