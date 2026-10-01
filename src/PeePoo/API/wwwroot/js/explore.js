// Explore page: Leaflet map for the server-rendered list, plus "near me" search via the public API.
// All user-provided text is inserted with textContent, never as HTML.
(function () {
  "use strict";

  var DEFAULT_CENTER = [18.4861, -69.9312]; // Santo Domingo
  var NEAR_RADIUS_KM = 10;

  var mapEl = document.getElementById("map");
  if (!mapEl || typeof L === "undefined") return;

  var form = document.querySelector("[data-explore-form]");
  var list = document.querySelector("[data-explore-list]");
  var status = document.querySelector("[data-explore-status]");
  var locateBtn = document.querySelector("[data-locate]");
  var initial = [];
  try { initial = JSON.parse(document.getElementById("places-data").textContent || "[]"); } catch (e) { initial = []; }

  var map = L.map(mapEl, { scrollWheelZoom: false }).setView(DEFAULT_CENTER, 12);
  L.tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
    maxZoom: 19,
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
  }).addTo(map);

  var markers = L.layerGroup().addTo(map);
  var meMarker = null;
  var userLocation = null;

  function pinIcon(place) {
    var label = place.rating != null ? Number(place.rating).toFixed(1) : "·";
    return L.divIcon({
      className: "pin" + (place.open === false ? " pin--closed" : ""),
      html: '<span class="pin-body"><span>' + label + "</span></span>", // numeric only
      iconSize: [34, 34],
      iconAnchor: [17, 41],
      popupAnchor: [0, -38]
    });
  }

  function popupFor(place) {
    var box = document.createElement("div");
    var name = document.createElement("strong");
    name.textContent = place.name;
    box.appendChild(name);
    box.appendChild(document.createElement("br"));
    var link = document.createElement("a");
    link.href = "/lugar/" + encodeURIComponent(place.id);
    link.textContent = "Ver ficha →";
    box.appendChild(link);
    return box;
  }

  function showOnMap(places, fit) {
    markers.clearLayers();
    var bounds = [];
    places.forEach(function (p) {
      if (typeof p.lat !== "number" || typeof p.lng !== "number") return;
      L.marker([p.lat, p.lng], { icon: pinIcon(p), title: p.name, alt: p.name })
        .bindPopup(popupFor(p))
        .addTo(markers);
      bounds.push([p.lat, p.lng]);
    });
    if (userLocation) bounds.push(userLocation);
    if (fit && bounds.length > 1) map.fitBounds(bounds, { padding: [40, 40], maxZoom: 15 });
    else if (fit && bounds.length === 1) map.setView(bounds[0], 15);
  }

  // ── Rendering the list from API results (mirrors _PlaceCard.cshtml) ──
  var typeNames = { Men: "Hombres", Women: "Mujeres", Family: "Familiar", Accessible: "Accesible" };

  function el(tag, cls, text) {
    var node = document.createElement(tag);
    if (cls) node.className = cls;
    if (text != null) node.textContent = text;
    return node;
  }

  function icon(id) {
    var ns = "http://www.w3.org/2000/svg";
    var svg = document.createElementNS(ns, "svg");
    svg.setAttribute("aria-hidden", "true");
    var use = document.createElementNS(ns, "use");
    use.setAttribute("href", "#" + id);
    svg.appendChild(use);
    return svg;
  }

  function stars(rating) {
    var full = Math.round(Math.max(0, Math.min(5, rating)));
    return "★★★★★".slice(0, full) + "☆☆☆☆☆".slice(0, 5 - full);
  }

  function distance(km) {
    if (km == null) return null;
    return km < 1 ? Math.round(km * 100) * 10 + " m" : km.toFixed(1).replace(".", ",") + " km";
  }

  function card(p) {
    var a = el("a", "place-card");
    a.href = "/lugar/" + encodeURIComponent(p.id);

    var head = el("div", "place-card-head");
    var left = el("div");
    left.appendChild(el("h3", null, p.name));
    if (p.address) left.appendChild(el("p", "addr", p.address));
    head.appendChild(left);
    head.appendChild(el("span", "tag", typeNames[p.type] || "Mixto"));
    a.appendChild(head);

    var meta = el("div", "place-meta");
    if (p.reviewCount > 0) {
      var avg = p.averageRating != null ? p.averageRating : p.rating;
      var r = el("span", "rating");
      var s = el("span", "stars", stars(avg));
      s.setAttribute("aria-hidden", "true");
      r.appendChild(s);
      r.appendChild(el("b", null, avg.toFixed(1).replace(".", ",")));
      r.appendChild(el("span", "muted", "(" + p.reviewCount + ")"));
      meta.appendChild(r);
    } else {
      meta.appendChild(el("span", "muted", "Sin reseñas todavía"));
    }
    var d = distance(p.distanceKm);
    if (d) meta.appendChild(el("span", null, d));
    if (!p.isAvailable) meta.appendChild(el("span", "tag tag--stop", "Cerrado"));
    a.appendChild(meta);

    var am = el("div", "amenities");
    function add(cond, id, label) {
      if (!cond) return;
      var chip = el("span", "amenity");
      chip.appendChild(icon(id));
      chip.appendChild(document.createTextNode(label));
      am.appendChild(chip);
    }
    add(p.isAccessible, "i-access", "Accesible");
    add(p.haveBabyChanger, "i-baby", "Cambiador");
    add(p.isFree, "i-free", "Gratis");
    add(p.openingHours, "i-clock", p.openingHours);
    a.appendChild(am);

    var li = el("li");
    li.appendChild(a);
    return li;
  }

  function renderList(places) {
    list.textContent = "";
    if (places.length === 0) {
      list.appendChild(el("li", "muted", "No hay lugares a menos de " + NEAR_RADIUS_KM + " km con esos filtros."));
      return;
    }
    places.forEach(function (p) { list.appendChild(card(p)); });
  }

  // ── Near me ──
  function queryFromForm() {
    var data = new FormData(form);
    var params = new URLSearchParams();
    if (data.get("q")) params.set("q", data.get("q"));
    if (data.get("tipo")) params.set("type", data.get("tipo"));
    if (data.get("accesible")) params.set("accessible", "true");
    if (data.get("cambiador")) params.set("babyChanger", "true");
    if (data.get("gratis")) params.set("free", "true");
    if (data.get("abierto")) params.set("availableOnly", "true");
    return params;
  }

  function searchNear() {
    var params = queryFromForm();
    params.set("lat", userLocation[0]);
    params.set("long", userLocation[1]);
    params.set("radiusKm", NEAR_RADIUS_KM);
    params.set("limit", "200");
    status.textContent = "Buscando cerca de ti…";
    fetch("/api/places?" + params.toString(), { headers: { Accept: "application/json" } })
      .then(function (r) { if (!r.ok) throw new Error(r.status); return r.json(); })
      .then(function (places) {
        renderList(places);
        showOnMap(places.map(function (p) {
          return { id: p.id, name: p.name, lat: p.lat, lng: p.long, rating: p.reviewCount > 0 ? p.averageRating : null, open: p.isAvailable };
        }), true);
        status.textContent = places.length + (places.length === 1 ? " lugar" : " lugares") + " a menos de " + NEAR_RADIUS_KM + " km · más cercanos primero";
      })
      .catch(function () { status.textContent = "No pudimos cargar los lugares. Inténtalo de nuevo."; });
  }

  if (locateBtn && "geolocation" in navigator) {
    locateBtn.hidden = false;
    locateBtn.addEventListener("click", function () {
      locateBtn.disabled = true;
      status.textContent = "Pidiendo tu ubicación…";
      navigator.geolocation.getCurrentPosition(function (pos) {
        locateBtn.disabled = false;
        userLocation = [pos.coords.latitude, pos.coords.longitude];
        if (meMarker) meMarker.remove();
        meMarker = L.marker(userLocation, { icon: L.divIcon({ className: "pin", html: '<span class="pin-me"></span>', iconSize: [18, 18], iconAnchor: [9, 9] }), title: "Tu ubicación", keyboard: false }).addTo(map);
        searchNear();
      }, function () {
        locateBtn.disabled = false;
        status.textContent = "No tenemos permiso para usar tu ubicación. Puedes buscar por zona o calle.";
      }, { enableHighAccuracy: false, timeout: 10000, maximumAge: 60000 });
    });

    // With a location, filters re-run the nearby search instead of reloading the page.
    form.addEventListener("submit", function (e) {
      if (!userLocation) return;
      e.preventDefault();
      searchNear();
    });
  }

  showOnMap(initial, true);
})();
