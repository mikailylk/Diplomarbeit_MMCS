using ExCSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// openxml
//using DocumentFormat.OpenXml;
//using DocumentFormat.OpenXml.Packaging;
//using DocumentFormat.OpenXml.Spreadsheet;

using Syncfusion.XlsIO;
using NetTopologySuite.Mathematics;
using System.Text.Json;
using System.Xml;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;

namespace ICU_App.Model;

public class TelemetryData
{
    // Telemetriedaten
    public double TIMESTAMP { get; set; }
    public double BATT_AMP { get; set; }
    public double BATT_VOLT { get; set; }
    public double BOARD_AMP { get; set; }
    public double HYDRO { get; set; }
    public double TEMP { get; set; }
    public double PRESSURE { get; set; }
    public double LONGITUDE { get; set; }
    public double LATITUDE { get; set; }

    public double? LONGITUDE_SMARTPHONE { get; set; }
    public double? LATITUDE_SMARTPHONE { get; set; }

    public override string ToString()
    {
        return $"Timestamp: {new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds((double)TIMESTAMP).ToLocalTime()}" +
            $"\nBatterystatus: {BATT_AMP}A, {BATT_VOLT}V\n" +
            $"Board current: {BOARD_AMP}A\n" +
            $"Hydro: {HYDRO}, Temperature: {TEMP}°C, Pressure: {PRESSURE}\n" +
            $"Location: Longitude: {LONGITUDE}; Latitude: {LATITUDE}";
    }
}

public class TelemetryDataCollection
{
    public List<TelemetryData> telemetryDataCollection;

    // save smartphone coordinates only for once (for now)
    private double _LONGITUDE_SMARTPHONE;
    private double _LATITUDE_SMARTPHONE;
    private DateTime _start_time;

    private const string filelocation_android_csv = "/storage/emulated/0/Documents/ICU_Tables";
    private const string filelocation_android_json = "/storage/emulated/0/Documents/ICU_Tables_JSON";

    public TelemetryDataCollection() 
    {
        telemetryDataCollection = new List<TelemetryData>();
    }
    public TelemetryDataCollection(double LONGITUDE_SMARTPHONE, double LATITUDE_SMARTPHONE)
    {
        // get location of smartphone for once and write it as every location
        this._LONGITUDE_SMARTPHONE = LONGITUDE_SMARTPHONE;
        this._LATITUDE_SMARTPHONE = LATITUDE_SMARTPHONE;

        // start time
        _start_time = DateTime.Now;

        telemetryDataCollection = new List<TelemetryData>();
    }

