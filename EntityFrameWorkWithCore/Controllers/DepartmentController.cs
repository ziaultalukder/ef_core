using EntityFrameWorkWithCore.Context;
using EntityFrameWorkWithCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Security.Policy;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace EntityFrameWorkWithCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DepartmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _distributedCache;

        public DepartmentController(ApplicationDbContext context, IDistributedCache distributedCache)
        {
            _context = context;
            _distributedCache = distributedCache;
        }

        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> AddDepartment(Department department)
        {


            _context.Departments.Add(department);
            if (_context.SaveChanges() > 0)
            {
                return Ok("Save Success");
            }
            else
            {
                return Ok("Save Failed");
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetDepartment(CancellationToken cancellationToken = default)
        {
            string email = "ziaulodesk@primotechltd.com";

            var ddd1 = email.Substring(email.LastIndexOf('@'));
            var ddd2 = ddd1.Length;
            var ddd3 = email.Length;
            var ddd4 = ddd3 - ddd2;

            var ddd = email.Substring(0, ddd4);


            var department = await _distributedCache.GetStringAsync(Constant.Department);
            if (department is not null)
            {
                var cachedDataString = department;
                var productList = JsonConvert.DeserializeObject<List<Department>>(department);

                return Ok(productList);
            }
            else
            {
                var data = _context.Departments.ToList();
                string serializedProductsLists = JsonConvert.SerializeObject(data);
                var options = new DistributedCacheEntryOptions
                {

                    // Remove item from cache after duration
                    AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(60),
                    //AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)

                    // Remove item from cache if unsued for the duration
                    SlidingExpiration = TimeSpan.FromSeconds(30)
                };
                await _distributedCache.SetStringAsync(Constant.Department, serializedProductsLists, options, cancellationToken);

                return Ok(data);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddDepartmentRedis(Department department)
        {
            var departments = await _distributedCache.GetAsync(Constant.AddDepartment);
            if (departments is not null)
            {
                var cachedDataString = Encoding.UTF8.GetString(departments);
                var productList = JsonConvert.DeserializeObject<List<Department>>(cachedDataString);
                var ddd = productList.Exists(c => c.Name == department.Name);

                if (ddd)
                {
                    return Ok(department.Name + " already exist");
                }
                else
                {
                    productList.Add(department);
                    string serializedProductsLists = JsonConvert.SerializeObject(productList);
                    await _distributedCache.SetAsync(Constant.AddDepartment, Encoding.UTF8.GetBytes(serializedProductsLists));
                }
                return Ok(productList);
            }
            else
            {
                var data = _context.Departments.ToList();
                string serializedProductsLists = JsonConvert.SerializeObject(data);
                await _distributedCache.SetAsync(Constant.AddDepartment, Encoding.UTF8.GetBytes(serializedProductsLists));
                return Ok(data);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddListOfDepartmentOnRedis(string token)
        {

            var departments = await _distributedCache.GetAsync(Constant.DepartmentList);
            if (departments is not null)
            {
                var cachedDataString = Encoding.UTF8.GetString(departments);
                var productList = JsonConvert.DeserializeObject<List<DepartmentListVM>>(cachedDataString);
                var ddd = productList.Exists(c => c.Token == token);

                if (ddd)
                {
                    return Ok(token + " already exist");
                }
                else
                {
                    var dad = new DepartmentListVM
                    {
                        Token = token,
                        ExpireTime = 5
                    };
                    productList.Add(dad);

                    string serializedProductsLists = JsonConvert.SerializeObject(productList);
                    await _distributedCache.SetAsync(Constant.DepartmentList, Encoding.UTF8.GetBytes(serializedProductsLists));
                }
                return Ok(productList);
            }
            else
            {
                List<DepartmentListVM> list = new List<DepartmentListVM>();
                var dad = new DepartmentListVM
                {
                    Token = token,
                    ExpireTime = 5
                };
                list.Add(dad);
                string serializedProductsLists = JsonConvert.SerializeObject(list);
                await _distributedCache.SetAsync(Constant.DepartmentList, Encoding.UTF8.GetBytes(serializedProductsLists));
                return Ok(list);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ReturnStatus()
        {
            return StatusCode(StatusCodes.Status400BadRequest, Result.Failure(new List<string> { "Invalid Amount "}) );
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> ForgotPassword(string email)
        {

            var Token = "awaituserManagerGeneratePasswordResetTokenAsync";
            var lnkHref = "http://172.40.0.120/notification_portal";
            string subject = "Your changed password";
            string body = "<b>Please find the Password Reset Link. </b><br/>" + lnkHref;
            SendEmail("mohammadziaulm62@gmail.com", "Reset Password", body, null);
            return Ok(email);
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
