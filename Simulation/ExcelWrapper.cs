using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Drawing.Chart;
using Simulation.Models;
using Utilities;

namespace Simulation
{
    public class ExcelWrapper: IDisposable
    {
        private string _outputFilename;
        private ExcelPackage _excelPackage;

        private int _currentStep = 0;

        public ExcelWrapper(string outputFilename = "servers.xlsx")
        {
            _excelPackage = new ExcelPackage();
            _outputFilename = outputFilename;
        }

        public void Initialize(int serversCount)
        {
            var steps = GlobalConstants.PROGNOSE_DEPTH;
            ExcelWorkbook workbook = _excelPackage.Workbook;
            for (int i = 0; i < serversCount; i++)
            {
                var ws = workbook.Worksheets.Add($"{i+1}");

                ws.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[1, 1].Value = "Step";
                MergeAndSetValue(ws.Cells[1, 2, 1, steps + 2], "CPU");
                MergeAndSetValue(ws.Cells[1, 1 * steps + 3, 1, 2 * steps + 3], "Memmory");
                MergeAndSetValue(ws.Cells[1, 2 * steps + 4, 1, 3 * steps + 4], "Network");
                MergeAndSetValue(ws.Cells[1, 3 * steps + 5, 1, 4 * steps + 5], "IOPS");
                for (int j = 0; j < steps + 1; j++)
                {
                    ws.Cells[2, 2 + j].Value = $"|+{j}|";
                    ws.Cells[2, 1 * steps + 3 + j].Value = $"|+{j}|";
                    ws.Cells[2, 2 * steps + 4 + j].Value = $"|+{j}|";
                    ws.Cells[2, 3 * steps + 5 + j].Value = $"|+{j}|";
                }
                ws.Cells[1, 4 * steps + 6].Value = "VM Count";
                MergeAndSetValue(ws.Cells[1, 4 * steps + 7, 1, 4 * steps + 8], "Migrations");
                ws.Cells[2, 4 * steps + 7].Value = "Send"; 
                ws.Cells[2, 4 * steps + 8].Value = "Recieve";

                MergeAndSetValue(ws.Cells[1, 4 * steps + 10, 1, 4 * steps + 13], "Capacity");
                ws.Cells[2, 4 * steps + 10].Value = "CPU";
                ws.Cells[2, 4 * steps + 11].Value = "Memmory";
                ws.Cells[2, 4 * steps + 12].Value = "Network";
                ws.Cells[2, 4 * steps + 13].Value = "IOPS";
            }
        }

        public void WriteServerStatistics(int step, Server server)
        {
            var depth = GlobalConstants.PROGNOSE_DEPTH;
            var capacityOffset = 4 * depth;
            var res = server.PrognosedUsedResources;
            var ws = _excelPackage.Workbook.Worksheets[server.Id];
            ws.Cells[2 + step, 1].Value = step;
            for (int j = 0; j < depth + 1; j++)
            {
                ws.Cells[2 + step, 2 + j].Value = res[j].CPU;
                ws.Cells[2 + step, 1 * depth + 3 + j].Value = res[j].Memmory;
                ws.Cells[2 + step, 2 * depth + 4 + j].Value = res[j].Network;
                ws.Cells[2 + step, 3 * depth + 5 + j].Value = res[j].IOPS;
            }
            ws.Cells[2 + step, 4 * depth + 6].Value = server.RunningVMs.Count;
            ws.Cells[2 + step, 4 * depth + 7].Value = server.SendingCount;
            ws.Cells[2 + step, 4 * depth + 8].Value = server.RecievingCount;

            ws.Cells[2 + step, capacityOffset + 10].Value = server.Resources.CPU;
            ws.Cells[2 + step, capacityOffset + 11].Value = server.Resources.Memmory;
            ws.Cells[2 + step, capacityOffset + 12].Value = server.Resources.Network;
            ws.Cells[2 + step, capacityOffset + 13].Value = server.Resources.IOPS;

            _currentStep = step;
        }

