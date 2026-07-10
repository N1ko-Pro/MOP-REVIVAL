import React, { createContext, useContext, useEffect, useMemo, useState } from 'react';

// Lightweight i18n: English (default) + Russian. Choice is persisted in localStorage.
// t('a.b.c', {x: 1}) resolves a dot-path in the active language, falling back to
// English, then to the key itself. Interpolates {name} placeholders.

export const LANGS = [
  { code: 'en', label: 'EN', name: 'English' },
  { code: 'ru', label: 'RU', name: 'Русский' },
];

const STORAGE_KEY = 'mopr.lang';
const I18nContext = createContext(null);

function get(obj, path) {
  return path.split('.').reduce((acc, key) => (acc == null ? undefined : acc[key]), obj);
}

function interpolate(str, vars) {
  if (!vars || typeof str !== 'string') return str;
  return str.replace(/\{(\w+)\}/g, (m, k) => (k in vars ? String(vars[k]) : m));
}

export function I18nProvider({ children }) {
  const [lang, setLangState] = useState(() => {
    const saved = typeof localStorage !== 'undefined' ? localStorage.getItem(STORAGE_KEY) : null;
    return LANGS.some((l) => l.code === saved) ? saved : 'en';
  });

  useEffect(() => {
    try {
      localStorage.setItem(STORAGE_KEY, lang);
    } catch {
      /* ignore */
    }
    if (typeof document !== 'undefined') document.documentElement.lang = lang;
  }, [lang]);

  const value = useMemo(() => {
    const t = (key, vars) => {
      const active = get(dict[lang], key);
      const fallback = active === undefined ? get(dict.en, key) : active;
      return interpolate(fallback === undefined ? key : fallback, vars);
    };
    return { lang, setLang: setLangState, t };
  }, [lang]);

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>;
}

export function useI18n() {
  const ctx = useContext(I18nContext);
  if (!ctx) throw new Error('useI18n must be used inside <I18nProvider>');
  return ctx;
}

