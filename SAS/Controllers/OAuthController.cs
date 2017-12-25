using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using SAS.Models;
using System.Security.Cryptography;
using System.Text;

namespace SAS.Controllers
{
    [RoutePrefix("oauth")]
    public class OAuthController : ApiController
    {
        private SASContext db = new SASContext();

        private static string HashPassword(string plainMessage)
        {
            byte[] data = Encoding.UTF8.GetBytes(plainMessage);
            using (HashAlgorithm sha = new SHA256Managed())
            {
                byte[] encryptedBytes = sha.TransformFinalBlock(data, 0, data.Length);
                return Convert.ToBase64String(sha.Hash);
            }
        }

        [Route("getclient")]
        public async Task<IHttpActionResult> GetClientinfo(int clientId, string redirect)
        {
            Owners TMP = new Owners();
            try
            {
                TMP = await db.Owners.FindAsync(clientId);
            }
            catch
            {
                return InternalServerError();
            }

            if (TMP == null)
                return BadRequest();

            if (TMP.RedirectUri != redirect)
            {
                return Unauthorized();
            }

            return Ok<string>(TMP.Name);
        }

        [Route("checkuser")]
        public async Task<IHttpActionResult> PostCheck([FromBody] AuthModel AAAA)
        {
            User TMP = new User();
            try
            {
                TMP = await db.Users.FirstOrDefaultAsync(c => c.UserName == AAAA.Username);
            }
            catch
            {
                return Unauthorized();
            }

            if (TMP == null)
                return Unauthorized();

            return Ok<string>(AAAA.Username);
        }

        [Route("login")]
        public async Task<IHttpActionResult> PostLogIn([FromBody] authmoels AAAA)
        {
            User TMP = new User();
            try
            {
                TMP = await db.Users.FirstOrDefaultAsync(c => c.UserName == AAAA.name);
            }
            catch
            {
                return Unauthorized();
            }

            if (TMP == null)
                return Unauthorized();

            if (TMP.UserPass != HashPassword(AAAA.pass))
                return Unauthorized();

            string src1 = DateTime.Now.ToString() + "|" + TMP.UserName + "|" + TMP.UserRole;
            string src2 = DateTime.Now.ToString() + "|" + TMP.UserName + "|" + TMP.UserRole;
            string src3 = DateTime.Now.ToString() + "|" + TMP.UserName + "|" + TMP.UserRole;

            string Codestr = HashPassword(src1);
            string Tokenstr = HashPassword(src2);
            string Refreshstr = HashPassword(src3);

            Code tmp_code = new Code();
            Token tmp_token = new Token();

            tmp_code.AccessCode = Codestr;
            tmp_code.Timeofrelease = DateTime.Now.AddMinutes(10);
            tmp_code.ownerId = AAAA.clientid;

            db.Codes.Add(tmp_code);
            await db.SaveChangesAsync();

            Code ttt = await db.Codes.FirstOrDefaultAsync(c => c.AccessCode == tmp_code.AccessCode);

            tmp_token.AccessToken = Tokenstr;
            tmp_token.RefreshToken = Refreshstr;
            tmp_token.TimeofreleaseAccessToken = DateTime.Now.AddMilliseconds(1);
            tmp_token.TimeofreleaseRefreshToken = DateTime.Now.AddMinutes(100);
            tmp_token.UserId = TMP.Id;
            tmp_token.AccessCodeId = ttt.Id;
            db.Tokens.Add(tmp_token);
            await db.SaveChangesAsync();
                        
            return Ok(ttt.AccessCode);
        }

