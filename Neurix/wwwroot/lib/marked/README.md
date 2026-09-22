# Marked

Local browser UMD build of Marked 17.0.4, distributed under the included MIT license.
Source: https://cdn.jsdelivr.net/npm/marked@17.0.4/lib/marked.umd.js

The shared chat widget loads this file with `defer` before `ai-chat.js`, so Markdown
rendering does not require a blocking request to an external CDN.
