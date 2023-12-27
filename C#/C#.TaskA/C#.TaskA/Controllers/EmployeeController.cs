using C_.TaskA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Packaging;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace C_.TaskA.Controllers
{
    public class EmployeeController : Controller
    {
        // GET: EmployeeController
        public async Task<IActionResult> Index()
        {
            List<EmployeeModel> employees = await FetchEmployees("https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==");
            GeneratePieChart(employees);
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
                    int totalHours = employee.Sum(x => Convert.ToInt32(x.TotalTime));

                    return new EmployeeModel
                    {
                        EmployeeName = employee.Key,
                        TotalTime = totalHours
                    };
                }).OrderBy(x => x.TotalTime).ToList();

                return employees;
            }
            else
            {
                throw new Exception("An error occured while fetching data");
            }
        }

        private static void GeneratePieChart(List<EmployeeModel> employees)
        {
            var chartData = new Dictionary<string, double>();

            foreach (EmployeeModel employee in employees)
            {
                if (employee.EmployeeName == null)
                {
                    chartData["Employee name not provided"] = employee.TotalTime;
                }
                else
                {
                    chartData[employee.EmployeeName] = employee.TotalTime;
                }
            }

            int width = 800;
            int height = 800;

            using (var bitmap = new Bitmap(width, height))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    int centerX = width / 2;
                    int centerY = height / 2;
                    int radius = Math.Min(centerX, centerY);

                    float startAngle = 0;
                    float currentAngle = startAngle;

                    double total = chartData.Values.Sum();

                    foreach (var dataPoint in chartData)
                    {
                        float sweepAngle = (float)(360 * (dataPoint.Value / total));

                        var brush = new SolidBrush(GetRandomColor());

                        graphics.FillPie(brush, centerX - radius, centerY - radius, 2 * radius, 2 * radius, currentAngle, sweepAngle);

                        var labelX = (float)(centerX + radius / 2 * Math.Cos((currentAngle + currentAngle + sweepAngle) * Math.PI / 360));
                        var labelY = (float)(centerY + radius / 2 * Math.Sin((currentAngle + currentAngle + sweepAngle) * Math.PI / 360));

                        var label = $"{dataPoint.Key} ({(dataPoint.Value / total * 100):F2}%)";

                        var rotateTransform = new System.Drawing.Drawing2D.Matrix();
                        rotateTransform.RotateAt(currentAngle + sweepAngle / 2, new PointF(labelX, labelY));

                        graphics.Transform = rotateTransform;

                        graphics.DrawString(label, new Font("Arial", 10), Brushes.Black, labelX, labelY, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                        
                        graphics.ResetTransform();
                        currentAngle += sweepAngle;
                    }

                    bitmap.Save("chart.png", System.Drawing.Imaging.ImageFormat.Png);

                    static Color GetRandomColor()
                    {
                        Random random = new Random();
                        return Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
                    }
                }
            }
        }
    }
}
