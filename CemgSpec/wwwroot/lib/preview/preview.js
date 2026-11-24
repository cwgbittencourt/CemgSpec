import highlight from "./vendor/highlight.js";

const diagramFormats = new Map([["mermaid", "Mermaid"], ["plantuml", "PlantUML"], ["puml", "PlantUML"],
    ["dot", "Graphviz"], ["graphviz", "Graphviz"]]);
const mermaidUrl = new URL("./vendor/mermaid.js", import.meta.url);
let mermaidBundle;

function loadMermaid() {
    mermaidBundle ??= fetch(mermaidUrl, { credentials: "same-origin", redirect: "error" })
        .then(response => {
            if (!response.ok) throw new Error("Mermaid indisponível.");
            return response.text();
        })
        .catch(error => {
            mermaidBundle = undefined;
            throw error;
        });
    return mermaidBundle;
}

export async function enhancePreview(root) {
    const diagrams = [];
    for (const code of root.querySelectorAll("pre > code[class^='language-']")) {
        if (code.closest(".preview-diagram")) continue;
        const language = code.className.slice("language-".length);
        const format = diagramFormats.get(language);
        if (format) {
            const diagram = createDiagram(code.parentElement, format);
            if (format !== "Mermaid") {
                diagram.status.textContent = format + " não suportado. Veja a fonte abaixo.";
            } else {
                diagrams.push(diagram);
            }
        } else if (highlight.getLanguage(language)) {
            const source = code.textContent;
            code.innerHTML = highlight.highlight(source, { language, ignoreIllegals: true }).value;
            code.classList.add("hljs");
        }
    }

    // Each diagram uses a separate opaque frame. Bound the number mounted for one document.
    const limit = 12;
    await Promise.all(diagrams.map((diagram, index) => index < limit
        ? renderMermaid(diagram)
        : showSource(diagram, "Limite de diagramas do preview. Veja a fonte abaixo.")));
}

function createDiagram(source, format) {
    const region = document.createElement("section");
    region.className = "preview-diagram";
    region.setAttribute("aria-label", format);
    const status = document.createElement("p");
    status.setAttribute("role", "status");
    source.before(region);
    region.append(status, source);
    return { region, status, source };
}

function showSource(diagram, message) {
    diagram.status.textContent = message;
    diagram.source.hidden = false;
}

