export async function init() {
    console.log('[chat.js] init start');

    if (typeof window.signalR === 'undefined') {
        console.error('[chat.js] SignalR client not loaded. Check script tag order.');
        return;
    }

    const statusEl = document.getElementById('status');
    const form = document.getElementById('promptForm');
    const promptEl = document.getElementById('prompt');
    const sendBtn = document.getElementById('sendBtn');
    const stopBtn = document.getElementById('stopBtn');
    const clearBtn = document.getElementById('clearBtn');
    const copyBtn = document.getElementById('copyBtn');
    const outEl = document.getElementById('output');
    const countEl = document.getElementById('tokenCount');

    function setStatus(text, cls = 'text-bg-secondary') {
        if (!statusEl) return;
        statusEl.className = `badge ${cls}`;
        statusEl.textContent = text;
    }
    function setBusy(isBusy) {
        if (sendBtn) sendBtn.disabled = isBusy;
        if (stopBtn) stopBtn.disabled = !isBusy;
        if (promptEl) promptEl.disabled = isBusy;
    }
    function updateCharCount() {
        if (countEl && outEl) countEl.textContent = `${outEl.textContent.length} chars`;
    }

    // Build hub URL respecting any PathBase (no leading slash).
    const hubUrl = new URL('chat', document.baseURI).toString();

    let connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, { withCredentials: true })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.onreconnecting(() => setStatus('reconnecting…', 'text-bg-warning'));
    connection.onreconnected(() => setStatus('connected', 'text-bg-success'));
    connection.onclose(() => setStatus('disconnected', 'text-bg-secondary'));

    try {
        await connection.start();
        setStatus('connected', 'text-bg-success');
        console.log('[chat.js] connection started');
    } catch (err) {
        console.error('[chat.js] connection start failed:', err);
        setStatus('disconnected', 'text-bg-danger');
        return;
    }

    let subscription = null;

    async function startStream(prompt) {
        if (!prompt) return;
        outEl.textContent = '';
        updateCharCount();
        setBusy(true);

        const stream = connection.stream('StreamAnswer', prompt);
        subscription = stream.subscribe({
            next: chunk => {
                for (const ch of chunk) outEl.textContent += ch;
                updateCharCount();
                if (outEl.parentElement) {
                    outEl.parentElement.scrollTop = outEl.parentElement.scrollHeight;
                }
            },
            complete: () => setBusy(false),
            error: err => {
                setBusy(false);
                outEl.textContent += `\n\n[error] ${err?.message ?? err}`;
                updateCharCount();
            }
        });
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        await startStream(promptEl.value.trim());
    });

    stopBtn.addEventListener('click', () => {
        subscription?.dispose();
        subscription = null;
        setBusy(false);
    });

    clearBtn.addEventListener('click', () => {
        outEl.textContent = '';
        updateCharCount();
        promptEl.focus();
    });

    copyBtn.addEventListener('click', async () => {
        try {
            await navigator.clipboard.writeText(outEl.textContent);
            copyBtn.textContent = 'Copied!';
            setTimeout(() => (copyBtn.textContent = 'Copy'), 1000);
        } catch { /* ignore */ }
    });
}
