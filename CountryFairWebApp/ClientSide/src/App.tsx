import { useEffect, useState } from "react";
import { getRoom } from "./network/client";
import { WaitingScreen } from "./screens/WatingScreen";
import { GameScreen } from "./screens/GameScreen";

/**
 * Root of the companion web client.
 *
 * Joins the Colyseus `fairsceneroom` on mount and waits for the server's `"gamejoined"`
 * message, which the room only sends once the Unity headset has joined too — that is the
 * signal to swap the waiting screen for the fair scene.
 *
 * The scene is currently rendered unconditionally so the tent ordering can be worked on
 * without a headset attached; the commented-out return is the paired behaviour.
 */
function App() {
  const [phase, setPhase] = useState<"waiting" | "game">("waiting");

  // Matchmaking runs once. `cancelled` guards against the connection resolving after the
  // component is gone (StrictMode mounts twice in development).
  useEffect(() => {
    let cancelled = false;

    getRoom()
      .then((room) => {
        if (cancelled) return;
        console.log("Connected to room:", room.name);
        room.onMessage("gamejoined", () => {
          console.log("Received gamejoined message");
          setPhase("game");
        });
      })
      .catch((err) => {
        if (!cancelled) console.error("Falha no matchmaking:", err);
      });

    // No leave() on cleanup: the room is a module-level singleton and is meant to survive
    // re-mounts. Leaving here would drop the seat the server reserved for this platform.
    return () => { cancelled = true };
  }, []);

  // return phase === "waiting" ? <WaitingScreen /> : <GameScreen />;

  return <GameScreen />
}

export default App;