// ---------------------------------------------------------------------------
// Dictionaries
// ---------------------------------------------------------------------------
const dict = {
  en: {
    nav: {
      home: 'Home',
      builder: 'Rule builder',
      format: 'Rule format',
      database: 'Database',
      docs: 'Mod docs',
      brandHq: 'HQ',
    },
    hero: {
      eyebrow: 'Modern Optimization Plugin · Revival',
      lead: 'The ultimate My Summer Car optimization mod — a full from-scratch rewrite with smart load distribution, live diagnostics, crash prevention and first-class mod compatibility.',
      ctaBuild: 'Make a rule file',
      ctaBrowse: 'Browse the database',
      version: 'Version',
      runtime: 'Runtime',
      license: 'License',
    },
    home: {
      pathsHead: 'Pick your path',
      pathsLead: 'Whether you play the game or make mods, start here.',
      authorTitle: 'I make mods',
      authorBody: 'Make your mod compatible in minutes. Build a rule file with a visual editor — no coding — and submit it for review.',
      authorCta: 'Open the rule builder →',
      playerTitle: 'I play the game',
      playerBody: 'Install the mod, learn the settings, and read the FAQ and console commands.',
      playerCta: 'Read the mod docs →',
      browseTitle: 'Browse rules',
      browseBody: 'Search every compatibility rule the mod ships with and downloads automatically.',
      browseCta: 'Open the database →',
      featHead: 'What MOP - REVIVAL does',
      featLead: 'Performance without the fragility — designed to speed up the game and stay out of the way.',
    },
    features: {
      scheduler: { t: 'Adaptive scheduler', b: 'A per-frame budget loads and unloads objects only as fast as the frame can afford, so MOP never causes the stutter it prevents.' },
      diag: { t: 'Built-in diagnostics', b: 'FPS, frame spikes and garbage-collection are tracked live. "mopr report" writes a full system, settings and rules report for bug hunting.' },
      crash: { t: 'Crash-resistant', b: 'Every object is handled defensively. A run of errors trips a failsafe that re-enables everything instead of breaking your game.' },
      compat: { t: 'Mod compatibility', b: 'A simple rule system lets any mod tell MOP what to leave alone or optimize — via .mopconfig files synced from this server.' },
      vehicle: { t: 'Vehicle-aware', b: 'Vehicles are auto-detected (modded ones too) and only their physics is suspended while parked far away, so saves are never affected.' },
      lang: { t: 'Instant localization', b: 'The whole settings menu switches between English and Russian instantly, with more languages easy to add.' },
    },
    common: {
      forAuthors: 'For mod authors',
      forPlayers: 'For players',
      compatibility: 'Compatibility',
      download: 'Download',
      loading: 'Loading…',
      object: 'Object name',
      copy: 'Copy',
      copied: 'Copied!',
    },
    builder: {
      eyebrow: 'For mod authors',
      title: 'Rule builder',
      lead: 'Describe how MOP should treat your mod\u2019s objects and get a ready-to-submit rule file. No coding — just fill in the fields.',
      step1: '1 · Your mod',
      modIdLabel: 'Mod ID (file name)',
      modIdHint: 'Must match your mod\u2019s ID exactly. The file becomes <ID>.mopconfig.',
      minVerLabel: 'Minimum MOP version',
      step2: '2 · Rules',
      addRule: 'Add a rule',
      empty: 'No rules yet. Add one above, or load the example.',
      loadExample: 'Load example',
      clear: 'Clear all',
      commentLabel: 'Note (optional)',
      commentPlaceholder: 'Why this rule is needed',
      remove: 'Remove',
      moveUp: 'Move up',
      moveDown: 'Move down',
      step3: '3 · Global flags',
      flagsLead: 'Optional switches that apply to the whole session. Leave them off unless you need them.',
      step4: '4 · Review & submit',
      previewTitle: 'Generated file',
      advanced: 'Advanced: paste or drop an existing file',
      actions: {
        ignore: 'Leave an object alone',
        toggle: 'Optimize an object',
        change_parent: 'Reparent an object',
        no_lod: 'Skip the distant stand-in',
      },
      actionHelp: {
        ignore: 'MOP will never enable or disable this object. Use it for anything that breaks when toggled.',
        toggle: 'Add this object to distance optimization and pick how it is toggled.',
        change_parent: 'Move the object under a new parent when the world loads (use "null" for a root object).',
        no_lod: 'Do not create a low-detail clone of this object for far distances.',
      },
      fields: {
        object: 'Object name',
        mode: 'How to toggle',
        parent: 'New parent (or null)',
        fullIgnore: 'Also skip the distant stand-in (fullignore)',
      },
    },
    format: {
      eyebrow: 'For mod authors',
      title: 'Rule format',
      lead: 'A rule file is a plain-text file named after your mod\u2019s ID (e.g. FishingMod.mopconfig). It tells MOP how to treat your mod\u2019s objects. This page is the full reference; the builder writes it for you.',
      whyHead: 'Why you might need one',
      whyBody: 'MOP speeds up the game by enabling and disabling objects based on distance. Most objects are fine, but some break when toggled — an item vanishes, an FSM resets, a quest stops working. A rule file lets you say "leave this alone" or "also optimize this", so your mod stays compatible.',
      coreHead: 'Core directives',
      headerHead: 'File header',
      flagsHead: 'Standalone flags',
      advancedHead: 'Advanced',
      exampleHead: 'Example',
      colSyntax: 'Syntax',
      colMeaning: 'What it does',
    },
    database: {
      eyebrow: 'Compatibility',
      title: 'Rule database',
      lead: 'Every compatibility rule served to the mod. The mod fetches /manifest.json and each file at /rules/<ModID>.mopconfig. Click a mod to see what its rule does.',
      searchPlaceholder: 'Search {count} rule files…',
      loadError: 'Could not load the manifest ({error}). Run "npm run manifest" to generate it.',
      loading: 'Loading manifest…',
      rules: 'rule(s)',
      noActive: 'No active directives.',
      downloadFile: 'Download {id}.mopconfig',
    },
    validator: {
      title: 'Validator',
      lead: 'Paste or drop a .mopconfig below. It is checked live against the format the mod actually uses.',
      modId: 'Mod ID',
      openFile: 'Open file',
      loadExample: 'Load example',
      waiting: 'Waiting for input…',
      valid: 'Valid \u2713 · {directives} directive(s), {warnings} warning(s)',
      invalid: '{errors} error(s), {warnings} warning(s)',
      noDirectives: 'No directives yet.',
      placeholder: '# Drop a .mopconfig here or start typing\nmin_ver: 4.0.0\nignore: MyObject',
      submit: 'Submit via GitHub',
      submitDisabled: 'Fix errors and set a mod ID first',
      submitHint: 'Submitting opens a prefilled GitHub page that creates a pull request adding your file to the database. You need a free GitHub account; a maintainer reviews it before it goes live.',
    },
    review: {
      head: 'How your rule reaches players',
      s1t: 'Submit',
      s1b: 'The builder opens a prefilled GitHub pull request with your <ID>.mopconfig.',
      s2t: 'Auto-check',
      s2b: 'The build validates every rule file; a submission with errors cannot be merged.',
      s3t: 'Review',
      s3b: 'A maintainer checks the rule makes sense and merges it into the database.',
      s4t: 'Sync',
      s4b: 'The server regenerates the manifest. The mod downloads your rule automatically for players who have your mod — no update needed.',
    },
    docs: {
      eyebrow: 'For players',
      title: 'Using the mod',
      lead: 'Everything you need to install, configure and troubleshoot MOP - REVIVAL.',
      nav: 'Documentation',
      s: {
        start: 'Getting started',
        settings: 'Settings',
        faq: 'FAQ',
        console: 'Console & troubleshooting',
      },
      reqHead: 'Requirements',
      req1: 'My Summer Car (Steam).',
      installHead: 'Installation',
      i1: 'Drop MOPR.dll into your Mods folder.',
      i2: 'Drop BouncyCastle.Crypto.dll into Mods/References (it lets MOP reach this rule server over HTTPS on the game\u2019s old runtime).',
      i3: 'Launch the game. MOP initializes on the main menu and starts optimizing once you load a save.',
      firstRun: 'On first launch you\u2019ll see a short animated load screen while MOP takes inventory of the world and applies its first pass — this is normal and only happens once per session.',
      settingsIntro: 'Open Mods → MOP - REVIVAL → Settings. Options are grouped into categories:',
      faqHead: 'FAQ',
      consoleHead: 'Console & troubleshooting',
      consoleIntro: 'Open the MSCLoader console (default ~) and type mopr for diagnostics:',
      colCommand: 'Command',
      colMeaning: 'What it does',
      reportNote: 'For bug reports, "mopr report" writes a full system, settings, rules and performance report to the MOP_Logs folder. Attach it — it makes issues far easier to fix.',
    },
    footer: {
      tagline: 'Info site & rule-file server for MOP - REVIVAL.',
      fine: 'MOP - REVIVAL is GPLv3, by ANICKON (2026). Based on MOP by Athlon (Konrad Figura), originating from KruFPS by Krutonium. My Summer Car is a game by Amistech Games; this project is not affiliated with or endorsed by Amistech.',
    },
  },

  ru: {
    nav: {
      home: 'Главная',
      builder: 'Конструктор правил',
      format: 'Формат правил',
      database: 'База правил',
      docs: 'Документация',
      brandHq: 'HQ',
    },
    hero: {
      eyebrow: 'Modern Optimization Plugin · Revival',
      lead: 'Лучший мод для оптимизации My Summer Car — полностью переписан с нуля: умное распределение нагрузки, живая диагностика, защита от вылетов и первоклассная совместимость с модами.',
      ctaBuild: 'Создать файл правил',
      ctaBrowse: 'Открыть базу правил',
      version: 'Версия',
      runtime: 'Среда',
      license: 'Лицензия',
    },
    home: {
      pathsHead: 'С чего начать',
      pathsLead: 'Играете вы или делаете моды — начните отсюда.',
      authorTitle: 'Я делаю моды',
      authorBody: 'Сделайте мод совместимым за минуты. Соберите файл правил в визуальном редакторе — без кода — и отправьте на проверку.',
      authorCta: 'Открыть конструктор →',
      playerTitle: 'Я играю',
      playerBody: 'Установите мод, разберитесь в настройках, прочитайте FAQ и команды консоли.',
      playerCta: 'Открыть документацию →',
      browseTitle: 'Смотреть правила',
      browseBody: 'Поиск по всем правилам совместимости, которые мод содержит и скачивает автоматически.',
      browseCta: 'Открыть базу →',
      featHead: 'Что делает MOP - REVIVAL',
      featLead: 'Производительность без хрупкости — ускоряет игру и не мешает.',
    },
    features: {
      scheduler: { t: 'Адаптивный планировщик', b: 'Покадровый бюджет включает и выключает объекты не быстрее, чем позволяет кадр, поэтому мод не вызывает те самые просадки, с которыми борется.' },
      diag: { t: 'Встроенная диагностика', b: 'FPS, спайки кадров и сборка мусора отслеживаются вживую. «mopr report» пишет полный отчёт о системе, настройках и правилах для поиска багов.' },
      crash: { t: 'Устойчив к вылетам', b: 'Каждый объект обрабатывается защищённо. Серия ошибок включает предохранитель, который возвращает всё на место, а не ломает игру.' },
      compat: { t: 'Совместимость с модами', b: 'Простая система правил позволяет любому моду сказать, что не трогать, а что оптимизировать — через .mopconfig с этого сервера.' },
      vehicle: { t: 'Понимает транспорт', b: 'Транспорт определяется автоматически (в т.ч. модовый); вдали замораживается только его физика, поэтому сохранения не страдают.' },
      lang: { t: 'Мгновенная локализация', b: 'Всё меню настроек мгновенно переключается между английским и русским, новые языки добавляются легко.' },
    },
    common: {
      forAuthors: 'Авторам модов',
      forPlayers: 'Игрокам',
      compatibility: 'Совместимость',
      download: 'Скачать',
      loading: 'Загрузка…',
      object: 'Имя объекта',
      copy: 'Копировать',
      copied: 'Скопировано!',
    },
    builder: {
      eyebrow: 'Авторам модов',
      title: 'Конструктор правил',
      lead: 'Опишите, как мод должен обходиться с объектами вашего мода, и получите готовый файл правил. Без кода — просто заполните поля.',
      step1: '1 · Ваш мод',
      modIdLabel: 'ID мода (имя файла)',
      modIdHint: 'Должно точно совпадать с ID вашего мода. Файл станет <ID>.mopconfig.',
      minVerLabel: 'Минимальная версия MOP',
      step2: '2 · Правила',
      addRule: 'Добавить правило',
      empty: 'Правил пока нет. Добавьте выше или загрузите пример.',
      loadExample: 'Загрузить пример',
      clear: 'Очистить всё',
      commentLabel: 'Примечание (необязательно)',
      commentPlaceholder: 'Зачем нужно это правило',
      remove: 'Удалить',
      moveUp: 'Вверх',
      moveDown: 'Вниз',
      step3: '3 · Глобальные флаги',
      flagsLead: 'Необязательные переключатели на всю сессию. Не включайте без необходимости.',
      step4: '4 · Проверка и отправка',
      previewTitle: 'Готовый файл',
      advanced: 'Дополнительно: вставить или перетащить готовый файл',
      actions: {
        ignore: 'Не трогать объект',
        toggle: 'Оптимизировать объект',
        change_parent: 'Сменить родителя',
        no_lod: 'Без дальней подставки',
      },
      actionHelp: {
        ignore: 'MOP никогда не будет включать или выключать этот объект. Для всего, что ломается при переключении.',
        toggle: 'Добавить объект в оптимизацию по дистанции и выбрать способ переключения.',
        change_parent: 'Переместить объект к новому родителю при загрузке мира (используйте «null» для корневого объекта).',
        no_lod: 'Не создавать упрощённую копию объекта для больших расстояний.',
      },
      fields: {
        object: 'Имя объекта',
        mode: 'Способ переключения',
        parent: 'Новый родитель (или null)',
        fullIgnore: 'Также без дальней подставки (fullignore)',
      },
    },
    format: {
      eyebrow: 'Авторам модов',
      title: 'Формат правил',
      lead: 'Файл правил — это текстовый файл с именем по ID вашего мода (например, FishingMod.mopconfig). Он говорит моду, как обходиться с объектами вашего мода. Здесь — полный справочник; конструктор напишет файл за вас.',
      whyHead: 'Зачем это нужно',
      whyBody: 'MOP ускоряет игру, включая и выключая объекты по дистанции. Большинство объектов это переносит нормально, но некоторые ломаются при переключении — предмет исчезает, FSM сбрасывается, квест перестаёт работать. Файл правил позволяет сказать «не трогать это» или «оптимизировать и это», чтобы мод оставался совместимым.',
      coreHead: 'Основные директивы',
      headerHead: 'Заголовок файла',
      flagsHead: 'Отдельные флаги',
      advancedHead: 'Дополнительно',
      exampleHead: 'Пример',
      colSyntax: 'Синтаксис',
      colMeaning: 'Что делает',
    },
    database: {
      eyebrow: 'Совместимость',
      title: 'База правил',
      lead: 'Все правила совместимости, отдаваемые моду. Мод запрашивает /manifest.json и каждый файл по /rules/<ModID>.mopconfig. Нажмите на мод, чтобы увидеть, что делает его правило.',
      searchPlaceholder: 'Поиск среди {count} файлов правил…',
      loadError: 'Не удалось загрузить манифест ({error}). Выполните «npm run manifest», чтобы его сгенерировать.',
      loading: 'Загрузка манифеста…',
      rules: 'правил(о)',
      noActive: 'Нет активных директив.',
      downloadFile: 'Скачать {id}.mopconfig',
    },
    validator: {
      title: 'Валидатор',
      lead: 'Вставьте или перетащите .mopconfig ниже. Он проверяется вживую по формату, который реально использует мод.',
      modId: 'ID мода',
      openFile: 'Открыть файл',
      loadExample: 'Загрузить пример',
      waiting: 'Ожидание ввода…',
      valid: 'Корректно \u2713 · директив: {directives}, предупреждений: {warnings}',
      invalid: 'ошибок: {errors}, предупреждений: {warnings}',
      noDirectives: 'Директив пока нет.',
      placeholder: '# Перетащите сюда .mopconfig или начните вводить\nmin_ver: 4.0.0\nignore: MyObject',
      submit: 'Отправить через GitHub',
      submitDisabled: 'Сначала исправьте ошибки и укажите ID мода',
      submitHint: 'Отправка откроет заранее заполненную страницу GitHub, создающую pull request с вашим файлом. Нужен бесплатный аккаунт GitHub; перед публикацией правило проверит мейнтейнер.',
    },
    review: {
      head: 'Как правило попадёт к игрокам',
      s1t: 'Отправка',
      s1b: 'Конструктор открывает готовый pull request на GitHub с вашим <ID>.mopconfig.',
      s2t: 'Авто-проверка',
      s2b: 'Сборка проверяет все файлы правил; заявку с ошибками нельзя влить.',
      s3t: 'Ревью',
      s3b: 'Мейнтейнер проверяет, что правило осмысленно, и вливает его в базу.',
      s4t: 'Синхронизация',
      s4b: 'Сервер пересобирает манифест. Мод сам скачает ваше правило игрокам, у которых установлен ваш мод — обновление не требуется.',
    },
    docs: {
      eyebrow: 'Игрокам',
      title: 'Как пользоваться модом',
      lead: 'Всё для установки, настройки и устранения проблем MOP - REVIVAL.',
      nav: 'Документация',
      s: {
        start: 'Начало работы',
        settings: 'Настройки',
        faq: 'FAQ',
        console: 'Консоль и устранение проблем',
      },
      reqHead: 'Требования',
      req1: 'My Summer Car (Steam).',
      installHead: 'Установка',
      i1: 'Положите MOPR.dll в папку Mods.',
      i2: 'Положите BouncyCastle.Crypto.dll в Mods/References (это даёт моду доступ к серверу правил по HTTPS на старой среде игры).',
      i3: 'Запустите игру. MOP инициализируется в главном меню и начинает оптимизацию после загрузки сохранения.',
      firstRun: 'При первом запуске появится короткий анимированный загрузочный экран, пока MOP составляет опись мира и делает первый проход — это нормально и бывает раз за сессию.',
      settingsIntro: 'Откройте Mods → MOP - REVIVAL → Settings. Опции сгруппированы по категориям:',
      faqHead: 'FAQ',
      consoleHead: 'Консоль и устранение проблем',
      consoleIntro: 'Откройте консоль MSCLoader (по умолчанию ~) и введите mopr для диагностики:',
      colCommand: 'Команда',
      colMeaning: 'Что делает',
      reportNote: 'Для баг-репортов «mopr report» пишет полный отчёт о системе, настройках, правилах и производительности в папку MOP_Logs. Приложите его — так проблему исправят гораздо быстрее.',
    },
    footer: {
      tagline: 'Инфо-сайт и сервер правил для MOP - REVIVAL.',
      fine: 'MOP - REVIVAL под GPLv3, автор ANICKON (2026). На основе MOP от Athlon (Konrad Figura), берущего начало от KruFPS от Krutonium. My Summer Car — игра Amistech Games; проект не связан с Amistech и не одобрен ей.',
    },
  },
};

