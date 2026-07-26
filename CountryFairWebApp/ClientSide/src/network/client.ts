import { Client } from "@colyseus/sdk";
import type { Room } from "@colyseus/sdk/Room";

const SERVER_PORT = import.meta.env.VITE_SERVER_PORT ?? "2567";

let roomPromise: Promise<Room> | null = null;

async function connect(): Promise<Room> {
  const client = new Client(`http://localhost:${SERVER_PORT}`);
  return client.joinOrCreate("fairsceneroom", { platform: "web" });
}

export function getRoom(): Promise<Room> {
  if (!roomPromise) {
    roomPromise = connect().catch((err) => {
      roomPromise = null;
      throw err;
    });
  }
  return roomPromise;
}