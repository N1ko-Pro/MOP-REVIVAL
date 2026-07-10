import React from 'react';
import Hero from './Hero.jsx';
import Features from './Features.jsx';
import { useI18n } from '../lib/i18n.jsx';

export default function Home() {
  const { t } = useI18n();

  const paths = [
    { href: '#/builder', badge: t('common.forAuthors'), title: t('home.authorTitle'), body: t('home.authorBody'), cta: t('home.authorCta'), primary: true },
    { href: '#/docs', badge: t('common.forPlayers'), title: t('home.playerTitle'), body: t('home.playerBody'), cta: t('home.playerCta') },
    { href: '#/database', badge: t('common.compatibility'), title: t('home.browseTitle'), body: t('home.browseBody'), cta: t('home.browseCta') },
  ];

  return (
    <>
      <Hero />
      <section className="section">
        <div className="section__head">
          <h2>{t('home.pathsHead')}</h2>
          <p>{t('home.pathsLead')}</p>
        </div>
        <div className="grid grid--paths">
          {paths.map((p) => (
            <a key={p.href} className={`card card--path${p.primary ? ' card--primary' : ''}`} href={p.href}>
              <span className="card__badge">{p.badge}</span>
              <h3>{p.title}</h3>
              <p>{p.body}</p>
              <span className="card__cta">{p.cta}</span>
            </a>
          ))}
        </div>
      </section>
      <Features />
    </>
  );
}
