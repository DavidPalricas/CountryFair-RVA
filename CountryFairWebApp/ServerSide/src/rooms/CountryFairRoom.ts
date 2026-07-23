import { Room } from "colyseus";


export class CountryFairRoom extends Room {
    // Game Clients : WebApp and VR
    maxClients : number = 2;

    protected message  : string = "";
}