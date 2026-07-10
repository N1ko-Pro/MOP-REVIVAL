import { useEffect, useState } from 'react';

// Minimal hash router. Routes look like "#/builder" or "#/database". The part
// after the route (e.g. "#/docs/settings") is exposed as `sub` for in-page anchors.

export const ROUTES = ['home', 'builder', 'format', 'database', 'docs'];

function parse(hash) {
  const clean = (hash || '').replace(/^#\/?/, '');
  const [page, sub] = clean.split('/');
  return {
    page: ROUTES.includes(page) ? page : 'home',
    sub: sub || '',
  };
}

export function useHashRoute() {
  const [route, setRoute] = useState(() => parse(window.location.hash));

  useEffect(() => {
    const onChange = () => {
      setRoute(parse(window.location.hash));
      window.scrollTo({ top: 0, behavior: 'auto' });
    };
    window.addEventListener('hashchange', onChange);
    return () => window.removeEventListener('hashchange', onChange);
  }, []);

  return route;
}

export function navigate(page) {
  window.location.hash = `#/${page}`;
}
