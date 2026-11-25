const { copyFileSync, existsSync, mkdirSync, readFileSync, writeFileSync } = require("node:fs");
const { dirname, join, posix, resolve } = require("node:path");
const { Script } = require("node:vm");

const root = resolve(__dirname, "..");
const output = resolve(process.argv[2] || join(root, ".utmp", "pages"));
const github = "https://github.com/xavikai/EscapeRoomRevolt/blob/main/";
const escapeRoom = "EscapeRoom/Documentacio/GuiaInteractiva/index.html";
const release = "https://github.com/xavikai/EscapeRoomRevolt/releases/tag/escaperoom-v0.2.3";

mkdirSync(output, { recursive: true });
copyFileSync(join(__dirname, "index.html"), join(output, "index.html"));

let html = readFileSync(join(root, escapeRoom), "utf8");
// Resolve repository links for both the hosted guide and the downloadable file.
html = html.replace(/href="((?:\.\.\/)+[^"#]+)(#[^"]*)?"/g, (_, relative, fragment = "") => {
  const target = posix.normalize(posix.join(posix.dirname(escapeRoom), relative));
  if (!/\.(md|json)$/.test(target) || !existsSync(join(root, target))) {
    throw new Error(`${escapeRoom}: enllaç sense destinació publicable: ${relative}`);
  }
  return `href="${github}${target}${fragment}"`;
});
const marker = "<!-- pages-home -->";
if (!html.includes(marker)) throw new Error(`${escapeRoom}: falta l'enllaç a les descàrregues`);
html = html.replace(marker, `<a class="chip" href="${release}" data-t="guideHome">Descàrregues</a>`);
for (const [, script] of html.matchAll(/<script>([\s\S]*?)<\/script>/g)) {
  new Script(script, { filename: escapeRoom });
}

const destination = join(output, "escape-room", "index.html");
mkdirSync(dirname(destination), { recursive: true });
writeFileSync(destination, html);
console.log(`Documentació preparada a ${output}`);
