import { useThree } from "@react-three/fiber";
import { useLayoutEffect, useRef } from "react";
import type { PerspectiveCamera as PerspectiveCameraImpl } from "three";

type CameraProps = {
    /** World-space position of the viewer. */
    position: [number, number, number];
    /** World-space point the camera is aimed at. */
    lookAt?: [number, number, number];
    /** Vertical field of view in degrees. */
    fov?: number;
    near?: number;
    far?: number;
};


/**
 * Perspective camera declared inside the scene and promoted to the rendering camera.
 *
 * Being a scene object rather than a Canvas prop is what lets the aim (`lookAt`) and the
 * projection be recomputed as the viewport changes; the two effects below cover the two
 * things R3F does not do for a camera it did not create itself.
 */
export function Camera({ position, lookAt = [0, 0, 0], fov = 60, near = 0.1, far = 1000 }: CameraProps) {
    const ref = useRef<PerspectiveCameraImpl>(null!);
    const set = useThree((state) => state.set);
    const size = useThree((state) => state.size);

    // Without this the Canvas renders through its own default camera and this one is just
    // another object in the scene.
    useLayoutEffect(() => {
        set({ camera: ref.current });
    }, [set]);

    /*
      R3F only fixes up the aspect of the camera it creates itself; on this one, installed by
      hand with set(), the aspect stayed at the default 1 while the canvas is wide. The image
      came out stretched horizontally (on a 1917x843 canvas, ~2.3x), and since the distortion
      grows towards the edges it was the outer tents that ended up visibly deformed.
    */
    useLayoutEffect(() => {
        ref.current.lookAt(lookAt[0], lookAt[1], lookAt[2]);
        ref.current.aspect = size.width / size.height;
        ref.current.updateProjectionMatrix();
    }, [position[0], position[1], position[2], lookAt[0], lookAt[1], lookAt[2], fov, near, far, size.width, size.height]);

    return (
        <perspectiveCamera ref={ref} position={position} fov={fov} near={near} far={far} />
    );
}
