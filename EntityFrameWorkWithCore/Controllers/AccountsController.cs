using EntityFrameWorkWithCore.Context;
using EntityFrameWorkWithCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading;
using static System.Net.WebRequestMethods;

namespace EntityFrameWorkWithCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext context;
        private readonly IDistributedCache distributedCache;

        public AccountsController(IConfiguration configuration, ApplicationDbContext _context, IDistributedCache _distributedCache)
        {
            _configuration = configuration;
            context = _context;
            distributedCache = _distributedCache;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Login(Login login)
        {
            var tokenExpireTime = DateTime.UtcNow.AddMinutes(1);
            var tokenSecurity = Encoding.ASCII.GetBytes(JwtTokenHandler.SecurityKey);
            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Name, login.UserName)
            });

            var signingCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(tokenSecurity),
                        SecurityAlgorithms.HmacSha256Signature);

            var description = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = tokenExpireTime,
                SigningCredentials = signingCredentials
            };

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtSecurityTokenHandler.CreateToken(description);
            var token = jwtSecurityTokenHandler.WriteToken(securityToken);

            var obj = new
            {
                name = login.UserName,
                token = token
            };
            return Ok(obj);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SendToken(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(email)) return StatusCode(StatusCodes.Status400BadRequest);
            string key = email;
            
            var department = await distributedCache.GetStringAsync(key);
            if (department is not null)
            {
                var cachedDataString = department;
                
                return StatusCode(StatusCodes.Status200OK, cachedDataString);
            }
            else
            {
                Random random = new Random();
                int otp = random.Next(100000);

                string serializedProductsLists = JsonConvert.SerializeObject(otp);
                var options = new DistributedCacheEntryOptions
                {
                    // Remove item from cache after duration
                    AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(60),
                    //AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)

                    // Remove item from cache if unsued for the duration
                    //SlidingExpiration = TimeSpan.FromSeconds(30)
                };
                await distributedCache.SetStringAsync(key, serializedProductsLists, options, cancellationToken);
                SendEmail(email, "OTP", "Your OTP: " + otp, null);

                return StatusCode(StatusCodes.Status200OK, "Please Check Your Email");
            }
        }

        //[HttpPost("[action]")]
        //public async Task<IActionResult> ValidateToken(string otp, string email)
        //{
        //    var existingToken = await distributedCache.GetStringAsync(email);
        //    if (existingToken == otp)
        //    {
        //        var cachedDataString = existingToken;
        //        var tokenGenerate = TokenGenerate(email);

        //        return StatusCode(StatusCodes.Status200OK, Result.Success(""));
        //    }
        //    else
        //    {
        //        return StatusCode(StatusCodes.Status400BadRequest, Result.Failure(new List<string> { " Token Not Matcg "}));
        //    }
        //}

        public static string TokenGenerate(string email)
        {
            var tokenExpireTime = DateTime.UtcNow.AddSeconds(50);
            var tokenSecurity = Encoding.ASCII.GetBytes(JwtTokenHandler.SecurityKey);
            
            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Email, email)
            });

            var signingCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(tokenSecurity),
                        SecurityAlgorithms.HmacSha256Signature);

            var description = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = tokenExpireTime,
                SigningCredentials = signingCredentials
            };

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtSecurityTokenHandler.CreateToken(description);
            var token = jwtSecurityTokenHandler.WriteToken(securityToken);

            if (string.IsNullOrEmpty(token))
            {
                var lnkHref = "http://localhost:5235/Home/ResetPassword?token=" + token;
                string subject = "<h1>Reset Your Password</h1>";
                string body = "<b>Please find the Password Reset Link and link available is for 3 minutes </b><br/>" + lnkHref;
                SendEmail(email, "OTP", subject + body, null);
            }
            else
            {
                return "Something wrong generate token";
            }
            return token;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ForgotPassword(User user)
        {
            var tokenExpireTime = DateTime.UtcNow.AddMinutes(2);
            var tokenSecurity = Encoding.ASCII.GetBytes(JwtTokenHandler.SecurityKey);
            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email)
            });

            var signingCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(tokenSecurity),
                        SecurityAlgorithms.HmacSha256Signature);

            var description = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = tokenExpireTime,
                SigningCredentials = signingCredentials
            };

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtSecurityTokenHandler.CreateToken(description);
            var token = jwtSecurityTokenHandler.WriteToken(securityToken);

            var lnkHref = "http://localhost:5235/Home/ResetPassword?token=" + token;
            string subject = "<h1>Reset Your Password</h1>";
            string body = "<b>Please find the Password Reset Link and link available is for 3 minutes </b><br/>" + lnkHref;
            //SendEmail("mohammadziaulm62@gmail.com", "Reset Password", body, null);
            return Ok(token);

        }

        [HttpPost("[action]")]
        public async Task<IActionResult> TokenGenerate2(string email)
        {
            var tokenExpireTime = DateTime.UtcNow.AddSeconds(50);
            var tokenSecurity = Encoding.ASCII.GetBytes(JwtTokenHandler.SecurityKey);
            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Email, email)
            });

            var signingCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(tokenSecurity),
                        SecurityAlgorithms.HmacSha256Signature);

            var description = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = tokenExpireTime,
                SigningCredentials = signingCredentials
            };

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtSecurityTokenHandler.CreateToken(description);
            var token = jwtSecurityTokenHandler.WriteToken(securityToken);

            return Ok(token);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ValidateToken1(string token)
        {
            try
            {

            }
            catch (Exception exception)
            {
                var sss = new
                {
                    title = "something went wrong",
                    success = false,
                    errorMessage = exception.Message,
                    statusCode = StatusCodes.Status500InternalServerError,
                };
                return Ok(sss);
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtTokenHandler.SecurityKey)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,
            }, out SecurityToken validatedToken);

            if (tokenHandler.CanValidateToken)
            {
                return StatusCode(StatusCodes.Status200OK, Result.Success("success"));
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, Result.Failure(new List<string> { "something wrong"}));
            }
            

        }
        
        [HttpPost("[action]")]
        public async Task<IActionResult> ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtTokenHandler.SecurityKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero,
                }, out SecurityToken validatedToken);

                if (tokenHandler.CanValidateToken)
                {
                    return StatusCode(StatusCodes.Status200OK, Result.Success("success"));
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, Result.Failure(new List<string> { "something wrong" }));
                }
            }
            catch (Exception exception)
            {
                var sss = new
                {
                    title = "something went wrong",
                    success = false,
                    errorMessage = exception.Message,
                    statusCode = StatusCodes.Status500InternalServerError,
                };
                return Ok(sss);
            }
        }

        public static bool SendEmail(string toEmail, string subject, string body, byte[] invoiceData)
        {
            try
            {
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                var credential = new NetworkCredential()
                {
                    UserName = "ziault626@gmail.com",
                    Password = "nijkfgzglfvexebs"
                };
                client.Credentials = credential;

                MailMessage mailMessage = new MailMessage(toEmail, toEmail, subject, body);
                mailMessage.IsBodyHtml = true;

                if (invoiceData != null)
                {
                    Guid guid = new Guid();
                    byte[] applicationPdfData = invoiceData;
                    Attachment attPdf = new Attachment(new MemoryStream(applicationPdfData), guid + ".pdf");
                    mailMessage.Attachments.Add(attPdf);
                }
                mailMessage.BodyEncoding = Encoding.UTF8;
                client.Send(mailMessage);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
