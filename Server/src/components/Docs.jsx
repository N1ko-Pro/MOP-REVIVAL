import React, { useEffect, useState } from 'react';
import { useI18n } from '../lib/i18n.jsx';
import { useHashRoute } from '../lib/router.js';

// Detailed docs content lives here, colocated and bilingual, so the shared
// i18n dictionary stays lean. Section headers/labels come from i18n.
const CONTENT = {
  en: {
    settings: [
      ['Performance', 'Pick a preset (Ultra Quality → Performance) that automatically sets how far objects stay loaded — there is no manual slider. "Other" lets you disable or fully destroy empty bottles to cut down on clutter (only one of the two applies).'],
      ['Optimization', 'The optimization modules. Basic: items, locations and vehicles. Advanced: optimize the Satsuma while driving, game-engine code patches and distant-physics sleep. Graphical: dynamic draw distance (eases the horizon indoors / near home) and indoor scenery culling.'],
      ['Graphics', 'Adjust the shadow distance and cap the framerate with an FPS limit.'],
      ['Game', 'Keep the game running in the background, and adaptive garbage cleanup — off by default; it frees memory periodically but can cause brief hitches.'],
      ['Fixes', 'Optional fixes: stop the Satsuma sliding downhill with the handbrake on, hide the flickering shallow-water lake weed, and hide tyre skidmarks that leak memory over time.'],
      ['Save protection', 'Protect save files, verify save integrity, restore bolt tightness the game resets by itself, and auto-backup saves — plus manual "back up now" and "restore latest backup" buttons.'],
      ['Rules server', 'Shows whether the rules server is reachable, checks for rule updates for your installed mods and opens the rules website.'],
      ['Diagnostics', 'The on-screen debug monitor (arrow keys switch pages) and detailed log messages — both off by default. The Emergency-mode "Disable optimization" switch turns MOP off entirely and re-enables everything.'],
    ],
    faq: [
      ['Can MOP corrupt my save?', 'No. Vehicles are only ever suspended by their physics while parked far away, and everything managed is re-enabled before the game writes a save. MOP also backs up and verifies your save.'],
      ['Does it work with modded cars and mods?', 'Yes. Vehicles (including modded ones) are auto-detected and handled physics-only. For objects that misbehave, a rule file tells MOP to leave them alone — and those download automatically for the mods you have installed.'],
      ['Something disappeared or a quest broke.', 'Try toggling the relevant module off, or flip the "Disable optimization" emergency switch. If it\u2019s a specific mod, a rule file is the proper fix. Please also run "mopr report" and attach the file to a bug report.'],
      ['How do the presets differ?', 'Each preset automatically sets how far objects stay loaded — Ultra Quality keeps them loaded furthest, Performance the least. There is no manual distance slider; everything else is controlled by the individual toggles.'],
    ],
    commands: [
      ['mopr status', 'Managed/active object counts and current settings.'],
      ['mopr perf', 'FPS, frame spikes and garbage-collection diagnostics.'],
      ['mopr overlay', 'Toggle the on-screen debug monitor.'],
      ['mopr rules', 'List the loaded compatibility rules and their sources.'],
      ['mopr report', 'Write a full diagnostics report and open it.'],
      ['mopr backup / restore / verify', 'Back up, restore or verify your save.'],
    ],
  },
  ru: {
    settings: [
      ['Быстродействие', 'Выберите пресет (Ultra Quality → Performance), который автоматически задаёт дальность загрузки объектов — ползунка вручную больше нет. В блоке «Другое» можно отключать или полностью уничтожать пустые бутылки, чтобы не копился мусор (работает что-то одно из двух).'],
      ['Оптимизация', 'Модули оптимизации. Базовые: предметы, локации и транспорт. Продвинутые: оптимизация Satsuma в режиме вождения, патчи кода движка и усыпление дальней физики. Графические: динамическая дальность прорисовки (сглаживает горизонт в помещении/у дома) и отсечение пейзажа в помещениях.'],
      ['Графика', 'Настройка дальности теней и ограничение частоты кадров (лимит FPS).'],
      ['Игра', 'Работа в фоне и адаптивная чистка мусора — по умолчанию выключена; периодически освобождает память, но возможны короткие подлагивания.'],
      ['Исправления', 'Необязательные фиксы: не даёт Satsuma сползать с холма с поднятым ручником, скрывает мерцающую ряску на мелководье и убирает следы шин, которые со временем расходуют память.'],
      ['Защита сохранений', 'Защита файлов сейва, проверка целостности, восстановление затяжки болтов, которую игра сбрасывает сама, и автобэкапы — плюс кнопки «сделать бэкап сейчас» и «восстановить последний бэкап».'],
      ['Сервер правил', 'Показывает доступность сервера правил, проверяет обновления правил для установленных модов и открывает сайт правил.'],
      ['Отладка', 'Экранный монитор отладки (страницы — стрелками) и подробные лог-сообщения — оба по умолчанию выключены. Переключатель аварийного режима «Отключить оптимизацию» полностью выключает мод и возвращает всё на место.'],
    ],
    faq: [
      ['Может ли MOP испортить сохранение?', 'Нет. У припаркованного вдали транспорта замораживается только физика, и всё управляемое возвращается до записи сейва. MOP также делает бэкап и проверяет сохранение.'],
      ['Работает ли с модовыми машинами и модами?', 'Да. Транспорт (в т.ч. модовый) определяется автоматически и обрабатывается только по физике. Для проблемных объектов файл правил говорит моду не трогать их — и он скачивается автоматически для установленных модов.'],
      ['Что-то исчезло или сломался квест.', 'Попробуйте выключить соответствующий модуль или аварийный переключатель «Отключить оптимизацию». Если дело в конкретном моде — правильное решение это файл правил. Также выполните «mopr report» и приложите файл к баг-репорту.'],
      ['Чем отличаются пресеты?', 'Каждый пресет автоматически задаёт дальность загрузки объектов: Ultra Quality держит их дальше всех, Performance — ближе всех. Ползунка дистанции вручную нет; остальное задаётся отдельными переключателями.'],
    ],
    commands: [
      ['mopr status', 'Счётчики управляемых/активных объектов и текущие настройки.'],
      ['mopr perf', 'FPS, спайки кадров и диагностика сборки мусора.'],
      ['mopr overlay', 'Переключить экранный монитор отладки.'],
      ['mopr rules', 'Список загруженных правил совместимости и их источников.'],
      ['mopr report', 'Записать полный отчёт диагностики и открыть его.'],
      ['mopr backup / restore / verify', 'Бэкап, восстановление или проверка сохранения.'],
    ],
  },
};

