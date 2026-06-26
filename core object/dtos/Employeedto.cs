using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Principal;

namespace Excel
{
    public class Employee
    {
      public  string Employ_id {  get; set; } 
      public string Username {  get; set; }
        public int Age { get; set; }
      public string Grade {  get; set; }
      public string Department {  get; set; }
    }
}
