using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PayEstimatorUWP
{
    class ExportSpreadsheetWorkbookFile
    {
        public async void CreatSpreadsheetWorkbookFile()
        {
            Stream stream;

            var savePicker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop
            };

            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Excel", new List<string>() { ".xlsx" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "CREWONCALL";

            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                using (stream = await file.OpenStreamForWriteAsync())
                {
                    using (var workbook = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                    {
                        var workbookPart = workbook.AddWorkbookPart();

                        workbook.WorkbookPart.Workbook = new Workbook
                        {
                            Sheets = new Sheets()
                        };

                        var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                        var sheetData = new SheetData();
                        sheetPart.Worksheet = new Worksheet(sheetData);

                        Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                        string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                        uint sheetId = 1;
                        if (sheets.Elements<Sheet>().Count() > 0)
                        {
                            sheetId =
                                sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                        }

                        Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = "Current Pay" };
                        sheets.Append(sheet);

                        Row headerRow = new Row();

                        List<string> names = new List<string>
                        {
                            "Date",
                            "Start",
                            "End",
                            "Client",
                            "Skill",
                            "LEVEL3A",
                            "LEVEL3B",
                            "LEVEL3SUN",
                            "VANDVRA",
                            "VANDVRB",
                            "VANDVRSUN",
                            "MRHRA",
                            "MRHRB",
                            "MRHRSUN",
                            "Hours",
                            "Break",
                            "PH"
                        };

                        foreach (string name in names)
                        {
                            Cell cell = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(name)
                            };
                            headerRow.AppendChild(cell);
                        }

                        sheetData.AppendChild(headerRow);

                        AppointmentStore appointmentStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);

                        DayOfWeek weekStart = DayOfWeek.Monday;
                        DateTimeOffset startingDate = DateTimeOffset.Now;

                        while (startingDate.DayOfWeek != weekStart)
                            startingDate = startingDate.AddDays(-1);

                        DateTimeOffset thisPayStart = startingDate.AddDays(-14);
                        var timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
                        var startTime = new DateTimeOffset(thisPayStart.Year, thisPayStart.Month, thisPayStart.Day, 0, 0, 0, timeZoneOffset);

                        TimeSpan duration = TimeSpan.FromDays(7);

                        FindAppointmentsOptions options = new FindAppointmentsOptions
                        {
                            MaxCount = 100
                        };
                        options.FetchProperties.Add(AppointmentProperties.Subject);
                        options.FetchProperties.Add(AppointmentProperties.Location);
                        options.FetchProperties.Add(AppointmentProperties.AllDay);
                        options.FetchProperties.Add(AppointmentProperties.StartTime);
                        options.FetchProperties.Add(AppointmentProperties.Duration);
                        options.FetchProperties.Add(AppointmentProperties.Details);
                        options.FetchProperties.Add(AppointmentProperties.DetailsKind);

                        IReadOnlyList<Appointment> appointments = await appointmentStore.FindAppointmentsAsync(startTime, duration, options);
                        List<Gig> gigsthispay = new List<Gig>();

                        if (appointments.Count > 0)
                        {
                            foreach (var appointment in appointments)
                            {
                                if (!appointment.AllDay)
                                {
                                    string MealBreak = null;
                                    try
                                    {
                                        if (appointment.Details.Contains("::NOBREAK"))
                                            MealBreak = "No break";
                                        if (appointment.Details.Contains("CrewOnCall::LEVEL3") && (appointment.StartTime.Date != startTime.AddDays(-1).Date))
                                            gigsthispay.Add(new Gig(appointment.StartTime, appointment.Duration, appointment.Subject, appointment.Location, "LEVEL3", MealBreak));
                                        if (appointment.Details.Contains("CrewOnCall::VANDVR") && (appointment.StartTime.Date != startTime.AddDays(-1).Date))
                                            gigsthispay.Add(new Gig(appointment.StartTime, appointment.Duration, appointment.Subject, appointment.Location, "VANDVR", MealBreak));
                                        if (appointment.Details.Contains("CrewOnCall::MR/HR") && (appointment.StartTime.Date != startTime.AddDays(-1).Date))
                                            gigsthispay.Add(new Gig(appointment.StartTime, appointment.Duration, appointment.Subject, appointment.Location, "MR/HR", MealBreak));
                                    }
                                    catch (ArgumentOutOfRangeException)
                                    {

                                    }
                                }
                            }
                        }

                        foreach (Gig gig in gigsthispay)
                        {
                            Row newRow = new Row();

                            Cell cell1 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(gig.StartDate)
                            };
                            newRow.AppendChild(cell1);

                            Cell cell2 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(gig.StartTime)
                            };
                            newRow.AppendChild(cell2);

                            Cell cell3 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(gig.EndTime)
                            };
                            newRow.AppendChild(cell3);

                            Cell cell4 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(gig.ClientName)
                            };
                            newRow.AppendChild(cell4);

                            Cell cell5 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(gig.Skill)
                            };
                            newRow.AppendChild(cell5);

                            Cell cell6 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(gig.LEVEL3A))
                            };
                            newRow.AppendChild(cell6);

                            Cell cell7 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(gig.LEVEL3B))
                            };
                            newRow.AppendChild(cell7);

                            Cell cell8 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(gig.LEVEL3SUN))
                            };
                            newRow.AppendChild(cell8);

                            Cell cell9 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(gig.VANDVRA))
                            };
                            newRow.AppendChild(cell9);

                            Cell cell10 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(gig.VANDVRB))
                            };
                            newRow.AppendChild(cell10);

                            Cell cell11 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(gig.VANDVRSUN))
                            };
                            newRow.AppendChild(cell11);

                            Cell cell12 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(gig.MRHRA))
                            };
                            newRow.AppendChild(cell12);

                            Cell cell13 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(gig.MRHRB))
                            };
                            newRow.AppendChild(cell13);

                            Cell cell14 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(gig.MRHRSUN))
                            };
                            newRow.AppendChild(cell14);

                            Cell cell15 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(gig.Hours))
                            };
                            newRow.AppendChild(cell15);

                            Cell cell16 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(gig.MealBreak)
                            };
                            newRow.AppendChild(cell16);

                            Cell cell17 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(gig.PH)
                            };
                            newRow.AppendChild(cell17);

                            sheetData.AppendChild(newRow);
                        }

                        if (gigsthispay.Count > 0)
                        {
                            Row newSubtotalRow = new Row();

                            Cell cellst1 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(null)
                            };
                            newSubtotalRow.AppendChild(cellst1);

                            Cell cellst2 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(null)
                            };
                            newSubtotalRow.AppendChild(cellst2);

                            Cell cellst3 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(null)
                            };
                            newSubtotalRow.AppendChild(cellst3);

                            Cell cellst4 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(null)
                            };
                            newSubtotalRow.AppendChild(cellst4);

                            Cell cellst5 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(null)
                            };
                            newSubtotalRow.AppendChild(cellst5);

                            Cell cellst6 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(F2: F" + Convert.ToString((gigsthispay.Count) + 1) + ")")
                            };
                            newSubtotalRow.AppendChild(cellst6);

                            Cell cellst7 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(G2: G" + Convert.ToString((gigsthispay.Count) + 1) + ")")
                            };
                            newSubtotalRow.AppendChild(cellst7);

                            Cell cellst8 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(H2: H" + Convert.ToString((gigsthispay.Count) + 1) + ")")
                            };
                            newSubtotalRow.AppendChild(cellst8);

                            Cell cellst9 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(I2: I" + Convert.ToString((gigsthispay.Count) + 1) + ")")
                            };
                            newSubtotalRow.AppendChild(cellst9);

                            Cell cellst10 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(J2: J" + Convert.ToString((gigsthispay.Count) + 1) + ")")
                            };
                            newSubtotalRow.AppendChild(cellst10);

                            Cell cellst11 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(K2: K" + Convert.ToString((gigsthispay.Count) + 1) + ")")
                            };
                            newSubtotalRow.AppendChild(cellst11);

                            Cell cellst12 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(L2: L" + Convert.ToString((gigsthispay.Count) + 1) + ")")
                            };
                            newSubtotalRow.AppendChild(cellst12);

                            Cell cellst13 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(M2: M" + Convert.ToString((gigsthispay.Count) + 1) + ")")
                            };
                            newSubtotalRow.AppendChild(cellst13);

                            Cell cellst14 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(N2: N" + Convert.ToString((gigsthispay.Count) + 1) + ")")
                            };
                            newSubtotalRow.AppendChild(cellst14);

                            Cell cellst15 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(O2: O" + Convert.ToString((gigsthispay.Count) + 1) + ")")
                            };
                            newSubtotalRow.AppendChild(cellst15);

                            Cell cellst16 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(null)
                            };
                            newSubtotalRow.AppendChild(cellst16);

                            Cell cellst17 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(null)
                            };
                            newSubtotalRow.AppendChild(cellst17);

                            sheetData.AppendChild(newSubtotalRow);

                            Row newPayRatesRow = new Row();

                            Cell cellpr1 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(null)
                            };
                            newPayRatesRow.AppendChild(cellpr1);

                            Cell cellpr2 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(null)
                            };
                            newPayRatesRow.AppendChild(cellpr2);

                            Cell cellpr3 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(null)
                            };
                            newPayRatesRow.AppendChild(cellpr3);

                            Cell cellpr4 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(null)
                            };
                            newPayRatesRow.AppendChild(cellpr4);

                            Cell cellpr5 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(null)
                            };
                            newPayRatesRow.AppendChild(cellpr5);

                            Cell cellpr6 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(PayRates.LEVEL3ARATE))
                            };
                            newPayRatesRow.AppendChild(cellpr6);

                            Cell cellpr7 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(PayRates.LEVEL3BRATE))
                            };
                            newPayRatesRow.AppendChild(cellpr7);

                            Cell cellpr8 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(PayRates.LEVEL3SUNRATE))
                            };
                            newPayRatesRow.AppendChild(cellpr8);

                            Cell cellpr9 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(PayRates.VANDVRARATE))
                            };
                            newPayRatesRow.AppendChild(cellpr9);

                            Cell cellpr10 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(PayRates.VANDVRBRATE))
                            };
                            newPayRatesRow.AppendChild(cellpr10);

                            Cell cellpr11 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(PayRates.VANDVRSUNRATE))
                            };
                            newPayRatesRow.AppendChild(cellpr11);

                            Cell cellpr12 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(PayRates.MRHRAARATE))
                            };
                            newPayRatesRow.AppendChild(cellpr12);

                            Cell cellpr13 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(PayRates.MRHRABRATE))
                            };
                            newPayRatesRow.AppendChild(cellpr13);

                            Cell cellpr14 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(PayRates.MRHRASUNRATE))
                            };
                            newPayRatesRow.AppendChild(cellpr14);

                            sheetData.AppendChild(newPayRatesRow);

                            Row totalRow = new Row();

                            Cell cellt1 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(null)
                            };
                            totalRow.AppendChild(cellt1);

                            Cell cellt2 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(null)
                            };
                            totalRow.AppendChild(cellt2);

                            Cell cellt3 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(null)
                            };
                            totalRow.AppendChild(cellt3);

                            Cell cellt4 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(null)
                            };
                            totalRow.AppendChild(cellt4);

                            Cell cellt5 = new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(null)
                            };
                            totalRow.AppendChild(cellt5);

                            Cell cellt6 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= F" + Convert.ToString((gigsthispay.Count) + 2) + " * F" + Convert.ToString((gigsthispay.Count) + 3))
                            };
                            totalRow.AppendChild(cellt6);

                            Cell cellt7 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= G" + Convert.ToString((gigsthispay.Count) + 2) + " * G" + Convert.ToString((gigsthispay.Count) + 3))
                            };
                            totalRow.AppendChild(cellt7);

                            Cell cellt8 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= H" + Convert.ToString((gigsthispay.Count) + 2) + " * H" + Convert.ToString((gigsthispay.Count) + 3))
                            };
                            totalRow.AppendChild(cellt8);

                            Cell cellt9 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= I" + Convert.ToString((gigsthispay.Count) + 2) + " * I" + Convert.ToString((gigsthispay.Count) + 3))
                            };
                            totalRow.AppendChild(cellt9);

                            Cell cellt10 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= J" + Convert.ToString((gigsthispay.Count) + 2) + " * J" + Convert.ToString((gigsthispay.Count) + 3))
                            };
                            totalRow.AppendChild(cellt10);

                            Cell cellt11 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= K" + Convert.ToString((gigsthispay.Count) + 2) + " * K" + Convert.ToString((gigsthispay.Count) + 3))
                            };
                            totalRow.AppendChild(cellt11);

                            Cell cellt12 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= L" + Convert.ToString((gigsthispay.Count) + 2) + " * L" + Convert.ToString((gigsthispay.Count) + 3))
                            };
                            totalRow.AppendChild(cellt12);

                            Cell cellt13 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= M" + Convert.ToString((gigsthispay.Count) + 2) + " * M" + Convert.ToString((gigsthispay.Count) + 3))
                            };
                            totalRow.AppendChild(cellt13);

                            Cell cellt14 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= N" + Convert.ToString((gigsthispay.Count) + 2) + " * N" + Convert.ToString((gigsthispay.Count) + 3))
                            };
                            totalRow.AppendChild(cellt14);

                            Cell cellt15 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(F" + Convert.ToString((gigsthispay.Count) + 4) + ": N" + Convert.ToString((gigsthispay.Count) + 4) + ")")
                            };
                            totalRow.AppendChild(cellt15);

                            sheetData.AppendChild(totalRow);
                        }

                        workbook.WorkbookPart.Workbook.Save();

                        // Close the document.
                        workbook.Close();
                    }
                }
            }
        }
    }
}
