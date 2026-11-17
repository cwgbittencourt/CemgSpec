import hashlib
import json
from pathlib import Path
import unittest
from unittest.mock import patch
import tempfile
import sys
import io
from contextlib import redirect_stdout

import maintain_evidence as evidence


class EvidenceMaintenanceTests(unittest.TestCase):
    def test_current_verifier_detects_modified_artifact(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            feature = root / "feature"
            (feature / "evidence").mkdir(parents=True)
            (feature / "verification").mkdir()
            artifact = feature / "evidence/log.txt"
            artifact.write_bytes(b"PASS")
            inventory = {"files": [{"path": "evidence/log.txt", "sha256": evidence.digest(b"PASS")}]}
            (feature / "evidence/maintenance-inventory.json").write_text(json.dumps(inventory))
            (feature / "evidence/maintenance-code-manifest.json").write_text('{"files": []}')
            (feature / "verification/implementation-stage.json").write_text('{"evidence": []}')
            with patch.object(evidence, "FEATURE", feature), patch.object(evidence, "REPO", root), patch.object(sys, "argv", ["maintain_evidence.py", "--verify"]), redirect_stdout(io.StringIO()):
                self.assertEqual(0, evidence.main())
                artifact.write_bytes(b"tampered")
                with self.assertRaisesRegex(RuntimeError, "Invalid current reference"):
                    evidence.main()

    def test_trx_metadata_and_filename_use_same_portable_identity(self):
        with patch.dict(evidence.os.environ, {"USERNAME": "exampleuser", "COMPUTERNAME": "EXAMPLEPC"}):
            self.assertEqual("runner_2026.trx", evidence.sanitize("exampleuser_EXAMPLEPC_2026.trx"))
            self.assertEqual('computerName="__MACHINE__"', evidence.sanitize('computerName="EXAMPLEPC"'))
        with patch.dict(evidence.os.environ, {"COMPUTERNAME": "TRABALHO"}):
            self.assertEqual("Próximo trabalho", evidence.sanitize("Próximo trabalho"))

    def test_json_escaped_paths_remain_valid_and_hide_home(self):
        value = json.dumps({"path": str(Path.home() / "AppData/Local/Temp/fixture/report.json")})
        result = json.loads(evidence.sanitize(value))
        self.assertIn("__USER_HOME__", result["path"])
        self.assertNotIn(str(Path.home()), result["path"])
        self.assertEqual("email=__EMAIL__", evidence.sanitize("email=example.person@example.invalid"))

    def test_hash_repair_propagates_to_parent_and_revision(self):
        log = evidence.FEATURE / "evidence/logs/example.txt"
        record = evidence.FEATURE / "verification/example.json"
        parent = evidence.FEATURE / "verification/parent.json"
        raw = str(Path.home()).encode()
        record_bytes = json.dumps({"path": "evidence/logs/example.txt", "sha256": evidence.digest(raw)}).encode()
        original = {log: raw, record: record_bytes, parent: json.dumps({"revision": "sha256:" + evidence.digest(record_bytes)}).encode()}
        updated, aliases = evidence.plan(original)
        self.assertFalse(aliases)
        self.assertEqual(evidence.digest(updated[log]), json.loads(updated[record])["sha256"])
        self.assertEqual("sha256:" + evidence.digest(updated[record]), json.loads(updated[parent])["revision"])
        self.assertEqual(updated, evidence.plan(updated)[0])

    def test_only_identical_raw_logs_are_consolidated_with_links_repaired(self):
        a = evidence.FEATURE / "evidence/logs/a.txt"
        b = evidence.FEATURE / "evidence/logs/duplicate/b.txt"
        failure = evidence.FEATURE / "evidence/logs/failure.txt"
        note = evidence.FEATURE / "evidence/note.md"
        original = {a: b"PASS", b: b"PASS", failure: b"FAIL", note: b"[log](logs/duplicate/b.txt)"}
        updated, aliases = evidence.plan(original)
        self.assertEqual({b: a}, aliases)
        self.assertEqual(b"FAIL", updated[failure])
        self.assertEqual(b"[log](logs/a.txt)", updated[note])

    def test_unknown_historical_hash_and_non_evidence_bytes_are_preserved(self):
        history = evidence.FEATURE / "verification/history.json"
        source = evidence.FEATURE / "evidence/archive.cs"
        data = {history: json.dumps({"revision": "sha256:" + "a" * 64, "state": "FAILED"}).encode(), source: b"// original source snapshot"}
        updated, _ = evidence.plan(data)
        self.assertEqual(data, updated)


if __name__ == "__main__":
    unittest.main()
