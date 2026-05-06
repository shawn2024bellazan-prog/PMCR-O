/**
 * PMCRO Documentation — templates/pmcro/public/main.js
 * Author: Shawn Bellazan · ThoughtLock: 2026-05-05
 *
 * DocFX modern template entry point.
 * Exports a default config object consumed by the modern template runtime.
 *
 * Docs: https://dotnet.github.io/docfx/docs/template.html
 */

export default {
    /** Dark mode by default — the PMCRO substrate lives in the dark. */
    defaultTheme: 'dark',

    /** Navbar icon links */
    iconLinks: [
        {
            icon: 'github',
            href: 'https://github.com/tooensure/projectname',
            title: 'Source on GitHub'
        }
    ],

    /**
     * Startup: inject TTS-mode toggle button into the page.
     * The button appends a class to <body> which main.css uses to widen
     * line-height and letter-spacing for listening comfort.
     */
    start: () => {
        injectTtsButton();
        restoreTtsMode();
    }
};

/* ─────────────────────────────────────────────────
   TTS MODE — listening-first reading toggle
   Adds a small floating button that widens line-height,
   increases font size, and strips inline code decoration
   so screen readers and TTS apps scan cleanly.
───────────────────────────────────────────────── */

const TTS_KEY = 'pmcro-tts-mode';
const TTS_CLASS = 'tts-mode';

function injectTtsButton() {
    const btn = document.createElement('button');
    btn.id = 'tts-toggle';
    btn.title = 'Toggle listening / reading mode';
    btn.setAttribute('aria-label', 'Toggle TTS listening mode');

    // Inline base styles — overridden by main.css on body.tts-mode
    Object.assign(btn.style, {
        position: 'fixed',
        bottom: '1.5rem',
        right: '1.5rem',
        zIndex: '9999',
        background: 'var(--pmcro-surface, #161b22)',
        border: '1px solid var(--pmcro-border, #21262d)',
        borderRadius: '8px',
        color: 'var(--pmcro-text-dim, #6e7681)',
        cursor: 'pointer',
        fontFamily: 'var(--pmcro-font-display, sans-serif)',
        fontSize: '0.75rem',
        fontWeight: '600',
        letterSpacing: '0.04em',
        padding: '0.4rem 0.75rem',
        transition: 'border-color 0.15s, color 0.15s',
        display: 'flex',
        alignItems: 'center',
        gap: '0.4rem',
        lineHeight: '1',
    });

    btn.innerHTML = `
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
      <path d="M9 18V5l12-2v13"/>
      <circle cx="6" cy="18" r="3"/>
      <circle cx="18" cy="16" r="3"/>
    </svg>
    <span id="tts-label">LISTEN</span>
  `;

    btn.addEventListener('mouseenter', () => {
        btn.style.borderColor = 'var(--pmcro-cyan, #39d0d8)';
        btn.style.color = 'var(--pmcro-cyan, #39d0d8)';
    });
    btn.addEventListener('mouseleave', () => {
        const active = document.body.classList.contains(TTS_CLASS);
        btn.style.borderColor = active ? 'var(--pmcro-cyan, #39d0d8)' : 'var(--pmcro-border, #21262d)';
        btn.style.color = active ? 'var(--pmcro-cyan, #39d0d8)' : 'var(--pmcro-text-dim, #6e7681)';
    });

    btn.addEventListener('click', toggleTtsMode);
    document.body.appendChild(btn);
}

function toggleTtsMode() {
    const active = document.body.classList.toggle(TTS_CLASS);
    localStorage.setItem(TTS_KEY, active ? '1' : '0');
    updateTtsButtonState(active);
}

function restoreTtsMode() {
    const saved = localStorage.getItem(TTS_KEY);
    if (saved === '1') {
        document.body.classList.add(TTS_CLASS);
        // Button may not be injected yet on first paint; use rAF
        requestAnimationFrame(() => updateTtsButtonState(true));
    }
}

function updateTtsButtonState(active) {
    const btn = document.getElementById('tts-toggle');
    const label = document.getElementById('tts-label');
    if (!btn || !label) return;

    label.textContent = active ? 'READING' : 'LISTEN';
    btn.style.borderColor = active ? 'var(--pmcro-cyan, #39d0d8)' : 'var(--pmcro-border, #21262d)';
    btn.style.color = active ? 'var(--pmcro-cyan, #39d0d8)' : 'var(--pmcro-text-dim, #6e7681)';
    btn.setAttribute('aria-pressed', active ? 'true' : 'false');
}