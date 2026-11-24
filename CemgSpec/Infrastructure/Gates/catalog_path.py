"""Capability-based virtual Path for CemgSpec check_gate execution.

Never touches the audited origin filesystem. Every document read is mediated
through the authenticated CemgSpec broker using opaque DocumentIds.
"""

from __future__ import annotations

import base64
import json
import posixpath
import sys
from pathlib import PurePosixPath


class ScopeViolationError(ValueError):
    """Raised when a path escapes the virtual feature catalog or targets unauthorized content."""


class BrokerAccessError(OSError):
    """Raised when the CemgSpec DocumentBroker rejects a read at read time (e.g. TOCTOU/Changed)."""

    def __init__(self, code: str, message: str) -> None:
        super().__init__(f"{code}: {message}")
        self.code = code
        self.message = message


class BrokerClient:
    """Line-delimited JSON RPC client bound to a single authenticated ExecutionChannel."""

    def __init__(self, run_id: str, token: str, catalog_generation: int) -> None:
        self._run_id = run_id
        self._token = token
        self._catalog_generation = catalog_generation

    def read_bytes(self, document_id: str) -> bytes:
        request = {
            "op": "read",
            "run_id": self._run_id,
            "token": self._token,
            "catalog_generation": self._catalog_generation,
            "document_id": document_id,
        }
        sys.stdout.write(json.dumps(request, ensure_ascii=False) + "\n")
        sys.stdout.flush()

        raw_line = sys.stdin.readline()
        if not raw_line:
            raise BrokerAccessError("ChannelClosed", "Broker channel closed before delivering document bytes.")

        try:
            response = json.loads(raw_line)
        except json.JSONDecodeError as exc:
            raise BrokerAccessError("InvalidBrokerResponse", str(exc)) from exc

        if not response.get("ok"):
            raise BrokerAccessError(
                str(response.get("code", "BrokerError")),
                str(response.get("message", "Broker denied document read.")),
            )

        encoded = response.get("content_base64")
        if not isinstance(encoded, str):
            raise BrokerAccessError("InvalidBrokerResponse", "Missing content_base64 payload.")
        return base64.b64decode(encoded)


def _normalize_relative_segment(raw: str) -> str:
    if not isinstance(raw, str) or not raw:
        raise ScopeViolationError("Empty path is not permitted.")
    if "\x00" in raw:
        raise ScopeViolationError("NUL byte in path is not permitted.")
    replaced = raw.replace("\\", "/")
    if replaced.startswith("/") or (len(replaced) >= 2 and replaced[1] == ":"):
        raise ScopeViolationError(f"Absolute or drive-qualified path is forbidden: {raw}")
    return replaced


class CatalogPath:
    """Virtual pathlib-compatible capability path restricted to a single feature catalog."""

    def __init__(
        self,
        virtual_relative: str,
        catalog_map: dict[str, str],
        broker: BrokerClient,
        *,
        is_root: bool = False,
    ) -> None:
        self._rel = virtual_relative  # "" for feature root, or normalized posix relative path
        self._catalog_map = catalog_map
        self._broker = broker
        self._is_root = is_root

    def __truediv__(self, other: object) -> CatalogPath:
        if isinstance(other, CatalogPath):
            segment = other._rel
        elif isinstance(other, str):
            segment = _normalize_relative_segment(other)
        else:
            raise ScopeViolationError(f"Unsupported path operand: {type(other).__name__}")

        combined = segment if not self._rel else f"{self._rel}/{segment}"
        return CatalogPath(combined, self._catalog_map, self._broker, is_root=False)

    def resolve(self) -> CatalogPath:
        if not self._rel:
            return CatalogPath("", self._catalog_map, self._broker, is_root=True)
        normalized = posixpath.normpath(self._rel)
        if normalized == ".":
            return CatalogPath("", self._catalog_map, self._broker, is_root=True)
        if normalized == ".." or normalized.startswith("../") or normalized.startswith("/"):
            raise ScopeViolationError(f"Path leaves virtual feature root: {self._rel}")
        return CatalogPath(normalized, self._catalog_map, self._broker, is_root=False)

    def is_relative_to(self, other: object) -> bool:
        if not isinstance(other, CatalogPath):
            return False
        try:
            self_norm = self.resolve()._rel
            other_norm = other.resolve()._rel
        except ScopeViolationError:
            return False
        if other_norm == "":
            return True
        return self_norm == other_norm or self_norm.startswith(other_norm + "/")

    @property
    def parents(self) -> tuple[CatalogPath, ...]:
        norm = self.resolve()._rel
        if not norm:
            return ()
        parts = PurePosixPath(norm).parents
        result = []
        for parent in parts:
            text = "" if str(parent) == "." else str(parent)
            result.append(CatalogPath(text, self._catalog_map, self._broker, is_root=(text == "")))
        return tuple(result)

    def read_bytes(self) -> bytes:
        resolved = self.resolve()._rel
        if not resolved:
            raise ScopeViolationError("Cannot read feature root directory as bytes.")
        doc_id = self._catalog_map.get(resolved) or self._catalog_map.get(resolved.lower())
        if not doc_id:
            raise FileNotFoundError(f"Document not present in authorized feature catalog: {resolved}")
        return self._broker.read_bytes(doc_id)

    def read_text(self, encoding: str = "utf-8") -> str:
        return self.read_bytes().decode(encoding)

    def __str__(self) -> str:
        return self._rel or "."

    def __repr__(self) -> str:
        return f"CatalogPath({self._rel!r})"


class PolicyOnlyPath:
    """Drop-in Path replacement inside check_gate that permits ONLY reading the pinned policy JSON."""

    _expected_core_file: str = ""
    _policy_text: str = ""

    def __init__(self, raw: str | PolicyOnlyPath, *, _kind: str = "init") -> None:
        self._raw = str(raw)
        self._kind = _kind

    @classmethod
    def configure(cls, expected_core_file: str, policy_text: str) -> None:
        cls._expected_core_file = expected_core_file
        cls._policy_text = policy_text

    def resolve(self) -> PolicyOnlyPath:
        if self._raw == self._expected_core_file:
            return PolicyOnlyPath(self._raw, _kind="core_file")
        raise ScopeViolationError(f"Direct filesystem Path.resolve is forbidden: {self._raw}")

    @property
    def parents(self) -> tuple[PolicyOnlyPath, PolicyOnlyPath]:
        if self._kind != "core_file":
            raise ScopeViolationError("Direct filesystem Path.parents is forbidden.")
        return (
            PolicyOnlyPath("<core_dir>", _kind="core_dir"),
            PolicyOnlyPath("<package_root>", _kind="package_root"),
        )

    def __truediv__(self, other: object) -> PolicyOnlyPath:
        if self._kind == "package_root" and str(other).replace("\\", "/") == "policies/gate-requirements.json":
            return PolicyOnlyPath("policies/gate-requirements.json", _kind="policy_file")
        raise ScopeViolationError(f"Direct filesystem access is forbidden: {self._raw}/{other}")

    def read_text(self, encoding: str = "utf-8") -> str:
        if self._kind != "policy_file":
            raise ScopeViolationError(f"Direct filesystem read_text is forbidden: {self._raw}")
        return self._policy_text

    def read_bytes(self) -> bytes:
        if self._kind != "policy_file":
            raise ScopeViolationError(f"Direct filesystem read_bytes is forbidden: {self._raw}")
        return self._policy_text.encode("utf-8")
