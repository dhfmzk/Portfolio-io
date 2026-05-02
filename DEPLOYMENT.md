# Unity WebGL GitHub Pages Deployment

This project uses a low-overhead deployment loop:

1. Build Unity WebGL locally into `docs/`.
2. Commit the generated `docs/` output.
3. Publish GitHub Pages from `main` and `/docs`.

## One-Time GitHub Pages Setup

In the GitHub repository:

1. Open `Settings > Pages`.
2. Set `Source` to `Deploy from a branch`.
3. Set `Branch` to `main`.
4. Set the folder to `/docs`.
5. Save.

The project site should be served at:

```text
https://dhfmzk.github.io/Portfolio-io/
```

## Local Build

Use Unity `2022.3.62f3`.

From the Unity Editor:

1. Open this project.
2. Select `Portfolio > Build WebGL for GitHub Pages`.
3. Wait for the build to finish.
4. Commit the updated `docs/` folder.
5. Push `main`.

From the command line:

```bash
"/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity" \
  -batchmode \
  -quit \
  -projectPath "$(pwd)" \
  -executeMethod Portfolio.Build.WebGLPagesBuild.Build
```

## Build Settings Applied By Script

The build script at `Assets/Editor/WebGLPagesBuild.cs` applies these settings before building:

- Build target: `WebGL`
- Output folder: `docs/`
- WebGL template: `APPLICATION:PWA`
- Compression: disabled
- Decompression fallback: disabled
- Data caching: enabled
- Hashed file names: disabled
- `.nojekyll`: recreated after every build

Compression is disabled because GitHub Pages does not give this repo direct per-file response header control. This keeps the first deployment path simple and avoids broken `.gz` or `.br` asset loading.

## Later Optimization

If the WebGL build becomes large, revisit one of these options:

- Enable gzip with Unity decompression fallback and keep GitHub Pages.
- Move the built files to static hosting with response header control, then use gzip or Brotli without fallback.
- Move deployment output to a `gh-pages` branch when committing generated files to `main` becomes too noisy.

## Smoke Test

After each WebGL build, serve `docs/` locally and verify the root page plus `Build/docs.loader.js`, `Build/docs.data`, and `Build/docs.wasm` return `200 OK`.