        public void DrawCharts()
        {
            foreach (var sheet in _excelPackage.Workbook.Worksheets)
            {
                var chart = CreatePredictionChart(sheet, "CPU", 0);
                chart.SetSize(1000, 800);
                chart.SetPosition(0, 0);
                chart = CreatePredictionChart(sheet, "Memory", 1);
                chart.SetSize(1000, 800);
                chart.SetPosition(800, 0);
                chart = CreatePredictionChart(sheet, "Network", 2);
                chart.SetSize(1000, 800);
                chart.SetPosition(1600, 0);
                chart = CreatePredictionChart(sheet, "IOPS", 3);
                chart.SetSize(1000, 800);
                chart.SetPosition(2400, 0);
                chart = CreateOneSeriesChart(sheet, "VMs", 4 * GlobalConstants.PROGNOSE_DEPTH + 6);
                chart.SetSize(1000, 400);
                chart.SetPosition(3200, 0);
                chart = CreateMigrationsChart(sheet);
                chart.SetSize(1000, 400);
                chart.SetPosition(3600, 0);
            }
        }

        private void MergeAndSetValue(ExcelRange range, object value)
        {
            range.Merge = true;
            range.Value = value;
        }

        private ExcelLineChart CreatePredictionChart(ExcelWorksheet sheet, string title, int columnOffset)
        {
            int depth = GlobalConstants.PROGNOSE_DEPTH;
            var chart = sheet.Drawings.AddChart(title, eChartType.Line) as ExcelLineChart;
            chart.Title.Text = title;
            for (int i = 0; i <= GlobalConstants.PROGNOSE_DEPTH; i++)
            {
                chart.Series
                    .Add(GetValuesCellRange(sheet, 2 + columnOffset*(depth + 1) + i, -i), GetValuesCellRange(sheet, 1, i))
                    .Header = $"+{i}";
            }
            chart.Series
                .Add(GetValuesCellRange(sheet, 4 * depth + 10 + columnOffset, 0), GetValuesCellRange(sheet, 1, 0))
                .Header = "capacity";
            return chart;
        }

        private ExcelLineChart CreateOneSeriesChart(ExcelWorksheet sheet, string title, int column)
        {
            var chart = sheet.Drawings.AddChart(title, eChartType.Line) as ExcelLineChart;
            chart.Title.Text = title;
            chart.Series.Add(GetValuesCellRange(sheet, column), GetValuesCellRange(sheet, 1));
            return chart;
        }

        private ExcelLineChart CreateMigrationsChart(ExcelWorksheet sheet)
        {
            var depth = GlobalConstants.PROGNOSE_DEPTH;
            var chart = sheet.Drawings.AddChart("Migrations", eChartType.Line) as ExcelLineChart;
            chart.Title.Text = "Migrations";
            chart.Series.Add(GetValuesCellRange(sheet, 4 * depth + 7), GetValuesCellRange(sheet, 1))
                .Header = "Sending";
            chart.Series.Add(GetValuesCellRange(sheet, 4 * depth + 8), GetValuesCellRange(sheet, 1))
                .Header = "Recieving";
            return chart;
        }

        private ExcelRange GetValuesCellRange(ExcelWorksheet sheet, int column, int rowOffset = 0)
        {
            if (rowOffset >= 0)
                return sheet.Cells[3 + rowOffset, column, 2 + _currentStep, column];
            else
                return sheet.Cells[3, column, 2 + _currentStep + rowOffset, column];
        }

        public void Save()
        {
            _excelPackage.Workbook.Worksheets.ForEach((ws) => ws.Cells.AutoFitColumns());
            _excelPackage.SaveAs(new System.IO.FileInfo(_outputFilename));
        }

        public void Dispose()
        {
            _excelPackage.Dispose();
        }
    }
}
