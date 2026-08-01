import { useState, type RefObject } from "react";
import { useFrame } from "@react-three/fiber";
import type { Vector3 } from "three";
import { Text3D } from "../Text3D";
import { DROP_RADIUS } from "./tentSlots";

type TentPlaceHolderProps = {
    /** World-space centre of the slot this ring marks. */
    position: readonly [number, number, number];
    /** Slot label, 1-based, shown floating above the ring. */
    number: number;
    /** The shared drag point; the highlight of the ring under the pointer comes from here. */
    dragPoint: RefObject<Vector3>;
};

/** Lifts the ring off the ground just enough to avoid z-fighting with the grass plane. */
const GROUND_OFFSET = 0.02;

/**
 * Height the number floats at above the ring. Lying flat on the ground it was nearly
 * illegible: the camera sits at 1.8 looking at 1.5, i.e. almost horizontally, so it saw the
 * text edge-on. It stands upright instead (no rotation, facing +Z, which is the camera side)
 * and above the ring, but below the tent tops (1.58) so it does not compete with them.
 */
const NUMBER_HEIGHT = 0.8;

/**
 * Drop target shown for each slot while a tent is being dragged — the R3F counterpart of
 * Unity's `TentPlaceHolder`. Highlights itself when the drag point falls inside its radius.
 */
export function TentPlaceHolder({ position, number, dragPoint }: TentPlaceHolderProps) {
    const [highlighted, setHighlighted] = useState(false);

    useFrame(() => {
        /* Same criterion as slotIndexAtX: only X counts, because the drag is pinned to the
           row line and every slot shares the same Z. */
        const inside = Math.abs(dragPoint.current.x - position[0]) <= DROP_RADIUS;

        /* The updater returns the same value when nothing changes and React bails out without
           rendering, so this does not cost a render per frame. */
        setHighlighted((current) => (current === inside ? current : inside));
    });

    return (
        <group position={[position[0], position[1] + GROUND_OFFSET, position[2]]}>
            {/* ringGeometry is born in the XY plane, hence it is laid down flat onto the
                ground. depthWrite false keeps the ring from occluding the number drawn above it. */}
            <mesh rotation={[-Math.PI / 2, 0, 0]}>
                <ringGeometry args={[DROP_RADIUS * 0.72, DROP_RADIUS, 48]} />
                <meshBasicMaterial
                    color={highlighted ? "#ffd166" : "#ff9700"}
                    transparent
                    opacity={highlighted ? 0.9 : 0.35}
                    depthWrite={false}
                />
            </mesh>

            <Text3D
                position={[0, NUMBER_HEIGHT, 0]}
                fontSize={0.6}
                anchorY="middle"
                color={highlighted ? "#ffd166" : "#ff9700"}
            >
                {String(number)}
            </Text3D>
        </group>
    );
}
