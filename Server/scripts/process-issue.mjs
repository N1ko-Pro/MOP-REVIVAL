// Turns a rule-submission issue into a validated rule file.
//
// Run by .github/workflows/rule-submission.yml on issues that contain the
// submission marker. Reads the issue body from ISSUE_BODY, extracts the mod ID
// and the fenced ```mopconfig block, validates it with the shared parser and —
// if valid — writes Server/rules/<ID>.mopconfig. Nothing is committed here; the
// workflow opens a pull request that a maintainer reviews before it goes live.
//
// Outputs (via GITHUB_OUTPUT):
//   status  = ok | invalid
//   message = human-readable error (when invalid)
//   id      = validated mod ID (when ok)
//   path    = repo-relative file path (when ok)

import { writeFileSync, appendFileSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { parseRuleFile, validateModId, SUBMISSION_MARKER } from '../src/lib/ruleSpec.js';

const serverRoot = join(dirname(fileURLToPath(import.meta.url)), '..');
const body = (process.env.ISSUE_BODY || '').replace(/\r\n?/g, '\n');

function out(key, value) {
  const file = process.env.GITHUB_OUTPUT;
  if (!file) return;
  appendFileSync(file, `${key}<<__MOPR_EOF__\n${value}\n__MOPR_EOF__\n`);
}

// A failed submission is not a failed workflow — we comment on the issue and
// exit 0 so the run stays green.
function invalid(message) {
  out('status', 'invalid');
  out('message', message);
  console.log(`invalid: ${message}`);
  process.exit(0);
}

// 1) Mod ID from the marker (authoritative; never guessed).
const marker = body.match(new RegExp(`<!--\\s*${SUBMISSION_MARKER}\\s+id=([^\\s>]+)\\s*-->`, 'i'));
if (!marker) {
  invalid('Could not find the submission marker. Please submit through the rule builder on the website.');
}
if (marker[1].includes('..')) invalid('Invalid mod ID.');
const idCheck = validateModId(marker[1]);
if (!idCheck.ok) invalid(`Invalid mod ID: ${idCheck.message}`);

// 2) Rule content from the first fenced code block.
const code = body.match(/```(?:mopconfig)?[ \t]*\n([\s\S]*?)```/);
if (!code) invalid('Could not find a rule code block in the issue.');
const content = `${code[1].replace(/\s+$/, '')}\n`;

// 3) Validate with the same parser the site and manifest build use.
const check = parseRuleFile(content);
if (!check.hasContent) invalid('The rule file has no directives.');
if (!check.ok) {
  const errs = check.lines
    .filter((l) => l.level === 'error')
    .map((l) => `- line ${l.n}: ${l.message}`)
    .join('\n');
  invalid(`The rule file has errors:\n${errs}`);
}

// 4) Write it. The ID is validated, so the path can never escape rules/.
const relPath = `Server/rules/${idCheck.id}.mopconfig`;
writeFileSync(join(serverRoot, 'rules', `${idCheck.id}.mopconfig`), content);

out('status', 'ok');
out('id', idCheck.id);
out('path', relPath);
console.log(`ok: wrote ${relPath} (${check.directiveCount} directive(s), ${check.warnings} warning(s))`);
