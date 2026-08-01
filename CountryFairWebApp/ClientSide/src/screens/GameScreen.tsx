import { useCallback, useRef, useState } from "react";
import { Canvas } from "@react-three/fiber";
import { Vector3 } from "three";
import { Plane } from "../GameProps/Plane";
import {GameLight} from "../GameProps/GameLight";
import { MiniGameTent } from "../GameProps/Tents/MiniGameTent";
import { TentPlaceHolder } from "../GameProps/Tents/TentPlaceHolder";
import { TENT_ROW_Z, TENT_SLOTS, TENT_Y, VIEWER_POSITION, type MiniGameType } from "../GameProps/Tents/tentSlots";
import { slotIndexAtX, swapIntoSlot, type TentOrder } from "../GameProps/Tents/tentOrder";
import{BigTop} from "../GameProps/BigTop";
import {FerrisWheel} from "../GameProps/FerrisWheel";
import{Camera} from "../GameProps/Camera";
import { Text3D } from "../GameProps/Text3D";
import "./GameScreen.css";

/** Left-to-right tent order the scene opens with; index is the slot index. */
const INITIAL_ORDER: TentOrder = ["fishing", "archery", "frisbee", "duckgame"];

/**
 * Decorative big tops filling the fair grounds between the ferris wheel (z = -10) and the
 * mini-game row (z = 9) — without them the middle of the screen was nothing but grass.
 * There are only four playable tents, and `BigTop` clones share geometry and materials with
 * the already-loaded model, so each of these costs a draw call rather than a new model.
 *
 * The ferris wheel takes up more ground than it looks: rotated 90 degrees and scaled to 0.4
 * it spans x from -10 to 10 and z from -13 to -4.5, and no tent may enter that box. Hence
 * the two rear ones sit beyond |x| = 12 and the rest stand in front of it, leaving the
 * central corridor clear.
 *
 * Scales run from 0.8 to nearly 2 (the BigTop base height is 4 metres) and the tallest is
 * also the nearest: that size difference is what gives depth to otherwise flat terrain.
 *
 * `position` is [x, z] in world units — y comes from TENT_Y; `rotationY` is in radians.
 */
const BIG_TOPS = [
    { position: [-15.4, 0.0], rotationY: 0.5, scale: 1.2 },
    { position: [15.0, -10.4], rotationY: -0.45, scale: 1.1 },
    { position: [-12.5, -3.0], rotationY: 0.75, scale: 1.9 },
    { position: [15.4, -1.0], rotationY: -1.0, scale: 1.8 },
    { position: [-15.0, 5.0], rotationY: 1.1, scale: 1.85 },
    { position: [15, 6.0], rotationY: -0.9, scale: 1.95 },
] as const;

/** Footer shown in the bottom-right corner. */
const COPYRIGHT = "© 2026 David Palricas";

/**
 * The fair scene: the therapist reorders the mini-game tents by dragging them along the row.
 *
 * This screen plays the role of Unity's `PlaceHolderManager` — it owns the tent order and
 * knows which tent is being dragged. Unity's `Dictionary<element, placeholder>` is just the
 * `order` array here, where the array index *is* the slot index.
 */
export function GameScreen() {
    const [order, setOrder] = useState<TentOrder>(INITIAL_ORDER);
    const [dragging, setDragging] = useState<MiniGameType | null>(null);

    // Shared between the grabbed tent and the placeholder rings; written on every
    // pointermove, so it lives in a ref rather than in state.
    const dragPoint = useRef(new Vector3());

    const handleDragEnd = useCallback((type: MiniGameType, x: number) => {
        const targetSlot = slotIndexAtX(x);

        // null = dropped in the dead zone between slots; the tent's useFrame walks it back to
        // its original slot on its own, because the order did not change.
        if (targetSlot !== null) {
            setOrder((current) => swapIntoSlot(current, type, targetSlot));
        }

        setDragging(null);
    }, []);

    return (
        <div className="game-screen">

            <Canvas>
                {/* Looks at the centre of the row so all four tents are framed the same way. */}
                <Camera position={VIEWER_POSITION} lookAt={[0, 1.4, TENT_ROW_Z]} />

                <Text3D position={[0, 4.8, TENT_ROW_Z]} fontSize={0.55} maxWidth={10}>
                    Clique e arraste nas tendas para trocar a ordem delas
                </Text3D>
                {/* Hemisphere (blue sky / green grass) instead of flat ambient light: the colour
                    bounce makes the green and red of the tents far livelier than a white
                    ambientLight managed. */}
                <hemisphereLight args={["#87ceeb", "#4f8f3a", 0.75]} />
                {/* Midday sun with a golden touch, between cold white and full golden-hour yellow. */}
                <GameLight position={[10, 14, 8]} color="#ffedb8" intensity={2.6} />
                {/* Cool fill from the opposite side so the shadows do not read as dead. */}
                <GameLight position={[-12, 6, -6]} color="#bcdfff" intensity={0.5} />

                <Plane position={[0, 0, 0]} size={80} texture="/textures/grass.jpg" textureRepeat={20} />
                <FerrisWheel position={[0, 0, -10]} rotation={[0, Math.PI / 2, 0]} scale={0.4} animationSpeed={0.5} />

                {BIG_TOPS.map(({ position, rotationY, scale }) => (
                    <BigTop
                        key={`${position[0]},${position[1]}`}
                        position={[position[0], TENT_Y, position[1]]}
                        rotation={[0, rotationY, 0]}
                        scale={scale}
                    />
                ))}

                {order.map((type, slot) => (
                    <MiniGameTent
                        key={type}
                        type={type}
                        slot={slot}
                        isDragging={dragging === type}
                        hidden={dragging !== null && dragging !== type}
                        dragPoint={dragPoint}
                        onDragStart={setDragging}
                        onDragEnd={handleDragEnd}
                    />
                ))}

                {dragging !== null && TENT_SLOTS.map((slot) => (
                    <TentPlaceHolder
                        key={slot.number}
                        position={slot.position}
                        number={slot.number}
                        dragPoint={dragPoint}
                    />
                ))}

            </Canvas>

            <small className="game-screen__copyright">{COPYRIGHT}</small>
        </div>
    );
}
