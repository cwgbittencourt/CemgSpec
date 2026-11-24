import { build } from "esbuild";
import { createHash } from "node:crypto";
import { mkdir, readFile, readdir, writeFile } from "node:fs/promises";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const root = dirname(fileURLToPath(import.meta.url));
const vendor = resolve(root, "../../CemgSpec/wwwroot/lib/preview/vendor");
const lockBytes = await readFile(join(root, "package-lock.json"));
const lock = JSON.parse(lockBytes);
const packagePaths = Object.keys(lock.packages).filter(Boolean).sort((a, b) => b.length - a.length);
const shared = {
    absWorkingDir: root,
    bundle: true,
    minify: true,
    legalComments: "eof",
    metafile: true,
    target: "es2022",
    platform: "browser"
};

await mkdir(vendor, { recursive: true });
const entries = [
    {
        name: "mermaid.js",
        source: 'export { default } from "mermaid";',
        format: "iife",
        globalName: "CemgSpecMermaid"
    },
    {
        name: "highlight.js",
        source: 'export { default } from "./node_modules/highlight.js/lib/common.js";',
        format: "esm"
    }
];

const bundles = {};
const shipped = new Set();
const sha256 = data => createHash("sha256").update(data).digest("hex");
for (const { name, source, ...options } of entries) {
    const outfile = join(vendor, name);
    const result = await build({
        ...shared,
        ...options,
        stdin: { contents: source, resolveDir: root },
        outfile
    });

    const inputs = Object.values(result.metafile.outputs).flatMap(output => Object.entries(output.inputs));
    const dependencies = new Set();
    for (const [input, detail] of inputs) {
        if (detail.bytesInOutput === 0 || !input.startsWith("node_modules/")) continue;
        const dependency = packagePaths.find(path => input.startsWith(path + "/"));
        if (!dependency || !lock.packages[dependency]) throw new Error("Missing lock entry: " + input);
        dependencies.add(dependency);
        shipped.add(dependency);
    }

    const bytes = await readFile(outfile);
    bundles[name] = { bytes: bytes.length, sha256: sha256(bytes), packages: [...dependencies].sort() };
}

const licenses = [
    "Third-party licenses for the CemgSpec preview bundles.",
    "DOMPurify is distributed under Apache-2.0.",
    "ELK is distributed under EPL-2.0. Corresponding source for elkjs 0.12.1:",
    "https://github.com/kieler/elkjs/tree/624ae90aa37110033a8af59c5ccbb292708b4317"
];
const packages = {};
for (const path of [...shipped].sort()) {
    const installed = join(root, path);
    const metadata = JSON.parse(await readFile(join(installed, "package.json"), "utf8"));
    const { version, integrity, resolved } = lock.packages[path];
    const license = metadata.license ?? (metadata.name === "khroma" && version === "2.1.0" ? "MIT" : null);
    if (metadata.version !== version || !license || !integrity)
        throw new Error("Unverified package metadata: " + path);

    const licenseFiles = (await readdir(installed))
        .filter(name => /^(?:licen[cs]e|copying|notice)(?:[.-].+)?$/i.test(name))
        .sort();
    if (licenseFiles.length === 0) throw new Error("Missing license text: " + path);
    packages[path] = { name: metadata.name, version, license, integrity, resolved };
    licenses.push("");
    licenses.push("======== " + metadata.name + " " + version + " (" + license + ") ========");
    for (const file of licenseFiles) {
        licenses.push("[" + file + "]");
        licenses.push((await readFile(join(installed, file), "utf8")).trimEnd());
    }
}

const notices = licenses.join("\n") + "\n";
await writeFile(join(vendor, "THIRD-PARTY-NOTICES.txt"), notices);
await writeFile(join(vendor, "manifest.json"), JSON.stringify({
    lockSha256: sha256(lockBytes),
    noticesSha256: sha256(notices),
    bundles,
    packages
}, null, 2) + "\n");
