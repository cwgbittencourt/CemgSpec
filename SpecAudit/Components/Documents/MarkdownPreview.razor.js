import { enhancePreview } from "../../lib/preview/preview.js";

const controllers = new WeakMap();
const rasterDataUri = /^data:image\/(png|jpeg|webp);base64,/;

// The HTML comes from MarkdownRenderer's allowlist. References are inert spans with an index;
// every activation is resolved by the server against the current catalog snapshot.
export function mount(container, html, references, authorizedImages, dotnet, mountId, elementId) {
    controllers.get(container)?.abort();
    const controller = new AbortController();
    controllers.set(container, controller);

    container.innerHTML = html;
    const kinds = new Map(references.map(reference => [reference.index, reference.kind]));

    for (const span of container.querySelectorAll("[data-preview-reference]")) {
        const index = Number(span.dataset.previewReference);
        const kind = kinds.get(index);
        if (kind === "DocumentLink" || kind === "ExternalLink") {
            span.setAttribute("role", "link");
            span.tabIndex = 0;
            span.classList.add("preview-link");
            if (kind === "ExternalLink") {
                span.classList.add("preview-link-external");
                span.title = "Abre em outra aba somente após o clique";
            }
        } else if (kind === "RemoteImage") {
            const cached = authorizedImages?.[String(index)];
            if (typeof cached === "string" && rasterDataUri.test(cached)) {
                const image = document.createElement("img");
                image.alt = span.textContent;
                image.src = cached;
                span.replaceWith(image);
            } else {
                remoteImage(span, index, dotnet, mountId);
            }
        } else {
            span.classList.add("preview-inert");
            span.title = kind === "LocalImage" ? "Imagem local fora do catálogo ou indisponível" : "Referência bloqueada pelo leitor";
        }
    }

    async function activate(event) {
        const link = event.target instanceof Element ? event.target.closest("[role='link'][data-preview-reference]") : null;
        if (!link || !container.contains(link)) return;
        if (event.type === "keydown" && event.key !== "Enter") return;
        event.preventDefault();
        const result = await dotnet.invokeMethodAsync("ActivateReference", mountId, Number(link.dataset.previewReference));
        if (result?.action === "external" && /^https?:\/\//i.test(result.url)) {
            window.open(result.url, "_blank", "noopener,noreferrer");
        } else if (result?.action === "anchor" && result.elementId) {
            goTo(container, result.elementId);
        }
    }
    container.addEventListener("click", activate, { signal: controller.signal });
    container.addEventListener("keydown", activate, { signal: controller.signal });

    if (elementId) goTo(container, elementId);
    enhancePreview(container)
        .catch(() => { })
        .then(() => {
            if (elementId && container.isConnected) document.getElementById(elementId)?.scrollIntoView();
        });
}

function remoteImage(span, index, dotnet, mountId) {
    const label = span.textContent;
    span.textContent = "";
    span.classList.add("preview-remote-image");
    const text = document.createElement("span");
    text.textContent = (label || "Imagem remota") + " — imagem remota não carregada";
    const button = document.createElement("button");
    button.type = "button";
    button.textContent = "Carregar nesta sessão";
    button.addEventListener("click", async () => {
        button.disabled = true;
        const uri = await dotnet.invokeMethodAsync("LoadRemoteImage", mountId, index);
        if (typeof uri === "string" && rasterDataUri.test(uri) && span.isConnected) {
            const image = document.createElement("img");
            image.alt = label;
            image.src = uri;
            span.replaceWith(image);
        } else {
            button.disabled = false;
        }
    });
    span.append(text, button);
}

export function goTo(container, elementId) {
    const element = document.getElementById(elementId);
    if (!element || !container.contains(element)) return;
    if (decodeURIComponent(location.hash.slice(1)) === elementId) {
        element.scrollIntoView();
    } else {
        location.hash = elementId;
    }
}

