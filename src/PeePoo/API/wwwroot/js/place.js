// Small static map on the place page.
(function () {
  var el = document.getElementById("mini-map");
  if (!el || typeof L === "undefined") return;
  var lat = parseFloat(el.dataset.lat), lng = parseFloat(el.dataset.lng);
  if (isNaN(lat) || isNaN(lng)) return;
  var map = L.map(el, { scrollWheelZoom: false, dragging: !L.Browser.mobile, zoomControl: true }).setView([lat, lng], 16);
  L.tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
    maxZoom: 19,
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
  }).addTo(map);
  L.marker([lat, lng], { icon: L.divIcon({ className: "pin", html: '<span class="pin-body"><span>WC</span></span>', iconSize: [34, 34], iconAnchor: [17, 41] }), keyboard: false }).addTo(map);
})();
