using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    public class AdminConsoleController : ControllerBase
    {
        [HttpGet("/admin")]
        public ContentResult Console()
        {
            return new ContentResult { Content = Html, ContentType = "text/html; charset=utf-8" };
        }

        private const string Html = """
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Moderation · PeePoo Finder</title>
<style>
  :root{ color-scheme: light dark; --t:#BF6952; --tx:#2A1712; --mu:#8C7F78; --ln:#EADFD8; --bg:#FBF7F4; --card:#fff; --bad:#C6483B; --good:#2F8F63; }
  @media (prefers-color-scheme: dark){ :root{ --tx:#F4ECE6; --mu:#B4A79F; --ln:#3A2B23; --bg:#17110D; --card:#241A15; } }
  *{box-sizing:border-box}
  body{margin:0;font-family:-apple-system,Segoe UI,Roboto,Helvetica,Arial,sans-serif;background:var(--bg);color:var(--tx);line-height:1.5}
  header{background:var(--t);color:#fff;padding:14px 20px;display:flex;align-items:center;gap:10px}
  header b{font-size:16px}
  header .sp{margin-left:auto;font-size:13px;opacity:.9}
  header button{margin-left:12px}
  .wrap{max-width:900px;margin:0 auto;padding:22px 18px}
  .card{background:var(--card);border:1px solid var(--ln);border-radius:14px;padding:18px;box-shadow:0 6px 20px rgba(42,23,18,.05)}
  h1{font-size:20px;margin:0 0 14px}
  label{display:block;font-size:13px;color:var(--mu);margin:12px 0 4px}
  input{width:100%;padding:11px 13px;border:1px solid var(--ln);border-radius:10px;background:transparent;color:var(--tx);font-size:15px}
  button{cursor:pointer;border:none;border-radius:10px;padding:9px 14px;font-weight:700;font-size:14px;background:var(--t);color:#fff}
  button.ghost{background:transparent;color:var(--t);border:1px solid var(--t)}
  button.danger{background:var(--bad)}
  button:disabled{opacity:.5;cursor:default}
  .msg{margin:12px 0;padding:10px 12px;border-radius:10px;font-size:14px;display:none}
  .msg.err{display:block;background:rgba(198,72,59,.12);color:var(--bad)}
  .msg.ok{display:block;background:rgba(47,143,99,.12);color:var(--good)}
  .row{border:1px solid var(--ln);border-radius:12px;padding:14px;margin:12px 0;background:var(--card)}
  .row .meta{display:flex;gap:8px;flex-wrap:wrap;align-items:center;font-size:12.5px;color:var(--mu);margin-bottom:6px}
  .tag{background:rgba(191,105,82,.14);color:var(--t);border-radius:999px;padding:2px 9px;font-weight:700}
  .reason{font-size:15px;margin:6px 0 12px;white-space:pre-wrap;word-break:break-word}
  .actions{display:flex;gap:8px;flex-wrap:wrap}
  .muted{color:var(--mu);font-size:14px}
  .empty{text-align:center;color:var(--mu);padding:30px}
  code{font-family:ui-monospace,Menlo,monospace;font-size:12px}
  .hidden{display:none}
</style>
</head>
<body>
<header>
  <b>PeePoo Finder</b><span>· Moderation</span>
  <span class="sp" id="who"></span>
  <button class="ghost hidden" id="refresh" style="background:rgba(255,255,255,.15);color:#fff;border:none">Refresh</button>
  <button class="ghost hidden" id="logout" style="background:rgba(255,255,255,.15);color:#fff;border:none">Sign out</button>
</header>

<div class="wrap">
  <div id="loginView" class="card" style="max-width:420px;margin:40px auto">
    <h1>Admin sign in</h1>
    <div class="msg" id="loginMsg"></div>
    <label>Email</label>
    <input id="email" type="email" autocomplete="username">
    <label>Password</label>
    <input id="password" type="password" autocomplete="current-password">
    <div style="margin-top:16px"><button id="loginBtn">Sign in</button></div>
    <p class="muted" style="margin-top:14px">Sign in with an account that has the Admin role.</p>
  </div>

  <div id="mainView" class="hidden">
    <h1>Open reports</h1>
    <div class="msg" id="mainMsg"></div>
    <div id="list"></div>
  </div>
</div>

<script>
"use strict";
var token = null;
var el = function(id){ return document.getElementById(id); };

function show(msgEl, kind, text){
  msgEl.className = "msg " + kind;
  msgEl.textContent = text;
}
function hideMsg(msgEl){ msgEl.className = "msg"; msgEl.textContent = ""; }

async function api(path, opts){
  opts = opts || {};
  opts.headers = opts.headers || {};
  if (token) opts.headers["Authorization"] = "Bearer " + token;
  var res = await fetch(path, opts);
  return res;
}

async function login(){
  hideMsg(el("loginMsg"));
  var email = el("email").value.trim();
  var password = el("password").value;
  if (!email || !password){ show(el("loginMsg"), "err", "Enter your email and password."); return; }
  el("loginBtn").disabled = true;
  try {
    var res = await fetch("/api/account/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email: email, password: password })
    });
    if (!res.ok){ show(el("loginMsg"), "err", "Sign in failed. Check your credentials."); return; }
    var data = await res.json();
    token = data.token;
    // verify admin by trying to load reports
    var check = await api("/api/admin/reports");
    if (check.status === 403){ token = null; show(el("loginMsg"), "err", "This account is not an admin."); return; }
    el("who").textContent = data.username ? ("@" + data.username) : "";
    el("loginView").classList.add("hidden");
    el("mainView").classList.remove("hidden");
    el("logout").classList.remove("hidden");
    el("refresh").classList.remove("hidden");
    var reports = await check.json();
    render(reports);
  } catch (e){
    show(el("loginMsg"), "err", "Could not reach the server.");
  } finally {
    el("loginBtn").disabled = false;
  }
}

function logout(){
  token = null;
  el("mainView").classList.add("hidden");
  el("logout").classList.add("hidden");
  el("refresh").classList.add("hidden");
  el("who").textContent = "";
  el("password").value = "";
  el("loginView").classList.remove("hidden");
}

async function loadReports(){
  hideMsg(el("mainMsg"));
  var res = await api("/api/admin/reports");
  if (res.status === 401 || res.status === 403){ logout(); return; }
  if (!res.ok){ show(el("mainMsg"), "err", "Could not load reports."); return; }
  render(await res.json());
}

function render(reports){
  var list = el("list");
  list.textContent = "";
  if (!reports || reports.length === 0){
    var empty = document.createElement("div");
    empty.className = "empty";
    empty.textContent = "No open reports. All clear.";
    list.appendChild(empty);
    return;
  }
  reports.forEach(function(r){
    var row = document.createElement("div");
    row.className = "row";

    var meta = document.createElement("div");
    meta.className = "meta";
    var tag = document.createElement("span");
    tag.className = "tag";
    tag.textContent = r.targetType || "?";
    meta.appendChild(tag);
    var by = document.createElement("span");
    by.textContent = "by @" + (r.reporterUsername || "unknown");
    meta.appendChild(by);
    var when = document.createElement("span");
    when.textContent = new Date(r.createdAt).toLocaleString();
    meta.appendChild(when);
    var idc = document.createElement("code");
    idc.textContent = r.targetId;
    meta.appendChild(idc);
    row.appendChild(meta);

    var reason = document.createElement("div");
    reason.className = "reason";
    reason.textContent = r.reason || "(no reason)";
    row.appendChild(reason);

    var actions = document.createElement("div");
    actions.className = "actions";

    var takedown = document.createElement("button");
    takedown.className = "danger";
    takedown.textContent = "Take down content";
    takedown.onclick = function(){ takeDown(r, takedown); };
    actions.appendChild(takedown);

    var dismiss = document.createElement("button");
    dismiss.className = "ghost";
    dismiss.textContent = "Dismiss";
    dismiss.onclick = function(){ dismiss.disabled = true; resolveReport(r.id); };
    actions.appendChild(dismiss);

    row.appendChild(actions);
    list.appendChild(row);
  });
}

async function takeDown(r, btn){
  btn.disabled = true;
  var path = r.targetType === "Visit"
    ? "/api/admin/visits/" + encodeURIComponent(r.targetId)
    : "/api/admin/places/" + encodeURIComponent(r.targetId);
  var res = await api(path, { method: "DELETE" });
  if (!res.ok && res.status !== 404){
    show(el("mainMsg"), "err", "Could not remove the content.");
    btn.disabled = false;
    return;
  }
  await resolveReport(r.id);
  show(el("mainMsg"), "ok", "Content removed and report resolved.");
}

async function resolveReport(id){
  var res = await api("/api/admin/reports/" + encodeURIComponent(id) + "/resolve", { method: "POST" });
  if (!res.ok){ show(el("mainMsg"), "err", "Could not resolve the report."); return; }
  await loadReports();
}

el("loginBtn").onclick = login;
el("logout").onclick = logout;
el("refresh").onclick = loadReports;
el("password").addEventListener("keydown", function(e){ if (e.key === "Enter") login(); });
</script>
</body>
</html>
""";
    }
}
