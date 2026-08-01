import { useEffect, useLayoutEffect, useRef, useState, type RefObject } from "react";
import { useFrame } from "@react-three/fiber";
import { Euler, MathUtils, Quaternion, type Group, type Vector3 } from "three";
import {Tent} from "./Tent";
import {ArcheryProps} from "../MiniGamesProps/ArcheryProps";
import { FrisbeeProps } from "../MiniGamesProps/FrisbeeProps";
import { FishingProps } from "../MiniGamesProps/FishingProps";
import { DuckProps } from "../MiniGamesProps/DuckProps";
import { Text3D } from "../Text3D";
import {TentRibbon} from "./TentRibbon";
import { DRAG_LIFT, TENT_NAMES, TENT_SLOTS, TENT_Y, type MiniGameType } from "./tentSlots";
import { useTentDrag } from "./useTentDrag";

/** Multiplier on the base scale while the pointer is over the tent. */
const HOVER_SCALE = 1.2;

/** Exponential damping of the animations: higher reaches the target sooner. */
const DAMPING = 12;

/**
 * Module-level temporaries instead of allocating inside useFrame. JavaScript is
 * single-threaded and neither of these is kept after use, so sharing them between the tents
 * is safe and avoids per-frame garbage.
 */
const targetEuler = new Euler();
const targetQuaternion = new Quaternion();

type MiniGameTentProps = {
    /** Which mini-game this tent advertises; also selects the props displayed in front of it. */
    type: MiniGameType;
    /** Index into TENT_SLOTS the tent animates towards while it is not being dragged. */
    slot: number;
    isDragging: boolean;
    /** Unity's `ElementSelected()`: tents that are not being dragged hide themselves. */
    hidden: boolean;
    /** Shared with the TentPlaceHolders, which read it to know which one is under the pointer. */
    dragPoint: RefObject<Vector3>;
    onDragStart: (type: MiniGameType) => void;
    /** Reports the drop position in X; the screen turns it into a slot. */
    onDragEnd: (type: MiniGameType, x: number) => void;
};


/**
 * One playable mini-game tent: the model, its signboard and slot ribbon, the mini-game props
 * in front of it, and the drag behaviour that lets it be moved along the row.
 *
 * Its transform is animated every frame rather than driven by props, so both a slot change
 * and the hover scale read as a motion instead of a jump.
 */
export function MiniGameTent({ type, slot, isDragging, hidden, dragPoint, onDragStart, onDragEnd }: MiniGameTentProps) {
    const miniGamePropsPos: [number, number, number] = [0.6, 0, 2];
    const groupRef = useRef<Group>(null);
    const [hovered, setHovered] = useState(false);

    const { onPointerDown } = useTentDrag({
        objectRef: groupRef,
        dragPoint,
        onDragStart: () => onDragStart(type),
        onDragEnd: (x) => onDragEnd(type, x),
    });

    const slotTransform = TENT_SLOTS[slot];

    /*
      Position and rotation are written in useFrame, so they are not passed as props on the
      <group>: the initial transform is set once on mount and the animation takes over from
      there. The dependency list is deliberately empty — when the slot changes we want the
      animated transition, not a jump.
    */
    useLayoutEffect(() => {
        const group = groupRef.current;

        if (group === null) {
            return;
        }

        group.position.set(...slotTransform.position);
        group.rotation.set(...slotTransform.rotation);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    // A tent that goes invisible or gets grabbed never receives a pointerout, so the hover
    // state has to be cleared by hand or it would stay stuck on.
    useEffect(() => {
        if (hidden || isDragging) {
            setHovered(false);
        }
    }, [hidden, isDragging]);

    // The cursor is a document-level property, so it is restored on cleanup — otherwise a
    // tent unmounting while hovered would leave the grab cursor behind.
    useEffect(() => {
        if (!hovered) {
            return;
        }

        document.body.style.cursor = "grab";
        return () => { document.body.style.cursor = "auto"; };
    }, [hovered]);

    // Drives position, rotation and scale towards their targets. `step` is a frame-rate
    // independent damping factor, so the motion looks the same regardless of refresh rate.
    useFrame((_, delta) => {
        const group = groupRef.current;

        if (group === null) {
            return;
        }

        const step = 1 - Math.exp(-DAMPING * delta);

        if (isDragging) {
            // Follows the pointer directly, lifted off the ground; no damping here or the
            // tent would lag behind the cursor.
            group.position.set(dragPoint.current.x, TENT_Y + DRAG_LIFT, dragPoint.current.z);
        } else {
            /* SnapToCurrentPlaceHolder(), but interpolated instead of instantaneous. */
            group.position.x = MathUtils.lerp(group.position.x, slotTransform.position[0], step);
            group.position.y = MathUtils.lerp(group.position.y, slotTransform.position[1], step);
            group.position.z = MathUtils.lerp(group.position.z, slotTransform.position[2], step);

            /* Quaternion slerp instead of interpolating Euler angles: stops the tent turning
               the wrong way round when the difference passes 180 degrees. */
            targetEuler.set(...slotTransform.rotation);
            targetQuaternion.setFromEuler(targetEuler);
            group.quaternion.slerp(targetQuaternion, step);
        }

        const targetScale = hovered && !isDragging ? HOVER_SCALE : 1;
        group.scale.setScalar(MathUtils.lerp(group.scale.x, targetScale, step));
    });

    return (
        <group
            ref={groupRef}
            name="MiniGameTent"
            /* three ignores invisible objects in the raycast, so this reproduces Unity's
               SetActive(false), including no longer receiving events. */
            visible={!hidden}
            onPointerDown={onPointerDown}
            /* R3F delivers the event to every intersection along the ray, so without the
               stopPropagation the tents behind this one would go into hover too. */
            onPointerOver={(event) => { event.stopPropagation(); setHovered(true); }}
            onPointerOut={() => setHovered(false)}
        >
            {/* Signboard and ribbon hide during the drag, the same way Unity hides
                tentText/tentNumber/ribbon while the tent is grabbed. */}
            <group visible={!isDragging}>
                {/* The tent is 1.58 tall, so the signboard sits at 1.7. maxWidth fits the
                    longest name ("Tenda do Jogo dos Patos") on a single line: with the default
                    of 1.5 only that one wrapped onto two lines and ended up taller than the
                    other three, which made the last tent stand out from the row. */}
                <Text3D position={[0,2, 0]} maxWidth={1.9} fontSize={0.4} >
                    {TENT_NAMES[type]}
                </Text3D>

                {/* The number comes from the slot and not from the tent, just as Unity's
                    SetTentNumber() reads it from currentPlaceHolder: on swapping slots the
                    tent adopts the number of its new one. */}
                <TentRibbon position={[-1.2, 1.7, 0]} number={slotTransform.number}/>
            </group>

            <Tent position={[0, 0, 0]}/>
            {type === "fishing" && <FishingProps position={miniGamePropsPos} />}
            {type === "archery" && <ArcheryProps position={miniGamePropsPos} />}
            {type === "frisbee" && <FrisbeeProps position={miniGamePropsPos} />}
            {type === "duckgame" && <DuckProps position={miniGamePropsPos} />}
        </group>
    );
}
