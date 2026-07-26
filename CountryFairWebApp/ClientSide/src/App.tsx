import { useEffect, useState } from "react";
import { getRoom } from "./network/client";
import { WaitingScreen } from "./screens/WatingScreen";
import { GameScreen } from "./screens/GameScreen";

function App() {
  const [phase, setPhase] = useState<"waiting" | "game">("waiting");

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

    return () => { cancelled = true }; // sem leave: a ligação é singleton, sobrevive a re-montagens
  }, []);

  return phase === "waiting" ? <WaitingScreen /> : <GameScreen />;
}

export default App;