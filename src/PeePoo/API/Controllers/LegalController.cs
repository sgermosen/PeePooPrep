using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

namespace API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    public class LegalController : ControllerBase
    {
        private readonly string _appName;
        private readonly string _supportEmail;
        private readonly string _updated;

        public LegalController(IConfiguration config)
        {
            _appName = config["Legal:AppName"] ?? "PeePoo Finder";
            _supportEmail = config["Legal:SupportEmail"] ?? "support@peepoo.app";
            _updated = config["Legal:LastUpdated"] ?? "2026";
        }

        [HttpGet("/privacy")]
        public ContentResult Privacy()
        {
            var body = $@"
<h1>Privacy Policy</h1>
<p class='muted'>Last updated: {_updated}</p>
<p>{_appName} (""we"") helps people find and share public restrooms. This policy explains what we collect and how we use it.</p>

<h2>Data we collect</h2>
<ul>
  <li><b>Account</b> — email, username and display name.</li>
  <li><b>Location</b> — your approximate location, only while using the app, to show nearby restrooms and to place a spot you add.</li>
  <li><b>Content you create</b> — places, reviews and photos you choose to submit.</li>
</ul>

<h2>How we use it</h2>
<p>To operate the app: authenticate you, show nearby spots, and display the content you and others contribute. We do not sell your data.</p>

<h2>Sharing</h2>
<p>Content you publish (places, reviews, photos and your display name) is visible to other users. We do not share your email or precise location with other users.</p>

<h2>Your choices</h2>
<p>You can edit your profile and <b>delete your account</b> at any time from the app (Profile → Delete account), or by contacting us. Deleting your account removes your personal data. See <a href='/account-deletion'>Account &amp; data deletion</a>.</p>

<h2>Contact</h2>
<p><a href='mailto:{_supportEmail}'>{_supportEmail}</a></p>";
            return Page("Privacy Policy", body);
        }

        [HttpGet("/account-deletion")]
        public ContentResult AccountDeletion()
        {
            var body = $@"
<h1>Account &amp; data deletion</h1>
<p>You can delete your {_appName} account and personal data at any time.</p>

<h2>Delete it in the app</h2>
<ol>
  <li>Open {_appName} and go to <b>Profile</b>.</li>
  <li>Tap <b>Delete account</b> and confirm.</li>
</ol>
<p>This permanently removes your account, your reviews and your photos.</p>

<h2>Request it by email</h2>
<p>If you can't access the app, email <a href='mailto:{_supportEmail}'>{_supportEmail}</a> from your account email address and ask us to delete your account. We will process the request and confirm by email.</p>

<h2>What is deleted</h2>
<ul>
  <li>Your account (email, username, display name).</li>
  <li>Your reviews and the photos you uploaded.</li>
  <li>Your saved favorites.</li>
</ul>
<p class='muted'>Places you added remain available to the community as anonymous entries. Aggregated, non-identifying data may be retained.</p>

<h2>Contact</h2>
<p><a href='mailto:{_supportEmail}'>{_supportEmail}</a></p>";
            return Page("Account & data deletion", body);
        }

        private ContentResult Page(string title, string body)
        {
            var html = $@"<!doctype html>
<html lang='en'><head>
<meta charset='utf-8'>
<meta name='viewport' content='width=device-width, initial-scale=1'>
<title>{title} · {_appName}</title>
<style>
  :root {{ color-scheme: light dark; }}
  body {{ font-family: -apple-system, Segoe UI, Roboto, Helvetica, Arial, sans-serif; line-height:1.6;
         max-width: 720px; margin: 0 auto; padding: 40px 22px; color:#2A1712; background:#FBF7F4; }}
  @media (prefers-color-scheme: dark) {{ body {{ color:#F4ECE6; background:#17110D; }} a {{ color:#E7B62F; }} }}
  h1 {{ font-size: 28px; }}
  h2 {{ font-size: 18px; margin-top: 28px; }}
  a {{ color:#BF6952; }}
  .muted {{ color:#8C7F78; font-size: 14px; }}
  .brand {{ font-weight:700; color:#BF6952; letter-spacing:.02em; }}
</style></head>
<body>
<p class='brand'>{_appName}</p>
{body}
</body></html>";
            return new ContentResult { Content = html, ContentType = "text/html; charset=utf-8" };
        }
    }
}
