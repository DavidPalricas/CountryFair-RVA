/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Colyseus server port, read from the shared CountryFairWebApp/.env */
  readonly SERVER_PORT: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
