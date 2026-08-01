/**
 * Replaces Unity's `OrderableTentElement`: composition (a hook) instead of inheritance.
 *
 * It deliberately does not use R3F's `onPointerMove` on a mesh, because during the drag the
 * tent itself covers the ground and the other tents are invisible (hence out of the
 * raycast). Instead it captures the pointer on the canvas and projects the camera ray
 * against a mathematical plane at tent height.
 *
 * The drag only moves in X: every tent lives on the same line (TENT_ROW_Z) and the only
 * thing the player can change is their order along it. Leaving Z free would just let the
 * tent be dragged out of the row, to a position matching no slot at all.
 */

import { useCallback, useEffect, useLayoutEffect, useRef, type RefObject } from "react";
import { useThree, type ThreeEvent } from "@react-three/fiber";
import { Plane, Raycaster, Vector2, Vector3, type Object3D } from "three";
import { TENT_ROW_Z, TENT_Y } from "./tentSlots";

type UseTentDragOptions = {
    /** The dragged object; its current position defines the grab offset. */
    objectRef: RefObject<Object3D | null>;
    /**
     * Mutable Vector3 the drag point is written into. Deliberately NOT state: it changes on
     * every pointermove and one setState per movement would re-render the whole scene. It
     * comes from outside because the TentPlaceHolders read it too, to know which of them is
     * under the pointer.
     */
    dragPoint: RefObject<Vector3>;
    onDragStart: () => void;
    /** X only: the drag point's Z is always TENT_ROW_Z and carries no information. */
    onDragEnd: (x: number) => void;
};

/** Returns the `onPointerDown` handler to spread onto the draggable object. */
export function useTentDrag({ objectRef, dragPoint, onDragStart, onDragEnd }: UseTentDragOptions) {
    const { camera, gl } = useThree();

    // Difference in X between the tent origin and the point the pointer grabbed it at:
    // without this the tent jumps under the cursor on the first frame.
    const grabOffsetX = useRef(0);

    const activePointer = useRef<number | null>(null);

    // Reused across events so nothing is allocated on each pointermove.
    const raycaster = useRef(new Raycaster());
    const pointerNdc = useRef(new Vector2());
    const groundPlane = useRef(new Plane(new Vector3(0, 1, 0), -TENT_Y));
    const groundHit = useRef(new Vector3());

    /**
     * The native listeners are registered once and live as long as the component, so the
     * callbacks have to be read through a ref — otherwise they would stay pinned to the
     * values of the render that registered them.
     */
    const handlers = useRef({ onDragStart, onDragEnd });

    useLayoutEffect(() => {
        handlers.current = { onDragStart, onDragEnd };
    });

    /** Screen -> NDC -> camera ray -> point on the ground plane. */
    const projectToGround = useCallback((event: PointerEvent) => {
        const bounds = gl.domElement.getBoundingClientRect();

        pointerNdc.current.set(
            ((event.clientX - bounds.left) / bounds.width) * 2 - 1,
            -((event.clientY - bounds.top) / bounds.height) * 2 + 1,
        );

        raycaster.current.setFromCamera(pointerNdc.current, camera);

        // Returns null when the ray is parallel to the ground (camera level with the plane).
        return raycaster.current.ray.intersectPlane(groundPlane.current, groundHit.current);
    }, [camera, gl]);

    // Canvas-level pointer listeners, registered once and torn down with the component. They
    // stay attached even when no drag is running; activePointer is what gates the work.
    useEffect(() => {
        const canvas = gl.domElement;

        const handlePointerMove = (event: PointerEvent) => {
            if (activePointer.current !== event.pointerId) {
                return;
            }

            const ground = projectToGround(event);

            if (ground === null) {
                return;
            }

            // Y stays at the value the tent uses and Z is pinned to the row line: only X
            // follows the pointer.
            dragPoint.current.set(ground.x + grabOffsetX.current, TENT_Y, TENT_ROW_Z);
        };

        /**
         * Serves both pointerup and pointercancel: the cancel is what guarantees the drag ends
         * when the gesture is stolen by the browser (scroll, system gesture), instead of
         * leaving a tent glued to the pointer.
         */
        const handlePointerEnd = (event: PointerEvent) => {
            if (activePointer.current !== event.pointerId) {
                return;
            }

            activePointer.current = null;

            if (canvas.hasPointerCapture(event.pointerId)) {
                canvas.releasePointerCapture(event.pointerId);
            }

            handlers.current.onDragEnd(dragPoint.current.x);
        };

        canvas.addEventListener("pointermove", handlePointerMove);
        canvas.addEventListener("pointerup", handlePointerEnd);
        canvas.addEventListener("pointercancel", handlePointerEnd);

        return () => {
            canvas.removeEventListener("pointermove", handlePointerMove);
            canvas.removeEventListener("pointerup", handlePointerEnd);
            canvas.removeEventListener("pointercancel", handlePointerEnd);
        };
    }, [dragPoint, gl, projectToGround]);

    const onPointerDown = useCallback((event: ThreeEvent<PointerEvent>) => {
        const tent = objectRef.current;

        if (tent === null || activePointer.current !== null) {
            return;
        }

        // Without this the tents behind this one in the row started being dragged too.
        event.stopPropagation();

        const ground = projectToGround(event.nativeEvent);

        if (ground === null) {
            return;
        }

        activePointer.current = event.nativeEvent.pointerId;

        // The capture keeps events coming even with the cursor outside the canvas.
        gl.domElement.setPointerCapture(event.nativeEvent.pointerId);

        grabOffsetX.current = tent.position.x - ground.x;
        dragPoint.current.set(tent.position.x, TENT_Y, TENT_ROW_Z);

        handlers.current.onDragStart();
    }, [dragPoint, gl, objectRef, projectToGround]);

    return { onPointerDown };
}
