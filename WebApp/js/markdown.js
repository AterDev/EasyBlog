const mermaid = window.mermaid;
const nomnoml = window.nomnoml;
class MarkdownHandler {
    copyContent = '&#128203;copy code';
    constructor() {
        document.addEventListener('DOMContentLoaded', () => this.init());
    }
    init() {
        this.initMermaid();
        this.initCodeCopy();
        this.initNomnoml();
    }
    initMermaid() {
        if (typeof mermaid !== 'undefined') {
            mermaid.initialize({ startOnLoad: true });
        }
    }
    initCodeCopy() {
        const languageDivs = document.querySelectorAll('div[class^="language-"]');
        languageDivs.forEach(languageDiv => this.addCopyIcon(languageDiv));
    }
    addCopyIcon(languageDiv) {
        const language = languageDiv.className.split(' ')[0].split('-')[1];
        const codeActionBar = document.createElement('div');
        codeActionBar.classList.add('code-action-bar', 'flex', 'justify-between');
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
        if (typeof nomnoml !== 'undefined') {
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
