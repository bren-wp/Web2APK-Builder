"use strict";
const fs = require("node:fs/promises");
const path = require("node:path");
const sharp = require("sharp");

const root = path.join(__dirname, "..", "assets");
(async () => {
  const pngToIco = (await import('png-to-ico')).default;
  const svg = await fs.readFile(path.join(root, "brand-icon.svg"));
  const sizes = [16, 24, 32, 48, 64, 128, 256];
  const paths = [];
  for (const size of sizes) {
    const dest = path.join(root, "icon-" + size + ".png");
    await sharp(svg).resize(size, size).png().toFile(dest);
    paths.push(dest);
  }
  await fs.writeFile(path.join(root, "icon.ico"), await pngToIco(paths));
  console.log("Windows icon generated from original Brendigo SVG.");
})().catch(err => { console.error(err); process.exitCode = 1; });
