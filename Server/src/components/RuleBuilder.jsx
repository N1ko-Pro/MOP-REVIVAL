import React, { useMemo, useState } from 'react';
import {
  BUILDER_ACTIONS,
  TOGGLE_MODES,
  MOP_VERSION,
  buildRuleText,
  parseRuleFile,
  validateModId,
  buildSubmitUrl,
  entryToLine,
} from '../lib/ruleSpec.js';
import { useI18n } from '../lib/i18n.jsx';
import Validator from './Validator.jsx';
import ReviewSteps from './ReviewSteps.jsx';

let seq = 0;
const uid = () => `e${seq++}`;

function newEntry(action) {
  return { id: uid(), action, object: '', mode: 'object', parent: 'null', fullIgnore: false, comment: '' };
}

const EXAMPLE_ENTRIES = [
  { id: uid(), action: 'ignore', object: 'GreatReactor', fullIgnore: false, mode: 'object', parent: 'null', comment: 'Breaks if disabled' },
  { id: uid(), action: 'toggle', object: 'GreatWarehouse', mode: 'object', fullIgnore: false, parent: 'null', comment: '' },
  { id: uid(), action: 'toggle', object: 'GreatBillboard', mode: 'renderer', fullIgnore: false, parent: 'null', comment: 'Can stop drawing' },
  { id: uid(), action: 'toggle', object: 'GreatModCar(1500kg)', mode: 'vehicle', fullIgnore: false, parent: 'null', comment: '' },
];

