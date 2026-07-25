import {Client } from "colyseus";
import { CountryFairRoom } from "./CountryFairRoom.js";
import { FairState } from "../schemas/FairSchema.js";

export class FairSceneRoom extends CountryFairRoom {
 
 onJoin(client: Client, options: any) {
    this.state = new FairState();
    this.message = "Seleciona a ordem das tendas"; 
    console.log("FairSceneRoom: onJoin", client.sessionId, options);
 }

}