const sectionIds = ['start', 'settings', 'faq', 'console'];

export default function Docs() {
  const { t, lang } = useI18n();
  const { sub } = useHashRoute();
  const [active, setActive] = useState('start');
  const c = CONTENT[lang] || CONTENT.en;

  // Highlight the section currently in view.
  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => entries.forEach((e) => e.isIntersecting && setActive(e.target.id)),
      { rootMargin: '-30% 0px -60% 0px', threshold: 0 },
    );
    sectionIds.forEach((id) => {
      const el = document.getElementById(id);
      if (el) observer.observe(el);
    });
    return () => observer.disconnect();
  }, []);

  // Scroll to the section named in the URL (#/docs/<sub>) — covers sidebar
  // clicks, direct links and browser back/forward. Offset is handled in CSS
  // via scroll-margin-top so the heading clears the sticky nav.
  useEffect(() => {
    if (!sub || !sectionIds.includes(sub)) return;
    setActive(sub);
    const el = document.getElementById(sub);
    if (el) window.requestAnimationFrame(() => el.scrollIntoView({ behavior: 'smooth', block: 'start' }));
  }, [sub]);

  return (
    <div className="page docs">
      <aside className="docs__side">
        <nav>
          <p className="docs__side-title">{t('docs.nav')}</p>
          {sectionIds.map((id) => (
            <a key={id} href={`#/docs/${id}`} className={active === id ? 'is-active' : ''}>
              {t(`docs.s.${id}`)}
            </a>
          ))}
        </nav>
      </aside>

      <div className="docs__body">
        <div className="page__head">
          <p className="page__eyebrow">{t('docs.eyebrow')}</p>
          <h1>{t('docs.title')}</h1>
          <p className="page__lead">{t('docs.lead')}</p>
        </div>

        <section id="start" className="section-block">
          <h2>{t('docs.s.start')}</h2>
          <h3 className="sub">{t('docs.reqHead')}</h3>
          <ul className="ticks">
            <li>{t('docs.req1')}</li>
            <li>
              <a href="https://github.com/piotrulos/MSCModLoader" target="_blank" rel="noreferrer">MSCLoader</a>.
            </li>
          </ul>
          <h3 className="sub">{t('docs.installHead')}</h3>
          <ol className="steps">
            <li>{t('docs.i1')}</li>
            <li>{t('docs.i2')}</li>
            <li>{t('docs.i3')}</li>
          </ol>
          <div className="callout">
            <p>{t('docs.firstRun')}</p>
          </div>
        </section>

        <section id="settings" className="section-block">
          <h2>{t('docs.s.settings')}</h2>
          <p>{t('docs.settingsIntro')}</p>
          <div className="deflist">
            {c.settings.map(([term, desc]) => (
              <div key={term} className="deflist__item">
                <div className="deflist__term">{term}</div>
                <div className="deflist__desc">{desc}</div>
              </div>
            ))}
          </div>
        </section>

        <section id="faq" className="section-block">
          <h2>{t('docs.faqHead')}</h2>
          {c.faq.map(([q, a]) => (
            <div key={q} className="qa">
              <h3>{q}</h3>
              <p>{a}</p>
            </div>
          ))}
        </section>

        <section id="console" className="section-block">
          <h2>{t('docs.consoleHead')}</h2>
          <p>{t('docs.consoleIntro')}</p>
          <div className="table">
            <div className="table__head">
              <span>{t('docs.colCommand')}</span>
              <span>{t('docs.colMeaning')}</span>
            </div>
            {c.commands.map(([cmd, desc]) => (
              <div key={cmd} className="table__row">
                <code>{cmd}</code>
                <div><p>{desc}</p></div>
              </div>
            ))}
          </div>
          <div className="callout">
            <p>{t('docs.reportNote')}</p>
          </div>
        </section>
      </div>
    </div>
  );
}
