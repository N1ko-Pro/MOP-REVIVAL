import React from 'react';
import { useI18n } from '../lib/i18n.jsx';

export default function Footer() {
  const { t } = useI18n();
  return (
    <footer className="footer">
      <div className="footer__inner">
        <p>
          <strong>MOP - REVIVAL</strong> — {t('footer.tagline')}
        </p>
        <p className="footer__fine">{t('footer.fine')}</p>
      </div>
    </footer>
  );
}
