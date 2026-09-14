# SteelFang — Web build

Playable build of SteelFang, deployed as a static site.

**Live:** https://steelfang.vercel.app

This folder is the deploy artifact: it is uploaded to Vercel as-is. Nothing
here is compiled by the host — Vercel cannot run Unity, so do **not** connect
this repository to Vercel's Git integration. Deploys are pushed from a local
machine with the Vercel CLI.

## Rebuilding

Requires a Unity editor **with the Web build module installed**. `6000.3.6f1`
does not have it; `6000.3.10f1` does.

```
"C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe" \
  -quit -batchmode -nographics -accept-apiupdate \
  -projectPath "<path-to-SteelFang_Project>" \
  -buildTarget WebGL \
  -executeMethod WebBuilder.BuildWeb
```

`WebBuilder.BuildWeb` lives in `Assets/Editor/WebBuilder.cs` and writes its
output here. It pins the compression settings that `vercel.json` depends on.

## Deploying

```
npm i -g vercel
cd SteelFang_Web
vercel --prod
```

## Why vercel.json matters

The build ships Brotli-compressed with Unity's decompression fallback turned
**off**, so the server must declare the encoding itself. Without the headers in
`vercel.json` the browser downloads compressed bytes, fails to parse the wasm,
and the page renders a black canvas with `Unable to parse Build/....wasm!` in
the console.

Each file needs the content type of its *decompressed* form:

| File | Content-Type | Content-Encoding |
|---|---|---|
| `.wasm.br` | `application/wasm` | `br` |
| `.framework.js.br` | `application/javascript` | `br` |
| `.data.br` | `application/octet-stream` | `br` |
| `.loader.js` | `application/javascript` | — (not compressed) |

Renaming this folder changes the emitted filenames (Unity names them after the
output directory), which would break those rules — update `vercel.json` and
`index.html` together if you ever rename it.

## index.html

Customised from Unity's default template: the footer bar is removed, the canvas
scales to the viewport at a fixed 1.6 aspect, and the page background is a
gradient sampled from the main-menu art.
