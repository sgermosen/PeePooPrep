// Moderation console. The token lives only in memory: closing the tab signs you out.
(function () {
  "use strict";
  var token = null;
  var $ = function (sel) { return document.querySelector(sel); };
  var loginForm = $("[data-admin-login]"), main = $("[data-admin-main]");
  var loginMsg = $("[data-login-msg]"), msg = $("[data-admin-msg]");

  function show(node, text, kind) { node.textContent = text; node.className = "notice notice--" + (kind || "ok"); node.hidden = false; }
  function el(tag, cls, text) { var n = document.createElement(tag); if (cls) n.className = cls; if (text != null) n.textContent = text; return n; }

  function api(path, opts) {
    opts = opts || {};
    opts.headers = Object.assign({ Accept: "application/json" }, opts.headers || {});
    if (token) opts.headers.Authorization = "Bearer " + token;
    return fetch(path, opts).then(function (res) {
      if (res.status === 401 || res.status === 403) { logout(); throw new Error("auth"); }
      return res;
    });
  }

  loginForm.addEventListener("submit", function (e) {
    e.preventDefault();
    loginMsg.hidden = true;
    var button = loginForm.querySelector("button");
    button.disabled = true;
    fetch("/api/account/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email: $("#admin-email").value.trim(), password: $("#admin-password").value })
    }).then(function (res) {
      return res.json().catch(function () { return {}; }).then(function (body) { return { ok: res.ok, body: body }; });
    }).then(function (r) {
      if (!r.ok) { show(loginMsg, r.body.message || "No se pudo entrar.", "err"); return; }
      if (!r.body.isAdmin) { show(loginMsg, "Esta cuenta no es de administrador.", "err"); return; }
      token = r.body.token;
      $("#admin-password").value = "";
      $("[data-admin-who]").textContent = "Conectado como @" + r.body.username;
      loginForm.hidden = true;
      main.hidden = false;
      load();
    }).catch(function () { show(loginMsg, "No se pudo conectar con el servidor.", "err"); })
      .finally(function () { button.disabled = false; });
  });

  function logout() {
    token = null;
    main.hidden = true;
    loginForm.hidden = false;
  }
  $("[data-admin-logout]").addEventListener("click", logout);
  $("[data-admin-refresh]").addEventListener("click", function () { msg.hidden = true; load(); });

  function load() {
    api("/api/admin/stats").then(function (r) { return r.json(); }).then(renderStats).catch(function () {});
    api("/api/admin/reports").then(function (r) { return r.json(); }).then(renderReports)
      .catch(function (e) { if (e.message !== "auth") show(msg, "No se pudieron cargar los reportes.", "err"); });
  }

  function renderStats(s) {
    var box = $("[data-admin-stats]");
    box.textContent = "";
    [["Lugares", s.places], ["Ocultos", s.hiddenPlaces], ["Reseñas", s.reviews], ["Reseñas ocultas", s.hiddenReviews],
     ["Personas", s.users], ["Reportes abiertos", s.openReports], ["Lugares (7 días)", s.placesLast7Days], ["Reseñas (7 días)", s.reviewsLast7Days]]
      .forEach(function (pair) {
        var d = el("div");
        d.appendChild(el("b", null, String(pair[1])));
        d.appendChild(el("span", null, pair[0]));
        box.appendChild(d);
      });
  }

  function renderReports(reports) {
    var list = $("[data-admin-reports]");
    list.textContent = "";
    if (!reports.length) { list.appendChild(el("p", "muted", "No hay reportes abiertos.")); return; }

    // One card per reported item, newest report first.
    var seen = {};
    reports.forEach(function (r) {
      if (seen[r.targetId]) { seen[r.targetId].reasons.push(r); return; }
      seen[r.targetId] = { first: r, reasons: [r] };
    });

    Object.keys(seen).forEach(function (id) {
      var group = seen[id], r = group.first;
      var card = el("article", "report");
      var head = el("div", "report-head");
      head.appendChild(el("span", "tag", r.targetType === "Visit" ? "Reseña" : "Lugar"));
      if (r.targetHidden) head.appendChild(el("span", "tag tag--stop", "Oculto"));
      head.appendChild(el("span", null, r.openReportsForTarget + (r.openReportsForTarget === 1 ? " reporte" : " reportes")));
      if (r.targetAuthor) head.appendChild(el("span", null, "Autor: @" + r.targetAuthor));
      card.appendChild(head);

      card.appendChild(el("h3", null, r.targetTitle || "(contenido eliminado)"));
      if (r.targetText) card.appendChild(el("blockquote", null, r.targetText));
      var reasons = el("ul", "small");
      group.reasons.forEach(function (x) {
        reasons.appendChild(el("li", null, "«" + x.reason + "» — @" + (x.reporterUsername || "?") + ", " + new Date(x.createdAt).toLocaleString()));
      });
      card.appendChild(reasons);

      var actions = el("div", "actions");
      if (r.targetType === "Place" && r.targetTitle) {
        var view = el("a", "btn btn--ghost btn--small", "Ver ficha");
        view.href = "/lugar/" + encodeURIComponent(r.targetId);
        view.target = "_blank";
        view.rel = "noopener";
        actions.appendChild(view);
      }
      actions.appendChild(button("Eliminar contenido", "", function () {
        var path = (r.targetType === "Visit" ? "/api/admin/visits/" : "/api/admin/places/") + encodeURIComponent(r.targetId);
        return api(path, { method: "DELETE" }).then(function (res) {
          if (!res.ok && res.status !== 404) throw new Error();
          return resolve(r.id, false);
        }).then(function () { show(msg, "Contenido eliminado y reportes cerrados."); });
      }));
      actions.appendChild(button(r.targetHidden ? "Restaurar y cerrar" : "Descartar reportes", "btn--ghost", function () {
        return resolve(r.id, true).then(function () { show(msg, r.targetHidden ? "Contenido visible otra vez." : "Reportes descartados."); });
      }));
      if (r.targetAuthor) {
        actions.appendChild(button("Suspender a @" + r.targetAuthor, "btn--ghost", function () {
          if (!confirm("¿Suspender a @" + r.targetAuthor + "? Se cierran sus sesiones y se ocultan sus reseñas.")) return Promise.resolve();
          return api("/api/admin/users/" + encodeURIComponent(r.targetAuthor) + "/ban", { method: "POST" }).then(function (res) {
            return res.json().catch(function () { return {}; }).then(function (body) {
              if (!res.ok) throw new Error(body.message || "");
              show(msg, "@" + r.targetAuthor + " fue suspendido.");
            });
          });
        }));
      }
      card.appendChild(actions);
      list.appendChild(card);
    });
  }

  function button(label, extra, action) {
    var b = el("button", "btn btn--small " + extra, label);
    b.type = "button";
    b.addEventListener("click", function () {
      b.disabled = true;
      Promise.resolve(action()).then(load).catch(function (e) {
        if (e && e.message !== "auth") show(msg, e.message || "La acción falló.", "err");
        b.disabled = false;
      });
    });
    return b;
  }

  function resolve(id, restore) {
    return api("/api/admin/reports/" + encodeURIComponent(id) + "/resolve" + (restore ? "?restore=true" : ""), { method: "POST" })
      .then(function (res) { if (!res.ok) throw new Error("No se pudo cerrar el reporte."); });
  }
})();
