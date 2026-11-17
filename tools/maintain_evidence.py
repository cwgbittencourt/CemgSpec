"""Sanitize documentary evidence and consolidate byte-identical raw artifacts.

Run from any directory: python tools/maintain_evidence.py --check | --apply | --verify.
Never executes evidence, changes product files, or rewrites Git history.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess

REPO = Path(__file__).resolve().parents[1]
FEATURE = REPO / "specs/001-auditar-projetos-speckit"
LEDGER = FEATURE / "evidence/maintenance-manifest.json"
TEXT = {".md", ".json", ".jsonl", ".txt", ".trx", ".xml", ".csv", ".log", ".diff"}
HASH = re.compile(r"(?<![a-fA-F0-9])[a-fA-F0-9]{64}(?![a-fA-F0-9])")


def digest(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def variants(value: str) -> set[str]:
    forward = value.replace("\\", "/").rstrip("/")
    back = forward.replace("/", "\\")
    return {forward, back, back.replace("\\", "\\\\")}


def sanitize(text: str) -> str:
    text = re.sub(r"(?i)\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b", "__EMAIL__", text)
    user = os.environ.get("USERNAME", "")
    machine = os.environ.get("COMPUTERNAME", "")
    if user and machine:
        text = re.sub(re.escape(user + "_" + machine), "runner", text, flags=re.I)
    # Longest roots first, including escaped JSON and mixed slash spellings.
    roots = [(str(REPO), "__REPO_ROOT__"),
             (str(Path.home()), "__USER_HOME__")]
    for value, token in roots:
        for spelling in sorted(variants(value), key=len, reverse=True):
            text = re.sub(re.escape(spelling), lambda _: token, text, flags=re.I)
    # Documents may record another checkout on this machine.
    text = re.sub(r"(?i)[A-Z]:[\\/]+Codex_Workspace[\\/]+Projetos[\\/]+SpecAudit", "__REPO_ROOT__", text)
    text = re.sub(r"(?i)[A-Z]:[\\/]+Users[\\/]+[^\\/\s\"<>]+", "__USER_HOME__", text)
    # TRX names and computerName contain workstation metadata, not test behavior.
    if machine:
        text = re.sub(r"(?i)((?:computerName|machineName)[\"']?\s*[:=]\s*[\"'])" + re.escape(machine) + r"([\"'])",
                      lambda m: m[1] + "__MACHINE__" + m[2], text)
    # A hostname may also be a normal word in prose. Never replace it globally.
    for value, token in [(user, "__USER__")]:
        if len(value) >= 3:
            text = re.sub(r"(?<![A-Za-z0-9])" + re.escape(value) + r"(?![A-Za-z0-9])", token, text, flags=re.I)
    return text


def payloads() -> dict[Path, bytes]:
    return {p: p.read_bytes() for p in FEATURE.rglob("*")
            if p.is_file() and p != LEDGER
            and not {"bin", "obj", ".git"}.intersection(p.relative_to(FEATURE).parts)}


def plan(original: dict[Path, bytes]) -> tuple[dict[Path, bytes], dict[Path, Path]]:
    groups: dict[str, list[Path]] = {}
    for path, data in original.items():
        if path.is_relative_to(FEATURE / "evidence/logs"):
            groups.setdefault(digest(data), []).append(path)
    aliases: dict[Path, Path] = {}
    for group in groups.values():
        ordered = sorted(group, key=lambda p: (len(p.relative_to(FEATURE).as_posix()), p.as_posix()))
        for duplicate in ordered[1:]:
            aliases[duplicate] = ordered[0]

    source: dict[Path, bytes] = {}
    for path, data in original.items():
        if path in aliases:
            continue
        if path.suffix.lower() in TEXT:
            # Preserve BOM and exact line endings; decoding/encoding is reversible.
            text = sanitize(data.decode("utf-8"))
            for duplicate, canonical in sorted(aliases.items(), key=lambda pair: len(str(pair[0])), reverse=True):
                old = duplicate.relative_to(FEATURE).as_posix()
                new = canonical.relative_to(FEATURE).as_posix()
                for spelling in sorted(variants(old), key=len, reverse=True):
                    replacement = new.replace("/", "\\\\" if "\\\\" in spelling else "\\") if "\\" in spelling else new
                    text = text.replace(spelling, replacement)
                # Markdown commonly omits the evidence/ prefix.
                text = text.replace(old.removeprefix("evidence/"), new.removeprefix("evidence/"))
            data = text.encode("utf-8")
        source[path] = data

    # Recompute from original sanitized templates on every pass; never rehash
    # stale historical IDs as if they described today's files.
    candidate = dict(source)
    for _ in range(100):
        changes = {digest(original[path]): digest(data)
                   for path, data in candidate.items() if original[path] != data}
        updated = {path: HASH.sub(lambda m: changes.get(m.group().lower(), m.group()), data.decode("utf-8")).encode("utf-8")
                   if path.suffix.lower() in TEXT else data
                   for path, data in source.items()}
        if updated == candidate:
            return updated, aliases
        candidate = updated
    raise RuntimeError("Hash dependency graph did not converge; no files written")


def apply(original: dict[Path, bytes], candidate: dict[Path, bytes], aliases: dict[Path, Path], renames: dict[Path, Path]) -> dict:
    previous = json.loads(LEDGER.read_text(encoding="utf-8")) if LEDGER.exists() else {"schema_version": 1, "rounds": []}
    changed = [{"path": path.relative_to(FEATURE).as_posix(),
                "original_sha256": digest(original[path]), "sanitized_sha256": digest(data)}
               for path, data in candidate.items() if data != original[path]]
    consolidated = [{"removed": path.relative_to(FEATURE).as_posix(),
                     "canonical": target.relative_to(FEATURE).as_posix(),
                     "original_sha256": digest(original[path]),
                     "canonical_sha256": digest(candidate[target])}
                    for path, target in aliases.items()]
    if not changed and not consolidated and not renames:
        return {"changed": 0, "consolidated": 0}
    # Every resolved removal is checked inside the intended logs directory and
    # required to have identical original bytes. No recursive deletion.
    logs = (FEATURE / "evidence/logs").resolve()
    for path, target in aliases.items():
        if not path.resolve().is_relative_to(logs) or original[path] != original[target]:
            raise RuntimeError("Unsafe consolidation; no files written")
    for old, new in renames.items():
        if not old.resolve().is_relative_to(logs) or new.exists() or old.parent != new.parent:
            raise RuntimeError("Unsafe metadata rename; no files written")
    for old, new in renames.items():
        old.rename(new)
    previous["rounds"].append({"base_commit": subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=REPO, text=True).strip(),
                               "operation": "Path sanitization and exact-byte deduplication; not a test rerun or acceptance",
                               "changed": changed, "consolidated": consolidated,
                               "renamed": [{"original_path_sha256": digest(old.relative_to(FEATURE).as_posix().encode()),
                                            "path": new.relative_to(FEATURE).as_posix()} for old, new in renames.items()]})
    for path, data in candidate.items():
        if data != original[path]:
            path.write_bytes(data)
    for path in aliases:
        path.unlink()
    LEDGER.write_text(sanitize(json.dumps(previous, ensure_ascii=False, indent=2)) + "\n", encoding="utf-8")
    return {"changed": len(changed), "consolidated": len(consolidated)}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    action = parser.add_mutually_exclusive_group(required=True)
    action.add_argument("--check", action="store_true")
    action.add_argument("--apply", action="store_true")
    action.add_argument("--verify", action="store_true")
    args = parser.parse_args()
    if args.verify:
        checked = 0
        records = [(FEATURE / "evidence/maintenance-inventory.json", FEATURE, "files"),
                   (FEATURE / "evidence/maintenance-code-manifest.json", REPO, "files"),
                   (FEATURE / "verification/implementation-stage.json", FEATURE, "evidence")]
        for record, root, field in records:
            data = json.loads(record.read_text(encoding="utf-8-sig"))
            for entry in data[field]:
                target = (root / entry["path"]).resolve()
                if not target.is_relative_to(root.resolve()) or digest(target.read_bytes()) != entry["sha256"]:
                    raise RuntimeError(f"Invalid current reference: {record.relative_to(FEATURE)} -> {entry['path']}")
                checked += 1
        print(json.dumps({"current_hash_references_verified": checked}))
        return 0
    original = payloads()
    renames = {path: path.with_name(sanitize(path.name)) for path in original
               if path.is_relative_to(FEATURE / "evidence/logs") and path.name != sanitize(path.name)}
    if len(set(renames.values())) != len(renames):
        raise RuntimeError("Metadata rename collision; no files written")
    original = {renames.get(path, path): data for path, data in original.items()}
    candidate, aliases = plan(original)
    summary = {"files_checked": len(original), "files_to_sanitize_or_rehash": sum(original[p] != data for p, data in candidate.items()),
               "exact_duplicates": len(aliases), "redundant_bytes": sum(len(original[p]) for p in aliases), "metadata_renames": len(renames)}
    if args.apply:
        summary.update(apply(original, candidate, aliases, renames))
    print(json.dumps(summary, indent=2))
    return int(args.check and (summary["files_to_sanitize_or_rehash"] > 0 or bool(aliases) or bool(renames)))


if __name__ == "__main__":
    raise SystemExit(main())
