import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { fileURLToPath } from 'node:url'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],

  // O .env esta na raiz da WebApp (CountryFairWebApp/.env), partilhado com o servidor.
  envDir: fileURLToPath(new URL('..', import.meta.url)),

  // Por defeito o Vite so expoe as variaveis VITE_*; SERVER_PORT precisa do prefixo SERVER_.
  envPrefix: ['VITE_', 'SERVER_'],
})
