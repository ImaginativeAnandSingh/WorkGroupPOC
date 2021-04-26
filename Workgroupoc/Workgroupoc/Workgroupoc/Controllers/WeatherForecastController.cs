using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Management;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Microsoft.AspNetCore.Authentication;


namespace Workgroupoc.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }
    [HttpGet]
    [Route("AddUserInWG")]
    public void AddUserInWG()
    {
      try
      {
        ManagementObject manage = new ManagementObject(string.Format("Win32_ComputerSystem.Name='{0}'", Environment.MachineName));
        object[] args = { "SampleGroup22", null, null, null };
        manage.InvokeMethod("JoinDomainOrWorkgroup", args);

        ManagementObjectSearcher mos =
       new ManagementObjectSearcher(@"root\CIMV2", @"SELECT * FROM Win32_ComputerSystem");
        foreach (ManagementObject mo in mos.Get())
        {
          Console.WriteLine(mo["Workgroup"]);
        }
        //Win32_ComputerSystem.Name = "BLRDRIFWN41236"
        DirectoryEntry AD = new DirectoryEntry("WinNT://" +
        Environment.MachineName + ",computer");
        DirectoryEntry NewUser = AD.Children.Add("TestUser", "user");
        NewUser.Invoke("SetPassword", new object[] { "#12345Abc" });
        NewUser.Invoke("Put", new object[] { "Description", "Test User from .NET" });
        NewUser.CommitChanges();
        DirectoryEntry grp;

        grp = AD.Children.Find("Guests", "group");
        if (grp != null) { grp.Invoke("Add", new object[] { NewUser.Path.ToString() }); }
        Console.WriteLine("Account Created Successfully");
        Console.ReadLine();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Console.ReadLine();
      }
    }

    [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    [HttpGet]
    [Route("FindUserInWG")]
    public void FindUserInWG()
    {
      try
      {
//        ManagementObject manage = new ManagementObject(string.Format("Win32_ComputerSystem.Name='{0}'", Environment.MachineName));
//        object[] args = { "SampleGroup22", null, null, null };
//        manage.InvokeMethod("JoinDomainOrWorkgroup", args);



//        ManagementObjectSearcher mos =
//new ManagementObjectSearcher(@"root\CIMV2", @"SELECT * FROM Win32_ComputerSystem");
//        foreach (ManagementObject mo in mos.Get())
//        {
//          Console.WriteLine(mo["Workgroup"]);
//        }

        DirectoryEntry AD = new DirectoryEntry("WinNT://" +
        Environment.MachineName + ",computer");
        //var user = AD.Children.Find("TestUser005");

        var user = AD.Children.Find("TestUser007", "user");
        AD.Children.Remove(user);

        Console.WriteLine("Account Created Successfully");
        Console.ReadLine();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Console.ReadLine();
      }
    }
    [HttpGet]
    [Route("RemoveUserInWG")]
    public IActionResult RemoveUserInWG(string userName)
    {
      try
      {



        DirectoryEntry AD = new DirectoryEntry("WinNT://" +
        Environment.MachineName + ",computer");
        var user = AD.Children.Find(userName, "user");
        if (user != null)
        {
          AD.Children.Remove(user);
          return Ok(Constants.userRemovedmsg);
        }
        else
          return Ok(Constants.userDoesntExistMsg);
      }
      catch (Exception ex)
      {
        return Ok(ex.ToString());
      }
    }
    //[NonAction]
    //public void CreateWorkGroup()
    //{
    //  try
    //  {
    //    var config = GetConfiguration();
    //    ManagementObject manage = new ManagementObject(Constants.managementObjectPath);
    //    object[] args = { config["workGroupName"], null, null, null };
    //    manage.InvokeMethod(Constants.createWGMethodName, args);



    //    ManagementObjectSearcher mos = new ManagementObjectSearcher(Constants.managementObjectScope, Constants.managementObjectQuery);
    //    foreach (ManagementObject mo in mos.Get())
    //    {
    //      Console.WriteLine(mo["Workgroup"]);
    //    }
    //  }
    //  catch (Exception ex)
    //  {
    //    throw ex;
    //  }
    //}
    //[NonAction]
    //public dynamic GetConfiguration()
    //{
    //  return new ConfigurationBuilder()
    //     .SetBasePath(Directory.GetCurrentDirectory())
    //     .AddJsonFile(Constants.configFileName).Build();
    //}

  }
}

