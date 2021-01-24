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
using Border = DocumentFormat.OpenXml.Spreadsheet.Border;
using X14 = DocumentFormat.OpenXml.Office2010.Excel;
using X15 = DocumentFormat.OpenXml.Office2013.Excel;

namespace PayEstimatorUWP
{
    class ExportSpreadsheetWorkbookFile
    {
        public async void CreateSpreadsheetWorkbookFile(string filename)
        {
            Stream stream;


            var savePicker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop
            };

            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Excel", new List<string>() { ".xlsx" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "CREWONCALL" + filename;

            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                using (stream = await file.OpenStreamForWriteAsync())
                {
                    using (var spreadsheetDocument = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                    {
                        var workbookPart = spreadsheetDocument.AddWorkbookPart();

                        WorkbookStylesPart workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();

                        GenerateWorkbookStylesPart1Content(workbookStylesPart); // <== Adding stylesheet using above function

                        spreadsheetDocument.WorkbookPart.Workbook = new Workbook
                        {
                            Sheets = new Sheets()
                        };

                        var sheetPart = spreadsheetDocument.WorkbookPart.AddNewPart<WorksheetPart>();
                        var sheetData = new SheetData();
                        sheetPart.Worksheet = new Worksheet(sheetData);

                        Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                        string relationshipId = spreadsheetDocument.WorkbookPart.GetIdOfPart(sheetPart);

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
                                CellValue = new CellValue(name),
                                StyleIndex = (UInt32Value)2U
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
                                    bool mealBreak = true;

                                    if (appointment.Details.Contains("::NOBREAK"))
                                        mealBreak = false;

                                    try
                                    {
                                        if (appointment.Details.Contains("CrewOnCall::LEVEL3") && (appointment.StartTime.Date != startTime.AddDays(-1).Date))
                                            gigsthispay.Add(new Gig(appointment.StartTime, appointment.Duration, appointment.Subject, appointment.Location, "LEVEL3", mealBreak));
                                        if (appointment.Details.Contains("CrewOnCall::VANDVR") && (appointment.StartTime.Date != startTime.AddDays(-1).Date))
                                            gigsthispay.Add(new Gig(appointment.StartTime, appointment.Duration, appointment.Subject, appointment.Location, "VANDVR", mealBreak));
                                        if (appointment.Details.Contains("CrewOnCall::MR/HR") && (appointment.StartTime.Date != startTime.AddDays(-1).Date))
                                            gigsthispay.Add(new Gig(appointment.StartTime, appointment.Duration, appointment.Subject, appointment.Location, "MR/HR", mealBreak));
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
                                CellValue = new CellValue(Convert.ToString(gig.MealBreak))
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
                                CellFormula = new CellFormula("= SUM(F2: F" + Convert.ToString((gigsthispay.Count) + 1) + ")"),
                                StyleIndex = (UInt32Value)2U
                            };
                            newSubtotalRow.AppendChild(cellst6);

                            Cell cellst7 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(G2: G" + Convert.ToString((gigsthispay.Count) + 1) + ")"),
                                StyleIndex = (UInt32Value)2U
                            };
                            newSubtotalRow.AppendChild(cellst7);

                            Cell cellst8 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(H2: H" + Convert.ToString((gigsthispay.Count) + 1) + ")"),
                                StyleIndex = (UInt32Value)2U
                            };
                            newSubtotalRow.AppendChild(cellst8);

                            Cell cellst9 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(I2: I" + Convert.ToString((gigsthispay.Count) + 1) + ")"),
                                StyleIndex = (UInt32Value)2U
                            };
                            newSubtotalRow.AppendChild(cellst9);

                            Cell cellst10 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(J2: J" + Convert.ToString((gigsthispay.Count) + 1) + ")"),
                                StyleIndex = (UInt32Value)2U
                            };
                            newSubtotalRow.AppendChild(cellst10);

