import { Client } from "@colyseus/sdk";
import type { Room } from "@colyseus/sdk/Room";

/** Must match SERVER_PORT in CountryFairWebApp/.env, which the Colyseus server listens on. */
const SERVER_PORT = import.meta.env.VITE_SERVER_PORT ?? "2567";

/**
 * The single in-flight/settled connection. Kept at module scope so React re-mounts
 * (StrictMode in development) reuse it instead of asking the server for a second seat —
 * the room only accepts one client per platform and would reject the duplicate.
 */
let roomPromise: Promise<Room> | null = null;

async function connect(): Promise<Room> {
  const client = new Client(`http://localhost:${SERVER_PORT}`);
  // `platform: "web"` is what the room uses to tell this client apart from the Unity headset.
  return client.joinOrCreate("fairsceneroom", { platform: "web" });
}

/**
 * Returns the shared room connection, opening it on first call.
 *
 * A failed connection clears the cache so a later call can retry; a successful one is kept
 * for the lifetime of the page.
 */
export function getRoom(): Promise<Room> {
  if (!roomPromise) {
    roomPromise = connect().catch((err) => {
      roomPromise = null;
      throw err;
    });
  }
  return roomPromise;
}
