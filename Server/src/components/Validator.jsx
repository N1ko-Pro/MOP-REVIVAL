import React, { useMemo, useRef, useState } from 'react';
import { parseRuleFile, validateModId, buildSubmitUrl, EXAMPLE_RULE } from '../lib/ruleSpec.js';
import { useI18n } from '../lib/i18n.jsx';

export default function Validator() {
  const { t } = useI18n();
  const [text, setText] = useState('');
  const [modId, setModId] = useState('');
  const [dragging, setDragging] = useState(false);
  const fileInput = useRef(null);

  const result = useMemo(() => parseRuleFile(text), [text]);
  const idCheck = useMemo(() => validateModId(modId), [modId]);

  const touched = text.trim().length > 0;
  const canSubmit = touched && result.ok && result.hasContent && idCheck.ok;

  function loadFile(file) {
    if (!file) return;
    const base = file.name.replace(/\.mopconfig$/i, '');
    const reader = new FileReader();
    reader.onload = () => {
      setText(String(reader.result || ''));
      if (!modId) setModId(base);
    };
    reader.readAsText(file);
  }

  function onDrop(event) {
    event.preventDefault();
    setDragging(false);
    loadFile(event.dataTransfer.files && event.dataTransfer.files[0]);
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

  const shownLines = result.lines.filter((l) => l.level !== 'blank');

  return (
    <div className="validator" id="validator">
      <div className="validator__head">
        <h3>{t('validator.title')}</h3>
        <p>{t('validator.lead')}</p>
      </div>

      <div className="validator__grid">
        <div
          className={`validator__editor${dragging ? ' is-dragging' : ''}`}
          onDragOver={(e) => {
            e.preventDefault();
            setDragging(true);
          }}
          onDragLeave={() => setDragging(false)}
          onDrop={onDrop}
        >
          <div className="validator__toolbar">
            <label className="validator__id">
              {t('validator.modId')}
              <input type="text" value={modId} spellCheck={false} placeholder="FishingMod" onChange={(e) => setModId(e.target.value)} />
            </label>
            <div className="validator__toolbar-actions">
              <button type="button" className="btn btn--ghost btn--sm" onClick={() => fileInput.current && fileInput.current.click()}>
                {t('validator.openFile')}
              </button>
              <button type="button" className="btn btn--ghost btn--sm" onClick={() => setText(EXAMPLE_RULE)}>
                {t('validator.loadExample')}
              </button>
              <input ref={fileInput} type="file" accept=".mopconfig,text/plain" hidden onChange={(e) => loadFile(e.target.files && e.target.files[0])} />
            </div>
          </div>
          {!idCheck.ok && modId.trim() !== '' && <p className="validator__idnote">{idCheck.message}</p>}
          <textarea
            className="validator__text"
            spellCheck={false}
            value={text}
            placeholder={t('validator.placeholder')}
            onChange={(e) => setText(e.target.value)}
          />
        </div>

        <div className="validator__report">
          <div className={`validator__status ${statusClass(touched, result)}`}>
            {!touched && t('validator.waiting')}
            {touched && result.ok && t('validator.valid', { directives: result.directiveCount, warnings: result.warnings })}
            {touched && !result.ok && t('validator.invalid', { errors: result.errors, warnings: result.warnings })}
          </div>

          <ul className="validator__lines">
            {shownLines.map((line) => (
              <li key={line.n} className={`vline vline--${line.level}`}>
                <span className="vline__n">{line.n}</span>
                <span className="vline__code">{line.raw.trim() || ' '}</span>
                {line.message && <span className="vline__msg">{line.message}</span>}
              </li>
            ))}
            {shownLines.length === 0 && <li className="vline vline--comment">{t('validator.noDirectives')}</li>}
          </ul>

          <div className="validator__actions">
            <button type="button" className="btn btn--ghost" onClick={download} disabled={!touched}>
              {t('common.download')}
            </button>
            <button type="button" className="btn btn--primary" onClick={() => canSubmit && window.open(buildSubmitUrl(idCheck.id, text), '_blank', 'noopener')} disabled={!canSubmit} title={canSubmit ? '' : t('validator.submitDisabled')}>
              {t('validator.submit')}
            </button>
          </div>
          <p className="validator__hint">{t('validator.submitHint')}</p>
        </div>
      </div>
    </div>
  );
}

function statusClass(touched, result) {
  if (!touched) return 'is-idle';
  if (!result.ok) return 'is-error';
  if (result.warnings > 0) return 'is-warn';
  return 'is-ok';
}
