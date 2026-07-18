import csv
from io import StringIO
from pathlib import Path

path = Path(__file__).resolve().parents[2] / "Resources_moved/Config/WeaponItems.csv"
text = path.read_text(encoding="utf-8")
lines = text.strip().splitlines()
reader = csv.reader(lines)
rows = list(reader)
header = rows[0]

new_header = [
    "id", "name", "pack", "category", "proficiencyType", "proficiencyOverride",
    "syntyPrefab", "icon", "worldPrefab", "gridW", "gridH", "itemType", "weight", "renderVertical",
]

if header != new_header:
    old = {name: i for i, name in enumerate(header)}
    migrated = [new_header]
    for row in rows[1:]:
        if not row:
            continue

        def g(key, default=""):
            i = old.get(key)
            return row[i] if i is not None and i < len(row) else default

        migrated.append([
            g("id"), g("name"), g("pack"), g("category"), "", "0",
            g("syntyPrefab"), g("icon"), g("worldPrefab"), g("gridW"), g("gridH"),
            g("itemType"), g("weight"), g("renderVertical"),
        ])
    rows = migrated
    header = new_header

idx = {name: i for i, name in enumerate(header)}

prof_map = {
    "GreatSword2H": "GreatSword",
    "HeavyWeapon2H": "HeavyWeapon",
    "Polearm2H": "Polearm",
    "Bow": "BowCrossbow",
    "Shield": "Shield",
    "Sword1H": "Longsword",
    "Hammer1H": "HammerAxe",
    "Dagger1H": "Dagger",
    "FirearmRifle": "Firearm",
    "FirearmPistol": "Firearm",
    "Misc1H": "Longsword",
}

changes = 0
for row in rows[1:]:
    if not row:
        continue

    name = row[idx["name"]]
    cat = row[idx["category"]]
    new_cat = cat

    if "Kingdom Axe" in name:
        new_cat = "HeavyWeapon2H"
    elif "Kingdom Hammer" in name:
        new_cat = "HeavyWeapon2H"
    elif name == "Hero Axe 01":
        new_cat = "Hammer1H"
    elif name == "Kingdom Elephant Gun 01":
        new_cat = "FirearmRifle"
    elif name == "Kingdom Elephant Gun 02":
        new_cat = "FirearmPistol"

    if new_cat != cat:
        changes += 1
        row[idx["category"]] = new_cat

    row[idx["proficiencyType"]] = prof_map.get(row[idx["category"]], "Longsword")
    row[idx["proficiencyOverride"]] = "0"

    if row[idx["category"]] == "FirearmPistol":
        row[idx["gridW"]] = "6"
        row[idx["gridH"]] = "2"
    elif row[idx["category"]] == "FirearmRifle":
        row[idx["gridW"]] = "10"
        row[idx["gridH"]] = "2"
    elif row[idx["category"]] == "HeavyWeapon2H":
        row[idx["weight"]] = "7"

out = StringIO()
writer = csv.writer(out, lineterminator="\n")
writer.writerow(header)
writer.writerows(rows[1:])
path.write_text(out.getvalue(), encoding="utf-8")
print(f"Updated WeaponItems.csv, category changes: {changes}, rows: {len(rows) - 1}")
