// Single source of truth for the MOP - REVIVAL rule-file format.
//
// Dependency-free and isomorphic: imported by
//   - the website (browser) for the docs, the rule builder and the live validator, and
//   - scripts/generate-manifest.mjs (Node) to validate every rule file at build time.
//
// KEEP IN SYNC WITH THE MOD PARSER: src/Rules/RuleParser.cs
// Only the directives handled by that parser are documented/validated here.

export const MOP_VERSION = '4.0.0b';

// GitHub repository that hosts the rule database (used by the "submit" flow).
// A submission opens a prefilled "new file" page that creates a pull request.
// Maintainer: point these at the real repository / path if it changes.
export const REPO = 'N1ko-Pro/MOP-REVIVAL';
export const RULES_PATH = 'Server/rules';

// ---------------------------------------------------------------------------
// Directive catalogue (drives validation, the docs tables and the builder).
// `kind` tells the validator how to check the arguments.
// ---------------------------------------------------------------------------
export const DIRECTIVES = {
  min_ver: {
    kind: 'version',
    group: 'meta',
    syntax: 'min_ver: 4.0.0',
    summary: 'Minimum MOP - REVIVAL version this file needs.',
    detail:
      'If the player runs an older MOP, the entire file is skipped with a warning. Keep it on the first line.',
  },

  ignore: {
    kind: 'ignore',
    group: 'core',
    syntax: 'ignore: ObjectName',
    summary: 'Never enable or disable this object.',
    detail:
      'The most important compatibility rule — use it for anything that breaks when toggled. Add "fullignore" to also stop MOP from creating a distant stand-in. Two names ("Place Object") restrict the ignore to one location.',
  },
  toggle: {
    kind: 'toggle',
    group: 'core',
    syntax: 'toggle: ObjectName vehicle',
    summary: 'Let MOP optimize an extra object.',
    detail:
      'Adds an object to distance-based optimization. Choose how it is toggled with the mode: whole object, renderer only, as an item, as a vehicle (physics only, save-safe), or multi-renderer.',
    modes: ['object', 'renderer', 'item', 'vehicle', 'vehicle_physics', 'multitoggle'],
  },
  change_parent: {
    kind: 'change_parent',
    group: 'core',
    syntax: 'change_parent: ObjectName NewParent',
    summary: 'Reparent an object when the world loads.',
    detail:
      'Detaches a fragile child from a parent that MOP toggles, so it is never disabled with it. Use "null" as the parent to make the object a root object.',
  },
  no_lod: {
    kind: 'name-required',
    group: 'core',
    syntax: 'no_lod: ObjectName',
    summary: 'Do not build a distant stand-in for this object.',
    detail: 'Opts a single object out of the LOD stand-in system (useful if its clone looks wrong far away).',
  },
  sector: {
    kind: 'sector',
    group: 'advanced',
    syntax: 'sector: x,y,z sx,sy,sz rx,ry,rz [Whitelist...]',
    summary: 'Define a custom indoor sector.',
    detail:
      'Advanced. Creates a box (position, scale, rotation) that culls outside scenery while the player is inside. Trailing names are kept visible (whitelist). Coordinates use commas, no spaces.',
  },

  // Standalone flags (no colon, no arguments) — apply to the whole session.
  ignore_mod_vehicles: {
    kind: 'flag',
    group: 'flags',
    syntax: 'ignore_mod_vehicles',
    summary: 'Only optimize stock vehicles.',
    detail: 'MOP leaves modded vehicles fully alone and manages only the vanilla ones.',
  },
  toggle_all_vehicles_physics_only: {
    kind: 'flag',
    group: 'flags',
    syntax: 'toggle_all_vehicles_physics_only',
    summary: 'Toggle every vehicle physics-only.',
    detail: 'Vehicles are never fully unloaded, only their physics is suspended. Safe default for physics-sensitive setups.',
  },
  no_lods: {
    kind: 'flag',
    group: 'flags',
    syntax: 'no_lods',
    summary: 'Disable the LOD stand-in system entirely.',
    detail: 'No distant clones are created for any object this session.',
  },
  satsuma_ignore_renderer: {
    kind: 'flag',
    group: 'flags',
    syntax: 'satsuma_ignore_renderer',
    summary: 'Never renderer-cull the Satsuma.',
    detail: 'Use if a mod that alters the Satsuma flickers or vanishes when its renderers are toggled.',
  },
  dont_destroy_empty_bottles: {
    kind: 'flag',
    group: 'flags',
    syntax: 'dont_destroy_empty_bottles',
    summary: 'Keep empty beer bottles.',
    detail: 'Stops MOP from destroying empty bottles (for mods that reuse them).',
  },
  skip_fury_collider_fix: {
    kind: 'flag',
    group: 'flags',
    syntax: 'skip_fury_collider_fix',
    summary: 'Skip the FURY collider fix.',
    detail: 'Compatibility escape hatch for the FURY mod.',
  },
};

export const FLAG_DIRECTIVES = Object.keys(DIRECTIVES).filter((k) => DIRECTIVES[k].kind === 'flag');

