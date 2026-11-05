"use strict";
const fs = require("node:fs");
const path = require("node:path");
const crypto = require("node:crypto");

const DEFAULTS = Object.freeze({
  name: "Moja web aplikacija", packageName: "com.brendigo.mojaaplikacija",
  sourceType: "url", url: "https://example.com", localPath: "",
  versionName: "1.0.0", versionCode: 1, minSdk: 24, targetSdk: 36,
  orientation: "portrait", theme: "dark", accent: "#7253FF",
  javascript: true, domStorage: true, pullRefresh: false,
  allowHttp: false, openExternal: true, camera: false,
  statusBar: true, splash: true, fullscreen: false
});
function assert(ok, message) { if (!ok) throw new Error(message); }
function cleanString(v, max) { return String(v == null ? "" : v).trim().slice(0, max); }
function validURL(url, allowHttp) {
  let parsed;
  try { parsed = new URL(url); } catch { return false; }
  return (parsed.protocol === "https:" || (allowHttp && parsed.protocol === "http:"))
    && !!parsed.hostname && !parsed.username && !parsed.password;
}
function normalize(data) {
  assert(data && typeof data === "object" && !Array.isArray(data), "Neispravan projekt.");
  const p = { ...DEFAULTS };
  p.id = typeof data.id === "string" && /^[a-f0-9-]{36}$/i.test(data.id) ? data.id : crypto.randomUUID();
  p.name = cleanString(data.name ?? p.name, 70);
  p.packageName = cleanString(data.packageName ?? p.packageName, 180);
  p.sourceType = data.sourceType === "local" ? "local" : "url";
  p.url = cleanString(data.url ?? p.url, 2000);
  p.localPath = cleanString(data.localPath || "", 1500);
  p.versionName = cleanString(data.versionName ?? p.versionName, 48);
  p.versionCode = Number(data.versionCode ?? 1);
  p.minSdk = Number(data.minSdk ?? 24);
  p.targetSdk = Number(data.targetSdk ?? 36);
  p.orientation = ["portrait","landscape","unspecified"].includes(data.orientation) ? data.orientation : "portrait";
  p.theme = data.theme === "light" ? "light" : "dark";
  p.accent = /^#[0-9a-fA-F]{6}$/.test(data.accent) ? data.accent.toUpperCase() : DEFAULTS.accent;
  for (const key of ["javascript","domStorage","pullRefresh","allowHttp","openExternal","camera","statusBar","splash","fullscreen"]) {
    p[key] = data[key] == null ? DEFAULTS[key] : data[key] === true;
  }
  assert(p.name.length >= 2, "Naziv aplikacije mora imati barem 2 znaka.");
  assert(/^[a-z][a-z0-9_]*(\.[a-z][a-z0-9_]*){1,}$/i.test(p.packageName), "Package ID mora biti npr. com.tvrtka.aplikacija.");
  assert(/^[0-9]+\.[0-9]+(\.[0-9]+)?([.-][a-zA-Z0-9]+)?$/.test(p.versionName), "Neispravno ime verzije.");
  assert(Number.isInteger(p.versionCode) && p.versionCode > 0 && p.versionCode < 2100000000, "Neispravan versionCode.");
  assert(Number.isInteger(p.minSdk) && p.minSdk >= 24 && p.minSdk <= 36, "minSdk mora biti između 24 i 36.");
  assert(Number.isInteger(p.targetSdk) && p.targetSdk >= 36 && p.targetSdk <= 37 && p.targetSdk >= p.minSdk, "targetSdk mora biti 36 ili 37.");
  if (p.sourceType === "url") assert(validURL(p.url, p.allowHttp), "Upiši valjani HTTPS URL (HTTP zahtijeva zasebno dopuštenje).");
  if (p.sourceType === "local") {
    assert(path.isAbsolute(p.localPath) && fs.existsSync(path.join(p.localPath, "index.html")), "Lokalna mapa mora sadržavati index.html.");
  }
  p.updatedAt = new Date().toISOString();
  p.createdAt = cleanString(data.createdAt || p.updatedAt, 30);
  return p;
}
class ProjectStore {
  constructor(userData) {
    this.file = path.join(userData, "projects.json");
    fs.mkdirSync(userData, { recursive: true });
  }
  all() {
    if (!fs.existsSync(this.file)) return [];
    let projects;
    try { projects = JSON.parse(fs.readFileSync(this.file, "utf8")); }
    catch { throw new Error("Datoteka projekata je oštećena. Napravi sigurnosnu kopiju prije nastavka."); }
    assert(Array.isArray(projects), "Neispravna baza projekata.");
    return projects;
  }
  get(id) { return this.all().find(p => p.id === id) || null; }
  save(input) {
    const projects = this.all();
    const previous = input.id && projects.find(p => p.id === input.id);
    const result = normalize({ ...input, id: previous ? previous.id : undefined, createdAt: previous ? previous.createdAt : undefined });
    const idx = projects.findIndex(p => p.id === result.id);
    if (idx >= 0) projects[idx] = result; else projects.unshift(result);
    this.write(projects);
    return result;
  }
  remove(id) { const before=this.all(); const after=before.filter(p=>p.id!==id); this.write(after); return after.length < before.length; }
  write(projects) {
    const temp = this.file + "." + process.pid + ".tmp";
    fs.writeFileSync(temp, JSON.stringify(projects, null, 2), { encoding: "utf8", mode: 0o600 });
    fs.renameSync(temp, this.file);
  }
}
module.exports = { DEFAULTS, normalize, validURL, ProjectStore };
