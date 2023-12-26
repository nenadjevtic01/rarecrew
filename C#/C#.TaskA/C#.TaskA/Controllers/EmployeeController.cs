using C_.TaskA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace C_.TaskA.Controllers
{
    public class EmployeeController : Controller
    {
        // GET: EmployeeController
        public async Task<IActionResult> Index()
        {
            List<EmployeeModel> employees = await FetchEmployees("https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==");
            return View(employees);
        }

        private async Task<List<EmployeeModel>> FetchEmployees(string path)
        {
                HttpClient client = new HttpClient();

                HttpResponseMessage response = await client.GetAsync(path);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    List<RawEmployeeModel> RawEmployees = JsonConvert.DeserializeObject<List<RawEmployeeModel>>(json);

                    List<EmployeeModel> employees = new List<EmployeeModel>();

                    foreach (var employee in RawEmployees)
                    {
                        TimeSpan span = employee.EndTimeUtc - employee.StarTimeUtc;
                        EmployeeModel transformedEmployee = new EmployeeModel
                        {
                            Id = employee.Id,
                            EmployeeName = employee.EmployeeName,
                            TotalTime = span.TotalHours
                        };
                        employees.Add(transformedEmployee);
                    }

                    employees = employees.GroupBy(x => x.EmployeeName).Select(employee =>
                    {
                        int totalHours=employee.Sum(x=>Convert.ToInt32(x.TotalTime));

                        return new EmployeeModel
                        {
                            EmployeeName = employee.Key,
                            TotalTime = totalHours
                        };
                    }).OrderBy(x=>x.TotalTime).ToList();

                    return employees;
                }
                else
                {
                    throw new Exception("An error occured while fetching data");
                }
        }

    }
}
