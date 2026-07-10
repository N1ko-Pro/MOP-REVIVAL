import React from 'react';
import { useI18n } from '../lib/i18n.jsx';
import { MOP_VERSION } from '../lib/ruleSpec.js';
import iconUrl from '../assets/icon.png';

export default function Hero() {
  const { t } = useI18n();
  return (
    <section className="hero">
      <div className="hero__inner">
        <img className="hero__icon" src={iconUrl} alt="MOPR" width="112" height="112" />
        <p className="hero__eyebrow">{t('hero.eyebrow')}</p>
        <h1 className="hero__title">
          MOP <span className="hero__accent">Revival</span>
        </h1>
        <p className="hero__lead">{t('hero.lead')}</p>
        <div className="hero__cta">
          <a className="btn btn--primary" href="#/builder">
            {t('hero.ctaBuild')}
          </a>
          <a className="btn btn--ghost" href="#/database">
            {t('hero.ctaBrowse')}
          </a>
        </div>
        <dl className="hero__stats">
          <div>
            <dt>{t('hero.version')}</dt>
            <dd>{MOP_VERSION}</dd>
          </div>
          <div>
            <dt>{t('hero.runtime')}</dt>
            <dd>.NET 3.5 · MSCLoader</dd>
          </div>
          <div>
            <dt>{t('hero.license')}</dt>
            <dd>GPLv3</dd>
          </div>
        </dl>
      </div>
    </section>
  );
}
