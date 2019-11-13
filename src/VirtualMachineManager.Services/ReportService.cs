using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.Services
{
    public class ReportService : IReportService, IDisposable
    {
        private int _prognoseDepth;
        private string _outputFilename;
        private ExcelPackage _excelPackage;

        private string[] _methods;

        private int _currentStep = 0;

        public ReportService(string outputFilename, int prognoseDepth)
        {
            _excelPackage = new ExcelPackage();
            _outputFilename = outputFilename;
            _prognoseDepth = prognoseDepth;
        }

        public void Initialize(IEnumerable<int> serverIds)
        {
            var steps = _prognoseDepth;
            ExcelWorkbook workbook = _excelPackage.Workbook;
            foreach (int serverId in serverIds)
            {
                var ws = workbook.Worksheets.Add($"{serverId}");

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

        public void InitVmPrognoseSheets(int vmId, params string[] methods)
        {
            _methods = methods;
            var steps = _prognoseDepth * methods.Length + 1;
            var ws = _excelPackage.Workbook.Worksheets.Add($"vm{vmId}");

            ws.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[1, 1].Value = "Step";
            MergeAndSetValue(ws.Cells[1, 2, 1, steps + 1], "CPU");
            MergeAndSetValue(ws.Cells[1, 1 * steps + 2, 1, 2 * steps + 1], "Memmory");
            MergeAndSetValue(ws.Cells[1, 2 * steps + 2, 1, 3 * steps + 1], "Network");
            MergeAndSetValue(ws.Cells[1, 3 * steps + 2, 1, 4 * steps + 1], "IOPS");

            ws.Cells[2, 0 * steps + 2].Value = $"|actual|";
            ws.Cells[2, 1 * steps + 2].Value = $"|actual|";
            ws.Cells[2, 2 * steps + 2].Value = $"|actual|";
            ws.Cells[2, 3 * steps + 2].Value = $"|actual|";

            for (int i = 0; i < methods.Length; i++)
            {
                for (int j = 1; j <= _prognoseDepth; j++)
                {
                    ws.Cells[2, 0 * steps + 2 + i * _prognoseDepth + j].Value = $"|{methods[i]}+{j}|";
                    ws.Cells[2, 1 * steps + 2 + i * _prognoseDepth + j].Value = $"|{methods[i]}+{j}|";
                    ws.Cells[2, 2 * steps + 2 + i * _prognoseDepth + j].Value = $"|{methods[i]}+{j}|";
                    ws.Cells[2, 3 * steps + 2 + i * _prognoseDepth + j].Value = $"|{methods[i]}+{j}|";
                }
            }
        }

        public void WriteVmPrognoseStatistics(int step, VM vm, Dictionary<Resource, Dictionary<string, float[]>> stat)
        {
            var steps = _prognoseDepth * _methods.Length + 1;
            var ws = _excelPackage.Workbook.Worksheets[$"vm{vm.Id}"];

            ws.Cells[2 + step, 0 * steps + 2].Value = vm.Resources.CPU;
            ws.Cells[2 + step, 1 * steps + 2].Value = vm.Resources.Memmory;
            ws.Cells[2 + step, 2 * steps + 2].Value = vm.Resources.Network;
            ws.Cells[2 + step, 3 * steps + 2].Value = vm.Resources.IOPS;

            if (!stat[Resource.Cpu].Any()) return;

            for (int i = 0; i < _methods.Length; i++)
            {
                for (int j = 1; j <= _prognoseDepth; j++)
                {
                    ws.Cells[2 + step, 0 * steps + 2 + i * _prognoseDepth + j].Value = stat[Resource.Cpu][_methods[i]][j-1];
                    ws.Cells[2 + step, 1 * steps + 2 + i * _prognoseDepth + j].Value = stat[Resource.Memory][_methods[i]][j-1];
                    ws.Cells[2 + step, 2 * steps + 2 + i * _prognoseDepth + j].Value = stat[Resource.Cpu][_methods[i]][j-1];
                    ws.Cells[2 + step, 3 * steps + 2 + i * _prognoseDepth + j].Value = stat[Resource.Iops][_methods[i]][j-1];
                }
            }

            _currentStep = step;
        }

        public void WriteServerStatistics(int step, Server server, IEnumerable<Resources> prognosed)  // TODO: Create ServerStatistic Model
        {
            var depth = _prognoseDepth;
            var capacityOffset = 4 * depth;
            var res =  new[] { server.UsedResources }.Concat(prognosed).ToArray(); // server.PrognosedUsedResources;
            var ws = _excelPackage.Workbook.Worksheets[$"{server.Id}"];
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

            ws.Cells[2 + step, capacityOffset + 10].Value = server.ResourcesCapacity.CPU;
            ws.Cells[2 + step, capacityOffset + 11].Value = server.ResourcesCapacity.Memmory;
            ws.Cells[2 + step, capacityOffset + 12].Value = server.ResourcesCapacity.Network;
            ws.Cells[2 + step, capacityOffset + 13].Value = server.ResourcesCapacity.IOPS;

            _currentStep = step;
        }

        public void DrawCharts()
        {
            ExcelLineChart chart;
            foreach (var sheet in _excelPackage.Workbook.Worksheets.Where(w => !w.Name.Contains("vm")))
            {
                chart = CreatePredictionChart(sheet, "CPU", 0);
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
                chart = CreateOneSeriesChart(sheet, "VMs", 4 * _prognoseDepth + 6);
                chart.SetSize(1000, 400);
                chart.SetPosition(3200, 0);
                chart = CreateMigrationsChart(sheet);
                chart.SetSize(1000, 400);
                chart.SetPosition(3600, 0);
            }
        }

        public void DrawVmForecastCharts()
        {
            var steps = _prognoseDepth * _methods.Length + 1;
            foreach (var sheet in _excelPackage.Workbook.Worksheets.Where(w => w.Name.Contains("vm")))
            {
                var chart = CreateVmPredictionChart(sheet, "CPU", 0 * steps + 2);
                chart.SetSize(1000, 800);
                chart.SetPosition(0, 0);
                chart = CreateVmPredictionChart(sheet, "Memory", 1 * steps + 2);
                chart.SetSize(1000, 800);
                chart.SetPosition(800, 0);
                chart = CreateVmPredictionChart(sheet, "Network", 2 * steps + 2);
                chart.SetSize(1000, 800);
                chart.SetPosition(1600, 0);
                chart = CreateVmPredictionChart(sheet, "IOPS", 3 * steps + 2);
                chart.SetSize(1000, 800);
                chart.SetPosition(2400, 0);
            }
        }

        private void MergeAndSetValue(ExcelRange range, object value)
        {
            range.Merge = true;
            range.Value = value;
        }

        private ExcelLineChart CreatePredictionChart(ExcelWorksheet sheet, string title, int columnOffset)
        {
            int depth = _prognoseDepth;
            var chart = sheet.Drawings.AddChart(title, eChartType.Line) as ExcelLineChart;
            chart.Title.Text = title;
            for (int i = 0; i <= _prognoseDepth; i++)
            {
                chart.Series
                    .Add(GetValuesCellRange(sheet, 2 + columnOffset * (depth + 1) + i, -i), GetValuesCellRange(sheet, 1, i))
                    .Header = $"+{i}";
            }
            chart.Series
                .Add(GetValuesCellRange(sheet, 4 * depth + 10 + columnOffset, 0), GetValuesCellRange(sheet, 1, 0))
                .Header = "capacity";
            return chart;
        }

        private ExcelLineChart CreateVmPredictionChart(ExcelWorksheet sheet, string title, int column)
        {
            var chart = sheet.Drawings.AddChart(title, eChartType.Line) as ExcelLineChart;
            chart.Title.Text = title;
            for (int m = 0; m < _methods.Length; m++)
            {
                for (int i = 1; i <= _prognoseDepth; i++)
                {
                    chart.Series
                        .Add(GetValuesCellRange(sheet, column + m*_prognoseDepth + i, -i), GetValuesCellRange(sheet, 1, i))
                        .Header = $"{_methods[m]}+{i}";
                }
            }
            chart.Series
                .Add(GetValuesCellRange(sheet, column, 0), GetValuesCellRange(sheet, 1, 0))
                .Header = "actual";
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
            var depth = _prognoseDepth;
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
            foreach (var ws in _excelPackage.Workbook.Worksheets)
                ws.Cells.AutoFitColumns();
            _excelPackage.SaveAs(new FileInfo(_outputFilename));
        }

        public void Dispose()
        {
            _excelPackage.Dispose();
        }
    }
}
