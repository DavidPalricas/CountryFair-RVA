import {Client } from "colyseus";
import {CountryFairRoom} from "./CountryFairRoom.js";
import {FairState } from "../schemas/FairSchema.js";


export class FairSceneRoom extends CountryFairRoom {
 onJoin(client: Client, options: any) {
   if(this.allPlatformsentered) {
      console.warn(`Rejecting client ${client.sessionId} because all platforms have already joined.`);
      client.leave();
      return;
   }

    if (!this.isPlatformValid(options.platform)) {
       throw new Error(`Invalid platform: ${options.platform}`);
    }

    var clientPlatform : string = options.platform;

    if (this.clientsEntered[clientPlatform]) {
      console.warn(`Rejecting client ${client.sessionId} because platform ${clientPlatform} has already joined.`);
      client.leave();
      return;
    }

    this.clientsEntered[clientPlatform] = true;

    if (clientPlatform === "game") {
      this.gameClientId = client.sessionId;
    }

    this.state = new FairState();
    this.message = "Seleciona a ordem das tendas"; 

     console.log(`Client ${client.sessionId} joined with options:`, options);

    if (Object.values(this.clientsEntered).every(client => client === true)) {
      this.allPlatformsentered = true;
      console.log('All platforms have joined. Streaming the game state to web device');

      this.clients.forEach((client) => {
        if (client.sessionId !== this.gameClientId) {
          console.log(`Sending game state to client ${client.sessionId}`);
          client.send("gamejoined", this.state);
        }
      });
    }
   }
}
