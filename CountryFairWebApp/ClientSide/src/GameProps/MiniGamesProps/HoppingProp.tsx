import { useRef } from "react";
import { useFrame } from "@react-three/fiber";
import type { Group, Object3D } from "three";

type HoppingPropProps = {
    /** The already-cloned GLTF scene; cloning is the caller's responsibility. */
    object: Object3D;
    /** Rest position in the parent's local space; the hop is added on top of its Y. */
    position: [number, number, number];
    /** Base orientation of the model, in radians. */
    rotation?: [number, number, number];
    scale?: number;
    /** Hops per second. */
    speed?: number;
    /** Peak hop height, in the parent group's local units. */
    height?: number;
    /** Fraction of the cycle spent in the air; the rest is the pause on the ground. */
    airTime?: number;
    /** Initial offset (0..1) so several copies do not hop in unison. */
    phase?: number;
    /** Pitch amplitude, in radians: nose up on the way up, down on the way down. */
    tilt?: number;
    /** Side-to-side sway amplitude, in radians. */
    sway?: number;
};

/**
 * Wraps a model in a cyclic hop. The hierarchy has three purposeful levels: the outer group
 * animates the height, the middle one holds the model's base orientation, and the inner one
 * animates pitch and roll already inside that frame of reference. Without that last level the
 * pitch would be about the world X and a model with a different yaw would tilt sideways.
 */
export function HoppingProp({
    object,
    position,
    rotation = [0, 0, 0],
    scale = 1,
    speed = 0.8,
    height = 0.05,
    airTime = 0.55,
    phase = 0,
    tilt = 0.25,
    sway = 0.08,
}: HoppingPropProps) {
    const hopRef = useRef<Group>(null);
    const tiltRef = useRef<Group>(null);

    // Driven by the shared clock rather than accumulated delta, so props with the same speed
    // stay in phase for the whole session.
    useFrame(({ clock }) => {
        const hop = hopRef.current;
        const pitch = tiltRef.current;

        if (hop === null || pitch === null) {
            return;
        }

        /* Position within the cycle, always in [0, 1). The modulo of a positive value needs no
           extra normalisation. */
        const cycle = (clock.elapsedTime * speed + phase) % 1;

        if (cycle < airTime) {
            /* Normalised parabola: 4u(1-u) is 0 at both ends and 1 in the middle, so `height`
               really is the peak of the hop. */
            const u = cycle / airTime;

            hop.position.y = position[1] + height * 4 * u * (1 - u);

            /* (1 - 2u) runs from +1 to -1, i.e. it flips the pitch at the highest point. */
            pitch.rotation.x = tilt * (1 - 2 * u);
        } else {
            hop.position.y = position[1];
            pitch.rotation.x = 0;
        }

        /* The sway runs the whole time, including during the pause, so the model is never
           completely still between hops. */
        pitch.rotation.z = sway * Math.sin(cycle * Math.PI * 2);
    });

    return (
        <group ref={hopRef} position={position}>
            <group rotation={rotation} scale={scale}>
                <group ref={tiltRef}>
                    <primitive object={object} />
                </group>
            </group>
        </group>
    );
}
