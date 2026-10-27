# Jednorazowy generator data/zbiory.json (mapowanie scenariusz -> zbiór z portalu GZM).
# Źródła: data/metadata.json (URL-e, licencje), scenarios/sNN/mapping.json (plik, atrybucja),
# scenarios/sNN/checkpoints.md (E1.fileRows), data/raw/*.csv (treść) oraz pliki z portalu
# (sha256 i rozmiar liczony z bajtów zdalnych: portal daje LF, lokalne pliki po checkoucie mają CRLF).
# Nazwy zasobów spisane jawnie (odmiana polska psuje automatyczne dopasowanie);
# poprawność potwierdza zgodność treści po normalizacji końców linii.
import json, os, re, hashlib, subprocess, urllib.request

def read_local(path):
    # pliki data/raw opuscily repo (pobiera je krok 0); czytaj z historii gita, gdy brak na dysku
    if os.path.exists(path):
        return open(path, "rb").read()
    rel = os.path.relpath(path, root).replace("\\", "/")
    for tip in subprocess.run(["git", "log", "--format=%H", "--", rel], cwd=root,
                              capture_output=True, text=True, check=True).stdout.split():
        probe = subprocess.run(["git", "cat-file", "-e", f"{tip}:{rel}"], cwd=root)
        if probe.returncode == 0:
            return subprocess.run(["git", "show", f"{tip}:{rel}"], cwd=root, capture_output=True, check=True).stdout
    raise SystemExit(f"brak pliku {rel} na dysku i w historii gita")

root = os.path.normpath(os.path.join(os.path.dirname(__file__), ".."))

RESOURCE = {
    "s01": "miejsca_pamieci_zabrze.csv",
    "s02": "stacje_rowerowe_aktualne_gzm.csv",
    "s03": "dzienna_dostepnosc_rowerow_gzm.csv",
    "s04": "rzeczy_znalezione_bytom.csv",
    "s05": "rzeczy_znalezione_dabrowa_gornicza.csv",
    "s06": "pomniki_przyrody_dabrowa_gornicza.csv",
    "s07": "obiekty_sportowe_gliwice.csv",
    "s08": "rezerwacje_rowerow_gzm.csv",
    "s09": "zabytki_archeologiczne_dabrowa_gornicza.csv",
    "s10": "boiska_i_stadiony_gliwice.csv",
    "s11": "miejsca_pamieci_katowice.csv",
    "s12": "zabytki_bytom.csv",
    "s13": "zajecia_sekcyjne_centrum_kultury_siemianowice_slaskie.csv",
    "s14": "2026-04-14 Lista automatów biletowych.csv",
    "s15": "uczniowskie_kluby_sportowe_katowice.csv",
}

meta = json.load(open(os.path.join(root, "data/metadata.json"), encoding="utf-8"))
by_name = {}
for d in meta["datasets"]:
    src = d.get("source", {})
    if src.get("resourceName") and src.get("fileUrl"):
        by_name.setdefault(src["resourceName"], []).append((d["code"], d["role"], src["fileUrl"]))

datasets = []
for i in range(1, 16):
    code = f"s{i:02d}"
    mapping = json.load(open(os.path.join(root, "scenarios", code, "mapping.json"), encoding="utf-8"))
    file = mapping["source"]["file"]
    path = os.path.join(root, file)
    name = RESOURCE[code]
    matches = by_name.get(name, [])
    if len(matches) != 1:
        raise SystemExit(f"{code}: zasób {name!r} w metadata.json: {matches}")
    meta_code, role, file_url = matches[0]
    req = urllib.request.Request(file_url, headers={"User-Agent": "Mozilla/5.0"})
    remote = urllib.request.urlopen(req, timeout=30).read()
    local = read_local(path)
    if remote.replace(b"\r\n", b"\n") != local.replace(b"\r\n", b"\n"):
        raise SystemExit(f"{code}: treść z portalu różni się od pliku lokalnego ({name})")
    sha = hashlib.sha256(remote).hexdigest()
    cp = open(os.path.join(root, "scenarios", code, "checkpoints.md"), encoding="utf-8").read()
    m = re.search(r"\| E1\.fileRows \|[^|]+\| (\d+) \|", cp)
    if not m:
        raise SystemExit(f"{code}: brak E1.fileRows w checkpoints.md")
    datasets.append({
        "code": code,
        "file": file,
        "resourceName": name,
        "metadataCode": meta_code,
        "role": role,
        "fileUrl": file_url,
        "sha256": sha,
        "sizeBytes": len(remote),
        "fileRows": int(m.group(1)),
        "headerLine": mapping["source"]["headerLine"],
        "separator": mapping["source"]["separator"],
        "dataset": mapping["dataset"],
        "sourceUrl": mapping["sourceUrl"],
        "attribution": mapping["attribution"],
    })

out = {
    "_opis": "Mapowanie scenariusz -> zbiór otwartych danych GZM (pobieranie w kroku 0 przez tools/nowy-portal.ps1). Wygenerowane tools/zbiory-gen.py z data/metadata.json, mapping.json, checkpoints.md i data/raw (2026-10-27). Nie edytuj ręcznie.",
    "portal": "https://otwartedane.metropoliagzm.pl",
    "license": "CC BY 4.0",
    "datasets": datasets,
}
with open(os.path.join(root, "data/zbiory.json"), "w", encoding="utf-8", newline="\n") as f:
    json.dump(out, f, ensure_ascii=False, indent=2)
    f.write("\n")
print("OK:", len(datasets), "zbiorow")
for d in datasets:
    print(f"  {d['code']} [{d['metadataCode']}/{d['role']}] {d['resourceName']} -> {d['file']} ({d['fileRows']} wierszy)")
