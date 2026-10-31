"""使用 F6 生成的程序集验证存档列表的两种格式及旧数据兼容性。

在 Unity 完成 F6 编译后，从项目根目录执行：
python Scripts/Tests/validate_save_collections.py
也可以通过 --assembly-directory 指定独立编译检查的程序集目录。
"""

import argparse
from pathlib import Path
import re
import shutil
import subprocess


ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--assembly-directory", type=Path, default=ROOT / "Temp/Bin/Debug")
    arguments = parser.parse_args()
    model = arguments.assembly_directory.resolve() / "ET.Model.dll"
    if not model.is_file():
        raise RuntimeError("ET.Model.dll not found; compile with F6 first")

    candidates = []
    for candidate in (ROOT / "Library/Bee/artifacts").glob("*/ET.Model.rsp"):
        text = candidate.read_text(encoding="utf-8-sig")
        if "-define:UNITY_EDITOR\n" not in text:
            candidates.append((candidate, text))
    if not candidates:
        raise RuntimeError("Player compiler response file not found; compile with F6 first")
    _, response = max(candidates, key=lambda item: item[0].stat().st_mtime)
    references = [line[3:].strip('"') for line in response.splitlines() if line.startswith("-r:")]
    references = [str((ROOT / value).resolve()) if not Path(value).is_absolute() else value for value in references]
    references.insert(0, str(model))

    dotnet = shutil.which("dotnet")
    if dotnet is None:
        raise RuntimeError("dotnet SDK not installed")
    sdks = subprocess.check_output([dotnet, "--list-sdks"], text=True)
    versions = []
    for line in sdks.splitlines():
        match = re.fullmatch(r"(\d+\.\d+\.\d+) \[(.+)\]", line.strip())
        if match:
            versions.append((tuple(map(int, match[1].split("."))), Path(match[2]) / match[1]))
    compiler = max(versions)[1] / "Roslyn/bincore/csc.dll"
    unity_data = next(re.match(r"(.+?/Editor/Data)/", value.replace("\\", "/"))[1]
                      for value in references if "/Editor/Data/" in value.replace("\\", "/"))
    mono = Path(unity_data) / "MonoBleedingEdge/bin/mono.exe"

    output = ROOT / "Temp/PuzzleGameTests/SaveCollections"
    output.mkdir(parents=True, exist_ok=True)
    runtime_paths = [value.replace(".ref.dll", ".dll") if value.endswith(".ref.dll") else value for value in references]
    runtime = output / "references.txt"
    runtime.write_text("\n".join(runtime_paths), encoding="utf-8")
    runner = output / "SaveCollectionRoundTrip.exe"
    compiler_arguments = ["-nologo", "-nostdlib+", "-langversion:11", "-target:exe", f'-out:"{runner}"']
    compiler_arguments += [f'-r:"{value}"' for value in references]
    compiler_arguments.append(f'"{ROOT / "Scripts/Tests/SaveCollectionRoundTrip.cs"}"')
    compiler_response = output / "test.rsp"
    compiler_response.write_text("\n".join(compiler_arguments), encoding="utf-8")
    subprocess.run([dotnet, str(compiler), f"@{compiler_response}"], cwd=ROOT, check=True)
    subprocess.run([str(mono), str(runner), str(runtime)], cwd=ROOT, check=True)


if __name__ == "__main__":
    main()
