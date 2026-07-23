import { Client } from "@colyseus/sdk";

// Vem do .env partilhado (CountryFairWebApp/.env), exposto pelo Vite (ver vite.config.ts).
const SERVER_PORT = import.meta.env.SERVER_PORT ?? "2567";

export async function connect() {
    const client = new Client(`http://localhost:${SERVER_PORT}`);
    const room = await client.joinOrCreate('my_room', {
        /* custom join options */
    });
    
    return room;
}
 
connect();