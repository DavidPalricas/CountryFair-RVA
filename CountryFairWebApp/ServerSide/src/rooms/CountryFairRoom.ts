import { Room } from "colyseus";

export class CountryFairRoom extends Room {
    protected static platforms: Array<string> = ["web", "game"];
    protected message  : string = "";

     protected clientsEntered: Record<string, boolean> = {
        ['web']: false,
        ['game']: false
    };

    protected allPlatformsentered: boolean = false;

    protected gameClientId : string = '';

    maxClients = 10;

    protected isPlatformValid(platform: string) : boolean {
        platform = platform.trim().toLowerCase()
         return CountryFairRoom.platforms.includes(platform);
    }
}