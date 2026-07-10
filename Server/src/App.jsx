import React, { useState } from 'react';
import Home from './components/Home.jsx';
import Docs from './components/Docs.jsx';
import Format from './components/Format.jsx';
import RuleBuilder from './components/RuleBuilder.jsx';
import Database from './components/Database.jsx';
import Footer from './components/Footer.jsx';
import { useHashRoute } from './lib/router.js';
import { useI18n, LANGS } from './lib/i18n.jsx';
import iconUrl from './assets/icon.png';

const order = ['home', 'builder', 'format', 'database', 'docs'];

const pages = {
  home: Home,
  builder: RuleBuilder,
  format: Format,
  database: Database,
  docs: Docs,
};

export default function App() {
  const { page } = useHashRoute();
  const { t, lang, setLang } = useI18n();
  const [menuOpen, setMenuOpen] = useState(false);
  const Page = pages[page] || Home;

  return (
    <>
      <header className="nav">
        <a className="nav__brand" href="#/" aria-label="MOPR" onClick={() => setMenuOpen(false)}>
          <img className="nav__logo" src={iconUrl} alt="MOPR" />
        </a>

        <button
          type="button"
          className="nav__burger"
          aria-label="Menu"
          aria-expanded={menuOpen}
          onClick={() => setMenuOpen((v) => !v)}
        >
          <span />
          <span />
          <span />
        </button>

        <nav className={`nav__links${menuOpen ? ' is-open' : ''}`}>
          {order.map((p) => (
            <a
              key={p}
              href={`#/${p}`}
              className={page === p ? 'is-active' : ''}
              onClick={() => setMenuOpen(false)}
            >
              {t(`nav.${p}`)}
            </a>
          ))}

          <div className="nav__lang" role="group" aria-label="Language">
            {LANGS.map((l) => (
              <button
                key={l.code}
                type="button"
                className={lang === l.code ? 'is-active' : ''}
                onClick={() => setLang(l.code)}
                title={l.name}
              >
                {l.label}
              </button>
            ))}
          </div>
        </nav>
      </header>

      <main id="top">
        <Page />
      </main>

      <Footer />
    </>
  );
}
