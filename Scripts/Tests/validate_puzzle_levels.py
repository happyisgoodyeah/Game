"""检查关卡 Excel 与 Luban 导出数据一致，并独立搜索每关的完整拼图解。

在项目根目录执行：python Scripts/Tests/validate_puzzle_levels.py
此检查不依赖第三方库；拖拽、动画和界面切换仍需在 Unity 中验证。
"""

import json
from pathlib import Path
import xml.etree.ElementTree as ET
import zipfile


ROOT = Path(__file__).resolve().parents[2]
PACKAGE = ROOT / "Packages/cn.etetet.yiuilubangen/Assets"
NAMESPACE = {"m": "http://schemas.openxmlformats.org/spreadsheetml/2006/main"}


def read_source():
    with zipfile.ZipFile(PACKAGE / "Editor/Luban/Datas/Grid.xlsx") as archive:
        strings = [
            "".join(element.itertext())
            for element in ET.fromstring(archive.read("xl/sharedStrings.xml")).findall("m:si", NAMESPACE)
        ] if "xl/sharedStrings.xml" in archive.namelist() else []
        sheet = ET.fromstring(archive.read("xl/worksheets/sheet1.xml"))
        records = []
        current = None
        for row in sheet.findall("m:sheetData/m:row", NAMESPACE):
            cells = {}
            for cell in row.findall("m:c", NAMESPACE):
                value = cell.find("m:v", NAMESPACE)
                text = value.text if value is not None else ""
                if cell.get("t") == "s" and text:
                    text = strings[int(text)]
                elif cell.get("t") == "inlineStr":
                    text = "".join(cell.find("m:is", NAMESPACE).itertext())
                cells["".join(filter(str.isalpha, cell.get("r")))] = text
            if cells.get("B", "").isdigit():
                current = {
                    "Id": int(cells["B"]), "Name": cells["C"],
                    "X": int(cells["D"]), "Y": int(cells["E"]),
                    "PuzzleCount": int(cells["F"]), "PrefabPath": cells["G"],
                    "SlotList": [int(value) for value in cells["L"].split(";") if value],
                    "PuzzleList": [],
                }
                records.append(current)
            if current is not None and cells.get("H"):
                current["PuzzleList"].append({
                    "Id": int(cells["H"]),
                    "Trans": {axis: float(cells.get(column) or 0) for axis, column in zip("xyz", "IJK")},
                })
        return records


def orientations(offsets):
    variants = set()
    for turns in range(4):
        rotated = []
        for x, y in offsets:
            for _ in range(turns):
                x, y = -y, x
            # 与 PuzzleSystem.GetCoveredPositions 的坐标约定保持一致。
            rotated.append((y, x))
        variants.add(tuple(sorted(rotated)))
    return variants


def has_solution(level, puzzle_configs, slot_configs):
    width, height = level["X"], level["Y"]
    available = frozenset(
        (index % width, index // width)
        for index, slot_id in enumerate(level["SlotList"])
        if slot_configs[slot_id]["AllowPlace"]
    )
    pieces = tuple(piece["Id"] for piece in level["PuzzleList"])
    assert sum(len(puzzle_configs[piece]["SlotOffset"]) for piece in pieces) == len(available), level["Id"]
    shapes = {piece: orientations(puzzle_configs[piece]["SlotOffset"]) for piece in pieces}
    failed = set()

    def search(unfilled, remaining):
        if not unfilled:
            return not remaining
        state = unfilled, remaining
        if state in failed:
            return False
        target_x, target_y = min(unfilled)
        tried = set()
        for index, piece in enumerate(remaining):
            if piece in tried:
                continue
            tried.add(piece)
            for shape in shapes[piece]:
                for offset_x, offset_y in shape:
                    origin_x, origin_y = target_x - offset_x, target_y - offset_y
                    covered = frozenset((origin_x + x, origin_y + y) for x, y in shape)
                    if covered <= unfilled and all(0 <= x < width and 0 <= y < height for x, y in covered):
                        if search(unfilled - covered, remaining[:index] + remaining[index + 1:]):
                            return True
        failed.add(state)
        return False

    return search(available, tuple(sorted(pieces)))


def main():
    load = lambda path: json.loads(path.read_text(encoding="utf-8-sig"))
    levels = load(PACKAGE / "Config/Json/ClientServer/GridConfigCategory.json")
    assert levels == read_source(), "Excel source and exported configuration differ"
    for mode in ("Client", "Server"):
        assert levels == load(PACKAGE / f"Config/Json/{mode}/GridConfigCategory.json"), mode
    puzzle_configs = {item["Id"]: item for item in load(PACKAGE / "Config/Json/ClientServer/PuzzleConfigCategory.json")}
    slot_configs = {item["Id"]: item for item in load(PACKAGE / "Config/Json/ClientServer/SlotConfigCategory.json")}
    assert len({item["Id"] for item in levels}) == len(levels), "Duplicate level IDs"
    signatures = set()
    for level in levels:
        assert len(level["SlotList"]) == level["X"] * level["Y"], level["Id"]
        assert level["PuzzleCount"] == len(level["PuzzleList"]), level["Id"]
        signature = level["X"], level["Y"], tuple(level["SlotList"])
        assert signature not in signatures, f'Duplicate board: {level["Id"]}'
        signatures.add(signature)
        assert has_solution(level, puzzle_configs, slot_configs), f'Unsolvable level: {level["Id"]}'
    print(f"Validated {len(levels)} distinct, solvable levels. Excel and Client/Server/ClientServer exports agree.")


if __name__ == "__main__":
    main()
