// Mobile menu toggle. Everything else on the site works without JavaScript.
(function () {
  var toggle = document.querySelector("[data-menu-toggle]");
  var nav = document.getElementById("site-nav");
  if (!toggle || !nav) return;
  toggle.addEventListener("click", function () {
    var open = nav.classList.toggle("is-open");
    toggle.setAttribute("aria-expanded", open ? "true" : "false");
  });
})();
