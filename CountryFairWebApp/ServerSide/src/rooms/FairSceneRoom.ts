import {Client } from "colyseus";
import { CountryFairRoom } from "./CountryFairRoom.js";

export class FairSceneRoom extends CountryFairRoom {
 
 onJoin(client: Client, options: any) {
    this.message = "Seleciona a ordem das tendas"; 
    console.log("FairSceneRoom: onJoin", client.sessionId, options);
 }

}