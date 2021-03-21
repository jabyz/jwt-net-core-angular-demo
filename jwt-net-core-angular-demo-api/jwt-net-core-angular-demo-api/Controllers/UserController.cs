using jwt_net_core_angular_demo_api.Models;
using jwt_net_core_angular_demo_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace jwt_net_core_angular_demo_api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class UserController : ControllerBase
  {
    
  private readonly IConfiguration _conf;
    private readonly string _JWT_Secret;
    private readonly IWebHostEnvironment _env;

    public UserController(IConfiguration conf, IWebHostEnvironment env)
    {
      _conf = conf;
      _JWT_Secret = _conf["ApplicationSettings:JWT_Secret"];
      _env = env;
    }

    [HttpPost]
    [Route("Login")]
    //POST : /api/User/Login
    public JsonResult loginUser(Usr user)
    {
      try
      {
        string qry = "dbo.spUsrLogin";
        SqlCommand myCmd = new SqlCommand(qry);
        myCmd.Parameters.AddWithValue("@pUserName", user.UserName);
        myCmd.Parameters.AddWithValue("@pPassword", user.Password);
        myCmd.CommandType = CommandType.StoredProcedure;

        Sql myClsSql = new Sql(_conf);

        DataTable dt = myClsSql.cmdToDataTable(myCmd);

        Msg myMsg = new Msg();
        myMsg.ResponseMessage = dt.Rows[0]["responseMessage"].ToString();
        myMsg.ResponseStatus = dt.Rows[0]["responseStatus"].ToString();

        //UNSUCCESSFUL LOGIN
        if (myMsg.ResponseStatus == "0")
        {
          return new JsonResult(new { myResponseMessage = myMsg.ResponseMessage, myResponseStatus = myMsg.ResponseStatus });
        }

        //SUCCESSFUL LOGIN RETURN JWT
        var tokenDescriptor = new SecurityTokenDescriptor
        {
          Subject = new ClaimsIdentity(new Claim[]
          {
                    new Claim("UserID",dt.Rows[0]["UserID"].ToString())
          }),
          Expires = DateTime.UtcNow.AddMinutes(5),
          SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JWT_Secret)), SecurityAlgorithms.HmacSha256Signature)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var token = tokenHandler.WriteToken(securityToken);
        return new JsonResult(new { myResponseMessage = myMsg.ResponseMessage, myToken = token, myResponseStatus = myMsg.ResponseStatus });
      }
      catch (Exception ex)
      {

        return new JsonResult(new { myResponseMessage = ex.Message, myResponseStatus = "0" });
      }
    }

    [HttpGet]
    [Authorize]
    //GET : /api/User
    public JsonResult getUser()
    {
      string UserID = User.Claims.First(c => c.Type == "UserID").Value;

      string qry = "SELECT UserName, FirstName +' ' + LastName FullName,Email FROM [Usr] WHERE Id = @UserID";

      SqlCommand myCmd = new SqlCommand(qry);
      myCmd.Parameters.AddWithValue("@UserID", UserID);
      myCmd.CommandType = CommandType.Text;

      Sql myClsSql = new Sql(_conf);

      DataTable dt = myClsSql.cmdToDataTable(myCmd);

      return new JsonResult(dt);
    }

    [HttpPost]
    [Route("Register")]
    //POST : /api/User/Register
    public JsonResult addUser(Usr user)
    {
      try
      {
        string qry = "spUsrRegister";
        SqlCommand myCmd = new SqlCommand(qry);
        myCmd.Parameters.AddWithValue("@pUserName", user.UserName);
        myCmd.Parameters.AddWithValue("@pPassword", user.Password);
        myCmd.Parameters.AddWithValue("@pFirstName", user.FirstName);
        myCmd.Parameters.AddWithValue("@pLastName", user.LastName);
        myCmd.Parameters.AddWithValue("@pEmail", user.Email);
        myCmd.CommandType = CommandType.StoredProcedure;

        Sql myClsSql = new Sql(_conf);

        DataTable dt = myClsSql.cmdToDataTable(myCmd);

        
        Msg myMsg = new Msg();
        myMsg.ResponseMessage = dt.Rows[0]["responseMessage"].ToString();
        myMsg.ResponseStatus = dt.Rows[0]["responseStatus"].ToString();

        enviarCorreoNewUsr(user);

        return new JsonResult(new { myResponseMessage = myMsg.ResponseMessage, myResponseStatus = myMsg.ResponseStatus });

      }
      catch (Exception ex)
      {

        return new JsonResult(new { myResponseMessage = ex.Message, myResponseStatus = "0" });
      }


    }

    [HttpPost]
    [Route("ValidateEmail")]
    //GET : /api/User/ValidateEmail
    //public JsonResult validateUserEmailCheck(User user)
    //{
    //string qry = "";

    //}


    public JsonResult ValidateEmail(object jsonObj)
    {
      dynamic obj = JObject.Parse(jsonObj.ToString());
      var uid = obj.uid;
      string qry = "UPDATE Usr SET EmailConfirmed = 1 WHERE REPLACE(Salt,'-','') = @UID";
      SqlCommand myCmd = new SqlCommand(qry);
      myCmd.Parameters.AddWithValue("@UID", uid.ToString());
      myCmd.CommandType = CommandType.Text;
      Sql mySql = new Sql(_conf);
      Msg myMsg = new Msg();

      int ra = mySql.xQuery(myCmd);

      if (ra == 1)
      {
        myMsg.ResponseMessage = "Successfully Verified Email, you may now Login!";
        myMsg.ResponseStatus = "1";
      }

      if (ra == 0)
      {
        myMsg.ResponseMessage = "Email not verified";
        myMsg.ResponseStatus = "0";
      }

      return new JsonResult(new { myResponseMessage = myMsg.ResponseMessage, myResponseStatus = myMsg.ResponseStatus });
    }

    public void enviarCorreoNewUsr(Usr user)
    {
      string qry = "SELECT REPLACE(Salt,'-','') SaltNoDash,* FROM [Usr] WHERE UserName = @UserName";
      SqlCommand myCmd = new SqlCommand(qry);
      myCmd.Parameters.AddWithValue("@UserName", user.UserName);
      myCmd.CommandType = CommandType.Text;

      Sql myClsSql = new Sql(_conf);

      DataTable dt = myClsSql.cmdToDataTable(myCmd);
      string UserName = dt.Rows[0]["UserName"].ToString();
      string FirstName = dt.Rows[0]["FirstName"].ToString();
      string SaltNoDash = dt.Rows[0]["SaltNoDash"].ToString();
      string Email = dt.Rows[0]["Email"].ToString();



      string body = string.Empty;
      StreamReader reader = new StreamReader(_env.ContentRootPath + "/resources/eMailNewUser.html");
      body = reader.ReadToEnd();
      body = body.Replace("[[FirstName]]", FirstName);
      body = body.Replace("[[UserName]]", UserName);
      body = body.Replace("[[param1]]", SaltNoDash);
      body = body.Replace("[[param1]]", SaltNoDash);
      body = body.Replace("[[Company]]", "[[Company]]");
      body = body.Replace("[[myUrl]]", "http://localhost:4200/user/validateEmail/");


      enviarCorreoHTML(Email, "Verify you new Novamex account", body);

    }
    public bool enviarCorreoHTML(string to, string subject, string body)
    {
      try
      {
        MailMessage mail = new MailMessage();
        mail.From = new MailAddress(_conf["ApplicationSettings:EmailUserName"]);
        mail.To.Add(new MailAddress(to));
        mail.Subject = subject;
        mail.Body = body;
        mail.IsBodyHtml = true;
        SmtpClient smtp = new SmtpClient(_conf["ApplicationSettings:EmailHost"])
        {
          Port = int.Parse(_conf["ApplicationSettings:EmailPort"]),
          Credentials = new System.Net.NetworkCredential(_conf["ApplicationSettings:EmailUserName"], _conf["ApplicationSettings:EmailPassword"]),
          EnableSsl = Convert.ToBoolean(_conf["ApplicationSettings:EmailEnableSsl"]),
        };


        smtp.Send(mail);
        mail.Attachments.Clear();
        mail.Attachments.Dispose();
        mail.Dispose();

        return true;
      }
      catch (Exception ex)
      {
        return false;
        throw;
      }
    }
  }
}
