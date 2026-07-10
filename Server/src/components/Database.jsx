import React, { useEffect, useMemo, useState } from 'react';
import { parseRuleFile } from '../lib/ruleSpec.js';
import { useI18n } from '../lib/i18n.jsx';

export default function Database() {
  const { t } = useI18n();
  const [manifest, setManifest] = useState(null);
  const [error, setError] = useState(null);
  const [query, setQuery] = useState('');
  const [openId, setOpenId] = useState(null);

  useEffect(() => {
    // Bypass the browser HTTP cache so the site always shows the current rule
    // count. The manifest keeps a 5-min Cache-Control for the mod's own sync.
    fetch('manifest.json', { cache: 'no-store' })
      .then((response) => {
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        return response.json();
      })
      .then(setManifest)
      .catch((err) => setError(err.message));
  }, []);

  const filtered = useMemo(() => {
    if (!manifest) return [];
    const needle = query.trim().toLowerCase();
    if (!needle) return manifest.rules;
    return manifest.rules.filter((rule) => rule.id.toLowerCase().includes(needle));
  }, [manifest, query]);

  return (
    <div className="page">
      <div className="page__head">
        <p className="page__eyebrow">{t('database.eyebrow')}</p>
        <h1>{t('database.title')}</h1>
        <p className="page__lead">{t('database.lead')}</p>
      </div>

      {error && <p className="db__status db__status--error">{t('database.loadError', { error })}</p>}
      {!manifest && !error && <p className="db__status">{t('database.loading')}</p>}

      {manifest && (
        <>
          <div className="db__bar">
            <input
              type="search"
              placeholder={t('database.searchPlaceholder', { count: manifest.count })}
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              aria-label="Search rule files"
            />
            <span className="db__count">
              {filtered.length} / {manifest.count}
            </span>
          </div>

          <ul className="db__list db__list--rows">
            {filtered.map((rule) => (
              <RuleRow
                key={rule.id}
                rule={rule}
                open={openId === rule.id}
                onToggle={() => setOpenId(openId === rule.id ? null : rule.id)}
              />
            ))}
          </ul>
        </>
      )}
    </div>
  );
}

function RuleRow({ rule, open, onToggle }) {
  const { t } = useI18n();
  const [content, setContent] = useState(null);
  const [loadError, setLoadError] = useState(null);

  useEffect(() => {
    if (!open || content !== null) return;
    fetch(rule.path)
      .then((r) => {
        if (!r.ok) throw new Error(`HTTP ${r.status}`);
        return r.text();
      })
      .then(setContent)
      .catch((e) => setLoadError(e.message));
  }, [open, content, rule.path]);

  const parsed = content !== null ? parseRuleFile(content) : null;

  return (
    <li className={`rule${open ? ' is-open' : ''}`}>
      <button type="button" className="rule__summary" onClick={onToggle} aria-expanded={open}>
        <span className="rule__chevron">{open ? '▾' : '▸'}</span>
        <span className="rule__id">{rule.id}</span>
        {typeof rule.minVer === 'string' && <span className="rule__badge">MOP {rule.minVer}+</span>}
        {typeof rule.directiveCount === 'number' && (
          <span className="rule__badge rule__badge--muted">{rule.directiveCount} {t('database.rules')}</span>
        )}
        <span className="rule__sha" title={rule.sha256}>{rule.sha256.slice(0, 8)}</span>
      </button>

      {open && (
        <div className="rule__detail">
          {loadError && <p className="db__status db__status--error">{loadError}</p>}
          {content === null && !loadError && <p className="db__status">{t('common.loading')}</p>}
          {parsed && (
            <>
              <ul className="rule__what">
                {parsed.lines
                  .filter((l) => l.level === 'ok')
                  .map((l) => (
                    <li key={l.n}>
                      <code>{l.raw.trim()}</code>
                      {l.message && <span> — {l.message}</span>}
                    </li>
                  ))}
                {parsed.directiveCount === 0 && <li>{t('database.noActive')}</li>}
              </ul>
              <pre className="code code--sm">
                <code>{content}</code>
              </pre>
              <a className="rule__download" href={rule.path} download>
                {t('database.downloadFile', { id: rule.id })}
              </a>
            </>
          )}
        </div>
      )}
    </li>
  );
}
