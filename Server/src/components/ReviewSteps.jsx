import React from 'react';
import { useI18n } from '../lib/i18n.jsx';

export default function ReviewSteps() {
  const { t } = useI18n();
  const steps = [
    { t: t('review.s1t'), b: t('review.s1b') },
    { t: t('review.s2t'), b: t('review.s2b') },
    { t: t('review.s3t'), b: t('review.s3b') },
    { t: t('review.s4t'), b: t('review.s4b') },
  ];
  return (
    <section className="section-block">
      <h2>{t('review.head')}</h2>
      <ol className="pipeline">
        {steps.map((s, i) => (
          <li key={i} className="pipeline__step">
            <span className="pipeline__num">{i + 1}</span>
            <div>
              <strong>{s.t}</strong>
              <p>{s.b}</p>
            </div>
          </li>
        ))}
      </ol>
    </section>
  );
}
