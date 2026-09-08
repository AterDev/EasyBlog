const mermaidSources = [
    'https://cdn.jsdelivr.net/npm/mermaid@10.9.0/dist/mermaid.min.js',
    'https://registry.npmmirror.com/mermaid/10.9.0/files/dist/mermaid.min.js'
];
const mermaidLoadTimeoutMilliseconds = 5000;

class MarkdownHandler {
    copyContent = '&#128203;copy code';
    mermaidMinScale = 0.2;
    mermaidMaxScale = 4;
    mermaidZoomFactor = 1.2;
    initialized = false;
    mermaidLoadPromise = null;

    constructor() {
        const initialize = () => {
            if (this.initialized) {
                return;
            }

            this.initialized = true;
            this.init();
        };

        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initialize, { once: true });
        } else {
            initialize();
        }
    }
    init() {
        this.initToc();
        this.initMermaid();
        this.initCodeCopy();
        this.initNomnoml();
    }
    initToc() {
        const tocLinks = Array.from(document.querySelectorAll(
            '.doc-toc .toc-block ul.toc a[href^="#"], .blog-detail-toc .toc-block ul.toc a[href^="#"]'));
        if (tocLinks.length === 0) {
            return;
        }

        const headingForLink = new Map();
        const headings = [];
        tocLinks.forEach(link => {
            const targetId = link.getAttribute('href')?.slice(1);
            const heading = targetId ? document.getElementById(targetId) : null;
            if (heading) {
                headingForLink.set(link, heading);
                headings.push(heading);
            }
        });

        if (headings.length === 0) {
            return;
        }

        const setActiveHeading = activeHeading => {
            tocLinks.forEach(link => {
                const tocItem = link.closest('li');
                const isActive = headingForLink.get(link) === activeHeading;
                tocItem?.classList.toggle('active', isActive);
                if (isActive) {
                    link.setAttribute('aria-current', 'location');
                }
                else {
                    link.removeAttribute('aria-current');
                }
            });
        };

        const getHash = () => {
            const hash = window.location.hash.slice(1);
            try {
                return decodeURIComponent(hash);
            }
            catch {
                return hash;
            }
        };

        const setActiveFromViewport = () => {
            const activationOffset = 96;
            let activeHeading = headings[0];
            headings.forEach(heading => {
                if (heading.getBoundingClientRect().top <= activationOffset) {
                    activeHeading = heading;
                }
            });
            setActiveHeading(activeHeading);
        };

        const setActiveFromHash = () => {
            const hash = getHash();
            const activeHeading = headings.find(heading => heading.id === hash);
            if (activeHeading) {
                setActiveHeading(activeHeading);
            }
            else {
                setActiveFromViewport();
            }
        };

        let frameId = 0;
        const scheduleViewportUpdate = () => {
            if (frameId) {
                return;
            }
            frameId = window.requestAnimationFrame(() => {
                frameId = 0;
                setActiveFromViewport();
            });
        };

        tocLinks.forEach(link => {
            link.addEventListener('click', () => {
                const heading = headingForLink.get(link);
                if (heading) {
                    setActiveHeading(heading);
                }
            });
        });

        window.addEventListener('hashchange', setActiveFromHash);
        window.addEventListener('scroll', scheduleViewportUpdate, { passive: true });
        window.addEventListener('resize', scheduleViewportUpdate);
        setActiveFromHash();
    }
    initMermaid() {
        const mermaidBlocks = document.querySelectorAll('pre.mermaid');
        mermaidBlocks.forEach(mermaidBlock => this.addMermaidViewer(mermaidBlock));
        const mermaidElements = [...document.querySelectorAll('pre.mermaid')]
            .filter(mermaidElement => !mermaidElement.hasAttribute('data-processed'));

        if (mermaidElements.length === 0) {
            return;
        }

        this.loadMermaid()
            .then(mermaid => this.renderMermaid(mermaid, mermaidElements))
            .catch(error => console.error('Failed to load Mermaid:', error));
    }
    loadMermaid() {
        if (window.mermaid) {
            return Promise.resolve(window.mermaid);
        }
        if (!this.mermaidLoadPromise) {
            this.mermaidLoadPromise = this.loadMermaidFromSources();
        }
        return this.mermaidLoadPromise;
    }
    async loadMermaidFromSources() {
        for (const source of mermaidSources) {
            try {
                await this.loadMermaidSource(source);
                if (window.mermaid) {
                    return window.mermaid;
                }

                throw new Error('The script did not expose window.mermaid.');
            } catch (error) {
                console.warn(`Failed to load Mermaid from ${source}:`, error);
            }
        }

        throw new Error('No Mermaid source could be loaded.');
    }
    loadMermaidSource(source) {
        return new Promise((resolve, reject) => {
            const script = document.createElement('script');
            let settled = false;
            let timeoutId;
            const cleanup = () => {
                window.clearTimeout(timeoutId);
                script.onload = null;
                script.onerror = null;
            };
            const fail = error => {
                if (settled) {
                    return;
                }

                settled = true;
                cleanup();
                script.remove();
                reject(error);
            };

            timeoutId = window.setTimeout(
                () => fail(new Error(`Timed out after ${mermaidLoadTimeoutMilliseconds}ms.`)),
                mermaidLoadTimeoutMilliseconds);
            script.onload = () => {
                if (settled) {
                    return;
                }

                settled = true;
                cleanup();
                resolve();
            };
            script.onerror = () => fail(new Error('The script request failed.'));
            script.async = true;
            script.src = source;
            document.head.appendChild(script);
        });
    }
    renderMermaid(mermaid, mermaidElements) {
        if (!mermaid || typeof mermaid.initialize !== 'function') {
            throw new Error('The loaded Mermaid script has an unsupported API.');
        }

        mermaid.initialize({ startOnLoad: false });
        const render = typeof mermaid.run === 'function'
            ? mermaid.run({ nodes: Array.from(mermaidElements) })
            : typeof mermaid.init === 'function'
                ? mermaid.init(undefined, mermaidElements)
                : null;
        if (render && typeof render.catch === 'function') {
            render.catch(error => console.error('Failed to render Mermaid chart:', error));
        }
    }
    addMermaidViewer(mermaidBlock) {
        if (mermaidBlock.closest('[data-mermaid-viewer]')) {
            return;
        }

        const parent = mermaidBlock.parentNode;
        if (!parent) {
            return;
        }

        const viewer = document.createElement('div');
        viewer.className = 'mermaid-viewer';
        viewer.dataset.mermaidViewer = 'true';

        const toolbar = document.createElement('div');
        toolbar.className = 'mermaid-action-bar';
        toolbar.setAttribute('role', 'toolbar');
        toolbar.setAttribute('aria-label', 'Mermaid chart controls');

        const panButton = this.createMermaidButton(
            'pan',
            '&#9995;',
            'Drag chart',
            'Drag chart with the left mouse button');
        panButton.setAttribute('aria-pressed', 'true');
        const zoomOutButton = this.createMermaidButton(
            'zoom-out',
            '&#8722;',
            'Zoom out',
            'Zoom out');
        const zoomInButton = this.createMermaidButton(
            'zoom-in',
            '&#43;',
            'Zoom in',
            'Zoom in');
        const previewButton = this.createMermaidButton(
            'preview',
            '&#9974;',
            'Preview chart',
            'Open chart preview');
        toolbar.append(panButton, zoomOutButton, zoomInButton, previewButton);

        const viewport = document.createElement('div');
        viewport.className = 'mermaid-viewport';
        viewport.tabIndex = 0;
        viewport.setAttribute('role', 'img');
        viewport.setAttribute(
            'aria-label',
            'Mermaid chart. Drag with the left mouse button to move it, or use the mouse wheel to zoom.');

        const stage = document.createElement('div');
        stage.className = 'mermaid-stage';
        parent.insertBefore(viewer, mermaidBlock);
        stage.appendChild(mermaidBlock);
        viewport.appendChild(stage);
        viewer.append(toolbar, viewport);

        const state = {
            block: mermaidBlock,
            viewport,
            stage,
            scale: 1,
            x: 0,
            y: 0,
            initialized: false,
            hasInteracted: false,
            contentWidth: 0,
            contentHeight: 0,
            panEnabled: true,
            dragging: false,
            pointerId: null,
            startPointerX: 0,
            startPointerY: 0,
            startX: 0,
            startY: 0,
            layoutFrame: 0
        };
        viewer.mermaidState = state;

        const updateLayout = () => {
            if (state.layoutFrame) {
                return;
            }
            state.layoutFrame = requestAnimationFrame(() => {
                state.layoutFrame = 0;
                if (!state.initialized || !state.hasInteracted) {
                    this.fitMermaidViewer(viewer);
                } else {
                    this.applyMermaidTransform(viewer);
                }
            });
        };

        panButton.addEventListener('click', () => {
            state.panEnabled = !state.panEnabled;
            panButton.setAttribute('aria-pressed', String(state.panEnabled));
            viewer.classList.toggle('mermaid-pan-disabled', !state.panEnabled);
        });
        zoomOutButton.addEventListener('click', () => {
            this.zoomMermaidViewer(viewer, 1 / this.mermaidZoomFactor);
        });
        zoomInButton.addEventListener('click', () => {
            this.zoomMermaidViewer(viewer, this.mermaidZoomFactor);
        });
        previewButton.addEventListener('click', () => {
            this.openMermaidPreview(viewer, previewButton);
        });

        viewport.addEventListener('wheel', event => {
            event.preventDefault();
            const rect = viewport.getBoundingClientRect();
            const factor = event.deltaY < 0
                ? this.mermaidZoomFactor
                : 1 / this.mermaidZoomFactor;
            this.zoomMermaidViewer(
                viewer,
                factor,
                event.clientX - rect.left,
                event.clientY - rect.top);
        }, { passive: false });

        viewport.addEventListener('pointerdown', event => {
            if (!state.panEnabled || event.button !== 0) {
                return;
            }

            state.dragging = true;
            state.pointerId = event.pointerId;
            state.startPointerX = event.clientX;
            state.startPointerY = event.clientY;
            state.startX = state.x;
            state.startY = state.y;
            viewport.classList.add('is-dragging');
            viewport.setPointerCapture(event.pointerId);
        });

        viewport.addEventListener('pointermove', event => {
            if (!state.dragging || event.pointerId !== state.pointerId) {
                return;
            }

            event.preventDefault();
            state.hasInteracted = true;
            state.x = state.startX + event.clientX - state.startPointerX;
            state.y = state.startY + event.clientY - state.startPointerY;
            this.applyMermaidTransform(viewer);
        });

        viewport.addEventListener('pointerup', event => {
            this.finishMermaidDrag(viewer, event.pointerId);
        });
        viewport.addEventListener('pointercancel', event => {
            this.finishMermaidDrag(viewer, event.pointerId);
        });
        viewport.addEventListener('lostpointercapture', event => {
            this.finishMermaidDrag(viewer, event.pointerId);
        });

        const mutationObserver = new MutationObserver(updateLayout);
        mutationObserver.observe(mermaidBlock, { childList: true });
        state.mutationObserver = mutationObserver;

        if (typeof ResizeObserver !== 'undefined') {
            const resizeObserver = new ResizeObserver(updateLayout);
            resizeObserver.observe(viewport);
            state.resizeObserver = resizeObserver;
        }

        updateLayout();
    }
    createMermaidButton(action, content, label, title) {
        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'mermaid-control';
        button.dataset.mermaidAction = action;
        button.innerHTML = content;
        button.setAttribute('aria-label', label);
        button.title = title;
        return button;
    }
    getMermaidSize(viewer) {
        const state = viewer.mermaidState;
        const svg = state?.block.querySelector('svg');
        if (!state || !svg) {
            return null;
        }

        const viewBox = (svg.getAttribute('viewBox') || '')
            .trim()
            .split(/[\s,]+/)
            .map(Number);
        let width = viewBox.length === 4 && Number.isFinite(viewBox[2]) ? viewBox[2] : 0;
        let height = viewBox.length === 4 && Number.isFinite(viewBox[3]) ? viewBox[3] : 0;

        if (!(width > 0 && height > 0) && state.contentWidth > 0 && state.contentHeight > 0) {
            width = state.contentWidth;
            height = state.contentHeight;
        }

        if (!(width > 0 && height > 0)) {
            width = Number.parseFloat(svg.getAttribute('width'));
            height = Number.parseFloat(svg.getAttribute('height'));
        }

        if (!(width > 0 && height > 0)) {
            const svgRect = svg.getBoundingClientRect();
            const scale = state.scale || 1;
            width = svgRect.width / scale;
            height = svgRect.height / scale;
        }

        if (!(width > 0 && height > 0)) {
            return null;
        }

        state.contentWidth = width;
        state.contentHeight = height;
        return { width, height };
    }
    fitMermaidViewer(viewer) {
        const state = viewer.mermaidState;
        const size = this.getMermaidSize(viewer);
        if (!state || !size || state.viewport.clientWidth === 0 || state.viewport.clientHeight === 0) {
            return;
        }

        const padding = 32;
        const fitScale = Math.min(
            1,
            (state.viewport.clientWidth - padding) / size.width,
            (state.viewport.clientHeight - padding) / size.height);
        state.scale = Math.max(this.mermaidMinScale, Math.min(this.mermaidMaxScale, fitScale));
        state.x = (state.viewport.clientWidth - size.width * state.scale) / 2;
        state.y = (state.viewport.clientHeight - size.height * state.scale) / 2;
        state.initialized = true;
        this.applyMermaidTransform(viewer);
    }
    applyMermaidTransform(viewer) {
        const state = viewer.mermaidState;
        const size = this.getMermaidSize(viewer);
        if (!state || !size) {
            return;
        }

        const width = Math.max(size.width * state.scale, 1);
        const height = Math.max(size.height * state.scale, 1);
        const svg = state.block.querySelector('svg');

        // Keep the SVG as the rendering surface. Scaling a transformed parent
        // can make Chromium cache the chart as a bitmap; changing the SVG's
        // actual viewport size keeps text and paths sharp at every zoom level.
        state.stage.style.transform = 'none';
        state.stage.style.left = `${state.x}px`;
        state.stage.style.top = `${state.y}px`;
        state.stage.style.width = `${width}px`;
        state.stage.style.height = `${height}px`;

        if (svg) {
            svg.setAttribute('width', `${width}`);
            svg.setAttribute('height', `${height}`);
            svg.style.width = `${width}px`;
            svg.style.height = `${height}px`;
            svg.style.maxWidth = 'none';
        }
    }
    zoomMermaidViewer(viewer, factor, centerX, centerY) {
        const state = viewer.mermaidState;
        if (!state) {
            return;
        }

        if (!state.initialized) {
            this.fitMermaidViewer(viewer);
        }

        const oldScale = state.scale;
        const newScale = Math.max(
            this.mermaidMinScale,
            Math.min(this.mermaidMaxScale, oldScale * factor));
        if (newScale === oldScale) {
            return;
        }

        const x = centerX ?? state.viewport.clientWidth / 2;
        const y = centerY ?? state.viewport.clientHeight / 2;
        const contentX = (x - state.x) / oldScale;
        const contentY = (y - state.y) / oldScale;
        state.scale = newScale;
        state.x = x - contentX * newScale;
        state.y = y - contentY * newScale;
        state.initialized = true;
        state.hasInteracted = true;
        this.applyMermaidTransform(viewer);
    }
    finishMermaidDrag(viewer, pointerId) {
        const state = viewer.mermaidState;
        if (!state || !state.dragging || state.pointerId !== pointerId) {
            return;
        }

        state.dragging = false;
        state.pointerId = null;
        state.viewport.classList.remove('is-dragging');
        if (state.viewport.hasPointerCapture(pointerId)) {
            state.viewport.releasePointerCapture(pointerId);
        }
    }
    openMermaidPreview(viewer, sourceButton) {
        if (document.querySelector('.mermaid-preview')) {
            return;
        }

        const parent = viewer.parentNode;
        if (!parent) {
            return;
        }

        const placeholder = document.createComment('Mermaid preview placeholder');
        parent.insertBefore(placeholder, viewer);

        const modal = document.createElement('div');
        modal.className = 'mermaid-preview';
        modal.setAttribute('role', 'dialog');
        modal.setAttribute('aria-modal', 'true');
        modal.setAttribute('aria-label', 'Mermaid chart preview');

        const panel = document.createElement('div');
        panel.className = 'mermaid-preview-panel';
        const closeButton = this.createMermaidButton(
            'close-preview',
            '&#10005;',
            'Close preview',
            'Close preview');
        closeButton.classList.add('mermaid-preview-close');

        const previewBody = document.createElement('div');
        previewBody.className = 'mermaid-preview-body';
        panel.append(closeButton, previewBody);
        modal.appendChild(panel);
        document.body.appendChild(modal);
        previewBody.appendChild(viewer);
        document.body.classList.add('mermaid-preview-open');

        const previousFocus = sourceButton;
        const close = () => {
            if (!modal.isConnected) {
                return;
            }

            if (placeholder.parentNode) {
                placeholder.parentNode.insertBefore(viewer, placeholder.nextSibling);
                placeholder.remove();
            }
            modal.remove();
            document.body.classList.remove('mermaid-preview-open');
            document.removeEventListener('keydown', onKeyDown);
            if (previousFocus && typeof previousFocus.focus === 'function') {
                previousFocus.focus();
            }
        };
        const onKeyDown = event => {
            if (event.key === 'Escape') {
                close();
            }
        };

        closeButton.addEventListener('click', close);
        modal.addEventListener('click', event => {
            if (event.target === modal) {
                close();
            }
        });
        document.addEventListener('keydown', onKeyDown);

        requestAnimationFrame(() => {
            modal.classList.add('is-open');
            if (!viewer.mermaidState?.hasInteracted) {
                this.fitMermaidViewer(viewer);
            } else {
                this.applyMermaidTransform(viewer);
            }
            closeButton.focus();
        });
    }
    initCodeCopy() {
        const languageDivs = document.querySelectorAll('div[class^="language-"]');
        languageDivs.forEach(languageDiv => this.addCopyIcon(languageDiv));
    }
    addCopyIcon(languageDiv) {
        const language = languageDiv.className.split(' ')[0].split('-')[1];
        const codeActionBar = document.createElement('div');
        codeActionBar.classList.add('code-action-bar');
        codeActionBar.innerHTML = `<span>${language}</span><span class="copy-icon">${this.copyContent}</span>`;
        languageDiv.parentNode.insertBefore(codeActionBar, languageDiv);
        const copyIcon = codeActionBar.querySelector('.copy-icon');
        copyIcon.addEventListener('click', () => this.copyCode(languageDiv, copyIcon));
    }
    copyCode(languageDiv, copyIcon) {
        const textToCopy = languageDiv.querySelector('pre').innerText;
        navigator.clipboard.writeText(textToCopy)
            .then(() => {
            copyIcon.innerHTML = '&#10003 copied!';
            setTimeout(() => {
                copyIcon.innerHTML = this.copyContent;
            }, 1000);
        })
            .catch(err => {
            console.error('Failed to copy: ', err);
        });
    }
    initNomnoml() {
        const nomnoml = window.nomnoml;
        if (nomnoml && typeof nomnoml.draw === 'function') {
            var nomnomlDivs = document.querySelectorAll('.nomnoml');
            if (nomnomlDivs.length > 0) {
                const nomnomlDiv = nomnomlDivs[0];
                const content = nomnomlDiv.textContent;
                nomnomlDiv.innerHTML = '';
                const canvas = document.createElement('canvas');
                nomnoml.draw(canvas, content);
                nomnomlDiv.appendChild(canvas);
            }
        }
    }
}
new MarkdownHandler();
