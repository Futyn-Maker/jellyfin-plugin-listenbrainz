#!/usr/bin/env python3
"""Add (or replace) a plugin version in a Jellyfin plugin repository manifest.

The manifest lists the newest version first. The checksum is the MD5 of the release zip,
which is what Jellyfin verifies when installing.

usage: update_manifest.py MANIFEST BUILD_YAML ZIP SOURCE_URL [IMAGE_URL]
"""

import datetime
import hashlib
import json
import pathlib
import sys

import yaml


def main() -> int:
    manifest_path, build_path, zip_path, source_url = sys.argv[1:5]
    image_url = sys.argv[5] if len(sys.argv) > 5 else None

    build = yaml.safe_load(pathlib.Path(build_path).read_text(encoding="utf-8"))
    checksum = hashlib.md5(pathlib.Path(zip_path).read_bytes()).hexdigest()

    path = pathlib.Path(manifest_path)
    manifest = json.loads(path.read_text(encoding="utf-8")) if path.exists() else []
    guid = str(build["guid"]).lower()
    old = next((p for p in manifest if str(p.get("guid", "")).lower() == guid), None)

    plugin = {
        "guid": guid,
        "name": build["name"],
        "description": build["description"],
        "overview": build["overview"],
        "owner": build["owner"],
        "category": build["category"],
    }
    if image_url or (old and old.get("imageUrl")):
        plugin["imageUrl"] = image_url or old["imageUrl"]

    version = {
        "version": str(build["version"]),
        "changelog": build["changelog"],
        "targetAbi": build["targetAbi"],
        "sourceUrl": source_url,
        "checksum": checksum,
        "timestamp": datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
    }
    older = [v for v in (old or {}).get("versions", []) if v.get("version") != version["version"]]
    plugin["versions"] = [version, *older]

    manifest = [p for p in manifest if p is not old] + [plugin]
    path.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(f"{build['name']} {version['version']} -> {path} (md5 {checksum})")
    return 0


if __name__ == "__main__":
    sys.exit(main())
