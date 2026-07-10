import React from 'react';
import { directivesByGroup, authorTips, EXAMPLE_RULE } from '../lib/ruleSpec.js';
import { useI18n, directiveText } from '../lib/i18n.jsx';

function useDirText() {
  const { lang } = useI18n();
  return (d) => {
    const loc = directiveText[lang] && directiveText[lang][d.name];
    return { summary: (loc && loc.summary) || d.summary, detail: (loc && loc.detail) || d.detail };
  };
}

function DirectiveTable({ rows }) {
  const { t } = useI18n();
  const dt = useDirText();
  return (
    <div className="table">
      <div className="table__head">
        <span>{t('format.colSyntax')}</span>
        <span>{t('format.colMeaning')}</span>
      </div>
      {rows.map((d) => {
        const { summary, detail } = dt(d);
        return (
          <div key={d.name} className="table__row">
            <code>{d.syntax}</code>
            <div>
              <strong>{summary}</strong>
              <p>{detail}</p>
            </div>
          </div>
        );
      })}
    </div>
  );
}

export default function Format() {
  const { t } = useI18n();

  return (
    <div className="page">
      <div className="page__head">
        <p className="page__eyebrow">{t('format.eyebrow')}</p>
        <h1>{t('format.title')}</h1>
        <p className="page__lead">{t('format.lead')}</p>
        <a className="btn btn--primary" href="#/builder">{t('hero.ctaBuild')}</a>
      </div>

      <section className="section-block">
        <h2>{t('format.whyHead')}</h2>
        <p>{t('format.whyBody')}</p>
        <ul className="ticks">
          {authorTips.map((tip) => (
            <li key={tip}>{tip}</li>
          ))}
        </ul>
      </section>

      <section className="section-block">
        <h2>{t('format.coreHead')}</h2>
        <DirectiveTable rows={directivesByGroup('core')} />

        <h3 className="sub">{t('format.headerHead')}</h3>
        <DirectiveTable rows={directivesByGroup('meta')} />

        <h3 className="sub">{t('format.flagsHead')}</h3>
        <DirectiveTable rows={directivesByGroup('flags')} />

        <h3 className="sub">{t('format.advancedHead')}</h3>
        <DirectiveTable rows={directivesByGroup('advanced')} />
      </section>

      <section className="section-block">
        <h2>{t('format.exampleHead')}</h2>
        <pre className="code">
          <code>{EXAMPLE_RULE}</code>
        </pre>
      </section>
    </div>
  );
}