        [Route("gettokens")]
        public async Task<IHttpActionResult> PostAccessToken([FromBody] authcodemoels AAAA)
        {
            // берешь код
            Code tmp = new Code();
            try
            {
                tmp = await db.Codes.FirstOrDefaultAsync(x => x.AccessCode == AAAA.code);
            }
            catch
            {
                return Unauthorized();
            }

            if (tmp == null)
                return Unauthorized();

            //проверить что еще не протух
            if (tmp.Timeofrelease <= DateTime.Now )
            {
                return Unauthorized();
            }

            //ебошишь за овнером
            Owners own = new Owners();
            try
            {
                own = await db.Owners.FirstOrDefaultAsync(x => x.RedirectUri == AAAA.redirect_uri);
            }
            catch
            {
                return Unauthorized();
            }


            // если это он то кидаем токен 
            Token tmp2 = new Token();

            try
            {
                tmp2 = await db.Tokens.FirstOrDefaultAsync(x => x.AccessCodeId == tmp.Id);
            }
            catch
            {
                return Unauthorized();
            }

            if (tmp2 == null)
                return Unauthorized();

            tokenmessage aaa = new tokenmessage();

            aaa.access_token = tmp2.AccessToken;
            aaa.refresh_token = tmp2.RefreshToken;
            aaa.token_type = "Bearer";

            return Ok<tokenmessage>(aaa);            
        }

        [Route("refresh")]
        public async Task<IHttpActionResult> PostRefresh([FromBody] MyModel key)
        {
            Token tmp = new Token();

            try
            {
                tmp = await db.Tokens.FirstOrDefaultAsync(x => x.RefreshToken == key.refreshtoken);
            }
            catch
            {
                return Unauthorized();
            }

            if (tmp == null)
                return Unauthorized();

            if (tmp.TimeofreleaseRefreshToken <= DateTime.Now)
            {
                return Unauthorized();
            }
            tmp.TimeofreleaseAccessToken = tmp.TimeofreleaseAccessToken.Value.AddYears(-1);
            tmp.TimeofreleaseRefreshToken = tmp.TimeofreleaseRefreshToken.Value.AddYears(-1);

            db.Entry(tmp).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                return InternalServerError();
            }
            
            User TMP = new User();
            try
            {
                TMP = await db.Users.FindAsync(tmp.UserId);
            }
            catch
            {
                return Unauthorized();
            }

            if (TMP == null)
                return Unauthorized();

            string src2 = DateTime.Now.ToString() + "|" + TMP.UserName + "|" + TMP.UserRole;
            string src3 = DateTime.Now.ToString() + "|" + TMP.UserName + "|" + TMP.UserRole;

            string Tokenstr = HashPassword(src2);
            string Refreshstr = HashPassword(src3);

            Token tmp_token = new Token();

            tmp_token.AccessToken = Tokenstr;
            tmp_token.RefreshToken = Refreshstr;
            tmp_token.TimeofreleaseAccessToken = DateTime.Now.AddMinutes(5);
            tmp_token.TimeofreleaseRefreshToken = DateTime.Now.AddMinutes(10);
            tmp_token.UserId = TMP.Id;
            tmp_token.AccessCodeId = 2;
            db.Tokens.Add(tmp_token);
            //await db.SaveChangesAsync();
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            tokenmessage aaa = new tokenmessage();

            aaa.access_token = tmp_token.AccessToken;
            aaa.refresh_token = tmp_token.RefreshToken;
            aaa.token_type = "Bearer";

            return Ok<tokenmessage>(aaa);  
        }


        [Route("check")]
        public async Task<IHttpActionResult> PostCheck([FromBody] authcheckmoels AAAA)
        {
            Token tmp = new Token();

            try
            {
                tmp = await db.Tokens.FirstOrDefaultAsync(x => x.AccessToken == AAAA.token);
            }
            catch
            {
                return Unauthorized();
            }

            if (tmp == null)
                return Unauthorized();

            if (tmp.TimeofreleaseAccessToken <= DateTime.Now)
            {
                return Unauthorized();
            }

            tmp.TimeofreleaseAccessToken = DateTime.Now.AddMinutes(30);
            tmp.TimeofreleaseRefreshToken = DateTime.Now.AddMinutes(30);
            db.Entry(tmp).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }


            User TMP = new User();
            try
            {
                TMP = await db.Users.FindAsync(tmp.UserId);
            }
            catch
            {
                return Unauthorized();
            }

            if (TMP == null)
                return Unauthorized();

            if (TMP.UserRole != AAAA.requedrole)
            {
                return Content(HttpStatusCode.Unauthorized, "Need ADMIN access.");
                //return Unauthorized();
            }

            return Ok();
        }
   
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TokenExists(int id)
        {
            return db.Tokens.Count(e => e.Id == id) > 0;
        }
    }
}