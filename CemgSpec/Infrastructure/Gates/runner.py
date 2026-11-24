"""Trusted CemgSpec runner for CEMG 3.0.0 check_gate.py.

Invokes ONLY validate() or validate_impossibility() from the hash-verified
packaged core using capability-based CatalogPath objects. Never accepts
--project, never invokes main() or active(), never receives the audited origin
filesystem path, and never writes files.
"""

from __future__ import annotations

import hashlib
import importlib.util
import json
import os
import sys

# Ensure sibling catalog_path.py is importable even under Python -I (isolated mode).
_RUNNER_DIR = os.path.dirname(os.path.abspath(__file__))
if _RUNNER_DIR not in sys.path:
    sys.path.insert(0, _RUNNER_DIR)

from catalog_path import (  # noqa: E402
    BrokerAccessError,
    BrokerClient,
    CatalogPath,
    PolicyOnlyPath,
    ScopeViolationError,
)


def _sha256_file(path: str) -> str:
    with open(path, "rb") as handle:
        return hashlib.sha256(handle.read()).hexdigest()


def _emit(payload: dict) -> None:
    sys.stdout.write(json.dumps(payload, ensure_ascii=False) + "\n")
    sys.stdout.flush()


def run() -> int:
    start_line = sys.stdin.readline()
    if not start_line:
        _emit({"op": "error", "code": "MissingStartPayload", "message": "No start payload received on stdin."})
        return 2

    try:
        req = json.loads(start_line)
    except json.JSONDecodeError as exc:
        _emit({"op": "error", "code": "InvalidStartPayload", "message": str(exc)})
        return 2

    if req.get("op") != "start":
        _emit({"op": "error", "code": "UnexpectedOp", "message": "First message must have op=start."})
        return 2

    run_id = str(req["run_id"])
    token = str(req["token"])
    catalog_generation = int(req["catalog_generation"])
    stage = str(req["stage"])
    record_rel = str(req["record_relative_path"])
    expected_revision = str(req["expected_revision"])
    core_path = str(req["core_path"])
    policy_path = str(req["policy_path"])
    expected_core_sha256 = str(req["expected_core_sha256"])
    expected_policy_sha256 = str(req["expected_policy_sha256"])
    raw_catalog = req.get("catalog_map", {})

    if stage not in ("spec", "plan", "tasks", "pr", "acceptance", "dev-impossibility"):
        _emit({"op": "error", "code": "InvalidStage", "message": f"Unsupported stage: {stage}"})
        return 2

    # Verify packaged core and policy SHA-256 before loading anything.
    actual_core_sha256 = _sha256_file(core_path)
    if actual_core_sha256 != expected_core_sha256:
        _emit({"op": "error", "code": "CoreIntegrityMismatch", "message": "Packaged check_gate.py hash mismatch."})
        return 2

    actual_policy_sha256 = _sha256_file(policy_path)
    if actual_policy_sha256 != expected_policy_sha256:
        _emit({"op": "error", "code": "PolicyIntegrityMismatch", "message": "Packaged gate-requirements.json hash mismatch."})
        return 2

    with open(policy_path, "r", encoding="utf-8") as policy_handle:
        policy_text = policy_handle.read()

    # Load packaged check_gate.py dynamically without running __main__ or active().
    spec = importlib.util.spec_from_file_location("cemgspec_cemg_check_gate", core_path)
    if spec is None or spec.loader is None:
        _emit({"op": "error", "code": "CoreLoadError", "message": "Unable to load packaged check_gate.py."})
        return 2
    core_module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(core_module)

    # Lock down core module I/O: only the verified policy JSON may be read via Path,
    # and subprocess is disabled so active() or external commands can never run.
    PolicyOnlyPath.configure(core_module.__file__, policy_text)
    core_module.Path = PolicyOnlyPath
    core_module.subprocess = None

    # Build case-insensitive + exact normalized virtual catalog map.
    catalog_map: dict[str, str] = {}
    for rel_key, doc_id in raw_catalog.items():
        norm = str(rel_key).replace("\\", "/").lstrip("/")
        catalog_map[norm] = str(doc_id)
        catalog_map[norm.lower()] = str(doc_id)

    broker = BrokerClient(run_id, token, catalog_generation)
    feature_root = CatalogPath("", catalog_map, broker, is_root=True)

    try:
        record_path = core_module.contained(feature_root, record_rel)
        if stage == "dev-impossibility":
            errors = core_module.validate_impossibility(feature_root, record_path, expected_revision)
        else:
            errors = core_module.validate(feature_root, record_path, stage, expected_revision)
    except BrokerAccessError as exc:
        _emit(
            {
                "op": "result",
                "outcome": None,
                "errors": [str(exc)],
                "scope_violation": exc.code == "OutOfScope",
                "broker_error_code": exc.code,
                "core_sha256": actual_core_sha256,
                "policy_sha256": actual_policy_sha256,
            }
        )
        return 2
    except ScopeViolationError as exc:
        _emit(
            {
                "op": "result",
                "outcome": None,
                "errors": [str(exc)],
                "scope_violation": True,
                "broker_error_code": "OutOfScope",
                "core_sha256": actual_core_sha256,
                "policy_sha256": actual_policy_sha256,
            }
        )
        return 2
    except (OSError, ValueError, KeyError, TypeError, json.JSONDecodeError) as exc:
        errors = [str(exc)]

    success_label = "DEV_IMPOSSIBILITY_RECORDED" if stage == "dev-impossibility" else "READY"
    outcome = "BLOCKED" if errors else success_label
    _emit(
        {
            "op": "result",
            "outcome": outcome,
            "errors": list(errors),
            "scope_violation": False,
            "broker_error_code": None,
            "core_sha256": actual_core_sha256,
            "policy_sha256": actual_policy_sha256,
        }
    )
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(run())