// Localized directive summary/detail: RU overrides, English falls back to ruleSpec.
export const directiveText = {
  ru: {
    min_ver: { summary: 'Минимальная версия MOP - REVIVAL для этого файла.', detail: 'Если у игрока версия старее, весь файл пропускается с предупреждением. Держите на первой строке.' },
    ignore: { summary: 'Никогда не включать и не выключать этот объект.', detail: 'Важнейшее правило совместимости — для всего, что ломается при переключении. Добавьте «fullignore», чтобы не создавать и дальнюю подставку. Два имени («Место Объект») ограничивают игнор одной локацией.' },
    toggle: { summary: 'Разрешить моду оптимизировать объект.', detail: 'Добавляет объект в оптимизацию по дистанции. Выберите способ: весь объект, только рендер, как предмет, как транспорт (только физика, безопасно для сейвов) или мульти-рендер.' },
    change_parent: { summary: 'Сменить родителя объекта при загрузке мира.', detail: 'Отцепляет хрупкий дочерний объект от родителя, который MOP переключает, чтобы он не выключался вместе с ним. «null» — сделать объект корневым.' },
    no_lod: { summary: 'Не создавать дальнюю подставку для объекта.', detail: 'Исключает один объект из системы LOD-подставок (полезно, если клон плохо выглядит издалека).' },
    sector: { summary: 'Задать свой внутренний сектор.', detail: 'Дополнительно. Создаёт бокс (позиция, масштаб, поворот), отсекающий внешний декор, пока игрок внутри. Имена в конце остаются видимыми (белый список). Координаты через запятую, без пробелов.' },
    ignore_mod_vehicles: { summary: 'Оптимизировать только штатный транспорт.', detail: 'MOP полностью не трогает модовый транспорт и управляет только ванильным.' },
    toggle_all_vehicles_physics_only: { summary: 'Весь транспорт — только физика.', detail: 'Транспорт никогда не выгружается полностью, замораживается только физика. Безопасный вариант.' },
    no_lods: { summary: 'Полностью отключить систему LOD-подставок.', detail: 'В этой сессии дальние клоны не создаются ни для чего.' },
    satsuma_ignore_renderer: { summary: 'Никогда не куллить рендеры Сатсумы.', detail: 'Если мод, меняющий Сатсуму, мерцает или исчезает при переключении рендеров.' },
    dont_destroy_empty_bottles: { summary: 'Сохранять пустые пивные бутылки.', detail: 'MOP не будет уничтожать пустые бутылки (для модов, которые их переиспользуют).' },
    skip_fury_collider_fix: { summary: 'Пропустить фикс коллайдера FURY.', detail: 'Аварийный переключатель совместимости для мода FURY.' },
  },
};
