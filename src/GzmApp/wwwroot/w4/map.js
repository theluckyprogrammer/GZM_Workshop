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
    zoomControl: false,
    attributionControl: true,
    scrollWheelZoom: true,
  });
  L.control.zoom({ position: "bottomright", zoomInTitle: "Przybliż", zoomOutTitle: "Oddal" }).addTo(map);

  L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
    maxZoom: 18,
    className: "map-tiles",
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
  }).addTo(map);

  // Floating panels cover the map on wide screens; keep data out from under them.
  function isOverlay() {
    return window.matchMedia("(min-width: 900px)").matches;
  }

  function leftInset() {
    if (!isOverlay()) return 0;
    var panel = document.querySelector(".panel-main");
    return panel ? panel.getBoundingClientRect().right : 0;
  }

  function rightInset() {
    if (!isOverlay()) return 0;
    var side = document.querySelector(".panel-side");
    return side ? window.innerWidth - side.getBoundingClientRect().left : 0;
  }

  function fitAll() {
    if (!bounds.isValid()) {
      map.setView([centerLat, centerLon], 10);
      return;
    }
    map.fitBounds(bounds, {
      paddingTopLeft: [leftInset() + 24, 24],
      paddingBottomRight: [Math.min(rightInset(), 320) + 24, 24],
    });
  }

  function visibleCenterFor(latlng, zoom) {
    var shift = leftInset() / 2;
    var pt = map.project(latlng, zoom).subtract([shift, 0]);
    return map.unproject(pt, zoom);
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

  function cssEscape(value) {
    if (window.CSS && CSS.escape) return CSS.escape(value);
    return String(value).replace(/"/g, '\\"');
  }

  function setActiveRow(commune) {
    document.querySelectorAll(".row.is-active").forEach(function (el) {
      el.classList.remove("is-active");
    });
    if (!commune) return;
    var row = document.querySelector('.row[data-commune="' + cssEscape(commune) + '"]');
    if (!row) return;
    row.classList.add("is-active");
    var box = row.closest(".list-scroll");
    if (box) box.scrollTop = row.offsetTop - box.clientHeight / 2 + row.offsetHeight / 2;
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
      var item = markersByCommune[key];
      item.marker.setIcon(makeIcon(item.deficit, key === commune));
      item.marker.setZIndexOffset(key === commune ? 1000 : 0);
    });

    var entry = markersByCommune[commune];
    if (entry && fly !== false) {
      var zoom = Math.max(map.getZoom(), 12);
      map.flyTo(visibleCenterFor(entry.marker.getLatLng(), zoom), zoom, { duration: 0.5 });
      entry.marker.openPopup();
    }

    setActiveRow(commune);
    updateDetail(commune);
  }

  function escapeHtml(text) {
    return String(text)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function popupHtml(p, deficit) {
    var gap = p.available - p.required;
    return (
      '<div class="pop"><strong class="pop-name">' + escapeHtml(p.commune) + "</strong>" +
      '<span class="pop-status ' + (deficit ? "is-bad" : "is-ok") + '">' + (deficit ? "Deficyt" : "Wystarcza") + "</span>" +
      '<span class="pop-nums">' + p.available + " dostępne · " + p.required + " wymagane · " +
      (gap > 0 ? "+" + gap : String(gap).replace("-", "\u2212")) + "</span></div>"
    );
  }

  points.forEach(function (p) {
    var deficit = p.available < p.required;
    var marker = L.marker([p.lat, p.lon], {
      icon: makeIcon(deficit, p.commune === selected),
      title: p.commune,
      zIndexOffset: deficit ? 100 : 0,
    });
    marker.bindPopup(popupHtml(p, deficit), {
      autoPanPaddingTopLeft: L.point(leftInset() + 16, 16),
      closeButton: false,
    });
    marker.on("click", function () {
      selectCommune(p.commune, false);
    });
    marker.addTo(map);
    markersByCommune[p.commune] = { marker: marker, deficit: deficit };
  });

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

  function onSwap(ev) {
    if (ev.target && ev.target.id === "wyniki" && selected) setActiveRow(selected);
  }
  document.body.addEventListener("htmx:afterSwap", onSwap);
  document.body.addEventListener("htmx:after:swap", onSwap);

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
    var entry = selected && markersByCommune[selected];
    if (!entry) {
      fitAll();
      return;
    }
    map.setView(visibleCenterFor(entry.marker.getLatLng(), 11), 11);
    entry.marker.openPopup();
    setActiveRow(selected);
  }, 300);

  window.addEventListener("resize", refreshSize);
})();
