/**
 * Neurix AI Chat — Enterprise Dual-Path Architecture
 * TEXT  → fetch() POST /api/ai/chat → Gemini REST API
 * VOICE → WebSocket /ws/chat → Gemini Live API (AUDIO only)
 */
document.addEventListener('DOMContentLoaded', () => {
    const fab          = document.getElementById('ai-chat-fab');
    const chatWindow   = document.getElementById('ai-chat-window');
    const closeBtn     = document.getElementById('ai-chat-close');
    const reconnectBtn = document.getElementById('ai-chat-reconnect');
    const inputField   = document.getElementById('ai-chat-input');
    const sendBtn      = document.getElementById('ai-chat-send');
    const micBtn       = document.getElementById('ai-chat-mic');
    const micPulse     = document.getElementById('ai-mic-pulse');
    const messagesArea = document.getElementById('ai-chat-messages');
    const statusDot    = document.getElementById('ai-status-dot');
    const statusLabel  = document.getElementById('ai-status-label');
    const statusRing   = document.getElementById('ai-status-ring');
    const connBanner   = document.getElementById('ai-connection-banner');

    if (!fab || !chatWindow || !messagesArea) return;

    // ── State ──
    let isOpen = false;
    let chatHistory = []; // In-memory context: [{role,text}]
    let isTextLoading = false;

    // Voice state
    let voiceWs = null;
    let isRecording = false;
    let audioCtx = null;
    let mediaStream = null;
    let processor = null;
    let playbackCtx = null;
    let thinkingEl = null;
    let nextAudioPlayTime = 0;
    let currentAiStreamTextNode = null;
    let vadAnalyser = null;
    let vadData = null;
    let vadSilenceTimer = null;
    let isSpeaking = false;
    let rafId = null;
    let micPulseTl = null;
    let thinkingTween = null;

    const gsapOk = () => typeof gsap !== 'undefined';

    const getLocalizedError = (msg) => {
        const lang = document.documentElement.getAttribute('lang') || 'en';
        const isAr = lang === 'ar';
        
        const msgStr = String(msg || '').toLowerCase();
        if (msgStr.includes("microphone")) {
            return msg;
        }

        return isAr ? 
            "المساعد الذكي يخضع للصيانة حالياً. يرجى المحاولة بعد قليل." : 
            "Our AI Assistant is currently undergoing maintenance. Please try again in a few moments.";
    };

    // ── Connection State ──
    // "idle" means "not currently busy", not "we verified Gemini is reachable" — only an
    // actual successful reply proves that. It is deliberately labelled "Ready", not
    // "Online", so opening the widget never claims connectivity nobody has checked.
    const setState = (state, detail) => {
        const isAr = (document.documentElement.getAttribute('lang') || 'en') === 'ar';
        const cfgs = {
            idle:        { dot: 'bg-cyan',       lbl: isAr ? 'جاهز' : 'Ready',                cls: 'text-cyan-600 dark:text-cyan-400' },
            connecting:  { dot: 'bg-amber-400',  lbl: isAr ? 'جارٍ الاتصال...' : 'Connecting...', cls: 'text-amber-600 dark:text-amber-400' },
            listening:   { dot: 'bg-red-500',    lbl: isAr ? 'يستمع...' : 'Listening...',      cls: 'text-red-500 dark:text-red-400' },
            processing:  { dot: 'bg-cyan',       lbl: isAr ? 'جارٍ المعالجة...' : 'Processing...', cls: 'text-cyan-600 dark:text-cyan-400' },
            error:       { dot: 'bg-red-500',    lbl: isAr ? 'خطأ' : 'Error',                  cls: 'text-red-500 dark:text-red-400' },
            unavailable: { dot: 'bg-slate-400',  lbl: isAr ? 'غير متاح' : 'Unavailable',       cls: 'text-slate-500 dark:text-slate-400' }
        };
        const c = cfgs[state] || cfgs.idle;
        if (statusDot) {
            statusDot.className = `w-1.5 h-1.5 rounded-full transition-colors duration-300 ${c.dot}`;
            if (state === 'connecting' || state === 'listening') statusDot.classList.add('animate-pulse');
        }
        if (statusLabel) { statusLabel.textContent = c.lbl; statusLabel.className = `transition-colors duration-300 ${c.cls}`; }
        if (connBanner) {
            if (state === 'error' && detail) {
                const cleanDetail = getLocalizedError(detail);
                connBanner.className = 'mb-3 px-3 py-2 rounded-xl text-xs text-center font-medium border bg-amber-50 dark:bg-amber-900/20 border-amber-200 dark:border-amber-700/30 text-amber-700 dark:text-amber-400';
                connBanner.textContent = cleanDetail; connBanner.classList.remove('hidden');
            } else if (state === 'unavailable') {
                connBanner.className = 'mb-3 px-3 py-2 rounded-xl text-xs text-center font-medium border bg-slate-50 dark:bg-slate-800/40 border-slate-200 dark:border-slate-700/30 text-slate-500 dark:text-slate-400';
                connBanner.textContent = isAr ? 'المساعد الذكي غير مُهيأ حالياً.' : 'The AI Assistant is not configured right now.';
                connBanner.classList.remove('hidden');
            } else { connBanner.classList.add('hidden'); }
        }
        if (reconnectBtn) reconnectBtn.classList.toggle('hidden', state !== 'error');
        if (sendBtn) sendBtn.disabled = state === 'unavailable';
        if (micBtn) micBtn.disabled = state === 'unavailable';
    };

    // Cheap local check (no Gemini call) — rules out "not configured" before the status
    // dot claims anything. Never polled repeatedly; only runs once per widget open.
    const checkAvailability = async () => {
        setState('connecting');
        try {
            const resp = await fetch('/api/ai/status');
            const data = resp.ok ? await resp.json() : { available: false };
            setState(data.available ? 'idle' : 'unavailable');
        } catch (e) {
            setState('unavailable');
        }
    };

    // ── UI Toggle ──
    const toggleChat = () => {
        isOpen = !isOpen;
        if (isOpen) {
            chatWindow.classList.remove('hidden');
            setTimeout(() => { chatWindow.classList.remove('scale-95', 'opacity-0'); chatWindow.classList.add('scale-100', 'opacity-100'); }, 10);
            if (inputField) inputField.focus();
            if (typeof lucide !== 'undefined') lucide.createIcons();
            checkAvailability();
        } else {
            chatWindow.classList.remove('scale-100', 'opacity-100');
            chatWindow.classList.add('scale-95', 'opacity-0');
            setTimeout(() => chatWindow.classList.add('hidden'), 300);
        }
    };
    fab.addEventListener('click', toggleChat);
    if (closeBtn) closeBtn.addEventListener('click', toggleChat);

    // ═══════════════════════════════════════════════
    //  TEXT FLOW — fetch() POST /api/ai/chat
    // ═══════════════════════════════════════════════
    const handleSend = async () => {
        const text = (inputField?.value || '').trim();
        if (!text || isTextLoading) return;

        appendMessage('User', text);
        chatHistory.push({ role: 'user', text });
        inputField.value = '';
        isTextLoading = true;
        setState('processing');
        showThinking();

        try {
            const resp = await fetch('/api/ai/chat', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ history: chatHistory })
            });
            removeThinking();

            if (!resp.ok) {
                const err = await resp.json().catch(() => ({}));
                console.error('[NeurixAI] Text API error:', resp.status, err);
                const detail = err.detail || err.error || `Server error (${resp.status})`;
                sysMsg(detail, 'error');
                setState('error', detail);
                isTextLoading = false;
                return;
            }

            const data = await resp.json();
            const aiText = data.text || '';
            appendMessage('AI', aiText);
            chatHistory.push({ role: 'model', text: aiText });
            setState('idle');
        } catch (e) {
            removeThinking();
            console.error('[NeurixAI] Network error:', e);
            sysMsg('Network error. Check your connection.', 'error');
            setState('error', 'Network error');
        }
        isTextLoading = false;
    };

    if (sendBtn) sendBtn.addEventListener('click', handleSend);
    if (inputField) inputField.addEventListener('keypress', e => { if (e.key === 'Enter') handleSend(); });

    // ═══════════════════════════════════════════════
    //  VOICE FLOW — WebSocket /ws/chat (AUDIO only)
    // ═══════════════════════════════════════════════
    if (micBtn) micBtn.addEventListener('click', async () => {
        if (!isRecording) await startVoice();
        else stopVoice();
    });

    const startVoice = async () => {
        // ═══════════════════════════════════════════════════════════════
        // CRITICAL: Create AudioContext SYNCHRONOUSLY inside the click
        // handler, BEFORE any async operation (getUserMedia, WebSocket).
        // Browsers require a "user gesture" to allow AudioContext.
        // Once we await getUserMedia, the gesture token is consumed
        // and the AudioContext would be permanently suspended.
        // ═══════════════════════════════════════════════════════════════
        if (!audioCtx) {
            audioCtx = new (window.AudioContext || window.webkitAudioContext)();
        }
        if (audioCtx.state === 'suspended') {
            await audioCtx.resume();
        }
        // Pre-create playback context too (same gesture requirement)
        if (!playbackCtx) {
            playbackCtx = new (window.AudioContext || window.webkitAudioContext)({ sampleRate: 24000 });
            nextAudioPlayTime = playbackCtx.currentTime;
        }
        if (playbackCtx.state === 'suspended') {
            await playbackCtx.resume();
        }

        // Get mic permission
        // NOTE: Do NOT constrain sampleRate here. Browsers may reject unsupported rates.
        // We resample to 16kHz in the AudioContext processing callback.
        try {
            mediaStream = await navigator.mediaDevices.getUserMedia({
                audio: {
                    channelCount: 1,
                    echoCancellation: true,
                    noiseSuppression: true,
                    autoGainControl: true
                }
            });
        } catch (err) {
            const msgs = {
                NotFoundError: 'No microphone found on this device.',
                NotAllowedError: 'Microphone permission denied. Please allow access in browser settings.',
                NotReadableError: 'Microphone is in use by another application.',
                OverconstrainedError: `Browser rejected audio constraints: ${err.constraint || 'unknown'}.`,
                AbortError: 'Microphone request was aborted.',
                SecurityError: 'Microphone access blocked by security policy (requires HTTPS).'
            };
            sysMsg(msgs[err.name] || `Microphone error: ${err.name} — ${err.message}`, 'error');
            return;
        }

        isRecording = true;
        setState('connecting');
        micBtn.classList.add('text-red-500', 'bg-red-100', 'dark:bg-red-900/30');
        micBtn.classList.remove('text-slate-500', 'dark:text-slate-400', 'bg-slate-100', 'dark:bg-slate-800');

        // Mic pulse is now driven dynamically by VAD
        if (gsapOk() && micPulse && micPulse.parentNode) gsap.set(micPulse, { opacity: 0, scale: 1 });

        // Connect WebSocket if needed
        if (!voiceWs || voiceWs.readyState !== WebSocket.OPEN) {
            const proto = location.protocol === 'https:' ? 'wss:' : 'ws:';
            voiceWs = new WebSocket(`${proto}//${location.host}/ws/chat`);
            voiceWs.onopen = () => { };
            voiceWs.onmessage = e => { try { handleVoiceMsg(JSON.parse(e.data)); } catch (err) { console.error('[NeurixAI] Parse error:', err); } };
            voiceWs.onerror = err => { console.error('[NeurixAI] Voice WS error:', err); };
            voiceWs.onclose = ev => {
                if (isRecording) stopVoice();
            };

            // Wait for voiceReady — use try/catch so 'return' exits startVoice()
            try {
                await new Promise((res, rej) => {
                    const origMsg = voiceWs.onmessage;
                    voiceWs.onmessage = e => {
                        try {
                            const m = JSON.parse(e.data);
                            if (m.type === 'voiceReady') { voiceWs.onmessage = origMsg; res(); }
                            else if (m.type === 'error') { rej(new Error(m.text)); }
                            else if (origMsg) origMsg(e);
                        } catch (err) { rej(err); }
                    };
                    voiceWs.onerror = () => rej(new Error('WS error'));
                    setTimeout(() => rej(new Error('Voice setup timeout')), 15000);
                });
            } catch (err) {
                console.error('[NeurixAI] Voice setup failed:', err);
                sysMsg(err.message || 'Voice setup failed.', 'error');
                stopVoice();
                return;
            }
        }

        // Guard: ensure mediaStream wasn't nulled by stopVoice() during WS setup
        if (!mediaStream || !mediaStream.active) {
            console.error('[NeurixAI] MediaStream is null/inactive after WS setup. Aborting audio capture.');
            sysMsg('Microphone stream lost during connection. Please try again.', 'error');
            stopVoice();
            return;
        }

        setState('listening');
        sysMsg('Listening... Click mic again to stop.', 'info');

        // Wire up the audio pipeline using the pre-created audioCtx
        try {

            const nativeRate = audioCtx.sampleRate;
            const targetRate = 16000;
            const downsampleRatio = Math.round(nativeRate / targetRate);

            const src = audioCtx.createMediaStreamSource(mediaStream);
            processor = audioCtx.createScriptProcessor(2048, 1, 1);

            // VAD Analyser
            vadAnalyser = audioCtx.createAnalyser();
            vadAnalyser.fftSize = 256;
            vadData = new Uint8Array(vadAnalyser.frequencyBinCount);
            src.connect(vadAnalyser);

            const checkVolume = () => {
                if (!isRecording) return;
                vadAnalyser.getByteFrequencyData(vadData);
                let sum = 0;
                for (let i = 0; i < vadData.length; i++) sum += vadData[i];
                let avg = sum / vadData.length;

                if (gsapOk() && micPulse && micPulse.parentNode) {
                    const scale = 1 + (avg / 255) * 1.5;
                    gsap.to(micPulse, { scale: scale, opacity: avg > 5 ? Math.min(0.8, avg / 50) : 0, duration: 0.1 });
                }

                if (avg > 5) {
                    if (!isSpeaking) {
                        isSpeaking = true;
                        setState('listening');
                        removeThinking();
                    }
                    if (vadSilenceTimer) { clearTimeout(vadSilenceTimer); vadSilenceTimer = null; }
                } else {
                    if (isSpeaking && !vadSilenceTimer) {
                        vadSilenceTimer = setTimeout(() => {
                            isSpeaking = false;
                            setState('processing');
                            showThinking();
                        }, 1000);
                    }
                }
                rafId = requestAnimationFrame(checkVolume);
            };
            checkVolume();

            processor.onaudioprocess = e => {
                if (!isRecording || !voiceWs || voiceWs.readyState !== WebSocket.OPEN) return;

                const inputData = e.inputBuffer.getChannelData(0);

                // Downsample from native rate to 16kHz by picking every Nth sample
                const outputLength = Math.floor(inputData.length / downsampleRatio);
                const i16 = new Int16Array(outputLength);
                for (let i = 0; i < outputLength; i++) {
                    const s = Math.max(-1, Math.min(1, inputData[i * downsampleRatio]));
                    i16[i] = s < 0 ? s * 0x8000 : s * 0x7FFF;
                }

                // Base64 encode the 16-bit PCM bytes
                const bytes = new Uint8Array(i16.buffer);
                let bin = '';
                for (let j = 0; j < bytes.length; j++) bin += String.fromCharCode(bytes[j]);
                voiceWs.send(JSON.stringify({ type: 'audio', data: btoa(bin) }));
            };

            src.connect(processor);
            processor.connect(audioCtx.destination);
        } catch (err) {
            console.error('[NeurixAI] Audio capture initialization failed:', err);
            sysMsg(`Audio capture failed: ${err.name} — ${err.message}`, 'error');
            stopVoice();
        }
    };

    const stopVoice = () => {
        isRecording = false;
        if (processor) { processor.disconnect(); processor = null; }
        // Do NOT close audioCtx — keep it alive for reuse (avoids needing a new user gesture)
        if (mediaStream) { mediaStream.getTracks().forEach(t => t.stop()); mediaStream = null; }

        micBtn?.classList.remove('text-red-500', 'bg-red-100', 'dark:bg-red-900/30');
        micBtn?.classList.add('text-slate-500', 'dark:text-slate-400', 'bg-slate-100', 'dark:bg-slate-800');
        if (micPulseTl) { micPulseTl.kill(); micPulseTl = null; }
        if (rafId) { cancelAnimationFrame(rafId); rafId = null; }
        if (vadSilenceTimer) { clearTimeout(vadSilenceTimer); vadSilenceTimer = null; }
        isSpeaking = false;
        if (gsapOk() && micPulse && micPulse.parentNode) gsap.set(micPulse, { opacity: 0, scale: 1 });

        setState('idle');
        // Don't close WS immediately — let it drain. Close after turnComplete or timeout.
    };

    const handleVoiceMsg = msg => {
        switch (msg.type) {
            case 'audio': 
                removeThinking();
                setState('idle');
                playAudio(msg.data, msg.mimeType); 
                break;
            case 'voiceTranscript':
                if (msg.sender === 'user') {
                    appendMessage('User', msg.text);
                } else {
                    removeThinking();
                    setState('idle');
                    streamAIText(msg.text);
                }
                break;
            case 'turnComplete': 
                removeThinking(); 
                if (currentAiStreamTextNode) {
                    currentAiStreamTextNode.classList.remove('ai-stream-text');
                    currentAiStreamTextNode = null;
                }
                break;
            case 'error': sysMsg(msg.text || 'Voice error.', 'error'); break;
            case 'voiceReady': break;
        }
    };

    // ── Audio Playback ──
    const playAudio = (b64, mime) => {
        try {
            if (!playbackCtx) {
                playbackCtx = new (window.AudioContext || window.webkitAudioContext)({ sampleRate: 24000 });
                nextAudioPlayTime = playbackCtx.currentTime;
            }
            const bin = atob(b64); const bytes = new Uint8Array(bin.length);
            for (let i = 0; i < bin.length; i++) bytes[i] = bin.charCodeAt(i);
            const i16 = new Int16Array(bytes.buffer); const f32 = new Float32Array(i16.length);
            for (let i = 0; i < i16.length; i++) f32[i] = i16[i] / 32768.0;
            const buf = playbackCtx.createBuffer(1, f32.length, 24000);
            buf.getChannelData(0).set(f32);
            const s = playbackCtx.createBufferSource(); s.buffer = buf; s.connect(playbackCtx.destination); 
            
            // Queue management: schedule exactly back-to-back, but don't fall behind current time
            if (nextAudioPlayTime < playbackCtx.currentTime) {
                nextAudioPlayTime = playbackCtx.currentTime + 0.05; // Small buffer if we underran
            }
            s.start(nextAudioPlayTime);
            nextAudioPlayTime += buf.duration;
        } catch (e) { console.error('[NeurixAI] Playback error:', e); }
    };

    const streamAIText = (text) => {
        if (!currentAiStreamTextNode) {
            const div = document.createElement('div');
            div.className = 'flex gap-3 mt-4';
            div.innerHTML = `<div class="w-8 h-8 rounded-full bg-cyan/10 flex-shrink-0 flex items-center justify-center text-cyan mt-1"><i data-lucide="bot" class="w-4 h-4"></i></div><div class="bg-white dark:bg-slate-800/80 border border-slate-200 dark:border-white/5 rounded-2xl rounded-tl-sm p-3 text-sm text-slate-700 dark:text-slate-300 shadow-sm leading-relaxed max-w-[85%] prose prose-sm prose-invert max-w-none ai-stream-text"></div>`;
            messagesArea.appendChild(div);
            currentAiStreamTextNode = div.querySelector('.ai-stream-text');
            currentAiStreamTextNode.dataset.fullText = '';
            if (typeof lucide !== 'undefined') lucide.createIcons();
        }
        currentAiStreamTextNode.dataset.fullText += text;
        if (typeof marked !== 'undefined') {
            currentAiStreamTextNode.innerHTML = marked.parse(currentAiStreamTextNode.dataset.fullText);
        } else {
            currentAiStreamTextNode.textContent = currentAiStreamTextNode.dataset.fullText;
        }
        messagesArea.scrollTop = messagesArea.scrollHeight;
    };

    // ── Message Rendering ──
    const appendMessage = (sender, text) => {
        const div = document.createElement('div');
        div.className = 'flex gap-3 mt-4';
        if (sender === 'User') {
            div.classList.add('flex-row-reverse');
            div.innerHTML = `<div class="bg-cyan/10 border border-cyan/20 rounded-2xl rounded-tr-sm p-3 text-sm text-cyan-900 dark:text-cyan-100 shadow-sm leading-relaxed max-w-[85%]">${esc(text)}</div>`;
        } else {
            const htmlContent = typeof marked !== 'undefined' ? marked.parse(text) : esc(text);
            div.innerHTML = `<div class="w-8 h-8 rounded-full bg-cyan/10 flex-shrink-0 flex items-center justify-center text-cyan mt-1"><i data-lucide="bot" class="w-4 h-4"></i></div><div class="bg-white dark:bg-slate-800/80 border border-slate-200 dark:border-white/5 rounded-2xl rounded-tl-sm p-3 text-sm text-slate-700 dark:text-slate-300 shadow-sm leading-relaxed max-w-[85%] prose prose-sm prose-invert max-w-none">${htmlContent}</div>`;
        }
        messagesArea.appendChild(div);
        messagesArea.scrollTop = messagesArea.scrollHeight;
        if (typeof lucide !== 'undefined') lucide.createIcons();
    };

    const sysMsg = (text, sev = 'info') => {
        if (sev === 'error') {
            setState('error', text);
            return;
        }
        const colors = { info: 'bg-slate-100 dark:bg-slate-800/60 text-slate-500 dark:text-slate-400 border-slate-200 dark:border-slate-700/40', warn: 'bg-amber-50 dark:bg-amber-900/20 text-amber-600 dark:text-amber-400 border-amber-200 dark:border-amber-700/30' };
        const icons = { info: 'info', warn: 'alert-triangle' };
        const cleanText = text;
        const div = document.createElement('div'); div.className = 'flex justify-center mt-3';
        div.innerHTML = `<div class="inline-flex items-center gap-2 px-3 py-1.5 rounded-full text-xs font-medium border ${colors[sev] || colors.info}"><i data-lucide="${icons[sev] || 'info'}" class="w-3 h-3"></i><span>${esc(cleanText)}</span></div>`;
        messagesArea.appendChild(div); messagesArea.scrollTop = messagesArea.scrollHeight;
        if (typeof lucide !== 'undefined') lucide.createIcons();
    };

    // ── Thinking Indicator ──
    const showThinking = () => {
        if (thinkingEl) return;
        thinkingEl = document.createElement('div'); thinkingEl.className = 'flex gap-3 mt-4';
        thinkingEl.innerHTML = `<div class="w-8 h-8 rounded-full bg-cyan/10 flex-shrink-0 flex items-center justify-center text-cyan mt-1"><i data-lucide="bot" class="w-4 h-4"></i></div><div class="bg-white dark:bg-slate-800/80 border border-slate-200 dark:border-white/5 rounded-2xl rounded-tl-sm px-4 py-3 shadow-sm flex items-center gap-1.5"><span class="ai-dot w-2 h-2 rounded-full bg-cyan/60 inline-block"></span><span class="ai-dot w-2 h-2 rounded-full bg-cyan/60 inline-block"></span><span class="ai-dot w-2 h-2 rounded-full bg-cyan/60 inline-block"></span></div>`;
        messagesArea.appendChild(thinkingEl); messagesArea.scrollTop = messagesArea.scrollHeight;
        if (typeof lucide !== 'undefined') lucide.createIcons();
        if (gsapOk()) {
            const dots = thinkingEl.querySelectorAll('.ai-dot');
            if (dots.length) thinkingTween = gsap.to(Array.from(dots), { y: -4, duration: 0.4, ease: 'power1.inOut', stagger: 0.15, repeat: -1, yoyo: true });
        }
    };
    const removeThinking = () => {
        if (thinkingTween) { thinkingTween.kill(); thinkingTween = null; }
        if (thinkingEl) { thinkingEl.remove(); thinkingEl = null; }
    };

    const esc = s => { const d = document.createElement('div'); d.textContent = s; return d.innerHTML; };
});
