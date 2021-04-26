using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Workgroupoc
{
  public static class Constants
  {
    public static string managementObjectScope = @"root\CIMV2";
    public static string managementObjectQuery = @"SELECT * FROM Win32_ComputerSystem";
    public static string accountCreatedMsg = "User Created Successfully";
    public static string userAddWGDescription = "User added in work group";
    public static string createWGMethodName = "JoinDomainOrWorkgroup";
    public static string managementObjectPath = $"Win32_ComputerSystem.Name={Environment.MachineName}";
    public static string configFileName = "appsettings.json";
    public static string userDoesntExistMsg = "User doesn't exist!!";
    public static string userRemovedmsg = "User removed from group";
    public static string userUpdatedmsg = "User password is Changed sucessfully!!";
    public static string userAuthenticatedMsg = "User is Authenticated";
    public static string unAuthorizedUserMsg = "User is not Authenticated!!";
    public static string pwdNotCatMsg = "Paswword should conain any three categeories!!";
    public static string pwdMinLenMsg = "Password length should not be less than 6";
    public static string pwdShdNtUNMsg = "Password should not contain user name!!";
    public static string usrpwdReqMsg = "Username and Password is mandatory!!";
    public static string wrkgrpLenMsg = "WorkGroup Length is either 0 or beyond 15 characters!!";
    public static string usrNotFoundMsg = "User not Found!!";
  }

}