export function directivesByGroup(group) {
  return Object.entries(DIRECTIVES)
    .filter(([, d]) => d.group === group)
    .map(([name, d]) => ({ name, ...d }));
}

// Human-friendly modes for the `toggle` directive (used by the builder + docs).
export const TOGGLE_MODES = [
  { value: 'object', label: 'Whole object' },
  { value: 'renderer', label: 'Renderer only' },
  { value: 'multitoggle', label: 'All renderers (multi)' },
  { value: 'item', label: 'As an item' },
  { value: 'vehicle', label: 'As a vehicle' },
  { value: 'vehicle_physics', label: 'Vehicle (physics only)' },
];

export const authorTips = [
  'Write spaces in object names as %20 — e.g. SATSUMA(557kg,%20248).',
  'Lines starting with # are comments.',
  'One directive per line. Unknown directives are ignored with a warning in the log.',
  "The file name must match the target mod's ID — e.g. FishingMod.mopconfig.",
];

export const EXAMPLE_RULE = `# MyGreatMod.mopconfig
min_ver: 4.0.0

# Never touch the fragile reactor (it breaks if disabled)
ignore: GreatReactor

# Also optimize the heavy warehouse (whole object)
toggle: GreatWarehouse

# The big sign can stop drawing but must keep its logic
toggle: GreatBillboard renderer

# Detach the trailer so it is never disabled with its parent
change_parent: GreatTrailer null

# Treat the mod car as a vehicle (physics-only, save-safe)
toggle: GreatModCar(1500kg) vehicle`;

// ---------------------------------------------------------------------------
// Builder model — an ordered list of {action, ...fields} the form edits, and a
// serializer that turns it into .mopconfig text. Actions map 1:1 onto parser
// directives but are phrased as author intents.
// ---------------------------------------------------------------------------
export const BUILDER_ACTIONS = [
  { id: 'ignore', directive: 'ignore', fields: ['object', 'fullIgnore'] },
  { id: 'toggle', directive: 'toggle', fields: ['object', 'mode'] },
  { id: 'change_parent', directive: 'change_parent', fields: ['object', 'parent'] },
  { id: 'no_lod', directive: 'no_lod', fields: ['object'] },
];

const esc = (name) => String(name || '').trim().replace(/ /g, '%20');

/** Serializes one builder entry into a directive line (or '' if incomplete). */
export function entryToLine(entry) {
  const object = esc(entry.object);
  switch (entry.action) {
    case 'ignore':
      if (!object) return '';
      return `ignore: ${object}${entry.fullIgnore ? ' fullignore' : ''}`;
    case 'toggle':
      if (!object) return '';
      return `toggle: ${object}${entry.mode && entry.mode !== 'object' ? ` ${entry.mode}` : ''}`;
    case 'change_parent': {
      if (!object) return '';
      const parent = esc(entry.parent) || 'null';
      return `change_parent: ${object} ${parent}`;
    }
    case 'no_lod':
      if (!object) return '';
      return `no_lod: ${object}`;
    default:
      return '';
  }
}

/** Builds a full .mopconfig from the builder state. */
export function buildRuleText({ modId, minVer, entries, flags }) {
  const out = [];
  if (modId) out.push(`# ${modId}.mopconfig`);
  if (minVer) out.push(`min_ver: ${minVer}`);
  if (out.length) out.push('');

  (entries || []).forEach((entry) => {
    const line = entryToLine(entry);
    if (line) {
      if (entry.comment) out.push(`# ${entry.comment}`);
      out.push(line);
    }
  });

  const on = Object.keys(flags || {}).filter((k) => flags[k]);
  if (on.length) {
    out.push('');
    out.push('# Global flags');
    on.forEach((f) => out.push(f));
  }

  return out.join('\n').trim() + '\n';
}

// ---------------------------------------------------------------------------
// Validation
// ---------------------------------------------------------------------------
// Numeric version with an optional letter suffix (e.g. 4.0.0b for a beta).
// The mod's parser strips non-digits per component, so a trailing tag is fine.
const VERSION_RE = /^\d+(\.\d+){0,3}[A-Za-z]*$/;
const VEC_RE = /^-?\d+(\.\d+)?,-?\d+(\.\d+)?,-?\d+(\.\d+)?$/;

/**
 * Parses a .mopconfig and returns per-line diagnostics plus a summary.
 * levels: 'blank' | 'comment' | 'ok' | 'warn' | 'error'
 */
