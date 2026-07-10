// Generates the rule manifest the MOP Revival mod consumes.
//
// Scans rules/*.mopconfig, computes a sha256 for each file, copies the files into
// public/rules/ and writes public/manifest.json. Vite then serves both at the site
// root, so the mod can fetch:
//   GET /manifest.json                 -> { rules: [{ id, path, sha256, size }] }
//   GET /rules/<ModID>.mopconfig       -> the rule file
//
// Run automatically on `npm run dev` and `npm run build` (pre-hooks), or directly
// with `npm run manifest`.

import { createHash } from 'node:crypto';
import {
  readdirSync,
  readFileSync,
  writeFileSync,
  mkdirSync,
  copyFileSync,
  existsSync,
  rmSync,
} from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { parseRuleFile } from '../src/lib/ruleSpec.js';

const projectRoot = join(dirname(fileURLToPath(import.meta.url)), '..');
const rulesDir = join(projectRoot, 'rules');
const publicDir = join(projectRoot, 'public');
const publicRulesDir = join(publicDir, 'rules');

// Start from a clean public/rules so deleted rule files don't linger.
if (existsSync(publicRulesDir)) {
  rmSync(publicRulesDir, { recursive: true, force: true });
}
mkdirSync(publicRulesDir, { recursive: true });

const files = existsSync(rulesDir)
  ? readdirSync(rulesDir).filter((f) => f.toLowerCase().endsWith('.mopconfig'))
  : [];

const rules = [];
let totalErrors = 0;
let totalWarnings = 0;

for (const file of files) {
  const buffer = readFileSync(join(rulesDir, file));
  const sha256 = createHash('sha256').update(buffer).digest('hex');

  // Validate every rule file at build time so a broken submission is caught early.
  const check = parseRuleFile(buffer.toString('utf8'));
  const id = file.replace(/\.mopconfig$/i, '');
  if (check.errors > 0) {
    totalErrors += check.errors;
    for (const line of check.lines.filter((l) => l.level === 'error')) {
      console.error(`[manifest] ERROR ${file}:${line.n}  ${line.message}`);
    }
  }
  if (check.warnings > 0) {
    totalWarnings += check.warnings;
    for (const line of check.lines.filter((l) => l.level === 'warn')) {
      console.warn(`[manifest] warn  ${file}:${line.n}  ${line.message}`);
    }
  }

  copyFileSync(join(rulesDir, file), join(publicRulesDir, file));
  rules.push({
    id,
    path: `rules/${file}`,
    sha256,
    size: buffer.length,
    minVer: check.minVer || null,
    directiveCount: check.directiveCount,
  });
}

rules.sort((a, b) => a.id.localeCompare(b.id));

const manifest = {
  generated: new Date().toISOString(),
  count: rules.length,
  rules,
};

writeFileSync(join(publicDir, 'manifest.json'), `${JSON.stringify(manifest, null, 2)}\n`);
console.log(
  `[manifest] ${rules.length} rule file(s) -> public/manifest.json` +
    ` (${totalErrors} error(s), ${totalWarnings} warning(s))`,
);

if (totalErrors > 0) {
  console.error('[manifest] Rule files contain errors. Fix them before deploying.');
}
