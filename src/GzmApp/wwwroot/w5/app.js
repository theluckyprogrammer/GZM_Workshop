(function () {
  "use strict";

  var filtrInput = document.getElementById("filtr");
  var searchInput = document.getElementById("szukaj");

  function refreshList() {
    if (!searchInput || typeof htmx === "undefined") return;
    htmx.ajax(
      "GET",
      "?handler=Szukaj&q=" +
        encodeURIComponent(searchInput.value || "") +
        "&filtr=" +
        encodeURIComponent((filtrInput && filtrInput.value) || "wszystkie"),
      { target: "#wyniki", swap: "innerHTML" }
    );
  }

  document.querySelectorAll(".filter-btn").forEach(function (btn) {
    btn.addEventListener("click", function () {
      var value = btn.getAttribute("data-filtr") || "wszystkie";
      if (filtrInput) filtrInput.value = value;
      document.querySelectorAll(".filter-btn").forEach(function (b) {
        b.classList.toggle("is-active", b === btn);
        b.setAttribute("aria-pressed", b === btn ? "true" : "false");
      });
      refreshList();
    });
  });

  document.querySelectorAll(".gap-item[data-commune]").forEach(function (btn) {
    btn.addEventListener("click", function () {
      var commune = btn.getAttribute("data-commune");
      if (!commune) return;
      document.dispatchEvent(new CustomEvent("rowerstat:select", { detail: { commune: commune } }));
    });
  });
})();
