
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.OleDb;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Management;

namespace Workgroupoc.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class WorkGroupController : ControllerBase
  {
    private char[] specialChar = new char[] { '~', '`', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '+', '=', '\'' };

    private readonly ILogger<WorkGroupController> _logger;
    public WorkGroupController(ILogger<WorkGroupController> logger)
    {
      _logger = logger;
    }
    [HttpGet]
    [Route("AuthenticateUserInWG")]
    public IActionResult AuthenticateUserInWG(string userName, string password)
    {
      try
      {
        if (!string.IsNullOrEmpty(userName) && (!string.IsNullOrEmpty(password)))
        {
          String passwordStatus = IsValidatePassword(userName, password);
          if (!string.IsNullOrEmpty(passwordStatus))
          {

            bool isValid;
            using (PrincipalContext pc = new PrincipalContext(ContextType.Machine))
            {
              UserPrincipal user = UserPrincipal.FindByIdentity(pc, userName);
              // validate the credentials
              isValid = pc.ValidateCredentials(userName.ToUpper(), password);
            }
            if (isValid)
            {
              _logger.LogInformation(Constants.userAuthenticatedMsg);
              return Ok(Constants.userAuthenticatedMsg);

            }
            else
            {
              _logger.LogInformation(Constants.unAuthorizedUserMsg);
              return NotFound(Constants.unAuthorizedUserMsg);
            }
          }
          else
          {
            _logger.LogInformation(passwordStatus);
            return Ok(passwordStatus);
          }
        }
        else
        {
          _logger.LogInformation(Constants.usrpwdReqMsg);
          return Ok(Constants.usrpwdReqMsg);

        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.Message.ToString());
        return Ok(ex.Message.ToString());
      }
    }
    [HttpGet]
    [Route("AddUserInWG")]
    public IActionResult AddUserInWG(string userName, string password)
    {
      try
      {
        if (!string.IsNullOrEmpty(userName)&&(!string.IsNullOrEmpty(password)))
        {
          String passwordStatus = IsValidatePassword(userName, password);
          if (string.IsNullOrEmpty(passwordStatus))
          {

            //CreateWorkGroup();

            // var config = GetConfiguration();
            DirectoryEntry AD = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
            DirectoryEntry NewUser = AD.Children.Add(userName, "user");
            NewUser.Invoke("SetPassword", new object[] { password });
            NewUser.Invoke("Put", new object[] { "Description", Constants.userAddWGDescription });
            NewUser.CommitChanges();

            //add user in group
            DirectoryEntry grp;
            grp = AD.Children.Find("Guests", "group");
            if (grp != null) { grp.Invoke("Add", new object[] { NewUser.Path.ToString() }); }
            _logger.LogInformation(Constants.accountCreatedMsg);
            return Ok(Constants.accountCreatedMsg);
          }
          else
          {
            _logger.LogInformation(passwordStatus);
            return Ok(passwordStatus);
          }
        }
        else
        {
          _logger.LogInformation(Constants.usrpwdReqMsg);
          return Ok(Constants.usrpwdReqMsg);

        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.Message.ToString());
        return Ok(ex.Message.ToString());
      }
    }
    [HttpGet]
    [Route("AddBulkUserInWG")]
    public IActionResult AddBulkUserInWG()
    {
      try
      {
        // CreateWorkGroup();
        using (OleDbConnection conn = new OleDbConnection())
        {
          string filename = @"C:\NewFolder\Users.xlsx";
          string fileExtension = Path.GetExtension(filename);
          if (fileExtension == ".xls")
            conn.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filename + ";" + "Extended Properties='Excel 8.0;HDR=YES;'";
          if (fileExtension == ".xlsx")
            conn.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filename + ";" + "Extended Properties='Excel 12.0 Xml;HDR=YES;'";
          using (OleDbCommand comm = new OleDbCommand())
          {
            comm.CommandText = "Select * from [User$]";
            comm.Connection = conn;

            DataSet ds = new DataSet();
            using (OleDbDataAdapter da = new OleDbDataAdapter())
            {
              da.SelectCommand = comm;
              da.Fill(ds, "User");
            }
            DataTable dataTable = ds.Tables["User"];
            var users = dataTable.AsEnumerable().
            Select(user => new User()
            {
              UserName = user.Field<string>("UserName"),
              Password = user.Field<string>("Password")
            }).ToList();

            var config = GetConfiguration();
            DirectoryEntry AD = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
            if (users != null)
            {

              users.ForEach(u =>
              {
                try
                {
                  if (!string.IsNullOrEmpty(u.UserName) && (!string.IsNullOrEmpty(u.Password)))
                  {
                    String passwordStatus = IsValidatePassword(u.UserName, u.Password);
                    if (string.IsNullOrEmpty(passwordStatus))
                    {
                      DirectoryEntry NewUser = AD.Children.Add(u.UserName, "user");
                      NewUser.Invoke("SetPassword", new object[] { u.Password });
                      NewUser.Invoke("Put", new object[] { "Description", Constants.userAddWGDescription });
                      NewUser.CommitChanges();

                      //add user in group
                      DirectoryEntry grp;
                      grp = AD.Children.Find("Guests", "group");
                      if (grp != null) { grp.Invoke("Add", new object[] { NewUser.Path.ToString() }); }
                      _logger.LogInformation($"user {u.UserName} added");
                    }
                    else
                    {
                      _logger.LogInformation(passwordStatus);
                    
                    }
                  }
                  else
                  {
                    _logger.LogInformation(Constants.usrpwdReqMsg);
                 

                  }


                }
                catch (Exception ex)
                {
                  _logger.LogInformation($"user {u.UserName} not added. Exception-{ex.ToString()}");
                }
              });
            }
          }
        }
        _logger.LogInformation(Constants.accountCreatedMsg);
        return Ok(Constants.accountCreatedMsg);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.Message.ToString());
        return Ok(ex.Message.ToString());
      }
    }

    [HttpGet]
    [Route("FindUserInWG")]
    public IActionResult FindUserInWG(string userName)
    {
      DirectoryEntry user;
      try
      {
        if (!string.IsNullOrEmpty(userName))
        {

          DirectoryEntry AD = new DirectoryEntry("WinNT://" +
       Environment.MachineName + ",computer");

          user = AD.Children.Find(userName, "user");

          if (user != null)
          {
            _logger.LogInformation($"{user.Name} is found.");
            return Ok(user.Name);
          }
          else
          {
            _logger.LogInformation(Constants.userDoesntExistMsg);
            return Ok(Constants.userDoesntExistMsg);
          }
        }
        else
        {
          _logger.LogInformation(Constants.usrpwdReqMsg);
          return Ok(Constants.usrpwdReqMsg);

        }
      }
      catch (Exception ex)
      {
        _logger.LogInformation(ex.Message.ToString());
        return Ok(ex.Message.ToString());
      }
    }
    [HttpGet]
    [Route("RemoveUserInWG")]
    public IActionResult RemoveUserInWG(string userName)
    {
      try
      {
        if (!string.IsNullOrEmpty(userName))
        {
          DirectoryEntry AD = new DirectoryEntry("WinNT://" +
        Environment.MachineName + ",computer");
          var user = AD.Children.Find(userName, "user");
          if (user != null)
          {
            AD.Children.Remove(user);
            _logger.LogInformation($"{user.Name} is Removed.");
            return Ok(Constants.userRemovedmsg);
          }
          else
            _logger.LogInformation(Constants.userDoesntExistMsg);
          return Ok(Constants.userDoesntExistMsg);
        }
        else
        {
          _logger.LogInformation(Constants.usrpwdReqMsg);
          return Ok(Constants.usrpwdReqMsg);

        }
      }

      catch (Exception ex)
      {
        _logger.LogError(ex.Message.ToString());
        return Ok(ex.ToString());
      }
    }
    [HttpGet]
    [Route("UpdateUserPasswordInWG")]
    public IActionResult UpdateUserPasswordInWG(string userName, string password)
    {
      try
      {
        if (!string.IsNullOrEmpty(userName) && (!string.IsNullOrEmpty(password)))
        {
          String passwordStatus = IsValidatePassword(userName, password);
          if (!string.IsNullOrEmpty(passwordStatus))
          {
            DirectoryEntry AD = new DirectoryEntry("WinNT://" +
        Environment.MachineName + ",computer");
            var user = AD.Children.Find(userName, "user");
            if (user != null)
            {

              object[] pwd = new object[] { password };
              object ret = user.Invoke("SetPassword", password);
              _logger.LogInformation(Constants.userUpdatedmsg);
              return Ok(Constants.userUpdatedmsg);
            }
            else
              _logger.LogInformation(Constants.userDoesntExistMsg);
            return Ok(Constants.userDoesntExistMsg);
          }
          else
          {
            _logger.LogInformation(passwordStatus);
            return Ok(passwordStatus);
          }
        }
        else
        {
          _logger.LogInformation(Constants.usrpwdReqMsg);
          return Ok(Constants.usrpwdReqMsg);

        }
      }

      catch (Exception ex)
      {
        _logger.LogError(ex.Message.ToString());
        return Ok(ex.ToString());
      }
    }

    [NonAction]
    public void CreateWorkGroup()
    {
      try
      {
        var config = GetConfiguration();
        string workGroupName = config["workGroupName"];
        if (!string.IsNullOrEmpty(workGroupName) && workGroupName.ToArray().Length < 15)
        {



          ManagementObject manage = new ManagementObject(Constants.managementObjectPath);
          object[] args = { config["workGroupName"], null, null, null };
          manage.InvokeMethod(Constants.createWGMethodName, args);


          ManagementObjectSearcher mos = new ManagementObjectSearcher(Constants.managementObjectScope, Constants.managementObjectQuery);
          foreach (ManagementObject mo in mos.Get())
          {
            Console.WriteLine(mo["Workgroup"]);
          }
        }
        else
        {
          _logger.LogInformation(Constants.wrkgrpLenMsg);


        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.Message.ToString());
        throw ex;
      }
    }
    [NonAction]
    public string IsValidatePassword(string password, string userName)
    {
      if (password.ToCharArray().Length > 6)
      {

        if ((!password.Any(char.IsUpper) && password.Any(char.IsLower) && specialChar.Any(password.Contains))
       || (password.Any(char.IsUpper) && password.Any(char.IsLower) && specialChar.Any(password.Contains))
       || (password.Any(char.IsUpper) && !password.Any(char.IsLower)  && specialChar.Any(password.Contains))
       || (password.Any(char.IsUpper) && password.Any(char.IsLower)&& specialChar.Any(password.Contains))
       || (password.Any(char.IsUpper) && password.Any(char.IsLower) && !specialChar.Any(password.Contains)))
        {
          if (password.Contains(userName))
          {
            return Constants.pwdShdNtUNMsg;
          }
          else
          {
            return "";
          }
        }
        else
        {
          return Constants.pwdNotCatMsg;
        }
      }
      else
      {
        return Constants.pwdMinLenMsg;
      }

    }
    [NonAction]
    public dynamic GetConfiguration()
    {
      return new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile(Constants.configFileName).Build();
    }
  }
}
