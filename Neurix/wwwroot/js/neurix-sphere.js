(function () {
    'use strict';

    function createRenderer(canvas, options, onFirstFrame) {
        var gl = canvas.getContext('webgl', {
            alpha: true,
            antialias: false,
            depth: false,
            stencil: false,
            powerPreference: 'low-power',
            preserveDrawingBuffer: false
        });

        if (!gl) {
            throw new Error('Sphere WebGL unavailable');
        }

        var vertexSource = [
            'attribute vec3 a_position;',
            'attribute vec3 a_field;',
            'uniform float u_time;',
            'uniform float u_progress;',
            'uniform float u_scale;',
            'uniform float u_aspect;',
            'uniform float u_pointSize;',
            'uniform float u_alpha;',
            'uniform float u_scatter;',
            'uniform vec2 u_offset;',
            'varying float v_depth;',
            'varying float v_alpha;',
            'void main() {',
            '  float ay = u_time * 0.18;',
            '  float ax = sin(u_time * 0.11) * 0.12;',
            '  float cy = cos(ay);',
            '  float sy = sin(ay);',
            '  float cx = cos(ax);',
            '  float sx = sin(ax);',
            '  vec3 p = vec3(a_position.x * cy + a_position.z * sy, a_position.y, -a_position.x * sy + a_position.z * cy);',
            '  p = vec3(p.x, p.y * cx - p.z * sx, p.y * sx + p.z * cx);',
            '  float depth = p.z * 0.5 + 0.5;',
            '  float perspective = 1.0 / (1.35 - p.z * 0.28);',
            '  float breath = 1.0 + sin(u_time * 0.9) * 0.018;',
            '  vec2 position = p.xy * perspective * u_scale * breath;',
            '  float seed = a_field.z;',
            '  float scatter = smoothstep(seed * 0.25, 1.0, u_scatter);',
            '  if (u_aspect >= 1.0) position.x /= u_aspect; else position.y *= u_aspect;',
            '  float orbit = a_field.x + u_time * (0.035 + seed * 0.025) + u_progress * 0.65;',
            '  vec2 field = vec2(cos(orbit), sin(orbit)) * a_field.y * 0.96;',
            '  field.y += sin(u_time * 0.16 + a_field.x) * 0.018;',
            '  vec2 burst = vec2(cos(a_field.x), sin(a_field.x)) * sin(scatter * 3.141593) * 0.18;',
            '  gl_Position = vec4(mix(position + u_offset, field, scatter) + burst, 0.0, 1.0);',
            '  gl_PointSize = mix((u_pointSize + depth * 1.8) * perspective, u_pointSize * (0.55 + seed * 0.55), scatter);',
            '  v_depth = mix(depth, seed, scatter);',
            '  float shimmer = 0.92 + sin(u_time * 0.45 + a_field.x) * 0.08;',
            '  v_alpha = u_alpha * mix(0.32 + depth * 0.68, (0.45 + seed * 0.55) * shimmer, scatter);',
            '}'
        ].join('\n');

        var fragmentSource = [
            'precision mediump float;',
            'uniform vec3 u_deepBlue;',
            'uniform vec3 u_brightBlue;',
            'varying float v_depth;',
            'varying float v_alpha;',
            'void main() {',
            '  float distanceToCenter = length(gl_PointCoord - vec2(0.5));',
            '  float dotAlpha = 1.0 - smoothstep(0.18, 0.5, distanceToCenter);',
            '  gl_FragColor = vec4(mix(u_deepBlue, u_brightBlue, v_depth), dotAlpha * v_alpha);',
            '}'
        ].join('\n');

        function shader(type, source) {
            var item = gl.createShader(type);
            gl.shaderSource(item, source);
            gl.compileShader(item);
            if (!gl.getShaderParameter(item, gl.COMPILE_STATUS)) throw new Error(gl.getShaderInfoLog(item));
            return item;
        }

        var program;
        try {
            program = gl.createProgram();
            gl.attachShader(program, shader(gl.VERTEX_SHADER, vertexSource));
            gl.attachShader(program, shader(gl.FRAGMENT_SHADER, fragmentSource));
            gl.linkProgram(program);
            if (!gl.getProgramParameter(program, gl.LINK_STATUS)) throw new Error(gl.getProgramInfoLog(program));
        } catch (error) {
            throw new Error('Sphere WebGL unavailable');
        }

        var mobile = options.mobile;
        var lowPower = options.lowPower;
        var pointCount = mobile ? 1100 : (lowPower ? 1600 : 2800);
        var positions = new Float32Array(pointCount * 6);
        var goldenAngle = Math.PI * (3 - Math.sqrt(5));

        for (var i = 0; i < pointCount; i++) {
            var y = 1 - (i / (pointCount - 1)) * 2;
            var radius = Math.sqrt(1 - y * y);
            var angle = goldenAngle * i;
            var seed = Math.sin(i * 127.1 + 311.7) * 43758.5453;
            positions[i * 6] = Math.cos(angle) * radius;
            positions[i * 6 + 1] = y;
            positions[i * 6 + 2] = Math.sin(angle) * radius;
            positions[i * 6 + 3] = angle % (Math.PI * 2);
            positions[i * 6 + 4] = Math.sqrt((i + 0.5) / pointCount);
            positions[i * 6 + 5] = seed - Math.floor(seed);
        }

        var buffer = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
        gl.bufferData(gl.ARRAY_BUFFER, positions, gl.STATIC_DRAW);
        gl.useProgram(program);

        var positionLocation = gl.getAttribLocation(program, 'a_position');
        gl.enableVertexAttribArray(positionLocation);
        gl.vertexAttribPointer(positionLocation, 3, gl.FLOAT, false, 24, 0);
        var fieldLocation = gl.getAttribLocation(program, 'a_field');
        gl.enableVertexAttribArray(fieldLocation);
        gl.vertexAttribPointer(fieldLocation, 3, gl.FLOAT, false, 24, 12);

        var uniforms = {
            scatter: gl.getUniformLocation(program, 'u_scatter'),
            time: gl.getUniformLocation(program, 'u_time'),
            progress: gl.getUniformLocation(program, 'u_progress'),
            scale: gl.getUniformLocation(program, 'u_scale'),
            aspect: gl.getUniformLocation(program, 'u_aspect'),
            pointSize: gl.getUniformLocation(program, 'u_pointSize'),
            alpha: gl.getUniformLocation(program, 'u_alpha'),
            deepBlue: gl.getUniformLocation(program, 'u_deepBlue'),
            brightBlue: gl.getUniformLocation(program, 'u_brightBlue'),
            offset: gl.getUniformLocation(program, 'u_offset')
        };

        gl.clearColor(0, 0, 0, 0);
        gl.enable(gl.BLEND);
        gl.blendFuncSeparate(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA, gl.ONE, gl.ONE_MINUS_SRC_ALPHA);

        function interpolate(from, to, amount) {
            return from + (to - from) * amount;
        }

        function range(progress, start, end) {
            return Math.max(0, Math.min(1, (progress - start) / (end - start)));
        }

        function updateScrollState() {
            var progress = state.progress;

            if (progress <= 0.25) {
                var first = range(progress, 0, 0.25);
                sphereState.scale = interpolate(1.6, 0.82, first);
                sphereState.alpha = interpolate(1, 0.72, first);
                sphereState.x = interpolate(0, 0.2, first);
                sphereState.y = interpolate(0, -0.05, first);
            } else if (progress <= 0.5) {
                var second = range(progress, 0.25, 0.5);
                sphereState.scale = interpolate(0.82, 0.66, second);
                sphereState.alpha = interpolate(0.72, 0.5, second);
                sphereState.x = interpolate(0.2, -0.18, second);
                sphereState.y = interpolate(-0.05, 0.04, second);
            } else {
                var last = range(progress, 0.5, 0.92);
                sphereState.scale = interpolate(0.66, 1.86, last);
                sphereState.alpha = interpolate(0.5, 0.62, last);
                sphereState.x = interpolate(-0.18, 0, last);
                sphereState.y = interpolate(0.04, 0, last);
            }
            sphereState.scatter = range(progress, 0.68, 0.92);
            sphereState.progress = progress;
        }

        function resize() {
            var pixelRatio = Math.min(state.pixelRatio || 1, lowPower ? 1 : 1.25);
            var width = Math.max(1, Math.floor(state.width * pixelRatio));
            var height = Math.max(1, Math.floor(state.height * pixelRatio));
            if (canvas.width !== width || canvas.height !== height) {
                canvas.width = width;
                canvas.height = height;
                gl.viewport(0, 0, width, height);
            }
            gl.uniform1f(uniforms.aspect, state.width / Math.max(1, state.height));
        }


        var state;
        var sphereState = {};
        var displayedState;
        var animationId = 0;
        var lastFrame = 0;
        var elapsed = 0;
        var firstFrame = true;
        var frameInterval = 1000 / (lowPower ? 30 : 60);

        function stop() {
            if (animationId) cancelAnimationFrame(animationId);
            animationId = 0;
            lastFrame = 0;
        }

        function draw(timestamp) {
            animationId = 0;
            if (!state.visible) return;
            var delta = lastFrame ? timestamp - lastFrame : frameInterval;
            if (delta < frameInterval - 0.5) {
                animationId = requestAnimationFrame(draw);
                return;
            }
            lastFrame = timestamp - (delta % frameInterval);
            elapsed += Math.min(delta, 100) / 1000;
            if (!displayedState || state.reduced) displayedState = Object.assign({}, sphereState);
            else {
                var blend = 1 - Math.exp(-Math.min(delta, 100) / 90);
                for (var key in sphereState) displayedState[key] = interpolate(displayedState[key], sphereState[key], blend);
            }
            gl.clear(gl.COLOR_BUFFER_BIT);
            if (firstFrame) {
                firstFrame = false;
                if (onFirstFrame) requestAnimationFrame(onFirstFrame);
            }
            gl.uniform1f(uniforms.time, state.reduced ? 0 : elapsed);
            gl.uniform1f(uniforms.progress, displayedState.progress);
            gl.uniform1f(uniforms.scale, displayedState.scale * 1.15);
            gl.uniform1f(uniforms.alpha, Math.min(1, displayedState.alpha * (state.dark ? 1 : 1.3)));
            gl.uniform1f(uniforms.scatter, displayedState.scatter);
            gl.uniform2f(uniforms.offset, displayedState.x, displayedState.y);
            gl.drawArrays(gl.POINTS, 0, pointCount);
            if (!state.reduced) animationId = requestAnimationFrame(draw);
        }

        return {
            update: function (next) {
                var sizeChanged = !state || next.width !== state.width || next.height !== state.height || next.pixelRatio !== state.pixelRatio;
                var themeChanged = !state || next.dark !== state.dark;
                state = next;
                if (themeChanged) {
                    // CMS theme palette: [lightDeep, lightBright, darkDeep, darkBright] as 0-1 RGB.
                    var themed = state.colors;
                    if (themed) {
                        var deep = themed[state.dark ? 2 : 0], bright = themed[state.dark ? 3 : 1];
                        gl.uniform3f(uniforms.deepBlue, deep[0], deep[1], deep[2]);
                        gl.uniform3f(uniforms.brightBlue, bright[0], bright[1], bright[2]);
                    } else if (state.dark) {
                        gl.uniform3f(uniforms.deepBlue, 0.02, 0.36, 0.95);
                        gl.uniform3f(uniforms.brightBlue, 0.24, 0.66, 1);
                    } else {
                        gl.uniform3f(uniforms.deepBlue, 0, 0.22, 0.65);
                        gl.uniform3f(uniforms.brightBlue, 0, 0.38, 0.86);
                    }
                }
                if (sizeChanged) resize();
                if (sizeChanged || themeChanged) gl.uniform1f(uniforms.pointSize, state.dark ? (mobile ? 2.25 : 2.6) : (mobile ? 3.6 : 4.2));
                updateScrollState();
                if (!state.visible) stop();
                else if (!animationId) animationId = requestAnimationFrame(draw);
            },
            destroy: function () {
                stop();
                gl.deleteBuffer(buffer);
                gl.deleteProgram(program);
            }
        };
    }

    if (typeof document === 'undefined') {
        var renderer;
        self.onmessage = function (event) {
            try {
                if (event.data.type === 'init') {
                    renderer = createRenderer(event.data.canvas, event.data.options, function () {
                        self.postMessage({ type: 'ready' });
                    });
                }
                if (renderer) renderer.update(event.data.state);
            } catch (_) {
                self.postMessage({ type: 'failed' });
            }
        };
        return;
    }

    var canvas = document.getElementById('neurix-sphere-canvas');
    if (!canvas) return;
    var scriptUrl = document.currentScript.src;
    var network = document.getElementById('neurix-network');
    var preview = document.getElementById('neurix-sphere-preview');
    var motion = matchMedia('(prefers-reduced-motion: reduce)');
    var mobile = matchMedia('(max-width: 767px), (pointer: coarse)').matches;
    var options = { mobile: mobile, lowPower: mobile || navigator.deviceMemory <= 4 || navigator.hardwareConcurrency <= 4 };
    var worker;
    var fallback;
    var transferred = false;
    var pageActive = true;
    var frameId = 0;
    var initTimer = 0;
    var readyTimer = 0;
    var maxScroll = 1;

    function measure() {
        maxScroll = Math.max(1, document.documentElement.scrollHeight - innerHeight);
        schedule();
    }

    function snapshot() {
        var progress = Math.max(0, Math.min(1, scrollY / maxScroll));
        var dark = document.documentElement.classList.contains('dark');
        if (preview && !preview.hidden) preview.style.opacity = progress > 0.02 ? '0' : '1';
        if (network) {
            var opacity = (Math.max(0, Math.min(1, (progress - 0.65) / 0.27)) * (dark ? 0.28 : 0.13)).toFixed(3);
            if (network.style.opacity !== opacity) network.style.opacity = opacity;
        }
        return { progress: progress, width: innerWidth, height: innerHeight, pixelRatio: devicePixelRatio || 1, dark: dark, colors: window.neurixSphereColors || null, reduced: motion.matches, visible: pageActive && !document.hidden };
    }

    function update() {
        frameId = 0;
        var state = snapshot();
        if (worker) worker.postMessage({ type: 'update', state: state });
        else if (fallback) fallback.update(state);
    }

    function schedule() {
        if (pageActive && !frameId) frameId = requestAnimationFrame(update);
    }

    function replaceCanvas() {
        var replacement = canvas.cloneNode(false);
        canvas.replaceWith(replacement);
        canvas = replacement;
        transferred = false;
    }

    function useFallback() {
        clearTimeout(readyTimer);
        if (worker) worker.terminate();
        worker = null;
        if (!pageActive || fallback) return;
        if (transferred) replaceCanvas();
        try {
            fallback = createRenderer(canvas, options, function () {
                if (preview) preview.hidden = true;
            });
            canvas.dataset.renderer = 'main';
            fallback.update(snapshot());
        } catch (_) {
            canvas.hidden = true;
            if (network) network.style.opacity = '0.14';
        }
    }

    function initialize() {
        initTimer = 0;
        if (!pageActive || document.hidden || worker || fallback) return;
        measure();
        if (transferred) replaceCanvas();
        if (typeof Worker === 'undefined' || !canvas.transferControlToOffscreen) {
            useFallback();
            return;
        }
        try {
            worker = new Worker(scriptUrl);
            worker.onmessage = function (event) {
                if (event.data.type === 'failed') useFallback();
                else if (event.data.type === 'ready') {
                    clearTimeout(readyTimer);
                    canvas.dataset.renderer = 'worker';
                    if (preview) preview.hidden = true;
                }
            };
            worker.onerror = useFallback;
            var offscreen = canvas.transferControlToOffscreen();
            transferred = true;
            worker.postMessage({ type: 'init', canvas: offscreen, options: options, state: snapshot() }, [offscreen]);
            readyTimer = setTimeout(useFallback, 3000);
        } catch (_) { useFallback(); }
    }

    function queueInitialize() {
        if (!pageActive || initTimer || worker || fallback) return;
        initTimer = setTimeout(initialize, 0);
    }

    window.addEventListener('scroll', schedule, { passive: true });
    window.addEventListener('resize', measure, { passive: true });
    window.addEventListener('neurix-theme-change', schedule);
    window.addEventListener('load', measure, { once: true });
    if (typeof ResizeObserver !== 'undefined') new ResizeObserver(measure).observe(document.body);
    motion.addEventListener('change', schedule);
    document.addEventListener('visibilitychange', function () {
        if (!document.hidden) queueInitialize();
        update();
    });
    window.addEventListener('pagehide', function () {
        pageActive = false;
        clearTimeout(initTimer);
        clearTimeout(readyTimer);
        cancelAnimationFrame(frameId);
        initTimer = frameId = 0;
        if (worker) worker.terminate();
        if (fallback) { fallback.destroy(); transferred = true; }
        worker = fallback = null;
    });
    window.addEventListener('pageshow', function () {
        pageActive = true;
        queueInitialize();
    });
    requestAnimationFrame(queueInitialize);
})();