                            Cell cellst11 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(K2: K" + Convert.ToString((gigsthispay.Count) + 1) + ")"),
                                StyleIndex = (UInt32Value)2U
                            };
                            newSubtotalRow.AppendChild(cellst11);

                            Cell cellst12 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(L2: L" + Convert.ToString((gigsthispay.Count) + 1) + ")"),
                                StyleIndex = (UInt32Value)2U
                            };
                            newSubtotalRow.AppendChild(cellst12);

                            Cell cellst13 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(M2: M" + Convert.ToString((gigsthispay.Count) + 1) + ")"),
                                StyleIndex = (UInt32Value)2U
                            };
                            newSubtotalRow.AppendChild(cellst13);

                            Cell cellst14 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(N2: N" + Convert.ToString((gigsthispay.Count) + 1) + ")"),
                                StyleIndex = (UInt32Value)2U
                            };
                            newSubtotalRow.AppendChild(cellst14);

                            Cell cellst15 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(O2: O" + Convert.ToString((gigsthispay.Count) + 1) + ")"),
                                StyleIndex = (UInt32Value)2U
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
                                CellValue = new CellValue(Convert.ToString(PayRates.LEVEL3ARATE)),
                                StyleIndex = (UInt32Value)4U
                            };
                            newPayRatesRow.AppendChild(cellpr6);

                            Cell cellpr7 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(PayRates.LEVEL3BRATE)),
                                StyleIndex = (UInt32Value)4U
                            };
                            newPayRatesRow.AppendChild(cellpr7);

                            Cell cellpr8 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(PayRates.LEVEL3SUNRATE)),
                                StyleIndex = (UInt32Value)4U
                            };
                            newPayRatesRow.AppendChild(cellpr8);

                            Cell cellpr9 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(PayRates.VANDVRARATE)),
                                StyleIndex = (UInt32Value)4U
                            };
                            newPayRatesRow.AppendChild(cellpr9);

                            Cell cellpr10 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(PayRates.VANDVRBRATE)),
                                StyleIndex = (UInt32Value)4U
                            };
                            newPayRatesRow.AppendChild(cellpr10);

                            Cell cellpr11 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(PayRates.VANDVRSUNRATE)),
                                StyleIndex = (UInt32Value)4U
                            };
                            newPayRatesRow.AppendChild(cellpr11);

                            Cell cellpr12 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(PayRates.MRHRAARATE)),
                                StyleIndex = (UInt32Value)4U
                            };
                            newPayRatesRow.AppendChild(cellpr12);

                            Cell cellpr13 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(PayRates.MRHRABRATE)),
                                StyleIndex = (UInt32Value)4U
                            };
                            newPayRatesRow.AppendChild(cellpr13);

                            Cell cellpr14 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(Convert.ToString(PayRates.MRHRASUNRATE)),
                                StyleIndex = (UInt32Value)4U
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
                                CellFormula = new CellFormula("= F" + Convert.ToString((gigsthispay.Count) + 2) + " * F" + Convert.ToString((gigsthispay.Count) + 3)),
                                StyleIndex = (UInt32Value)3U
                            };
                            totalRow.AppendChild(cellt6);

                            Cell cellt7 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= G" + Convert.ToString((gigsthispay.Count) + 2) + " * G" + Convert.ToString((gigsthispay.Count) + 3)),
                                StyleIndex = (UInt32Value)3U
                            };
                            totalRow.AppendChild(cellt7);

                            Cell cellt8 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= H" + Convert.ToString((gigsthispay.Count) + 2) + " * H" + Convert.ToString((gigsthispay.Count) + 3)),
                                StyleIndex = (UInt32Value)3U
                            };
                            totalRow.AppendChild(cellt8);

                            Cell cellt9 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= I" + Convert.ToString((gigsthispay.Count) + 2) + " * I" + Convert.ToString((gigsthispay.Count) + 3)),
                                StyleIndex = (UInt32Value)3U
                            };
                            totalRow.AppendChild(cellt9);

                            Cell cellt10 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= J" + Convert.ToString((gigsthispay.Count) + 2) + " * J" + Convert.ToString((gigsthispay.Count) + 3)),
                                StyleIndex = (UInt32Value)3U
                            };
                            totalRow.AppendChild(cellt10);

                            Cell cellt11 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= K" + Convert.ToString((gigsthispay.Count) + 2) + " * K" + Convert.ToString((gigsthispay.Count) + 3)),
                                StyleIndex = (UInt32Value)3U
                            };
                            totalRow.AppendChild(cellt11);

                            Cell cellt12 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= L" + Convert.ToString((gigsthispay.Count) + 2) + " * L" + Convert.ToString((gigsthispay.Count) + 3)),
                                StyleIndex = (UInt32Value)3U
                            };
                            totalRow.AppendChild(cellt12);

                            Cell cellt13 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= M" + Convert.ToString((gigsthispay.Count) + 2) + " * M" + Convert.ToString((gigsthispay.Count) + 3)),
                                StyleIndex = (UInt32Value)3U
                            };
                            totalRow.AppendChild(cellt13);

                            Cell cellt14 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= N" + Convert.ToString((gigsthispay.Count) + 2) + " * N" + Convert.ToString((gigsthispay.Count) + 3)),
                                StyleIndex = (UInt32Value)3U
                            };
                            totalRow.AppendChild(cellt14);

                            Cell cellt15 = new Cell
                            {
                                DataType = CellValues.Number,
                                CellFormula = new CellFormula("= SUM(F" + Convert.ToString((gigsthispay.Count) + 4) + ": N" + Convert.ToString((gigsthispay.Count) + 4) + ")"),
                                StyleIndex = (UInt32Value)3U
                            };
                            totalRow.AppendChild(cellt15);

                            sheetData.AppendChild(totalRow);
                        }

                        spreadsheetDocument.WorkbookPart.Workbook.Save();

                        // Close the document.
                        spreadsheetDocument.Close();
                    }
                }
            }
        }

        // Creates an Stylesheet instance and adds its children.
        private void GenerateWorkbookStylesPart1Content(WorkbookStylesPart workbookStylesPart1)
        {
            Stylesheet stylesheet1 = new Stylesheet() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "x14ac x16r2 xr" } };
            stylesheet1.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            stylesheet1.AddNamespaceDeclaration("x14ac", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");
            stylesheet1.AddNamespaceDeclaration("x16r2", "http://schemas.microsoft.com/office/spreadsheetml/2015/02/main");
            stylesheet1.AddNamespaceDeclaration("xr", "http://schemas.microsoft.com/office/spreadsheetml/2014/revision");

            NumberingFormats numberingFormats1 = new NumberingFormats() { Count = (UInt32Value)1U };
            NumberingFormat numberingFormat1 = new NumberingFormat() { NumberFormatId = (UInt32Value)44U, FormatCode = "_-\"$\"* #,##0.00_-;\\-\"$\"* #,##0.00_-;_-\"$\"* \"-\"??_-;_-@_-" };

            numberingFormats1.Append(numberingFormat1);

            Fonts fonts1 = new Fonts() { Count = (UInt32Value)4U };

            Font font1 = new Font();
            FontSize fontSize1 = new FontSize() { Val = 11D };
            FontName fontName1 = new FontName() { Val = "Calibri" };

            font1.Append(fontSize1);
            font1.Append(fontName1);

            Font font2 = new Font();
            FontSize fontSize2 = new FontSize() { Val = 11D };
            FontName fontName2 = new FontName() { Val = "Calibri" };

            font2.Append(fontSize2);
            font2.Append(fontName2);

            Font font3 = new Font();
            Bold bold1 = new Bold();
            FontSize fontSize3 = new FontSize() { Val = 11D };
            FontName fontName3 = new FontName() { Val = "Calibri" };
            FontFamilyNumbering fontFamilyNumbering1 = new FontFamilyNumbering() { Val = 2 };

            font3.Append(bold1);
            font3.Append(fontSize3);
            font3.Append(fontName3);
            font3.Append(fontFamilyNumbering1);

            Font font4 = new Font();
            Italic italic1 = new Italic();
            FontSize fontSize4 = new FontSize() { Val = 11D };
            FontName fontName4 = new FontName() { Val = "Calibri" };
            FontFamilyNumbering fontFamilyNumbering2 = new FontFamilyNumbering() { Val = 2 };

            font4.Append(italic1);
            font4.Append(fontSize4);
            font4.Append(fontName4);
            font4.Append(fontFamilyNumbering2);

            fonts1.Append(font1);
            fonts1.Append(font2);
            fonts1.Append(font3);
            fonts1.Append(font4);

            Fills fills1 = new Fills() { Count = (UInt32Value)2U };

            Fill fill1 = new Fill();
            PatternFill patternFill1 = new PatternFill() { PatternType = PatternValues.None };

            fill1.Append(patternFill1);

            Fill fill2 = new Fill();
            PatternFill patternFill2 = new PatternFill() { PatternType = PatternValues.Gray125 };

            fill2.Append(patternFill2);

            fills1.Append(fill1);
            fills1.Append(fill2);

            Borders borders1 = new Borders() { Count = (UInt32Value)1U };

            Border border1 = new Border();
            LeftBorder leftBorder1 = new LeftBorder();
            RightBorder rightBorder1 = new RightBorder();
            TopBorder topBorder1 = new TopBorder();
            BottomBorder bottomBorder1 = new BottomBorder();
            DiagonalBorder diagonalBorder1 = new DiagonalBorder();

            border1.Append(leftBorder1);
            border1.Append(rightBorder1);
            border1.Append(topBorder1);
            border1.Append(bottomBorder1);
            border1.Append(diagonalBorder1);

            borders1.Append(border1);

            CellStyleFormats cellStyleFormats1 = new CellStyleFormats() { Count = (UInt32Value)2U };
            CellFormat cellFormat1 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U };
            CellFormat cellFormat2 = new CellFormat() { NumberFormatId = (UInt32Value)44U, FontId = (UInt32Value)1U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, ApplyFont = false, ApplyFill = false, ApplyBorder = false, ApplyAlignment = false, ApplyProtection = false };

            cellStyleFormats1.Append(cellFormat1);
            cellStyleFormats1.Append(cellFormat2);

            // Cell Formats <== cell styleindex is referring to one of these
            CellFormats cellFormats1 = new CellFormats() { Count = (UInt32Value)5U };

            // StyleIndex = (UInt32Value)0U : Normal Format
            CellFormat cellFormat3 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U };

            // StyleIndex = (UInt32Value)1U : Currency Format
            CellFormat cellFormat4 = new CellFormat() { NumberFormatId = (UInt32Value)44U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)1U, ApplyFont = true };

            // StyleIndex = (UInt32Value)2U : Bold Font
            CellFormat cellFormat5 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)2U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyFont = true };

            // StyleIndex = (UInt32Value)3U : Bold Font : Currency Format
            CellFormat cellFormat6 = new CellFormat() { NumberFormatId = (UInt32Value)44U, FontId = (UInt32Value)2U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)1U, ApplyFont = true };

            // StyleIndex = (UInt32Value)4U : Italic Font : Currency Format
            CellFormat cellFormat7 = new CellFormat() { NumberFormatId = (UInt32Value)44U, FontId = (UInt32Value)3U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)1U, ApplyFont = true };

            cellFormats1.Append(cellFormat3);
            cellFormats1.Append(cellFormat4);
            cellFormats1.Append(cellFormat5);
            cellFormats1.Append(cellFormat6);
            cellFormats1.Append(cellFormat7);

            CellStyles cellStyles1 = new CellStyles() { Count = (UInt32Value)2U };
            CellStyle cellStyle1 = new CellStyle() { Name = "Currency", FormatId = (UInt32Value)1U, BuiltinId = (UInt32Value)4U };
            CellStyle cellStyle2 = new CellStyle() { Name = "Normal", FormatId = (UInt32Value)0U, BuiltinId = (UInt32Value)0U };

            cellStyles1.Append(cellStyle1);
            cellStyles1.Append(cellStyle2);
            DifferentialFormats differentialFormats1 = new DifferentialFormats() { Count = (UInt32Value)0U };
            TableStyles tableStyles1 = new TableStyles() { Count = (UInt32Value)0U, DefaultTableStyle = "TableStyleMedium2", DefaultPivotStyle = "PivotStyleLight16" };

            StylesheetExtensionList stylesheetExtensionList1 = new StylesheetExtensionList();

            StylesheetExtension stylesheetExtension1 = new StylesheetExtension() { Uri = "{EB79DEF2-80B8-43e5-95BD-54CBDDF9020C}" };
            stylesheetExtension1.AddNamespaceDeclaration("x14", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/main");
            X14.SlicerStyles slicerStyles1 = new X14.SlicerStyles() { DefaultSlicerStyle = "SlicerStyleLight1" };

            stylesheetExtension1.Append(slicerStyles1);

            StylesheetExtension stylesheetExtension2 = new StylesheetExtension() { Uri = "{9260A510-F301-46a8-8635-F512D64BE5F5}" };
            stylesheetExtension2.AddNamespaceDeclaration("x15", "http://schemas.microsoft.com/office/spreadsheetml/2010/11/main");
            X15.TimelineStyles timelineStyles1 = new X15.TimelineStyles() { DefaultTimelineStyle = "TimeSlicerStyleLight1" };

            stylesheetExtension2.Append(timelineStyles1);

            stylesheetExtensionList1.Append(stylesheetExtension1);
            stylesheetExtensionList1.Append(stylesheetExtension2);

            stylesheet1.Append(numberingFormats1);
            stylesheet1.Append(fonts1);
            stylesheet1.Append(fills1);
            stylesheet1.Append(borders1);
            stylesheet1.Append(cellStyleFormats1);
            stylesheet1.Append(cellFormats1);
            stylesheet1.Append(cellStyles1);
            stylesheet1.Append(differentialFormats1);
            stylesheet1.Append(tableStyles1);
            stylesheet1.Append(stylesheetExtensionList1);

            workbookStylesPart1.Stylesheet = stylesheet1;
        }


    }
}