export default function RuleBuilder() {
  const { t } = useI18n();
  const [modId, setModId] = useState('');
  const [minVer, setMinVer] = useState(MOP_VERSION);
  const [entries, setEntries] = useState([]);
  const [nextAction, setNextAction] = useState('ignore');
  const [showAdvanced, setShowAdvanced] = useState(false);
  const [copied, setCopied] = useState(false);

  const text = useMemo(() => buildRuleText({ modId, minVer, entries }), [modId, minVer, entries]);
  const result = useMemo(() => parseRuleFile(text), [text]);
  const idCheck = useMemo(() => validateModId(modId), [modId]);
  const canSubmit = result.ok && result.hasContent && idCheck.ok;
  const readyCount = useMemo(() => entries.filter((e) => entryToLine(e) !== '').length, [entries]);

  const patch = (id, changes) => setEntries((list) => list.map((e) => (e.id === id ? { ...e, ...changes } : e)));
  const remove = (id) => setEntries((list) => list.filter((e) => e.id !== id));
  const move = (id, dir) =>
    setEntries((list) => {
      const i = list.findIndex((e) => e.id === id);
      const j = i + dir;
      if (i < 0 || j < 0 || j >= list.length) return list;
      const copy = list.slice();
      [copy[i], copy[j]] = [copy[j], copy[i]];
      return copy;
    });

  function copy() {
    navigator.clipboard?.writeText(text).then(() => {
      setCopied(true);
      setTimeout(() => setCopied(false), 1500);
    });
  }

  function download() {
    const id = idCheck.ok ? idCheck.id : 'MyMod';
    const blob = new Blob([text], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${id}.mopconfig`;
    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(url);
  }

  return (
    <div className="page">
      <div className="page__head">
        <p className="page__eyebrow">{t('builder.eyebrow')}</p>
        <h1>{t('builder.title')}</h1>
        <p className="page__lead">{t('builder.lead')}</p>
      </div>

      <div className="builder">
        {/* Step 1 — mod */}
        <section className="builder__step">
          <h2>{t('builder.step1')}</h2>
          <div className="field-row">
            <label className="field">
              <span>{t('builder.modIdLabel')}</span>
              <input type="text" value={modId} spellCheck={false} placeholder="FishingMod" onChange={(e) => setModId(e.target.value)} />
              <small>{t('builder.modIdHint')}</small>
              {!idCheck.ok && modId.trim() !== '' && <small className="field__err">{idCheck.message}</small>}
            </label>
            <label className="field field--sm">
              <span>{t('builder.minVerLabel')}</span>
              <input type="text" value={minVer} spellCheck={false} placeholder={MOP_VERSION} onChange={(e) => setMinVer(e.target.value)} />
            </label>
          </div>
        </section>

        {/* Step 2 — rules */}
        <section className="builder__step">
          <h2>
            {t('builder.step2')}
            {entries.length > 0 && (
              <span className="builder__count">{t('builder.rulesReady', { done: readyCount, total: entries.length })}</span>
            )}
          </h2>

          <div className="builder__add">
            <select value={nextAction} onChange={(e) => setNextAction(e.target.value)} aria-label="Rule type">
              {BUILDER_ACTIONS.map((a) => (
                <option key={a.id} value={a.id}>
                  {t(`builder.actions.${a.id}`)}
                </option>
              ))}
            </select>
            <button type="button" className="btn btn--primary btn--sm" onClick={() => setEntries((l) => [...l, newEntry(nextAction)])}>
              + {t('builder.addRule')}
            </button>
            <span className="builder__add-spacer" />
            <button type="button" className="btn btn--ghost btn--sm" onClick={() => { setEntries(EXAMPLE_ENTRIES.map((e) => ({ ...e, id: uid() }))); if (!modId) setModId('MyGreatMod'); }}>
              {t('builder.loadExample')}
            </button>
            <button type="button" className="btn btn--ghost btn--sm" onClick={() => setEntries([])} disabled={entries.length === 0}>
              {t('builder.clear')}
            </button>
          </div>

          {entries.length === 0 && <p className="builder__empty">{t('builder.empty')}</p>}

          <ul className="builder__list">
            {entries.map((e, i) => {
              const objEmpty = e.object.trim() === '';
              const complete = entryToLine(e) !== '';
              return (
              <li key={e.id} className={`builder__entry${complete ? '' : ' builder__entry--todo'}`}>
                <div className="builder__entry-head">
                  <span className="builder__entry-kind">{t(`builder.actions.${e.action}`)}</span>
                  <span className={`builder__entry-status ${complete ? 'is-ready' : 'is-todo'}`}>
                    {complete ? t('builder.entryReady') : t('builder.entryIncomplete')}
                  </span>
                  <span className="builder__entry-tools">
                    <button type="button" title={t('builder.moveUp')} disabled={i === 0} onClick={() => move(e.id, -1)}>↑</button>
                    <button type="button" title={t('builder.moveDown')} disabled={i === entries.length - 1} onClick={() => move(e.id, 1)}>↓</button>
                    <button type="button" title={t('builder.remove')} className="builder__del" onClick={() => remove(e.id)}>✕</button>
                  </span>
                </div>
                <p className="builder__entry-help">{t(`builder.actionHelp.${e.action}`)}</p>

                <div className="field-row">
                  <label className={`field${objEmpty ? ' field--required' : ''}`}>
                    <span>{t('builder.fields.object')}</span>
                    <input type="text" value={e.object} spellCheck={false} placeholder={t('builder.objectPlaceholder')} onChange={(ev) => patch(e.id, { object: ev.target.value })} />
                    {objEmpty && <small className="field__req">{t('builder.objectRequired')}</small>}
                  </label>

                  {e.action === 'toggle' && (
                    <label className="field field--sm">
                      <span>{t('builder.fields.mode')}</span>
                      <select value={e.mode} onChange={(ev) => patch(e.id, { mode: ev.target.value })}>
                        {TOGGLE_MODES.map((m) => (
                          <option key={m.value} value={m.value}>{m.label}</option>
                        ))}
                      </select>
                    </label>
                  )}

                  {e.action === 'change_parent' && (
                    <label className="field field--sm">
                      <span>{t('builder.fields.parent')}</span>
                      <input type="text" value={e.parent} spellCheck={false} placeholder="null" onChange={(ev) => patch(e.id, { parent: ev.target.value })} />
                    </label>
                  )}
                </div>

                {e.action === 'ignore' && (
                  <label className="check">
                    <input type="checkbox" checked={e.fullIgnore} onChange={(ev) => patch(e.id, { fullIgnore: ev.target.checked })} />
                    <span>{t('builder.fields.fullIgnore')}</span>
                  </label>
                )}

                <label className="field">
                  <span>{t('builder.commentLabel')}</span>
                  <input type="text" value={e.comment} placeholder={t('builder.commentPlaceholder')} onChange={(ev) => patch(e.id, { comment: ev.target.value })} />
                </label>
              </li>
              );
            })}
          </ul>
        </section>

        {/* Step 3 — review & submit */}
        <section className="builder__step">
          <h2>{t('builder.step4')}</h2>

          <div className={`builder__checklist${canSubmit ? ' is-complete' : ''}`}>
            <span className="builder__checklist-head">{t('builder.checklistHead')}</span>
            <ul>
              <li className={idCheck.ok ? 'is-done' : 'is-todo'}>{t('builder.checklist.modId')}</li>
              <li className={result.hasContent ? 'is-done' : 'is-todo'}>{t('builder.checklist.content')}</li>
              <li className={result.ok ? 'is-done' : 'is-todo'}>{t('builder.checklist.valid')}</li>
            </ul>
            {canSubmit && <p className="builder__checklist-ok">{t('builder.allDone')}</p>}
          </div>

          <div className="builder__preview">
            <div className="builder__preview-head">
              <span>{t('builder.previewTitle')}</span>
              <span className={`validator__status ${statusClass(result)}`}>
                {result.ok
                  ? t('validator.valid', { directives: result.directiveCount, warnings: result.warnings })
                  : t('validator.invalid', { errors: result.errors, warnings: result.warnings })}
              </span>
            </div>
            <pre className="code"><code>{text}</code></pre>
            <div className="validator__actions">
              <button type="button" className="btn btn--ghost" onClick={copy}>{copied ? t('common.copied') : t('common.copy')}</button>
              <button type="button" className="btn btn--ghost" onClick={download} disabled={!result.hasContent}>{t('common.download')}</button>
              <button
                type="button"
                className="btn btn--primary"
                disabled={!canSubmit}
                title={canSubmit ? '' : t('validator.submitDisabled')}
                onClick={() => canSubmit && window.open(buildSubmitUrl(idCheck.id, text), '_blank', 'noopener')}
              >
                {t('validator.submit')}
              </button>
            </div>
            <p className="validator__hint">{t('validator.submitHint')}</p>
          </div>

          <details className="builder__adv" open={showAdvanced} onToggle={(e) => setShowAdvanced(e.target.open)}>
            <summary>{t('builder.advanced')}</summary>
            {showAdvanced && <Validator />}
          </details>
        </section>
      </div>

      <ReviewSteps />
    </div>
  );
}

function statusClass(result) {
  if (!result.hasContent) return 'is-idle';
  if (!result.ok) return 'is-error';
  if (result.warnings > 0) return 'is-warn';
  return 'is-ok';
}
