using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JwtAuthentication.Api.ApiModels.SampleModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthentication.Api.ApiControllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class SampleDataController : Controller
    {
        private static string[] _summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet]
        public IEnumerable<WeatherForecast> WeatherForecasts()
        {
            Random rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                DateFormatted = DateTime.Now.AddDays(index).ToString("d", CultureInfo.InvariantCulture),
                TemperatureC = rng.Next(-20, 55),
                Summary = _summaries[rng.Next(_summaries.Length)]
            });
        }
    }
}