export function parseRuleFile(text) {
  const rawLines = String(text).replace(/\r\n?/g, '\n').split('\n');
  const lines = [];

  let errors = 0;
  let warnings = 0;
  let directiveCount = 0;
  let minVer = null;
  let seenDirective = false;

  const add = (n, raw, level, message) => {
    lines.push({ n, raw, level, message: message || '' });
    if (level === 'error') errors += 1;
    else if (level === 'warn') warnings += 1;
  };

  rawLines.forEach((raw, i) => {
    const n = i + 1;
    const line = raw.trim();

    if (line === '') return add(n, raw, 'blank');
    if (line.startsWith('#')) return add(n, raw, 'comment');

    const colon = line.indexOf(':');

    // No colon -> standalone flag (or unknown).
    if (colon === -1) {
      const key = line.toLowerCase();
      const def = DIRECTIVES[key];
      if (def && def.kind === 'flag') {
        directiveCount += 1;
        seenDirective = true;
        add(n, raw, 'ok', def.summary);
      } else {
        add(n, raw, 'warn', `Unknown directive "${line}" — it will be ignored.`);
      }
      return;
    }

    const key = line.slice(0, colon).trim().toLowerCase();
    const rest = line.slice(colon + 1).trim();
    const args = rest.length ? rest.split(/\s+/) : [];
    const def = DIRECTIVES[key];

    if (!def) {
      add(n, raw, 'warn', `Unknown directive "${key}" — it will be ignored.`);
      return;
    }

    switch (def.kind) {
      case 'version':
        if (!VERSION_RE.test(rest)) {
          add(n, raw, 'error', `Invalid version "${rest}". Expected e.g. 4.0.0.`);
        } else {
          minVer = rest;
          if (seenDirective) add(n, raw, 'warn', 'min_ver should be the first directive in the file.');
          else add(n, raw, 'ok', `Targets MOP ${rest}+`);
        }
        break;

      case 'ignore':
        if (args.length === 0) add(n, raw, 'error', 'ignore needs an object name.');
        else add(n, raw, 'ok', def.summary);
        break;

      case 'toggle':
        if (args.length === 0) {
          add(n, raw, 'error', 'toggle needs an object name.');
        } else if (args.length >= 2 && !def.modes.includes(args[1].toLowerCase())) {
          add(n, raw, 'warn', `Unusual mode "${args[1]}". Known modes: ${def.modes.join(', ')}.`);
        } else {
          add(n, raw, 'ok', def.summary);
        }
        break;

      case 'change_parent':
        if (args.length < 2) {
          add(n, raw, 'error', 'change_parent needs an object and a parent (use "null" for a root object).');
        } else {
          add(n, raw, 'ok', def.summary);
        }
        break;

      case 'name-required':
        if (args.length === 0) add(n, raw, 'error', `${key} needs an object name.`);
        else add(n, raw, 'ok', def.summary);
        break;

      case 'sector':
        if (args.length < 3) {
          add(n, raw, 'error', 'sector needs position, scale and rotation (x,y,z each).');
        } else if (![args[0], args[1], args[2]].every((v) => VEC_RE.test(v))) {
          add(n, raw, 'warn', 'Each of the first three tokens should be "x,y,z" numbers.');
        } else {
          add(n, raw, 'ok', def.summary);
        }
        break;

      default:
        add(n, raw, 'warn', `Unknown directive "${key}".`);
        break;
    }

    directiveCount += 1;
    seenDirective = true;
  });

  return {
    lines,
    errors,
    warnings,
    directiveCount,
    minVer,
    ok: errors === 0,
    hasContent: directiveCount > 0,
  };
}

/** Validates a mod ID / file base name. */
export function validateModId(id) {
  const trimmed = String(id).trim().replace(/\.mopconfig$/i, '');
  if (!trimmed) return { ok: false, id: '', message: 'Enter your mod ID.' };
  if (!/^[A-Za-z0-9_.-]+$/.test(trimmed)) {
    return { ok: false, id: trimmed, message: 'Use only letters, numbers, _ . and -.' };
  }
  return { ok: true, id: trimmed, message: '' };
}

// Machine-readable marker the submission workflow looks for in an issue body.
// It also carries the validated mod ID so the automation never has to guess it.
export const SUBMISSION_MARKER = 'mopr-rule-submission';

/**
 * Builds the issue body a submission opens. The GitHub Action
 * (.github/workflows/rule-submission.yml) reads the `id=` from the marker and
 * the fenced ```mopconfig block, validates it and opens a pull request.
 */
export function buildSubmissionBody(modId, content) {
  const clean = String(content).replace(/```/g, '').replace(/\s+$/, '');
  return (
    `<!-- ${SUBMISSION_MARKER} id=${modId} -->\n` +
    `Submitted from the MOP - REVIVAL rule builder — please don't edit the block below by hand.\n\n` +
    `**Mod ID:** \`${modId}\`\n\n` +
    '```mopconfig\n' +
    clean +
    '\n```\n\n' +
    `_Automation validates this and opens a pull request; a maintainer reviews it before it goes live._`
  );
}

/**
 * Builds a GitHub "new issue" URL prefilled with the rule. Opening an issue
 * needs only a free GitHub account — no fork, no pull request from the author.
 * The submission workflow turns the issue into a validated pull request.
 */
export function buildSubmitUrl(modId, content) {
  const title = `Rule submission: ${modId}.mopconfig`;
  const params = new URLSearchParams({ title, body: buildSubmissionBody(modId, content) });
  return `https://github.com/${REPO}/issues/new?${params.toString()}`;
}
