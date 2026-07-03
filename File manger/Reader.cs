using Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using System.ComponentModel;
using ClosedXML.Excel;
using System.Runtime.CompilerServices;


namespace Excel
{
    public class Read
    {
        public static List<Employee> Reader (List<Employee> employees , string filePath)
        {
             ExcelPackage.License.SetNonCommercialPersonal("Nonso");
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
            var worksheet = package.Workbook.Worksheets[0];

            int rowCount = worksheet.Dimension.Rows;

            // Start at row 2 assuming row 1 = headers
            for (int row = 2; row <= rowCount; row++)
            {
                employees.Add(new Employee
                {
                    Employ_id = (worksheet.Cells[row, 1].Text),
                    Username = worksheet.Cells[row, 2].Text,
                    Age = int.Parse(worksheet.Cells[row, 3].Text),
                    Grade = worksheet.Cells[row, 4].Text,
                    Department = worksheet.Cells[row, 5].Text
                });
            }
            }

            return employees; 
        }
        
    }
}