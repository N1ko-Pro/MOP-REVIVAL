import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Static files under public/ (manifest.json, rules/*.mopconfig) are served at the
// site root, exactly where the mod's RemoteRuleSync expects them.
export default defineConfig({
  plugins: [react()],
  build: {
    outDir: 'dist',
    emptyOutDir: true,
  },
});
