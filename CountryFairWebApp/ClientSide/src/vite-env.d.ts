/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Porta do servidor Colyseus, lida de CountryFairWebApp/.env */
  readonly SERVER_PORT: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
