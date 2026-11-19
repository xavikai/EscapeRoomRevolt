const { copyFileSync, existsSync, mkdirSync, readFileSync, writeFileSync } = require("node:fs");
const { dirname, join, posix, resolve } = require("node:path");
const { Script } = require("node:vm");

const root = resolve(__dirname, "..");
const output = resolve(process.argv[2] || join(root, ".utmp", "pages"));
const github = "https://github.com/xavikai/EscapeRoomRevolt/blob/main/";
const escapeRoom = "EscapeRoom/Framework/docs/guia/index.html";
const survival = "Survival/Documentacio/GuiaInteractiva/index.html";
const routes = new Map([
  [escapeRoom, "escape-room"],
  [survival, "survival"],
]);

mkdirSync(output, { recursive: true });
copyFileSync(join(__dirname, "index.html"), join(output, "index.html"));

for (const [source, route] of routes) {
  let html = readFileSync(join(root, source), "utf8");
  // The Escape Room renderer also contains relative links inside its HTML templates.
  html = html.replace(/href="((?:\.\.\/)+[^"#]+)(#[^"]*)?"/g, (_, relative, fragment = "") => {
    const target = posix.normalize(posix.join(posix.dirname(source), relative));
    if (!/\.(md|json)$/.test(target) || !existsSync(join(root, target))) {
      throw new Error(`${source}: enllaç sense destinació publicable: ${relative}`);
    }
    return `href="${github}${target}${fragment}"`;
  });
  const homeLink = source === escapeRoom
    ? '<a class="chip" href="../" data-t="guideHome">Totes les guies</a>'
    : '<a href="../">Totes les guies</a> · ';
  const marker = "<!-- pages-home -->";
  if (!html.includes(marker)) throw new Error(`${source}: falta l'enllaç a la portada`);
  html = html.replace(marker, homeLink);
  for (const [, script] of html.matchAll(/<script>([\s\S]*?)<\/script>/g)) {
    new Script(script, { filename: source });
  }
  const destination = join(output, route, "index.html");
  mkdirSync(dirname(destination), { recursive: true });
  writeFileSync(destination, html);
}

const survivalScript = join(root, posix.dirname(survival), "survival-guide.js");
new Script(readFileSync(survivalScript, "utf8"), { filename: survivalScript });
copyFileSync(survivalScript, join(output, "survival", "survival-guide.js"));
console.log(`Documentació preparada a ${output}`);
