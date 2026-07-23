/**
 * IMPORTANT:
 * ---------
 * Do not manually edit this file if you'd like to host your server on Colyseus Cloud
 *
 * If you're self-hosting, you can see "Raw usage" from the documentation.
 * 
 * See: https://docs.colyseus.io/server
 */
import { listen } from "@colyseus/tools";

// Import Colyseus config
import app from "./app.config.js";

// Carrega o .env partilhado pela WebApp (CountryFairWebApp/.env).
process.loadEnvFile(new URL("../../.env", import.meta.url));

listen(app, Number(process.env.SERVER_PORT) || 2567);