    public async Task<bool> SaveToCSVFile(string filename = "")
    {
        // Create Directory (if not exists)
        Directory.CreateDirectory(filelocation_android_csv);

        filename = Path.Combine(filelocation_android_csv, $"ICU_Table_{_start_time.ToString("dd-MM-yyyy--hh-mm-ss")}_until_{DateTime.Now.ToString("dd-MM-yyyy--hh-mm-ss")}.xlsx");

        try
        {
            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                // Excel app init
                Syncfusion.XlsIO.IApplication app = excelEngine.Excel;

                // Assigns default application version
                app.DefaultVersion = ExcelVersion.Xlsx;

                // new workbook
                IWorkbook workbook = app.Workbooks.Create(1);

                IWorksheet worksheet = workbook.Worksheets[0];

                // Headlines
                worksheet.Range["A1"].Value = nameof(TelemetryData.TIMESTAMP);
                worksheet.Range["B1"].Value = nameof(TelemetryData.BATT_AMP);
                worksheet.Range["C1"].Value = nameof(TelemetryData.BATT_VOLT);
                worksheet.Range["D1"].Value = nameof(TelemetryData.BOARD_AMP);
                worksheet.Range["E1"].Value = nameof(TelemetryData.HYDRO);
                worksheet.Range["F1"].Value = nameof(TelemetryData.TEMP);
                worksheet.Range["G1"].Value = nameof(TelemetryData.PRESSURE);
                worksheet.Range["H1"].Value = nameof(TelemetryData.LONGITUDE);
                worksheet.Range["I1"].Value = nameof(TelemetryData.LATITUDE);
                worksheet.Range["J1"].Value = nameof(TelemetryData.LONGITUDE_SMARTPHONE);
                worksheet.Range["K1"].Value = nameof(TelemetryData.LATITUDE_SMARTPHONE);

                // mark headlines bold
                IStyle bold_style = workbook.Styles.Add("Bold_Style");
                bold_style.Font.Bold = true;
                bold_style.ColorIndex = ExcelKnownColors.Grey_25_percent;
                worksheet.Range["A1:K1"].CellStyle = bold_style;

                for (int i = 2; i < telemetryDataCollection.Count; i++)
                {
                    worksheet.Range[i, 1].Text = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).
                        AddSeconds((double)telemetryDataCollection[i - 2].TIMESTAMP).ToLocalTime().ToString();
                    // worksheet.Range[i, 1].NumberFormat = "TT.MM.JJJJ hh:mm";     // not needed
                    worksheet.Range[i, 2].Value2 = telemetryDataCollection[i - 2].BATT_AMP;
                    worksheet.Range[i, 3].Value2 = telemetryDataCollection[i - 2].BATT_VOLT;
                    worksheet.Range[i, 4].Value2 = telemetryDataCollection[i - 2].BOARD_AMP;
                    worksheet.Range[i, 5].Value2 = telemetryDataCollection[i - 2].HYDRO;
                    worksheet.Range[i, 6].Value2 = telemetryDataCollection[i - 2].TEMP;
                    worksheet.Range[i, 7].Value2 = telemetryDataCollection[i - 2].PRESSURE;
                    worksheet.Range[i, 8].Value2 = telemetryDataCollection[i - 2].LONGITUDE;
                    worksheet.Range[i, 9].Value2 = telemetryDataCollection[i - 2].LATITUDE;

                    // for now, get location of smartphone just once and write it to Attribute
                    telemetryDataCollection[i - 2].LONGITUDE_SMARTPHONE = _LONGITUDE_SMARTPHONE;
                    telemetryDataCollection[i - 2].LATITUDE_SMARTPHONE = _LATITUDE_SMARTPHONE;

                    worksheet.Range[i, 10].Value2 = telemetryDataCollection[i - 2].LONGITUDE_SMARTPHONE;
                    worksheet.Range[i, 11].Value2 = telemetryDataCollection[i - 2].LATITUDE_SMARTPHONE;
                }
                for (int i = 1; i < 12; i++)
                {
                    worksheet.AutofitColumn(i);
                }

                #region Current_Voltage_Diagram
                IChartShape chart_volt_amp = worksheet.Charts.Add();
                chart_volt_amp.Name = "Voltage & Current Consumption";
                chart_volt_amp.ChartTitle = "Voltage & Current Consumption";
                chart_volt_amp.ChartType = ExcelChartType.Line_Markers_Stacked;

                chart_volt_amp.HasLegend = true;
                chart_volt_amp.Legend.Position = ExcelLegendPosition.Bottom;

                IChartSerie batt_amp_ser = chart_volt_amp.Series.Add("Battery_Current_Consumption");
                batt_amp_ser.Values = worksheet.Range[$"B2:B{telemetryDataCollection.Count - 1}"];
                batt_amp_ser.CategoryLabels = worksheet.Range[$"A2:A{telemetryDataCollection.Count - 1}"];

                IChartSerie board_amp_ser = chart_volt_amp.Series.Add("Board_Current_Consumption");
                board_amp_ser.Values = worksheet.Range[$"C2:C{telemetryDataCollection.Count - 1}"];
                board_amp_ser.CategoryLabels = worksheet.Range[$"A2:A{telemetryDataCollection.Count - 1}"];

                IChartSerie batt_volt_ser = chart_volt_amp.Series.Add("Battery_Voltage_Consumption");
                batt_volt_ser.Values = worksheet.Range[$"C2:C{telemetryDataCollection.Count - 1}"];
                batt_volt_ser.UsePrimaryAxis = false; // zweite Achse verwenden
                batt_volt_ser.CategoryLabels = worksheet.Range[$"A2:A{telemetryDataCollection.Count - 1}"];

                chart_volt_amp.PrimaryCategoryAxis.Title = "Time";
                chart_volt_amp.PrimaryValueAxis.Title = "Current [A]";
                chart_volt_amp.SecondaryValueAxis.Title = "Voltage [V]";
                #endregion

                // activate gridlines
                chart_volt_amp.PrimaryValueAxis.HasMajorGridLines = true;
                chart_volt_amp.PrimaryCategoryAxis.HasMajorGridLines = true;

                // place left of 12th column
                chart_volt_amp.LeftColumn = 12;
                chart_volt_amp.TopRow = 2;
                chart_volt_amp.RightColumn = 42;
                chart_volt_amp.BottomRow = 42;

                // save workbook
                FileStream stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                workbook.SaveAs(stream);
                stream.Dispose();
                stream.Close();
            }
        }
        catch (Exception ex)
        {
            // saving failed
            return false;
        }

        // saving was ok
        return true;

        #region openxml
        // Create a new Excel file. openxml
        //using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Create(filename, SpreadsheetDocumentType.Workbook))
        //{
        //    // Add a WorkbookPart to the document.
        //    WorkbookPart workbookPart = spreadsheet.AddWorkbookPart();
        //    workbookPart.Workbook = new Workbook();

        //    // Add a WorksheetPart to the WorkbookPart.
        //    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        //    worksheetPart.Worksheet = new Worksheet(new SheetData());

        //    // Add Sheets to the Workbook.
        //    Sheets sheets = spreadsheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

        //    // Append a new worksheet and associate it with the workbook.
        //    Sheet sheet = new Sheet()
        //    {
        //        Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
        //        SheetId = 1,
        //        Name = "Sheet1"
        //    };
        //    sheets.Append(sheet);

        //    // Get the SheetData from the Worksheet.
        //    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

        //    // Add a new row to the sheet data.
        //    Row row = new Row();
        //    sheetData.Append(row);

        //    // Add cells to the row.
        //    DocumentFormat.OpenXml.Spreadsheet.Cell cell1 = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellValue = new CellValue("Hello"), DataType = CellValues.String };
        //    row.Append(cell1);
        //    DocumentFormat.OpenXml.Spreadsheet.Cell cell2 = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellValue = new CellValue("World"), DataType = CellValues.String };
        //    row.Append(cell2);

        //    // Save the changes to the worksheet.
        //    worksheetPart.Worksheet.Save();

        //    // Close the document.
        //    spreadsheet.Close();
        //}
        #endregion
    }

    public async Task<bool> SaveToJSON(string filename = "")
    {
        // Create Directory (if not exists)
        Directory.CreateDirectory(filelocation_android_json);

        filename = Path.Combine(filelocation_android_json, $"ICU_Table_JSON_{_start_time.ToString("dd-MM-yyyy--hh-mm-ss")}_until_{DateTime.Now.ToString("dd-MM-yyyy--hh-mm-ss")}.json");

        try
        {
            using FileStream writestream = File.Create(filename);
            await JsonSerializer.SerializeAsync(writestream, telemetryDataCollection);
            await writestream.DisposeAsync();
        }
        catch (Exception ex)
        {
            // something went wrong 
            return false;
        }

        return true;
    }

    public async Task<ObservableCollection<string>> GetListOfJsonDataStartStop()
    {
        string fileName = "ICU_Table_JSON_";
        string fileExtension = ".json";

        ObservableCollection<string> list_filebydate = new ObservableCollection<string>();

        string[] files = Directory.GetFiles(filelocation_android_json, $"{fileName}*{fileExtension}");

        foreach (string file in files)
        {
            string[] splitted_file_name = file.Split(filelocation_android_json)[1].Split('_');

            list_filebydate.Add($"from {splitted_file_name[3]} until {splitted_file_name[5].Split('.')[0]}");
        }

        return list_filebydate;
    }

    public async Task<bool> ReadFromJSON(DateTime start_time, DateTime stop_time, string filename = "")
    {
        //List<TelemetryData> data = JsonSerializer.DeserializeAsync();
        filename = Path.Combine(filelocation_android_json, $"ICU_Table_JSON_{start_time.ToString("dd-MM-yyyy--hh-mm-ss")}_until_{stop_time.ToString("dd-MM-yyyy--hh-mm-ss")}.json");
        
        using FileStream readstream = File.OpenRead(filename);

        telemetryDataCollection = await JsonSerializer.DeserializeAsync<List<TelemetryData>>(readstream);

        return true;
    }

    public void FillWithDummyData(double longitude_smartphone, double latitude_smartphone)
    {
        Random random = new Random();

        DateTime dt = DateTime.UtcNow;

        for (int i = 0; i < 60; i++)
        {
            double r = random.NextDouble();
            double g = random.NextDouble();
            TelemetryData telemetryData = new TelemetryData()
            {
                TIMESTAMP = (double)dt.Subtract(new DateTime(1970, 1, 1, 0, 0, 59 - i)).TotalSeconds,
                BATT_AMP = 14 * r, BATT_VOLT = 12 * g, BOARD_AMP = 2 * r,
                HYDRO = 10 * r, PRESSURE = 1013 * r, TEMP = 23 * r,
                LONGITUDE = 9.854 + 0.5*r, LATITUDE = 49.452 + 0.5*g,
                LONGITUDE_SMARTPHONE = longitude_smartphone, LATITUDE_SMARTPHONE = latitude_smartphone
            };
            telemetryDataCollection.Add(telemetryData);
        }

        //string parsetext = """{"TIMESTAMP":1676301385.0022044,"BATT_AMP":2.594517762355612,"BATT_VOLT":2.223872367733382,"BOARD_AMP":0.37064539462223034,"HYDRO":1.8532269731111517,"TEMP":4.262422038155649,"PRESSURE":187.73189237615966,"LONGITUDE":49.452,"LATITUDE":9.854}""";

        //try
        //{
        //    TelemetryData parsetest = JsonSerializer.Deserialize<TelemetryData>(ss);
        //}
        //catch (Exception ex)
        //{

        //}
    }
}

public class TelemetryDataChartModel
{
    public DateTime x_dt { get; set; }
    public double y_battamp { get; set; }
    public double y_battvolt { get; set; }
    public double y_boardamp { get; set; }

    public static TelemetryDataChartModel ParseTelemetryData(TelemetryData telemetryData)
    {
        return new TelemetryDataChartModel()
        {
            x_dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds((double)telemetryData.TIMESTAMP).ToLocalTime(),
            y_battamp = telemetryData.BATT_AMP,
            y_battvolt = telemetryData.BATT_VOLT,
            y_boardamp = telemetryData.BOARD_AMP
        };
    }
}

public class TelemetryDataChartModelCollection : List<TelemetryDataChartModel>
{
    public static TelemetryDataChartModelCollection ParseTelemetryDataCollection(TelemetryDataCollection telemetryDatas)
    {
        TelemetryDataChartModelCollection chartCollection = new TelemetryDataChartModelCollection();

        foreach (TelemetryData telemetry in telemetryDatas.telemetryDataCollection)
        {
            chartCollection.Add(TelemetryDataChartModel.ParseTelemetryData(telemetry));
        }
        return chartCollection;
    }
}

