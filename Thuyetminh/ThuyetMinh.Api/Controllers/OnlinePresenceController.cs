using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Api.Services;

namespace ThuyetMinh.Api.Controllers;

[ApiController]
public sealed class OnlinePresenceController : ControllerBase
{
    // Da dang nhap cookie moi duoc tinh. API nay nam ngoai /api de dung cookie scheme hien co.
    [HttpPost("/presence/ping")]
    [Authorize(AuthenticationSchemes = "Cookies")]
    public IActionResult Ping()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
            username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(username)) username = "Tai khoan";

        var sessionId = Request.Cookies["tm_online_session"];
        if (string.IsNullOrWhiteSpace(sessionId) || sessionId.Length != 32 || !sessionId.All(Uri.IsHexDigit))
        {
            sessionId = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
            Response.Cookies.Append("tm_online_session", sessionId, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Path = "/",
                MaxAge = TimeSpan.FromHours(12)
            });
        }
        OnlinePresenceTracker.Touch(sessionId, username);
        Response.Headers.CacheControl = "no-store";
        return Ok(new { online = true });
    }

    [HttpGet("/presence/online")]
    [Authorize(AuthenticationSchemes = "Cookies", Roles = "Admin")]
    public IActionResult Online()
    {
        var users = OnlinePresenceTracker.GetOnline();
        Response.Headers.CacheControl = "no-store";
        return Ok(new
        {
            onlineSessions = users.Length,
            distinctAccounts = users.Select(x => x.Username)
                .Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            users = users.Select(x => new { username = x.Username, lastSeenUtc = x.LastSeenUtc }).ToArray()
        });
    }

    [HttpGet("/Admin/OnlineUsers")]
    [Authorize(AuthenticationSchemes = "Cookies", Roles = "Admin")]
    public IActionResult Dashboard()
    {
        Response.Headers.CacheControl = "no-store";
        return Content("""
<!doctype html>
<html lang="vi"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Nguoi dung dang online</title>
<style>body{font:16px system-ui,sans-serif;max-width:900px;margin:42px auto;padding:0 18px;color:#263b4c;background:#f4f8f8}a{color:#087d73}.panel{background:white;border:1px solid #d9e6e4;border-radius:16px;padding:26px;box-shadow:0 8px 26px #00000009}.num{font-size:54px;font-weight:800;color:#00877c;margin:0}table{border-collapse:collapse;width:100%;margin-top:22px}td,th{padding:12px;text-align:left;border-bottom:1px solid #e8eeee}small{color:#5a6d72}</style>
</head><body><div class="panel"><p><a href="/Admin/ReviewList">â† Quay lai trang Admin</a></p>
<h1>So nguoi dang su dung he thong</h1><p class="num" id="count">...</p>
<p><b>Phien dang nhap online</b> Â· <span id="accounts">...</span> tai khoan khac nhau</p>
<small>Cap nhat moi 5 giay. Trinh duyet dang nhap gui tin hieu moi 15 giay; phien mat ket noi duoc bo sau 45 giay.</small>
<table><thead><tr><th>Tai khoan</th><th>Hoat dong cach day</th></tr></thead><tbody id="people"></tbody></table>
<p id="error" role="status" style="color:#ba3737"></p></div>
<script>
async function ping(){try{await fetch('/presence/ping',{method:'POST',credentials:'same-origin',cache:'no-store'});}catch{}}
async function refreshOnline(){
 try{
  const r=await fetch('/presence/online',{credentials:'same-origin',cache:'no-store'});
  if(!r.ok) throw new Error('HTTP '+r.status);
  const data=await r.json();
  document.getElementById('count').textContent=data.onlineSessions;
  document.getElementById('accounts').textContent=data.distinctAccounts;
  const tbody=document.getElementById('people');tbody.replaceChildren();
  for(const user of data.users){
   const tr=document.createElement('tr');const a=document.createElement('td');const b=document.createElement('td');
   a.textContent=user.username;b.textContent=Math.max(0,Math.floor((Date.now()-Date.parse(user.lastSeenUtc))/1000))+' giay';
   tr.append(a,b);tbody.append(tr);
  }
  document.getElementById('error').textContent='';
 }catch(e){document.getElementById('error').textContent='Khong tai duoc danh sach: '+e.message;}
}
ping().then(refreshOnline);setInterval(ping,15000);setInterval(refreshOnline,5000);
</script></body></html>
""", "text/html; charset=utf-8");
    }
}