/* ============================================================
   Neurix "Neural Singularity" ΓÇö Global 3D Particle Sphere
   ============================================================
   Stack   : Three.js r128 + GSAP 3.12 + ScrollTrigger
   Purpose : Persistent, scroll-linked WebGL particle sphere
   Target  : #neurix-gl (fixed full-viewport canvas in _Layout)

   Architecture:
   - True procedural particle ASSEMBLY intro (not a mesh.scale)
   - Each particle flies from a random origin to its sphere target
   - Staggered per-particle timing for organic formation
   - body.neurix-loading hides DOM; sphere IS the preloader
   - ONE master ScrollTrigger timeline for all scroll states
   - Footer climax: sphere grows massive, spins 4├ù speed
   ============================================================ */

(function () {
    'use strict';

    // ΓöÇΓöÇ Failsafe: reveal UI even if everything fails ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    function forceRevealUI() {
        document.body.classList.remove('neurix-loading');
    }

    // ΓöÇΓöÇ Guard: bail if libs missing ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    if (typeof THREE === 'undefined' || typeof gsap === 'undefined') {
        console.warn('[Neurix Sphere] Three.js or GSAP not loaded ΓÇö revealing UI.');
        forceRevealUI();
        return;
    }

    var prefersReduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // ΓöÇΓöÇ Constants ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    var PARTICLE_COUNT   = window.innerWidth < 768 ? 3000 : 6000;
    var SPHERE_RADIUS    = 6.0;
    var INTRO_DURATION   = 1.8;
    var BREATH_AMPLITUDE = 0.035;
    var BASE_ROTATION    = 0.0015;
    var SCATTER_RADIUS   = 0.3;   // initial random cluster size
    var STAGGER_SPREAD   = 0.45;  // max delay offset per particle (0ΓÇô45% of timeline)

    // ΓöÇΓöÇ Brand Colors ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    var COLOR_CYAN      = new THREE.Color(0x00DFFF);
    var COLOR_AZURE     = new THREE.Color(0x0076FF);
    var COLOR_DEEP_BLUE = new THREE.Color(0x004CC7);

    // ΓöÇΓöÇ DOM references ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    var container = document.getElementById('neurix-gl');

    if (!container) {
        console.warn('[Neurix Sphere] #neurix-gl not found.');
        forceRevealUI();
        return;
    }

    // ΓöÇΓöÇ Three.js Setup ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    var renderer, scene, camera, particles, particleMaterial, geometry;
    var targetPositions;       // Float32Array ΓÇö final sphere surface positions
    var startPositions;        // Float32Array ΓÇö random initial scatter positions
    var particleStagger;       // Float32Array ΓÇö per-particle delay offset [0, STAGGER_SPREAD]
    var clock = new THREE.Clock();
    var animationId = null;
    var isRunning = false;
    var introComplete = false; // locks the intro lerp after assembly finishes
    var isDarkMode = document.documentElement.classList.contains('dark');

    // Sphere state ΓÇö single source of truth for GSAP tweens
    var sphereState = {
        scale:         1.0,
        opacity:       1.0,
        offsetX:       0,
        offsetY:       0,
        rotationX:     0,
        rotationY:     0,
        rotationSpeed: 1.0
    };

    // Intro proxy ΓÇö tweened from 0ΓåÆ1 during particle assembly
    var introState = { progress: 0 };

    try {
        renderer = new THREE.WebGLRenderer({ alpha: true, antialias: false });
        renderer.setPixelRatio(Math.min(window.devicePixelRatio, 1.5));
        renderer.setSize(window.innerWidth, window.innerHeight);
        renderer.setClearColor(0x000000, 0);
        container.appendChild(renderer.domElement);

        scene = new THREE.Scene();

        camera = new THREE.PerspectiveCamera(60, window.innerWidth / window.innerHeight, 0.1, 100);
        camera.position.z = 10;

        // ΓöÇΓöÇ Build Particle Geometry ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
        geometry = new THREE.BufferGeometry();

        var positions      = new Float32Array(PARTICLE_COUNT * 3); // active (GPU) positions
        targetPositions    = new Float32Array(PARTICLE_COUNT * 3); // sphere surface targets
        startPositions     = new Float32Array(PARTICLE_COUNT * 3); // random initial cluster
        particleStagger    = new Float32Array(PARTICLE_COUNT);     // per-particle delay

        for (var i = 0; i < PARTICLE_COUNT; i++) {
            // ΓöÇΓöÇ Target: Fibonacci sphere distribution ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
            var phi   = Math.acos(1 - 2 * (i + 0.5) / PARTICLE_COUNT);
            var theta = Math.PI * (1 + Math.sqrt(5)) * i;

            var tx = SPHERE_RADIUS * Math.sin(phi) * Math.cos(theta);
            var ty = SPHERE_RADIUS * Math.sin(phi) * Math.sin(theta);
            var tz = SPHERE_RADIUS * Math.cos(phi);

            targetPositions[i * 3]     = tx;
            targetPositions[i * 3 + 1] = ty;
            targetPositions[i * 3 + 2] = tz;

            // ΓöÇΓöÇ Start: tiny random cluster near center ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
            var sx = (Math.random() - 0.5) * SCATTER_RADIUS;
            var sy = (Math.random() - 0.5) * SCATTER_RADIUS;
            var sz = (Math.random() - 0.5) * SCATTER_RADIUS;

            startPositions[i * 3]     = sx;
            startPositions[i * 3 + 1] = sy;
            startPositions[i * 3 + 2] = sz;

            // Initialize active positions at the start cluster
            positions[i * 3]     = sx;
            positions[i * 3 + 1] = sy;
            positions[i * 3 + 2] = sz;

            // ΓöÇΓöÇ Stagger: random delay offset per particle ΓöÇΓöÇΓöÇΓöÇΓöÇ
            // Particles near the top of the sphere form first (visual wave effect)
            var normalizedY = (ty / SPHERE_RADIUS + 1) * 0.5; // 0=bottom, 1=top
            particleStagger[i] = (1 - normalizedY) * STAGGER_SPREAD + Math.random() * 0.05;
        }

        geometry.setAttribute('position', new THREE.BufferAttribute(positions, 3));

        // Soft circular dot texture (64px, multi-stop glow)
        var dotCanvas = document.createElement('canvas');
        dotCanvas.width = 64;
        dotCanvas.height = 64;
        var ctx = dotCanvas.getContext('2d');
        var grad = ctx.createRadialGradient(32, 32, 0, 32, 32, 32);
        grad.addColorStop(0, 'rgba(255,255,255,1)');
        grad.addColorStop(0.15, 'rgba(255,255,255,0.9)');
        grad.addColorStop(0.4, 'rgba(255,255,255,0.5)');
        grad.addColorStop(0.7, 'rgba(255,255,255,0.15)');
        grad.addColorStop(1, 'rgba(255,255,255,0)');
        ctx.fillStyle = grad;
        ctx.fillRect(0, 0, 64, 64);
        var dotTexture = new THREE.CanvasTexture(dotCanvas);

        // Material
        particleMaterial = new THREE.PointsMaterial({
            size: 0.15,
            color: isDarkMode ? COLOR_CYAN.clone() : COLOR_DEEP_BLUE.clone(),
            map: dotTexture,
            transparent: true,
            opacity: 0.9,
            blending: isDarkMode ? THREE.AdditiveBlending : THREE.NormalBlending,
            depthWrite: false,
            sizeAttenuation: true
        });

        particles = new THREE.Points(geometry, particleMaterial);
        scene.add(particles);

    } catch (e) {
        console.warn('[Neurix Sphere] WebGL init failed:', e);
        forceRevealUI();
        return;
    }

    // ΓöÇΓöÇ Theme Reactivity ΓÇö MutationObserver ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    function applyThemeToSphere(dark) {
        isDarkMode = dark;
        var targetColor = dark ? COLOR_CYAN : COLOR_DEEP_BLUE;

        gsap.to(particleMaterial.color, {
            r: targetColor.r,
            g: targetColor.g,
            b: targetColor.b,
            duration: 0.6,
            ease: 'power2.inOut'
        });

        particleMaterial.blending = dark ? THREE.AdditiveBlending : THREE.NormalBlending;
        particleMaterial.needsUpdate = true;
    }

    var themeObserver = new MutationObserver(function (mutations) {
        for (var i = 0; i < mutations.length; i++) {
            if (mutations[i].attributeName === 'class') {
                var nowDark = document.documentElement.classList.contains('dark');
                if (nowDark !== isDarkMode) {
                    applyThemeToSphere(nowDark);
                }
                break;
            }
        }
    });
    themeObserver.observe(document.documentElement, { attributes: true, attributeFilter: ['class'] });

    // ΓöÇΓöÇ Render Loop ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    function render() {
        if (!isRunning) return;
        animationId = requestAnimationFrame(render);

        var elapsed = clock.getElapsedTime();
        var posAttr = particles.geometry.attributes.position;
        var posArr  = posAttr.array;

        if (!introComplete) {
            // ΓöÇΓöÇ INTRO MODE: per-particle lerp from start ΓåÆ target ΓöÇΓöÇ
            var globalProgress = introState.progress;

            for (var i = 0; i < PARTICLE_COUNT; i++) {
                var i3 = i * 3;
                var stagger = particleStagger[i];

                // Per-particle progress: remap global progress accounting for stagger offset
                // Particle doesn't start moving until globalProgress > its stagger value
                var localProgress;
                if (globalProgress <= stagger) {
                    localProgress = 0;
                } else {
                    localProgress = Math.min(1, (globalProgress - stagger) / (1 - stagger));
                }

                // Smooth easing per particle (cubic ease-out)
                var eased = 1 - Math.pow(1 - localProgress, 3);

                // Lerp: start + (target - start) * eased
                posArr[i3]     = startPositions[i3]     + (targetPositions[i3]     - startPositions[i3])     * eased;
                posArr[i3 + 1] = startPositions[i3 + 1] + (targetPositions[i3 + 1] - startPositions[i3 + 1]) * eased;
                posArr[i3 + 2] = startPositions[i3 + 2] + (targetPositions[i3 + 2] - startPositions[i3 + 2]) * eased;
            }
        } else {
            // ΓöÇΓöÇ POST-INTRO MODE: breathing on formed sphere ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
            var breathFactor = 1 + Math.sin(elapsed * 1.2) * BREATH_AMPLITUDE;
            if (prefersReduced) breathFactor = 1;

            for (var j = 0; j < PARTICLE_COUNT; j++) {
                var j3 = j * 3;
                posArr[j3]     = targetPositions[j3]     * breathFactor;
                posArr[j3 + 1] = targetPositions[j3 + 1] * breathFactor;
                posArr[j3 + 2] = targetPositions[j3 + 2] * breathFactor;
            }
        }

        posAttr.needsUpdate = true;

        // GSAP-driven transforms (apply in both modes)
        particles.scale.setScalar(sphereState.scale);
        particles.position.x = sphereState.offsetX;
        particles.position.y = sphereState.offsetY;

        // Continuous rotation
        if (!prefersReduced) {
            sphereState.rotationY += BASE_ROTATION * sphereState.rotationSpeed;
        }
        particles.rotation.x = sphereState.rotationX;
        particles.rotation.y = sphereState.rotationY;

        // Opacity
        particleMaterial.opacity = sphereState.opacity * 0.9;

        renderer.render(scene, camera);
    }

    function startRender() {
        if (isRunning) return;
        isRunning = true;
        clock.start();
        render();
    }

    function stopRender() {
        isRunning = false;
        if (animationId) {
            cancelAnimationFrame(animationId);
            animationId = null;
        }
    }

    // ΓöÇΓöÇ Resize ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    var resizeTimeout;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(function () {
            var w = window.innerWidth;
            var h = window.innerHeight;
            camera.aspect = w / h;
            camera.updateProjectionMatrix();
            renderer.setSize(w, h);
        }, 150);
    });

    // ΓöÇΓöÇ ACT 0: True Particle Assembly Intro ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    // Each particle independently flies from its random origin
    // to its target position on the sphere surface.
    // Staggered timing creates an organic wave formation.
    // UI is hidden until assembly is 100% complete.
    function runIntroSequence() {
        startRender();

        if (prefersReduced) {
            introState.progress = 1;
            introComplete = true;
            forceRevealUI();
            setupMasterTimeline();
            return;
        }

        gsap.to(introState, {
            progress: 1,
            duration: INTRO_DURATION,
            ease: 'expo.out',
            onComplete: function () {
                // Lock intro ΓÇö switch render loop to breathing mode
                introComplete = true;

                // Snap all positions to exact targets (clean up floating point)
                var posArr = particles.geometry.attributes.position.array;
                for (var i = 0; i < PARTICLE_COUNT * 3; i++) {
                    posArr[i] = targetPositions[i];
                }
                particles.geometry.attributes.position.needsUpdate = true;

                // Reveal the DOM UI (CSS transition handles the fade)
                document.body.classList.remove('neurix-loading');

                // Wire up scroll interactions after a brief settle
                setTimeout(function () {
                    setupMasterTimeline();
                }, 700);
            }
        });
    }

    // ΓöÇΓöÇ MASTER SCROLL TIMELINE ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    // ONE timeline, ONE ScrollTrigger, full page scroll.
    // All sphere state mapped linearly as percentages.
    // Flawless forward AND backward scrubbing.
    function setupMasterTimeline() {
        if (typeof ScrollTrigger === 'undefined') {
            console.warn('[Neurix Sphere] ScrollTrigger not loaded.');
            return;
        }

        gsap.registerPlugin(ScrollTrigger);

        var masterTL = gsap.timeline({
            scrollTrigger: {
                trigger: document.body,
                start: 'top top',
                end: 'bottom bottom',
                scrub: 1.5
            }
        });

        // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
        // 0% ΓåÆ 25%: HERO ΓåÆ DIVISIONS
        // Sphere drifts right, scales down, color shifts
        // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
        masterTL.to(sphereState, {
            scale:     0.55,
            offsetX:   3.5,
            offsetY:  -0.5,
            rotationX: 0.4,
            opacity:   0.8,
            rotationSpeed: 1.2,
            duration:  25,
            ease: 'none'
        }, 0);

        masterTL.to({}, {
            duration: 25,
            ease: 'none',
            onUpdate: function () {
                var p = this.progress();
                var from = isDarkMode ? COLOR_CYAN : COLOR_DEEP_BLUE;
                particleMaterial.color.copy(from).lerp(COLOR_AZURE, p);
            }
        }, 0);

        // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
        // 25% ΓåÆ 50%: DIVISIONS ΓåÆ STATS
        // Sphere drifts left, fades slightly
        // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
        masterTL.to(sphereState, {
            offsetX:  -3.0,
            offsetY:   0.3,
            scale:     0.4,
            opacity:   0.5,
            rotationX: -0.2,
            rotationSpeed: 0.8,
            duration:  25,
            ease: 'none'
        }, 25);

        masterTL.to({}, {
            duration: 25,
            ease: 'none',
            onUpdate: function () {
                var p = this.progress();
                var to = isDarkMode ? COLOR_CYAN : COLOR_DEEP_BLUE;
                particleMaterial.color.copy(COLOR_AZURE).lerp(to, p);
            }
        }, 25);

        // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
        // 50% ΓåÆ 75%: STATS ΓåÆ CONTACT (building up to climax)
        // Sphere re-centers, starts growing
        // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
        masterTL.to(sphereState, {
            offsetX:   0,
            offsetY:   0,
            scale:     1.8,
            opacity:   0.6,
            rotationX: 0.15,
            rotationSpeed: 2.5,
            duration:  25,
            ease: 'none'
        }, 50);

        masterTL.to({}, {
            duration: 25,
            ease: 'none',
            onUpdate: function () {
                var p = this.progress();
                var from = isDarkMode ? COLOR_CYAN : COLOR_DEEP_BLUE;
                particleMaterial.color.copy(from).lerp(COLOR_AZURE, p * 0.5);
            }
        }, 50);

        // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
        // 75% ΓåÆ 100%: FOOTER CLIMAX
        // Massive sphere, fast spin, intelligence core
        // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
        masterTL.to(sphereState, {
            scale:     2.5,
            offsetX:   0,
            offsetY:   0,
            opacity:   0.35,
            rotationX: 0,
            rotationSpeed: 4.0,
            duration:  25,
            ease: 'none'
        }, 75);

        masterTL.to({}, {
            duration: 25,
            ease: 'none',
            onUpdate: function () {
                var p = this.progress();
                var from = isDarkMode ? COLOR_AZURE : COLOR_DEEP_BLUE;
                var to   = isDarkMode ? COLOR_CYAN : COLOR_AZURE;
                particleMaterial.color.copy(from).lerp(to, p);
            }
        }, 75);
    }

    // ΓöÇΓöÇ Visibility Optimization ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    document.addEventListener('visibilitychange', function () {
        if (document.hidden) {
            stopRender();
        } else {
            startRender();
        }
    });

    // ΓöÇΓöÇ Boot ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    runIntroSequence();

})();
