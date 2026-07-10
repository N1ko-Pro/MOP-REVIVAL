import React from 'react';
import { useI18n } from '../lib/i18n.jsx';

const keys = ['scheduler', 'diag', 'crash', 'compat', 'vehicle', 'lang'];

export default function Features() {
  const { t } = useI18n();
  return (
    <section id="features" className="section">
      <div className="section__head">
        <h2>{t('home.featHead')}</h2>
        <p>{t('home.featLead')}</p>
      </div>
      <div className="grid grid--features">
        {keys.map((k) => (
          <article key={k} className="card">
            <h3>{t(`features.${k}.t`)}</h3>
            <p>{t(`features.${k}.b`)}</p>
          </article>
        ))}
      </div>
    </section>
  );
}
