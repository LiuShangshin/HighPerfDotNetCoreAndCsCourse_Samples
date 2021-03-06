﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using SimlpeSampleStockAppModel;

namespace SimpleSampleStockAppServices.HistoricalData
{

    public class HistoricalPriceStorage
    {
        private string _folderPath;
        private TelemetryClient telemetryClient = new TelemetryClient();

        public HistoricalPriceStorage(String FolderPath)
        {
            _folderPath = FolderPath;
        }
        public IEnumerable<HistoricalValue> GetHistoricalQuotes(String symbol)
        {
            var retVal = new List<HistoricalValue>();
            var startTime = DateTime.UtcNow;
            var timer = Stopwatch.StartNew();

            try
            {
                retVal = GetHistoricalQuotesInternal(symbol);
            }
            finally
            {
                timer.Stop();
                telemetryClient
                    .TrackDependency("HistoricalDataStorage", symbol,
                    startTime, timer.Elapsed, retVal.Any());
            }

            return retVal;
        }
        private List<HistoricalValue> GetHistoricalQuotesInternal(String symbol)
        {
            var retVal = new List<HistoricalValue>();
            symbol = symbol.Replace('.', '_');
            var logPath = _folderPath + System.IO.Path.DirectorySeparatorChar + symbol + ".csv";
            var logFile = System.IO.File.OpenRead(logPath);
            using (var logReader = new System.IO.StreamReader(logFile))
            {

                string line;
                while ((line = logReader.ReadLine()) != null)
                {
                    var items = line.Split(';');
                    var date = items[0].Split('-');
                    retVal.Add(new HistoricalValue
                    {
                        Date = new DateTime(Int32.Parse(date[0]), Int32.Parse(date[1]), Int32.Parse(date[2])),
                        Close = Decimal.Parse(items[1].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture),
                        High = Decimal.Parse(items[2].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture),
                        Low = Decimal.Parse(items[3].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture),
                        Open = Decimal.Parse(items[4].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture),
                        Volume = long.Parse(items[5].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture)
                    });
                }
            }

            return retVal;
        }
    }
}

