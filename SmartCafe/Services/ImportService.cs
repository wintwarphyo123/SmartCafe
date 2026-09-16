using OfficeOpenXml;
using System.Reflection;

namespace SmartCafe.Services
{
    public class ImportService
    {
        public List<T> ImportFromExcelStream<T>(Stream fileStream,
           KeyValuePair<string, string>[] columnMapping) where T : new()
        {
            var list = new List<T>();
            using var package = new ExcelPackage(fileStream);
            var workSheet = package.Workbook.Worksheets.FirstOrDefault();
            if (workSheet == null || workSheet.Dimension == null)
            {
                return list;
            }
            int rowCount = workSheet.Dimension.Rows;
            int colCount = workSheet.Dimension.Columns;

            var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int col = 1; col <= colCount; col++)
            {
                string headerValue = workSheet.Cells[1, col].Text.Trim().Replace(" ", "");
                if (!string.IsNullOrEmpty(headerValue) && !headerMap.ContainsKey(headerValue))
                {
                    headerMap.Add(headerValue, col);
                }
            }
            PropertyInfo[] properties = typeof(T).GetProperties();
            for (int row = 2; row <= rowCount; row++)
            {
                // Row တစ်ခုလုံး အလွတ် ဖြစ်နေပါက Skip လုပ်မည်
                if (IsRowEmpty(workSheet, row, colCount)) continue;

                T item = new T();
                bool hasData = false;

                foreach (var mapping in columnMapping)
                {
                    string excelHeader = mapping.Key.Trim().Replace(" ", "");
                    string propName = mapping.Value;

                    if (headerMap.TryGetValue(excelHeader, out int colIndex))
                    {
                        object? cellValue = workSheet.Cells[row, colIndex].Value;
                        string cellText = workSheet.Cells[row, colIndex].Text.Trim();
                        PropertyInfo? prop = properties.FirstOrDefault(p => p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase));

                        if (prop != null && !prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrEmpty(cellText))
                            {
                                SetPropertyValue(item, prop, cellValue ?? cellText);
                                hasData = true;
                            }
                        }
                    }
                }
                PropertyInfo? nameProp = typeof(T).GetProperty("Name") ?? typeof(T).GetProperty("IngredientName");
                if (nameProp != null)
                {
                    var nameVal = nameProp.GetValue(item)?.ToString();
                    if (string.IsNullOrWhiteSpace(nameVal))
                    {
                        continue;
                    }
                }

                if (hasData)
                {
                    list.Add(item);
                }
            }

            return list;
        }

        private static bool IsRowEmpty(ExcelWorksheet worksheet, int row, int colCount)
        {
            for (int col = 1; col <= colCount; col++)
            {
                if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, col].Text))
                    return false;
            }
            return true;
        }

        private static void SetPropertyValue<T>(T item, PropertyInfo prop, object rawValue)
        {
            try
            {
                Type targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                object? convertedValue = null;
                string rawString = rawValue.ToString()?.Trim() ?? "";

                rawString = rawString.Replace("\r\n", " ").Replace("\n", " ");

                if (targetType == typeof(string))
                {
                    convertedValue = rawString;
                }
                else if (targetType == typeof(int))
                {
                    if (int.TryParse(rawString, out int intVal)) convertedValue = intVal;
                }
                else if (targetType == typeof(float))
                {
                    if (float.TryParse(rawString, out float floatVal)) convertedValue = floatVal;
                }
                else if (targetType == typeof(double))
                {
                    if (double.TryParse(rawString, out double doubleVal)) convertedValue = doubleVal;
                }
                else if (targetType == typeof(decimal))
                {
                    if (decimal.TryParse(rawString, out decimal decimalVal)) convertedValue = decimalVal;
                }
                else if (targetType == typeof(DateTime))
                {
                    if (DateTime.TryParse(rawString, out DateTime dateVal)) convertedValue = dateVal;
                }

                if (convertedValue != null)
                {
                    prop.SetValue(item, convertedValue);
                }
            }
            catch
            {
                // Type Conversion Error တက်ပါက Data မပျက်စေရန် အသိအမှတ်ပြု၍ Skip လုပ်မည်
            }
        }
    }
}
