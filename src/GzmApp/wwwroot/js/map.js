(function () {
  "use strict";

  var root = document.getElementById("mapa-root");
  var mapEl = document.getElementById("map");
  if (!root || !mapEl || typeof L === "undefined") return;

  var points = [];
  try {
    points = JSON.parse(root.getAttribute("data-points") || "[]");
  } catch (_) {
    points = [];
  }

  var centerLat = parseFloat(root.getAttribute("data-center-lat")) || 50.3;
  var centerLon = parseFloat(root.getAttribute("data-center-lon")) || 18.9;
  var bounds = L.latLngBounds(
    [parseFloat(root.getAttribute("data-min-lat")), parseFloat(root.getAttribute("data-min-lon"))],
    [parseFloat(root.getAttribute("data-max-lat")), parseFloat(root.getAttribute("data-max-lon"))]
  );

  var map = L.map(mapEl, {
    zoomControl: true,
    attributionControl: true,
    scrollWheelZoom: true,
  });

  L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
    maxZoom: 18,
    className: "map-tiles",
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
  }).addTo(map);

  function fitAll() {
    if (bounds.isValid()) map.fitBounds(bounds.pad(0.05));
    else map.setView([centerLat, centerLon], 10);
  }

  fitAll();

  var markersByCommune = Object.create(null);
  var selected = (root.getAttribute("data-selected") || "").trim() || null;

  function markerHtml(deficit, active) {
    var cls = "map-pin" + (deficit ? " is-bad" : " is-ok") + (active ? " is-active" : "");
    return '<span class="' + cls + '"></span>';
  }

  function makeIcon(deficit, active) {
    var box = active ? 28 : 24;
    return L.divIcon({
      className: "map-pin-wrap",
      html: markerHtml(deficit, active),
      iconSize: [box, box],
      iconAnchor: [box / 2, box / 2],
      popupAnchor: [0, -14],
    });
  }

  function setActiveRow(commune) {
    document.querySelectorAll(".row.is-active").forEach(function (el) {
      el.classList.remove("is-active");
    });
    if (!commune) return;
    var row = document.querySelector('.row[data-commune="' + cssEscape(commune) + '"]');
    if (row) {
      row.classList.add("is-active");
      row.scrollIntoView({ block: "nearest" });
    }
  }

  function cssEscape(value) {
    if (window.CSS && CSS.escape) return CSS.escape(value);
    return String(value).replace(/"/g, '\\"');
  }

  function updateDetail(commune) {
    if (!commune || typeof htmx === "undefined") return;
    htmx.ajax("GET", "?handler=Mapa&gmina=" + encodeURIComponent(commune), {
      target: "#mapa-detail",
      swap: "innerHTML",
    });
  }

  function selectCommune(commune, fly) {
    if (!commune) return;
    selected = commune;
    root.setAttribute("data-selected", commune);

    Object.keys(markersByCommune).forEach(function (key) {
      var entry = markersByCommune[key];
      entry.marker.setIcon(makeIcon(entry.deficit, key === commune));
    });

    var entry = markersByCommune[commune];
    if (entry && fly !== false) {
      map.flyTo(entry.marker.getLatLng(), Math.max(map.getZoom(), 12), { duration: 0.5 });
      entry.marker.openPopup();
    }

    setActiveRow(commune);
    updateDetail(commune);
  }

  points.forEach(function (p) {
    var deficit = p.available < p.required;
    var marker = L.marker([p.lat, p.lon], {
      icon: makeIcon(deficit, p.commune === selected),
      title: p.commune,
    });
    marker.bindPopup(
      "<strong>" +
        escapeHtml(p.commune) +
        "</strong><br>dostępne " +
        p.available +
        " · wymagane " +
        p.required
    );
    marker.on("click", function () {
      selectCommune(p.commune, false);
    });
    marker.addTo(map);
    markersByCommune[p.commune] = { marker: marker, deficit: deficit };
  });

  function escapeHtml(text) {
    return String(text)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  document.addEventListener("rowerstat:select", function (ev) {
    var commune = ev.detail && ev.detail.commune;
    if (commune) selectCommune(commune, true);
  });

  document.addEventListener("click", function (ev) {
    var row = ev.target.closest && ev.target.closest(".row[data-commune]");
    if (row) selectCommune(row.getAttribute("data-commune"), true);
  });

  document.addEventListener("keydown", function (ev) {
    if (ev.key !== "Enter" && ev.key !== " ") return;
    var row = ev.target.closest && ev.target.closest(".row[data-commune]");
    if (!row) return;
    ev.preventDefault();
    selectCommune(row.getAttribute("data-commune"), true);
  });

  document.body.addEventListener("htmx:afterSwap", function (ev) {
    if (ev.target && ev.target.id === "wyniki" && selected) setActiveRow(selected);
  });

  function refreshSize() {
    map.invalidateSize(true);
  }

  // Layout settles after fonts/flex; without this the canvas stays blank.
  refreshSize();
  requestAnimationFrame(refreshSize);
  setTimeout(refreshSize, 50);
  setTimeout(refreshSize, 250);
  setTimeout(function () {
    refreshSize();
    if (selected && markersByCommune[selected]) {
      map.setView(markersByCommune[selected].marker.getLatLng(), 11);
      markersByCommune[selected].marker.openPopup();
      setActiveRow(selected);
    } else {
      fitAll();
    }
  }, 300);

  window.addEventListener("resize", refreshSize);
})();