async function renderMermaid(diagram) {
    const text = diagram.source.textContent;
    diagram.status.textContent = "Renderizando Mermaid…";
    if (text.length > 20_000 || text.trimStart().startsWith("---") || /%%\s*\{/.test(text)) {
        showSource(diagram, "Mermaid inválido ou com configuração embutida. Veja a fonte abaixo.");
        return;
    }

    let frame;
    try {
        const bundle = await loadMermaid();
        if (!diagram.region.isConnected) return;
        const nonce = crypto.randomUUID().replaceAll("-", "");
        frame = document.createElement("iframe");
        frame.title = "Diagrama Mermaid";
        frame.setAttribute("sandbox", "allow-scripts");
        frame.referrerPolicy = "no-referrer";
        frame.className = "preview-diagram-frame";
        const canvas = document.createElement("div");
        const viewport = document.createElement("div");
        viewport.className = "preview-diagram-viewport";
        viewport.append(canvas);
        canvas.append(frame);
        const drawing = waitForDrawing(frame, text);
        const script = (bundle + "\n(" + runMermaidFrame.toString() + ")();").replace(/<\/script/gi, "<\\/script");
        const policy = "default-src 'none'; base-uri 'none'; form-action 'none'; style-src 'unsafe-inline'; script-src 'nonce-" + nonce + "'";
        frame.srcdoc = '<!doctype html><html lang="pt-BR"><head><meta charset="utf-8">'
            + '<meta http-equiv="Content-Security-Policy" content="' + policy + '">'
            + '<style>body{margin:0} #drawing{display:inline-block}</style></head>'
            + '<body><div id="drawing"></div><script nonce="' + nonce + '">' + script + '</script></body></html>';
        diagram.source.before(viewport);
        const { width, height } = await drawing;

        diagram.region.setAttribute("aria-label", "Diagrama Mermaid");
        diagram.status.hidden = true;
        diagram.source.hidden = true;
        frame.style.width = width + "px";
        frame.style.height = height + "px";

        let scale = 1;
        function resize() {
            frame.style.transform = "scale(" + scale + ")";
            canvas.style.width = Math.ceil(width * scale) + "px";
            canvas.style.height = Math.ceil(height * scale) + "px";
        }
        const controls = document.createElement("div");
        controls.className = "preview-diagram-controls";
        controls.append(
            button("Diminuir zoom", () => { scale = Math.max(.5, scale - .25); resize(); }),
            button("Aumentar zoom", () => { scale = Math.min(3, scale + .25); resize(); }),
            button("Ver fonte", event => {
                diagram.source.hidden = !diagram.source.hidden;
                event.currentTarget.setAttribute("aria-expanded", String(!diagram.source.hidden));
            }, { "aria-expanded": "false" })
        );
        viewport.before(controls);
        resize();
    } catch {
        frame?.closest(".preview-diagram-viewport")?.remove();
        diagram.region.setAttribute("aria-label", "Mermaid inválido");
        showSource(diagram, "Mermaid inválido ou indisponível. Veja a fonte abaixo.");
    }
}

function button(label, onClick, attributes = {}) {
    const control = document.createElement("button");
    control.type = "button";
    control.textContent = label;
    control.addEventListener("click", onClick);
    for (const [name, value] of Object.entries(attributes)) control.setAttribute(name, value);
    return control;
}

function waitForDrawing(frame, source) {
    return new Promise((resolve, reject) => {
        function cleanUp() {
            window.clearTimeout(timeout);
            window.removeEventListener("message", receive);
            frame.removeEventListener("load", send);
        }
        function send() {
            frame.contentWindow?.postMessage({ type: "cemgspec-mermaid-input", source }, "*");
        }
        function receive(event) {
            if (event.source !== frame.contentWindow || event.origin !== "null"
                || event.data?.type !== "cemgspec-mermaid-result") return;
            cleanUp();
            const { width, height } = event.data;
            if (event.data.ok && Number.isFinite(width) && Number.isFinite(height) &&
                width > 0 && height > 0 && width <= 8192 && height <= 8192) {
                resolve({ width, height });
            } else {
                reject(new Error("Diagrama inválido."));
            }
        }
        const timeout = window.setTimeout(() => {
            cleanUp();
            reject(new Error("Tempo de renderização esgotado."));
        }, 15_000);
        window.addEventListener("message", receive);
        frame.addEventListener("load", send, { once: true });
    });
}

// Serialized into the opaque frame together with the locked Mermaid bundle.
// Only the status and drawing dimensions are sent back. The SVG stays inside the frame.
function runMermaidFrame() {
    window.addEventListener("message", async event => {
        if (event.source !== parent || event.data?.type !== "cemgspec-mermaid-input"
            || typeof event.data.source !== "string") return;

        const mermaid = CemgSpecMermaid.default;
        const drawing = document.getElementById("drawing");
        try {
            mermaid.initialize({
                startOnLoad: false,
                securityLevel: "strict",
                htmlLabels: false,
                maxTextSize: 20000,
                maxEdges: 500,
                suppressErrorRendering: true
            });
            const result = await mermaid.render("cemgspec-diagram", event.data.source);
            const svgNamespace = "http://www.w3.org/2000/svg";
            const template = document.createElement("template");
            template.innerHTML = result.svg;
            const svg = template.content.firstElementChild;
            if (svg?.namespaceURI !== svgNamespace || svg.localName !== "svg") throw new Error("Invalid SVG.");

            // Mermaid can emit SVG links even in strict mode. Keep the shape, without navigation.
            for (const link of svg.querySelectorAll("a")) link.replaceWith(...link.childNodes);
            for (const element of svg.querySelectorAll(
                "script, foreignObject, image, feImage, animate, animateMotion, animateTransform, set, discard"
            )) element.remove();
            for (const element of [svg, ...svg.querySelectorAll("*")]) {
                if (element.namespaceURI !== svgNamespace) {
                    element.remove();
                    continue;
                }
                for (const attribute of [...element.attributes]) {
                    if (attribute.localName.startsWith("on") ||
                        (attribute.localName === "href" && !attribute.value.startsWith("#"))) {
                        element.removeAttributeNode(attribute);
                    }
                }
            }
            drawing.replaceChildren(svg);
            const box = drawing.getBoundingClientRect();
            parent.postMessage({
                type: "cemgspec-mermaid-result",
                ok: true,
                width: Math.ceil(box.width),
                height: Math.ceil(box.height)
            }, "*");
        } catch {
            drawing.replaceChildren();
            parent.postMessage({ type: "cemgspec-mermaid-result", ok: false }, "*");
        }
    }, { once: true });
}
